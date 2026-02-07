# Architecture Guide

## Overview

Storefront follows a **Modular Monolith** architecture with **Clean Architecture** principles. This provides the organizational benefits of microservices while maintaining the operational simplicity of a monolith.

---

## Architectural Principles

### 1. Modular Monolith

**What it is:**
- Single deployable application
- Physically separated modules
- Isolated database schemas
- Independent migration histories
- Modules communicate via well-defined contracts

**Benefits:**
- ✅ Simple deployment (one application)
- ✅ Shared database (ACID transactions)
- ✅ Easy to refactor to microservices later
- ✅ Lower operational complexity
- ✅ Faster development

### 2. Clean Architecture

Each module follows the **Onion Architecture** pattern:

```
┌─────────────────────────────────────┐
│         API Layer                   │  Controllers (thin)
├─────────────────────────────────────┤
│      Infrastructure Layer           │  DbContext, Services, External APIs
├─────────────────────────────────────┤
│      Application Layer              │  Commands, Queries, DTOs, Interfaces
├─────────────────────────────────────┤
│        Domain Layer                 │  Entities, Value Objects, Enums
└─────────────────────────────────────┘
```

**Dependency Rule:** Outer layers depend on inner layers, never the reverse.

### 3. Database Isolation

**Single Physical Database, Separate Schemas:**

```sql
storefront (database)
├── identity  (schema)
│   ├── Users
│   ├── Roles
│   ├── RefreshTokens
│   └── __EFMigrationsHistory_Identity
├── catalog   (schema)
│   ├── Products
│   ├── Categories
│   ├── Brands
│   ├── ProductImages
│   ├── ProductBundleItems
│   └── __EFMigrationsHistory_Catalog
└── content   (schema)
    ├── BlogPosts
    ├── StaticPages
    └── __EFMigrationsHistory_Content
```

**Benefits:**
- Logical separation
- Independent migrations
- Cross-module queries possible (when needed)
- Schema-level permissions

---

## System Modules

### Module 1: Identity

**Responsibility:** Authentication & Authorization

**Features:**
- Admin user management
- Role-based access control (Admin, Manager, User)
- JWT token generation
- Refresh token handling

**Technology:**
- ASP.NET Core Identity
- JWT Bearer authentication
- BCrypt password hashing

**Schema:** `identity`

**Key Files:**
- `src/Modules/Identity/Core/Domain/Entities/ApplicationUser.cs`
- `src/Modules/Identity/Infrastructure/Persistence/IdentityDbContext.cs`
- `src/Modules/Identity/Infrastructure/Services/TokenService.cs`

---

### Module 2: Catalog

**Responsibility:** Product catalog & inventory

**Features:**
- Product management (Simple + Bundle products)
- Hierarchical categories
- Brand management
- Fuzzy search (PostgreSQL trigrams)
- Async image processing (Channels + ImageSharp)
- Price configuration toggle

**Technology:**
- EF Core 10
- ImageSharp (WebP conversion)
- System.Threading.Channels
- PostgreSQL GIN indexes

**Schema:** `catalog`

**Key Files:**
- `src/Modules/Catalog/Core/Domain/Entities/Product.cs`
- `src/Modules/Catalog/Core/Domain/Entities/ProductBundleItem.cs`
- `src/Modules/Catalog/Infrastructure/Services/ImageUploadService.cs`
- `src/Modules/Catalog/Infrastructure/BackgroundJobs/ImageProcessingBackgroundService.cs`

**Bundle System:**
```
Product (Bundle)
├── ProductBundleItem → Product (Component 1)
├── ProductBundleItem → Product (Component 2)
└── ProductBundleItem → Product (Component 3)
```

---

### Module 3: Content

**Responsibility:** SEO content management

**Features:**
- Blog posts
- Static pages (About, Contact)
- SEO metadata (owned entity)
- Slug generation with uniqueness
- Sitemap generation

**Technology:**
- EF Core 10
- Owned entities for SEO

**Schema:** `content`

**Key Files:**
- `src/Modules/Content/Core/Domain/Entities/BlogPost.cs`
- `src/Modules/Content/Core/Domain/ValueObjects/SeoMetadata.cs`
- `src/Modules/Content/Infrastructure/Services/SlugService.cs`

---

## Design Patterns

### 1. CQRS (Command Query Responsibility Segregation)

**Commands:** Modify state (Create, Update, Delete)
**Queries:** Read state (Get, List, Search)

```csharp
// Command
public record CreateProductCommand(...) : IRequest<Result<string>>;

// Query
public record GetProductDetailsQuery(string ProductId) : IRequest<Result<ProductDto>>;
```

