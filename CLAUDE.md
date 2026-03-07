# Storefront — Project Rules for Claude Code

B2B furniture manufacturer platform. Partners (businesses) browse the catalog, build a cart, and submit order requests. Admin staff manages products, partners, color charts, and processes orders with pricing/status updates.

## Running the Project

```bash
# 1. Start database
docker-compose up -d

# 2. Start backend (auto-migrates on startup)
cd src/API/Storefront.Api && dotnet run

# 3. Start frontend
cd web && npm run dev
```

Ports: Backend `http://localhost:8080` · Frontend `http://localhost:3000` · Swagger `http://localhost:8080/swagger` · PostgreSQL `localhost:5432`

Seeded admin credentials: `admin@storefront.com` / `AdminPassword123!`

## Key Architectural Decisions

- **Modular Monolith** — .NET 10, Clean Architecture + CQRS per module
- No public e-commerce (no checkout, no payment processing)
- Pricing is disabled by default (`Features.Pricing.Enabled = false`). The app operates as a quote-request system: partners request orders, admin manages status/shipping. Pricing UI (product prices, order pricing dialog) only appears when the feature flag is enabled.
- Public storefront is toggleable via feature flag (`Features.PublicStorefront.Enabled`, defaults off)
- All business logic lives in the backend — frontend is display only

## Tech Stack

- **Backend:** .NET 10, C# 14, PostgreSQL 16, EF Core 10, MediatR, FluentValidation, JWT auth
- **Frontend:** Next.js 15 (App Router), React 19, TypeScript (strict), Tailwind CSS, shadcn/ui, TanStack Query v5, Axios, sonner (toasts)
- **Infrastructure:** Docker Compose (dev), Nginx (prod)

---

## Backend Rules

### Module Structure

Every module lives under `src/Modules/{ModuleName}/Storefront.Modules.{ModuleName}/`:

```
Core/
  Domain/Entities/       ← Pure domain entities
  Domain/Enums/          ← Domain enums
  Application/Commands/  ← MediatR IRequest<Result<T>> + handler
  Application/Queries/   ← MediatR IRequest<Result<T>> + handler
  Application/Validators/← FluentValidation validators
Infrastructure/
  Persistence/{Module}DbContext.cs  ← One DbContext per module, own schema
API/
  Controllers/           ← Thin controllers: auth → mediator → HTTP response
```

### CQRS with MediatR

- Commands mutate state. Queries read state. Never mix.
- Command/Query record + Handler class go in the same file for simple cases.
- Always use `IRequest<Result<T>>` or `IRequest<Result>` return types.

### Result Pattern (never throw for business errors)

```csharp
// Correct
return Result<string>.Failure(Error.NotFound("Product.NotFound", "Product not found."));
return Result<string>.Success(product.Id);

// Wrong — never throw ApplicationException for business logic
throw new ApplicationException("Product not found");
```

Error types: `Error.NotFound`, `Error.Validation`, `Error.Conflict`, `Error.Unauthorized`.

### Database Schemas (physical isolation)

- Each module has its own PostgreSQL schema: `identity`, `catalog`, `content`, `orders`
- Cross-module data access is forbidden through DbContext
- Schema is set in DbContext: `builder.HasDefaultSchema("catalog")`
- Never run raw migrations manually — the app auto-creates/updates schemas on startup via `DatabaseExtensions.cs`
- New tables: add DDL in `DatabaseExtensions.cs` AND add EF Core configuration in the module's DbContext

