# Storefront - Quick Reference Card

## 🚀 Most Common Commands

### Start Development Environment
```bash
# Terminal 1: Start Database
docker-compose up -d db

# Terminal 2: Start API
cd src/API/Storefront.Api && dotnet watch run

# Terminal 3: Start Frontend
cd web && npm run dev
```

### Test Everything
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Storefront.ArchitectureTests/Storefront.ArchitectureTests.csproj
dotnet test tests/Storefront.UnitTests/Storefront.UnitTests.csproj
dotnet test tests/Storefront.IntegrationTests/Storefront.IntegrationTests.csproj
```

### Database Migrations

#### Add Migration
```bash
# Identity
dotnet ef migrations add MigrationName -p src/Modules/Identity/Storefront.Modules.Identity/Storefront.Modules.Identity.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c IdentityDbContext -o Infrastructure/Persistence/Migrations

# Catalog
dotnet ef migrations add MigrationName -p src/Modules/Catalog/Storefront.Modules.Catalog/Storefront.Modules.Catalog.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c CatalogDbContext -o Infrastructure/Persistence/Migrations

# Content
dotnet ef migrations add MigrationName -p src/Modules/Content/Storefront.Modules.Content/Storefront.Modules.Content.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c ContentDbContext -o Infrastructure/Persistence/Migrations
```

#### Apply Migrations
```bash
dotnet ef database update -p src/Modules/Identity/Storefront.Modules.Identity/Storefront.Modules.Identity.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c IdentityDbContext
dotnet ef database update -p src/Modules/Catalog/Storefront.Modules.Catalog/Storefront.Modules.Catalog.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c CatalogDbContext
dotnet ef database update -p src/Modules/Content/Storefront.Modules.Content/Storefront.Modules.Content.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c ContentDbContext
```

### Reset Database
```bash
docker-compose down -v
docker-compose up -d db
sleep 5
# Then apply all migrations (see above)
```

---

## 📍 URLs

| Service | URL | Notes |
|---------|-----|-------|
| Frontend | http://localhost:3000 | Next.js app |
| Backend API | http://localhost:8080 | .NET API |
| Swagger | http://localhost:8080/swagger | API docs |
| PostgreSQL | localhost:5432 | Direct connection |

---

## 🔑 Default Credentials

```
Email: admin@storefront.com
Password: AdminPassword123!
```

---

## 🧪 Test Patterns

### Unit Test (AAA Pattern)
```csharp
[Fact]
public void Method_Scenario_ExpectedResult()
{
    // Arrange
    var sut = new SystemUnderTest();
    
    // Act
    var result = sut.DoSomething();
    
    // Assert
    result.Should().Be(expected);
}
```

### Integration Test
```csharp
[Fact]
public async Task ApiEndpoint_Should_Return_Success()
{
    // Arrange
    var token = await GetAuthTokenAsync();
    _client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);
    
    // Act
    var response = await _client.GetAsync("/api/catalog/products");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

---

## 🐳 Docker Quick Commands

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop all
docker-compose down

# Rebuild and start
docker-compose up -d --build

# Execute in container
docker exec -it storefront-postgres psql -U postgres -d storefront
```

---

## 📦 Package Management

### Backend
```bash
# Restore
dotnet restore

# Add package
dotnet add package PackageName

# Update all
dotnet list package --outdated
```

### Frontend
```bash
# Install
npm install

# Add package
npm install package-name

# Add shadcn component
npx shadcn-ui@latest add button
```

---

## 🔍 Useful PostgreSQL Queries

```sql
-- List all schemas
SELECT schema_name FROM information_schema.schemata;

-- View products
SELECT * FROM catalog."Products";

-- View migrations
SELECT * FROM identity."__EFMigrationsHistory_Identity";
SELECT * FROM catalog."__EFMigrationsHistory_Catalog";
SELECT * FROM content."__EFMigrationsHistory_Content";

-- Check extensions
SELECT * FROM pg_extension;
```

---

## 🛠️ Troubleshooting

### Port in use
```bash
# Find process
lsof -i :8080

# Kill it
kill -9 <PID>
```

### Docker permission denied
```bash
sudo usermod -aG docker $USER
# Log out and back in
```

### EF Core tools not found
```bash
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

---

## 📂 Project Structure

```
Storefront/
├── src/
│   ├── API/Storefront.Api/              # Entry point
│   ├── Shared/Storefront.SharedKernel/  # Result<T>, abstractions
│   └── Modules/
│       ├── Identity/                     # Auth & Users
│       ├── Catalog/                      # Products & Categories
│       └── Content/                      # Blog & Pages
├── tests/
│   ├── ArchitectureTests/               # NetArchTest rules
│   ├── UnitTests/                       # Domain & service tests
│   └── IntegrationTests/                # API tests
└── web/                                 # Next.js frontend
```

---

## 🎯 Module Isolation Rules

✅ **DO**: Use SharedKernel for cross-module abstractions  
✅ **DO**: Use separate schemas (identity, catalog, content)  
✅ **DO**: Use separate migration history tables  

❌ **DON'T**: Reference modules directly  
❌ **DON'T**: Share DbContexts between modules  
❌ **DON'T**: Use the same migration history table  

---

## 🔐 Security Notes

- JWT tokens in HTTP-Only cookies
- Password hashing with Identity
- Input validation with FluentValidation
- CORS configured for localhost
- ⚠️ Change JWT secret in production!
- ⚠️ Enable HTTPS in production!

---

## 📊 Testing Hierarchy

```
Architecture Tests (11 rules)
    ↓ Enforces design
Unit Tests (15+ tests)
    ↓ Tests logic
Integration Tests (12+ tests)
    ↓ Tests APIs
```

---

## 💡 Pro Tips

1. **Use `dotnet watch run`** for hot reload during development
2. **Run architecture tests first** - they're fastest and catch design issues
3. **Use Testcontainers** - no manual database setup needed
4. **FluentAssertions** - makes test failures readable
5. **Keep tests isolated** - each should run independently

---

## 📚 Full Documentation

- **COMMANDS.md** - Complete command reference
- **TESTING.md** - Testing guide
- **README.md** - Project overview
- **ADMIN.md** - Admin dashboard guide

---

**For detailed information, see COMMANDS.md**

Last Updated: December 17, 2025