**Implementation:** MediatR library

**Benefits:**
- Clear separation of concerns
- Easier testing
- Optimized read models
- Scalable (can split later)

### 2. Result Pattern

**Instead of exceptions for flow control:**

```csharp
// ❌ Old way
public Product GetProduct(string id)
{
    var product = _db.Products.Find(id);
    if (product == null) throw new NotFoundException();
    return product;
}

// ✅ New way
public Result<Product> GetProduct(string id)
{
    var product = _db.Products.Find(id);
    if (product == null) 
        return Error.NotFound("Product.NotFound", "Product not found");
    return Result<Product>.Success(product);
}
```

**Benefits:**
- Explicit error handling
- Type-safe errors
- Better API responses
- No performance penalty of exceptions

### 3. Repository Pattern

**Not used - Direct DbContext access in handlers**

Why? With EF Core and CQRS, repository adds unnecessary abstraction. Handlers use DbContext directly for flexibility.

### 4. Domain Events

**Reserved for future use** - Placeholder in SharedKernel for:
- Event-driven workflows
- Module communication
- Outbox pattern

### 5. Background Processing

**Image Upload Flow:**

```
Controller → ImageUploadService → Channel<ImageUploadMessage>
                                        ↓
              ImageProcessingBackgroundService (Consumer)
                                        ↓
              Generate WebP variants (200px, 600px, 1200px)
                                        ↓
              Save to /uploads/products/{id}/
                                        ↓
              Update ProductImage in database
```

**Benefits:**
- Non-blocking HTTP requests
- Parallel image processing
- Fault tolerance
- Retry logic

---

## Data Flow

### Create Product (Bundle) Example

```
1. Client sends POST /api/catalog/products
         ↓
2. ProductsController receives request
         ↓
3. MediatR sends CreateProductCommand
         ↓
4. CreateProductCommandValidator validates
         ↓
5. CreateProductCommandHandler:
   - Validates SKU uniqueness
   - Validates category/brand existence
   - Validates component products (if bundle)
   - Creates Product entity
   - Creates ProductBundleItem entities
   - Saves to database
         ↓
6. Returns Result<string> (product ID)
         ↓
7. Controller maps result to HTTP response
         ↓
8. Client receives 201 Created with product ID
```

---

## Module Communication

**Rule:** Modules CANNOT reference each other directly.

**Enforced by:**
- Physical project boundaries
- NetArchTest rules in tests
- Code review

**Future:** Use events for cross-module communication when needed.

---

## Security Architecture

### Authentication Flow

```
1. Client → POST /api/identity/auth/login (email, password)
         ↓
2. SignInManager validates credentials
         ↓
3. TokenService generates JWT + RefreshToken
         ↓
4. RefreshToken saved to database
         ↓
5. Returns { accessToken, refreshToken, expiresIn }
         ↓
6. Client stores tokens
         ↓
7. Subsequent requests include: Authorization: Bearer <token>
         ↓
8. JWT middleware validates token
         ↓
9. Sets HttpContext.User (claims)
         ↓
10. [Authorize] attribute checks roles
```

### Token Refresh Flow

```
1. Access token expires (30 minutes)
         ↓
2. Client → POST /api/identity/auth/refresh { refreshToken }
         ↓
3. Validate refresh token in database
         ↓
4. Generate new access token
         ↓
5. Optionally rotate refresh token
         ↓
6. Return new tokens
```

---

## Performance Considerations

### Image Processing

- **Async:** Upload returns 202 Accepted immediately
- **Background:** Processing happens in BackgroundService
- **Formats:** Convert all to WebP (60-80% smaller)
- **Sizes:** Thumbnail (200px), Medium (600px), Large (1200px)

### Database Indexes

**B-Tree Indexes:**
- SKU (unique)
- Foreign keys (CategoryId, BrandId)
- Status fields (IsActive, StockStatus)

**GIN Trigram Indexes:**
- Product.Name (fuzzy search)
- Product.Description (fuzzy search)
- Product.SKU (fuzzy search)

### Caching Strategy

**Development:** No caching (fast iteration)

**Production:**
- Nginx caches static assets (images)
- API responses can add cache headers
- Consider Redis for product lists (future)

---

## Scalability Path

### Current: Modular Monolith
```
API + DB (single deployment)
```

### Future: Microservices
```
Identity Service → Identity DB
Catalog Service → Catalog DB
Content Service → Content DB
API Gateway (routing)
```

**Migration is easy** because modules are already isolated!