### Controllers are Thin

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
{
    var result = await _mediator.Send(command, ct);
    return result.IsSuccess
        ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
        : result.Error.Type switch
        {
            "Validation" => BadRequest(...),
            "NotFound"   => NotFound(...),
            _            => StatusCode(500, ...)
        };
}
```

### Authentication & Authorization

| Role | Login Endpoint | Token Storage | API Prefix |
|---|---|---|---|
| Admin | `POST /api/identity/auth/login` | `localStorage.accessToken` | `/api/admin/*`, `/api/identity/admin/*` |
| Partner | `POST /api/identity/partners/auth/login` | `localStorage.partner_access_token` | `/api/partner/*` |
| Public | None | None | `/api/catalog/*`, `/api/content/*` |

- Admin endpoints: `[Authorize(Roles = "Admin")]`
- Partner endpoints: `[Authorize]`
- Public endpoints: no `[Authorize]` attribute
- User ID: `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`
- Partner company ID: `User.FindFirst("companyId")?.Value`
- Partner company name: `User.FindFirst("companyName")?.Value`

### Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Commands | `{Action}{Entity}Command` | `CreateProductCommand` |
| Queries | `Get{Entity}Query` | `GetProductByIdQuery` |
| Handlers | `{Command/Query}Handler` | `CreateProductCommandHandler` |
| DTOs | `{Entity}Dto` | `ProductDto`, `OrderSummaryDto` |
| Controllers | `{Entity}Controller` | `ProductsController` |

### Async & EF Core

- All I/O methods are `async Task<T>`. Always pass `CancellationToken`.
- Use `Select()` to project to DTOs — never load full entities unnecessarily.
- Use `Include()` for related data you'll use. Avoid N+1.
- Never use `.Result` or `.Wait()` on async operations.

---

## Frontend Rules

### Directory Structure

```
web/src/
  app/
    admin/                ← Admin panel (auth required)
    partner/              ← Partner portal (partner auth)
    (public)/             ← Public storefront (togglable)
  components/
    ui/                   ← shadcn/ui base components
    admin/                ← Admin-specific components
    products/             ← Product display components
    orders/               ← Order/cart components
    layout/               ← Header, Footer, Sidebar
  lib/
    api.ts                ← Shared axios instance + global types
    api/                  ← Domain-specific API clients (orders.ts, partners.ts, settings.ts)
    utils.ts              ← Utilities (cn, formatPrice, getImageUrl, generateSlug)
```

### Server vs Client Components

- Public SEO pages → Server Components (async, no "use client")
- Admin/partner pages → Client Components ("use client" — they're behind auth)
- Never fetch data with `useEffect` + `axios` in Server Components

### Data Fetching (TanStack Query v5)

```tsx
const { data, isLoading } = useQuery({
  queryKey: ["products", filters],
  queryFn: () => catalogApi.searchProducts(filters),
});

const mutation = useMutation({
  mutationFn: (data) => catalogApi.createProduct(data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ["products"] });
    toast.success("Product created");
  },
  onError: (err: any) => toast.error(err.response?.data?.message || "Failed"),
});
```

- Query keys must include all variables that affect the result
- Always `invalidateQueries` on successful mutations
- All API calls go through `lib/api.ts` axios instance — never call axios directly in components

### Toast Notifications

```tsx
import { toast } from "sonner";
toast.success("Saved");
toast.error("Failed to save");
// Never use @/components/ui/use-toast — always sonner
```

### Image Handling

```tsx
<Image src={getImageUrl(product.primaryImageUrl)} alt={product.name} unoptimized />
```

- Use `getImageUrl()` from `lib/utils` for API images
- `unoptimized` prop is required because images are served from the .NET API directly

### TypeScript

- No `any` types except `catch (err: any)` and unavoidable interop
- Always type API response generics: `api.get<Product[]>()`
- `params` in Next.js 15 are `Promise<{id: string}>` — always `await params` or `use(params)`

### Styling

- Tailwind utility classes only — no CSS modules, no inline `style={}` except dynamic values
- Use `cn()` from `lib/utils` for conditional classes
- Use shadcn/ui components from `@/components/ui/` — never create from scratch if one exists
- Dark mode is not implemented

---

## Feature Flags

Stored in `content.AppSettings`, fetched from `GET /api/content/settings`.

Current flags:
- `Features.Blog.Enabled` — show/hide blog section
- `Features.Pricing.Enabled` — show/hide product prices AND order pricing UI (Set Pricing button, pricing summary, pricing dialog in admin orders). Defaults to `false` — the app operates as a pure quote-request system without prices.
- `Features.PublicCatalog.Enabled` — allow public browsing
- `Features.PublicStorefront.Enabled` — show public website (off = redirect to partner login). Defaults to `false`.
- `Site.Name`, `Site.ContactEmail`, `Site.MaintenanceMode`

Adding a new flag:
1. Add seed entry in `DatabaseExtensions.cs → SeedDefaultSettingsAsync()`
2. Use `settingsApi.isFeatureEnabled("Features.X.Enabled")` in components

## Module Boundaries

```
Frontend → API → Module Controllers → MediatR → Handlers → DbContext → PostgreSQL
```

Modules never import from each other's namespaces. If Module A needs Module B's data, duplicate the read through a separate query (domain events planned for future).

## Adding a New Feature — Checklist

### Backend
- [ ] Create Domain entity in `Core/Domain/Entities/`
- [ ] Add `DbSet<Entity>` to module's DbContext and configure in `OnModelCreating`
- [ ] Add DDL to `DatabaseExtensions.cs`
- [ ] Create Command(s) + Handler(s) + Validator(s) in `Core/Application/Commands/`
- [ ] Create Query(ies) + Handler(s) + DTO(s) in `Core/Application/Queries/`
- [ ] Create Controller in `API/Controllers/` with proper `[Authorize]`
- [ ] Verify: `dotnet build src/API/Storefront.Api/Storefront.Api.csproj`

### Frontend
- [ ] Add API client methods in `web/src/lib/api/`
- [ ] Create page in `web/src/app/admin/` or `web/src/app/partner/`
- [ ] Add navigation link in sidebar or layout
- [ ] Verify: `npx tsc --noEmit` and `npx next lint`

## What NOT to Do

- Never put business logic in controllers, pages, or React components
- Never access one module's DbContext from another module
- Never use `.Wait()` or `.Result` on async operations
- Never hardcode API URLs — use the shared `api` axios instance
- Never commit secrets, connection strings, or JWT secrets
- Never add `console.log` without removing before commit
- Never create UI components from scratch if shadcn/ui has one
- Do not add comments that narrate what the code does — only comment non-obvious business rules and workarounds
