# Storefront - Complete Command Reference

## Table of Contents
1. [Project Setup](#project-setup)
2. [Database Commands](#database-commands)
3. [Development Commands](#development-commands)
4. [Testing Commands](#testing-commands)
5. [Docker Commands](#docker-commands)
6. [Migration Commands](#migration-commands)
7. [Frontend Commands](#frontend-commands)
8. [Useful Scripts](#useful-scripts)
9. [Important Notes](#important-notes)

---

## Project Setup

### Initial Solution Creation
```bash
# Create solution
dotnet new sln -n Storefront

# Create projects
dotnet new webapi -n Storefront.Api -o src/API/Storefront.Api
dotnet new classlib -n Storefront.SharedKernel -o src/Shared/Storefront.SharedKernel
dotnet new classlib -n Storefront.Infrastructure -o src/Infrastructure/Storefront.Infrastructure

# Create module projects
dotnet new classlib -n Storefront.Modules.Identity -o src/Modules/Identity/Storefront.Modules.Identity
dotnet new classlib -n Storefront.Modules.Catalog -o src/Modules/Catalog/Storefront.Modules.Catalog
dotnet new classlib -n Storefront.Modules.Content -o src/Modules/Content/Storefront.Modules.Content

# Add projects to solution
dotnet sln Storefront.sln add src/API/Storefront.Api/Storefront.Api.csproj
dotnet sln Storefront.sln add src/Shared/Storefront.SharedKernel/Storefront.SharedKernel.csproj
dotnet sln Storefront.sln add src/Infrastructure/Storefront.Infrastructure/Storefront.Infrastructure.csproj
dotnet sln Storefront.sln add src/Modules/Identity/Storefront.Modules.Identity/Storefront.Modules.Identity.csproj
dotnet sln Storefront.sln add src/Modules/Catalog/Storefront.Modules.Catalog/Storefront.Modules.Catalog.csproj
dotnet sln Storefront.sln add src/Modules/Content/Storefront.Modules.Content/Storefront.Modules.Content.csproj

# Create test projects
dotnet new xunit -n Storefront.UnitTests -o tests/Storefront.UnitTests
dotnet new xunit -n Storefront.IntegrationTests -o tests/Storefront.IntegrationTests
dotnet new xunit -n Storefront.ArchitectureTests -o tests/Storefront.ArchitectureTests

# Add test projects to solution
dotnet sln Storefront.sln add tests/Storefront.UnitTests/Storefront.UnitTests.csproj
dotnet sln Storefront.sln add tests/Storefront.IntegrationTests/Storefront.IntegrationTests.csproj
dotnet sln Storefront.sln add tests/Storefront.ArchitectureTests/Storefront.ArchitectureTests.csproj
```

### Install Required Packages

#### SharedKernel
```bash
cd src/Shared/Storefront.SharedKernel
dotnet add package MediatR
```

#### Infrastructure
```bash
cd src/Infrastructure/Storefront.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

#### Identity Module
```bash
cd src/Modules/Identity/Storefront.Modules.Identity
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package FluentValidation
dotnet add package MediatR
```

#### Catalog Module
```bash
cd src/Modules/Catalog/Storefront.Modules.Catalog
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package SixLabors.ImageSharp
dotnet add package FluentValidation
dotnet add package MediatR
```

#### Content Module
```bash
cd src/Modules/Content/Storefront.Modules.Content
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package FluentValidation
dotnet add package MediatR
```

#### API Project
```bash
cd src/API/Storefront.Api
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package MediatR
dotnet add package FluentValidation.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore
```

#### Test Projects
```bash
# Architecture Tests
cd tests/Storefront.ArchitectureTests
dotnet add package NetArchTest.Rules
dotnet add package FluentAssertions

# Unit Tests
cd tests/Storefront.UnitTests
dotnet add package FluentAssertions
dotnet add package NSubstitute
dotnet add package Bogus

# Integration Tests
cd tests/Storefront.IntegrationTests
dotnet add package FluentAssertions
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Testcontainers.PostgreSql
dotnet add package NSubstitute
dotnet add package Bogus
```

### Restore All Dependencies
```bash
cd /Users/yasincoskun/Projects/Storefront
dotnet restore
```

---

## Database Commands

### Start PostgreSQL (Docker)
```bash
docker-compose up -d db
```

### Stop PostgreSQL
```bash
docker-compose down
```

### Connect to PostgreSQL
```bash
docker exec -it storefront-postgres psql -U postgres -d storefront
```

### Useful PostgreSQL Queries
```sql
-- List all schemas
SELECT schema_name FROM information_schema.schemata;

-- List tables in catalog schema
SELECT table_name FROM information_schema.tables WHERE table_schema = 'catalog';

-- Check if extensions are enabled
SELECT * FROM pg_extension;

-- View migration history for Identity
SELECT * FROM identity."__EFMigrationsHistory_Identity";

-- View migration history for Catalog
SELECT * FROM catalog."__EFMigrationsHistory_Catalog";

-- View migration history for Content
SELECT * FROM content."__EFMigrationsHistory_Content";
```

---

## Migration Commands

### Identity Module
```bash
# Add migration
dotnet ef migrations add InitialIdentity \
  -p src/Modules/Identity/Storefront.Modules.Identity/Storefront.Modules.Identity.csproj \
  -s src/API/Storefront.Api/Storefront.Api.csproj \
  -c IdentityDbContext \
  -o Infrastructure/Persistence/Migrations

# Update database
dotnet ef database update \
  -p src/Modules/Identity/Storefront.Modules.Identity/Storefront.Modules.Identity.csproj \
  -s src/API/Storefront.Api/Storefront.Api.csproj \
  -c IdentityDbContext

# Remove last migration
dotnet ef migrations remove \
  -p src/Modules/Identity/Storefront.Modules.Identity/Storefront.Modules.Identity.csproj \
  -s src/API/Storefront.Api/Storefront.Api.csproj \
  -c IdentityDbContext
```

### Catalog Module
```bash
# Add migration
dotnet ef migrations add InitialCatalog \
  -p src/Modules/Catalog/Storefront.Modules.Catalog/Storefront.Modules.Catalog.csproj \
  -s src/API/Storefront.Api/Storefront.Api.csproj \
  -c CatalogDbContext \
  -o Infrastructure/Persistence/Migrations

# Update database
dotnet ef database update \
  -p src/Modules/Catalog/Storefront.Modules.Catalog/Storefront.Modules.Catalog.csproj \
  -s src/API/Storefront.Api/Storefront.Api.csproj \
  -c CatalogDbContext

# Remove last migration
dotnet ef migrations remove \
  -p src/Modules/Catalog/Storefront.Modules.Catalog/Storefront.Modules.Catalog.csproj \
  -s src/API/Storefront.Api/Storefront.Api.csproj \
  -c CatalogDbContext
```

### Content Module
```bash
# Add migration
dotnet ef migrations add InitialContent \
  -p src/Modules/Content/Storefront.Modules.Content/Storefront.Modules.Content.csproj \
  -s src/API/Storefront.Api/Storefront.Api.csproj \
  -c ContentDbContext \
  -o Infrastructure/Persistence/Migrations

# Update database
dotnet ef database update \
  -p src/Modules/Content/Storefront.Modules.Content/Storefront.Modules.Content.csproj \
  -s src/API/Storefront.Api/Storefront.Api.csproj \
  -c ContentDbContext

# Remove last migration
dotnet ef migrations remove \
  -p src/Modules/Content/Storefront.Modules.Content/Storefront.Modules.Content.csproj \
  -s src/API/Storefront.Api/Storefront.Api.csproj \
  -c ContentDbContext
```

### Apply All Migrations at Once
```bash
# Identity
dotnet ef database update -p src/Modules/Identity/Storefront.Modules.Identity/Storefront.Modules.Identity.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c IdentityDbContext

# Catalog
dotnet ef database update -p src/Modules/Catalog/Storefront.Modules.Catalog/Storefront.Modules.Catalog.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c CatalogDbContext

# Content
dotnet ef database update -p src/Modules/Content/Storefront.Modules.Content/Storefront.Modules.Content.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c ContentDbContext
```

### Reset Database (Drop and Recreate)
```bash
# Drop database
docker-compose down -v

# Start fresh
docker-compose up -d db

# Wait for PostgreSQL to be ready
sleep 5

# Apply all migrations
dotnet ef database update -p src/Modules/Identity/Storefront.Modules.Identity/Storefront.Modules.Identity.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c IdentityDbContext
dotnet ef database update -p src/Modules/Catalog/Storefront.Modules.Catalog/Storefront.Modules.Catalog.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c CatalogDbContext
dotnet ef database update -p src/Modules/Content/Storefront.Modules.Content/Storefront.Modules.Content.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c ContentDbContext
```

---

## Development Commands

### Run Backend API
```bash
cd src/API/Storefront.Api
dotnet run
```

### Run API with Hot Reload
```bash
cd src/API/Storefront.Api
dotnet watch run
```

### Build Solution
```bash
# Build entire solution
dotnet build

# Build in Release mode
dotnet build -c Release

# Clean build
dotnet clean && dotnet build
```

### Check for Code Issues
```bash
# Format code
dotnet format

# Restore, build, and run
dotnet restore && dotnet build && dotnet run --project src/API/Storefront.Api/Storefront.Api.csproj
```

---

## Testing Commands

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
# Architecture tests
dotnet test tests/Storefront.ArchitectureTests/Storefront.ArchitectureTests.csproj

# Unit tests
dotnet test tests/Storefront.UnitTests/Storefront.UnitTests.csproj

# Integration tests
dotnet test tests/Storefront.IntegrationTests/Storefront.IntegrationTests.csproj
```

### Run Tests with Detailed Output
```bash
dotnet test --verbosity normal
```

### Run Tests with Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~ProductsControllerTests.CreateProduct_WithAuth_Should_Return_Success"
```

### Run Tests in Watch Mode (TDD)
```bash
dotnet watch test --project tests/Storefront.UnitTests/Storefront.UnitTests.csproj
```

### Generate Coverage Report
```bash
# Install ReportGenerator (one-time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator \
  -reports:"tests/**/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:Html

# Open report
open coverage-report/index.html  # macOS
xdg-open coverage-report/index.html  # Linux
start coverage-report/index.html  # Windows
```

---

## Docker Commands

### Start All Services
```bash
docker-compose up -d
```

### Start Specific Service
```bash
docker-compose up -d db
docker-compose up -d api
docker-compose up -d web
```

### Stop All Services
```bash
docker-compose down
```

### Stop and Remove Volumes
```bash
docker-compose down -v
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f db
docker-compose logs -f web
```

### Rebuild Services
```bash
# Rebuild all
docker-compose build

# Rebuild specific service
docker-compose build api

# Rebuild and start
docker-compose up -d --build
```

### Execute Commands in Containers
```bash
# Access API container
docker exec -it storefront-api bash

# Access Database
docker exec -it storefront-postgres psql -U postgres -d storefront

# Access Web container
docker exec -it storefront-web sh
```

### Clean Up Docker
```bash
# Remove all stopped containers
docker container prune

# Remove unused images
docker image prune

# Remove unused volumes
docker volume prune

# Full cleanup (CAUTION: Removes everything)
docker system prune -a --volumes
```

---

## Frontend Commands

### Initial Setup
```bash
cd web
npm install
```

### Development
```bash
# Start development server
npm run dev

# Start on specific port
npm run dev -- -p 3001

# Build for production
npm run build

# Start production server
npm start

# Lint code
npm run lint

# Type check
npm run type-check
```

### Add Dependencies
```bash
# Add production dependency
npm install <package-name>

# Add dev dependency
npm install -D <package-name>

# Example: Add shadcn/ui components
npx shadcn-ui@latest add button
npx shadcn-ui@latest add card
npx shadcn-ui@latest add input
npx shadcn-ui@latest add table
```

### Clean Install
```bash
rm -rf node_modules package-lock.json
npm install
```

---

## Useful Scripts

### Quick Start (All Services)
```bash
#!/bin/bash
# Save as: scripts/start.sh

# Start database
docker-compose up -d db

# Wait for database to be ready
sleep 5

# Apply migrations
dotnet ef database update -p src/Modules/Identity/Storefront.Modules.Identity/Storefront.Modules.Identity.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c IdentityDbContext
dotnet ef database update -p src/Modules/Catalog/Storefront.Modules.Catalog/Storefront.Modules.Catalog.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c CatalogDbContext
dotnet ef database update -p src/Modules/Content/Storefront.Modules.Content/Storefront.Modules.Content.csproj -s src/API/Storefront.Api/Storefront.Api.csproj -c ContentDbContext

# Start API
cd src/API/Storefront.Api
dotnet run &

# Start frontend
cd ../../web
npm run dev
```

### Run Tests Before Commit
```bash
#!/bin/bash
# Save as: scripts/pre-commit-test.sh

echo "Running architecture tests..."
dotnet test tests/Storefront.ArchitectureTests/Storefront.ArchitectureTests.csproj --verbosity quiet

if [ $? -ne 0 ]; then
    echo "❌ Architecture tests failed!"
    exit 1
fi

echo "Running unit tests..."
dotnet test tests/Storefront.UnitTests/Storefront.UnitTests.csproj --verbosity quiet

if [ $? -ne 0 ]; then
    echo "❌ Unit tests failed!"
    exit 1
fi

echo "✅ All tests passed!"
exit 0
```

---

## Important Notes

### Default Credentials
```
Email: admin@storefront.com
Password: AdminPassword123!
```

### API Endpoints
- **Backend API**: http://localhost:8080
- **Frontend**: http://localhost:3000
- **Swagger UI**: http://localhost:8080/swagger

### Connection Strings

#### Development (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=storefront;Username=postgres;Password=postgres"
  }
}
```

#### Docker (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db;Port=5432;Database=storefront;Username=postgres;Password=postgres"
  }
}
```

### JWT Configuration
```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-here-minimum-32-chars",
    "Issuer": "Storefront.Api",
    "Audience": "Storefront.Web",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Module Isolation Rules

1. **Modules CANNOT reference each other**
   - ❌ Catalog → Identity (Direct reference)
   - ✅ Catalog → SharedKernel (Shared abstractions)

2. **Database Isolation**
   - Each module has its own schema
   - Identity: `identity` schema
   - Catalog: `catalog` schema
   - Content: `content` schema

3. **Migration History Isolation**
   - Identity: `__EFMigrationsHistory_Identity`
   - Catalog: `__EFMigrationsHistory_Catalog`
   - Content: `__EFMigrationsHistory_Content`

### Common Issues & Solutions

#### Issue: Migration Conflicts
```bash
# Solution: Ensure you're using the correct DbContext
dotnet ef migrations add MyMigration -c IdentityDbContext -o Infrastructure/Persistence/Migrations
```

#### Issue: Port Already in Use
```bash
# Find process using port 8080
lsof -i :8080

# Kill process
kill -9 <PID>
```

#### Issue: Docker Permission Denied
```bash
# Add user to docker group
sudo usermod -aG docker $USER

# Log out and back in
```

#### Issue: PostgreSQL Connection Failed
```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# View PostgreSQL logs
docker logs storefront-postgres

# Restart PostgreSQL
docker-compose restart db
```

#### Issue: EF Core Tools Not Found
```bash
# Install globally
dotnet tool install --global dotnet-ef

# Update to latest version
dotnet tool update --global dotnet-ef
```

---

## Environment Variables

### Backend (.env or docker-compose.yml)
```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=storefront;Username=postgres;Password=postgres
Jwt__SecretKey=your-secret-key-here
Jwt__Issuer=Storefront.Api
Jwt__Audience=Storefront.Web
Jwt__ExpiryMinutes=60
```

### Frontend (.env.local)
```env
NEXT_PUBLIC_API_URL=http://localhost:8080
API_URL=http://api:8080
```

---

## Performance Tips

1. **Use compiled queries for frequently executed queries**
2. **Enable response compression in production**
3. **Use AsNoTracking() for read-only queries**
4. **Implement pagination for large datasets**
5. **Use Redis for caching (future enhancement)**
6. **Enable connection pooling for PostgreSQL**

---

## Security Checklist

- ✅ JWT tokens stored in HTTP-Only cookies
- ✅ CORS configured for specific origins
- ✅ Password hashing with Identity
- ✅ SQL injection protection via EF Core parameterization
- ✅ Input validation with FluentValidation
- ✅ Authorization policies for protected endpoints
- ⚠️ Enable HTTPS in production
- ⚠️ Implement rate limiting
- ⚠️ Add security headers (CSP, HSTS, etc.)

---

## Deployment Checklist

### Before Deploying

1. **Run all tests**
   ```bash
   dotnet test
   ```

2. **Build in Release mode**
   ```bash
   dotnet build -c Release
   ```

3. **Check for security vulnerabilities**
   ```bash
   dotnet list package --vulnerable
   ```

4. **Update appsettings.Production.json**
   - Change JWT secret
   - Update connection string
   - Enable HTTPS
   - Configure logging

5. **Build Docker images**
   ```bash
   docker-compose build
   ```

6. **Run integration tests against production-like environment**

### Production Environment

- Use managed PostgreSQL (AWS RDS, Azure Database, etc.)
- Enable SSL/TLS for database connections
- Set up automated backups
- Configure monitoring and alerting
- Use environment variables for secrets
- Enable container health checks
- Set up reverse proxy (Nginx) with SSL

---

## Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Next.js Documentation](https://nextjs.org/docs)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Docker Documentation](https://docs.docker.com/)
- [Modular Monolith Architecture](https://www.milanjovanovic.tech/blog/modular-monolith-architecture)

---

**Last Updated**: December 17, 2025  
**Version**: 1.0.0  
**Maintainer**: Storefront Development Team