---

## Testing Strategy

### Architecture Tests (NetArchTest)

Automatically validate:
- ✅ Domain doesn't depend on Infrastructure
- ✅ Modules don't reference each other
- ✅ Commands implement IRequest<Result<T>>
- ✅ Handlers follow naming conventions

### Unit Tests

Test domain logic in isolation:
- Product entity behavior
- Result pattern
- Value objects
- Business rules

### Integration Tests

Test full HTTP request/response cycle:
- Real database (Testcontainers)
- Full DI container (WebApplicationFactory)
- Authenticated requests
- Database state verification

**Coverage:** ~70% (focus on critical paths)

---

## Configuration Management

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."  // PostgreSQL connection
  },
  "Jwt": {
    "Secret": "...",            // JWT signing key
    "Issuer": "...",
    "Audience": "..."
  },
  "CatalogSettings": {
    "PricingEnabled": false,    // Toggle pricing
    "RequirePriceForProducts": false,
    "ShowPriceLabel": "Contact for Quote",
    "AllowPriceInquiry": true
  }
}
```

### Environment-Specific Configs

- `appsettings.json` - Base settings
- `appsettings.Development.json` - Override for dev
- `appsettings.Production.json` - Override for prod
- `.env` files - Sensitive data (not committed)

---

## Deployment Architecture

### Development

```
Docker Compose:
├── db (PostgreSQL 16)
├── api (.NET 10 - hot reload)
└── web (Next.js 15 - hot reload)
```

### Production

```
Docker Compose:
├── db (PostgreSQL 16 + backups)
├── api (.NET 10 Alpine - optimized)
├── web (Next.js standalone)
└── nginx (reverse proxy + static files)
```

**Nginx Routes:**
- `/` → Next.js (port 3000)
- `/api/` → .NET API (port 8080)
- `/uploads/` → Static files (direct serving)

---

## Key Design Decisions

### Why Modular Monolith?
- Simplicity of deployment
- ACID transactions across modules
- Easy to split later if needed
- Lower operational overhead than microservices

### Why CQRS without Event Sourcing?
- Simpler to understand
- Adequate for CRUD operations
- Can add events later if needed
- Better performance for reads

### Why Result Pattern?
- Explicit error handling
- Type-safe errors
- Better API design
- No exception performance penalty

### Why Schema Isolation?
- Logical module boundaries
- Independent migrations
- Clear ownership
- Prevents coupling

### Why Background Image Processing?
- Non-blocking HTTP responses
- Better UX (202 Accepted)
- Fault tolerance (retry logic)
- Scalable (queue can grow)

### Why No Microservices?
- Single database reduces complexity
- Easier to develop and debug
- Adequate for current scale
- Can migrate to microservices when needed

---

## Anti-Patterns Avoided

❌ **God Objects** - Large entities doing everything  
✅ **Single Responsibility** - Small, focused entities

❌ **Anemic Domain Model** - Entities with only properties  
✅ **Rich Domain Model** - Entities with behavior

❌ **Repository Over-abstraction** - Generic repository for everything  
✅ **Direct DbContext** - Flexibility per use case

❌ **Service Layer God Classes** - One service doing everything  
✅ **CQRS Handlers** - One handler per operation

❌ **Throwing Exceptions for Flow Control**  
✅ **Result Pattern** - Explicit success/failure

---

## Architecture Evolution

### Phase 1: Current (Modular Monolith)
```
Single API + Single DB (multiple schemas)
```

### Phase 2: Vertical Scaling
```
API (multiple instances) + Load Balancer
Single DB (connection pooling)
```

### Phase 3: Read Replicas
```
API → Primary DB (writes)
   ↘→ Read Replica (reads)
```

### Phase 4: Microservices (if needed)
```
Identity Service → Identity DB
Catalog Service → Catalog DB
Content Service → Content DB
Order Service → Order DB
API Gateway + Event Bus
```

---

## Module Details

### Identity Module

**Boundaries:**
- IN: Login requests, token refresh
- OUT: JWT tokens, user info
- NO direct access to other modules

**Storage:** `identity` schema

**Dependencies:**
- ASP.NET Core Identity
- JWT Bearer
- SharedKernel (Result pattern)

---

### Catalog Module

**Boundaries:**
- IN: Product CRUD, search, image uploads
- OUT: Product data, images
- NO dependency on Identity (auth handled by API layer)

**Storage:** `catalog` schema

**Dependencies:**
- ImageSharp (image processing)
- Channels (background jobs)
- SharedKernel (Result pattern)

**Special Features:**
- Fuzzy search (pg_trgm)
- Background image processing
- Bundle product support

---

### Content Module

**Boundaries:**
- IN: Blog/page CRUD, SEO data
- OUT: Content, sitemaps
- NO dependency on other modules

**Storage:** `content` schema

**Dependencies:**
- SharedKernel (Result pattern)

**Special Features:**
- Slug generation with uniqueness
- SEO metadata as owned entity
- Sitemap generation

---

## Extension Points

### Adding a New Module

1. Create project structure:
   ```
   src/Modules/NewModule/
   ├── Core/
   │   ├── Domain/
   │   └── Application/
   ├── Infrastructure/
   └── API/
   ```

2. Create DbContext with schema:
   ```csharp
   protected override void OnModelCreating(ModelBuilder builder)
   {
       builder.HasDefaultSchema("newmodule");
   }
   ```

3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddNewModule(configuration);
   ```

4. Add architecture tests:
   ```csharp
   Types.InAssembly(NewModuleAssembly)
       .Should().NotHaveDependencyOn("Storefront.Modules.*")
   ```

---

## Technology Choices Explained

### Why PostgreSQL over SQL Server?
- ✅ Open source (no licensing costs)
- ✅ Better full-text search (pg_trgm)
- ✅ JSON support (future flexibility)
- ✅ Excellent Docker support
- ✅ Battle-tested at scale

### Why Next.js over Blazor?
- ✅ Better SEO (Server Components)
- ✅ Larger ecosystem (npm packages)
- ✅ Faster builds
- ✅ Separate frontend/backend teams possible

### Why MediatR over Direct Service Calls?
- ✅ Decoupled handlers
- ✅ Pipeline behaviors (logging, validation)
- ✅ Easier testing
- ✅ Clear command/query separation

### Why JWT over Session Cookies?
- ✅ Stateless (no server-side storage)
- ✅ Works with mobile apps
- ✅ Can be validated in multiple services
- ✅ Scales horizontally

---

## Performance Benchmarks

### Image Processing

| Operation | Time | Notes |
|-----------|------|-------|
| Upload (1MB JPEG) | ~50ms | HTTP request only |
| Background Processing | ~500ms | Generate 3 WebP variants |
| Storage | ~70% reduction | WebP vs JPEG |

### Database Queries

| Query | Time | Notes |
|-------|------|-------|
| Get Product by ID | <10ms | Indexed primary key |
| Search (fuzzy) | 20-50ms | GIN trigram index |
| List Products (20) | 15-30ms | Paginated with indexes |

### API Response

| Endpoint | Time | Notes |
|----------|------|-------|
| GET /products | 20-40ms | With pagination |
| POST /products | 30-60ms | Validation + save |
| POST /images | 50-80ms | Queue + return 202 |

---

## Monitoring & Observability

### Logging

**Serilog** configured for structured logging:

```csharp
Log.Information("Product {ProductId} created by {UserId}", productId, userId);
```

**Log Levels:**
- Debug: Development only
- Information: Important business events
- Warning: Recoverable errors
- Error: Unhandled exceptions

### Health Checks

```http
GET /health → 200 OK
```

**Checks:**
- API is running
- Can connect to database
- Background services running

### Future: Observability Stack

- **Metrics:** Prometheus
- **Tracing:** OpenTelemetry
- **Logs:** Elasticsearch + Kibana
- **Alerts:** Grafana

---

## Security Considerations

### Input Validation

- ✅ FluentValidation on all commands
- ✅ Parameterized SQL (EF Core)
- ✅ MaxLength constraints
- ✅ Type safety (C# + TypeScript)

### Authentication

- ✅ JWT with short expiry (30 min)
- ✅ Refresh tokens with rotation
- ✅ BCrypt password hashing
- ✅ HTTP-only cookies (frontend)

### Authorization

- ✅ Role-based access control
- ✅ [Authorize] attributes on controllers
- ✅ Claims-based permissions

### Data Protection

- ✅ Secrets in environment variables
- ✅ Connection strings not committed
- ✅ .gitignore for sensitive files

---

## References

- [Clean Architecture (Uncle Bob)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Modular Monolith (Kamil Grzybek)](https://www.kamilgrzybek.com/design/modular-monolith-primer/)
- [CQRS Pattern (Martin Fowler)](https://martinfowler.com/bliki/CQRS.html)
- [Result Pattern](https://enterprisecraftsmanship.com/posts/error-handling-exception-or-result/)

---

**Next:** [API Reference](API_REFERENCE.md) | [Testing Guide](TESTING.md)
