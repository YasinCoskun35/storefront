# Installation Guide

## Prerequisites

### Required Software
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 20+** - [Download](https://nodejs.org/) (only for admin panel)
- **Visual Studio Code** - [Download](https://code.visualstudio.com/) (recommended)

### Recommended VS Code Extensions
- C# Dev Kit
- Docker
- ESLint (for frontend)
- Tailwind CSS IntelliSense

---

## Installation Steps

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd Storefront
```

### 2. Start PostgreSQL Database

```bash
# Start database container
docker-compose up -d

# Verify it's running
docker ps

# Expected output:
# CONTAINER: storefront-db
# STATUS: Up X seconds (healthy)
```

### 3. Start Backend API

#### Option A: Visual Studio Code (Recommended)

1. Open the project in VS Code
2. Press `F5`
3. Select **".NET Core Launch (API)"**
4. Swagger opens automatically at `http://localhost:8080/swagger`

#### Option B: Terminal

```bash
cd src/API/Storefront.Api
dotnet restore
dotnet run

# API runs at: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
```

**First Run:** Database tables will be created automatically. You'll see:

```
🔄 Attempting database initialization (attempt 1/5)...
✅ Identity schema initialized
✅ Catalog schema initialized (5 tables)
✅ Content schema initialized (2 tables)
🌱 Seeding identity data...
✅ Default admin user created successfully
```

### 4. Test the API

1. Open Swagger UI: `http://localhost:8080/swagger`
2. Click **POST /api/identity/auth/login**
3. Login with:
   ```json
   {
     "email": "admin@storefront.com",
     "password": "AdminPassword123!"
   }
   ```
4. Copy the `accessToken` from response
5. Click **Authorize** button (top-right)
6. Paste token in format: `Bearer <your-token>`
7. Now you can test all endpoints!

### 5. Start Admin Panel (Optional)

```bash
cd web
npm install
npm run dev

# Admin panel: http://localhost:3000
```

**Login:** Same credentials as API

---

## Verification

### Check Database Tables

```bash
# List all schemas
docker exec storefront-db psql -U postgres -d storefront -c "\dn"

# Should show: identity, catalog, content

# List catalog tables
docker exec storefront-db psql -U postgres -d storefront -c "\dt catalog.*"

# Should show: Categories, Brands, Products, ProductImages, ProductBundleItems
```

### Check API Health

```bash
# API health check
curl http://localhost:8080/health

# Expected: HTTP 200 OK
```

### Check API Endpoints

Open: `http://localhost:8080/swagger`

You should see endpoints for:
- Authentication (`/api/identity/auth`)
- Products (`/api/catalog/products`)
- Categories (`/api/catalog/categories`)
- Blog (`/api/content/blog`)
- Pages (`/api/content/pages`)

---

## Troubleshooting

### Database Connection Failed

**Error:** `Connection refused` or `transient failure`

**Solution:**
```bash
# Check if Docker is running
docker ps

# If no containers, start database
docker-compose up -d

# Wait 5 seconds for database to initialize
sleep 5

# Try starting API again
```

### Port Already in Use

**Error:** `EADDRINUSE` on port 8080 or 5432

**Solution:**
```bash
# For API (port 8080)
pkill -9 dotnet

# For Database (port 5432)
docker-compose down
docker-compose up -d
```

### Build Errors

**Error:** Package version mismatches

**Solution:**
```bash
# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

### Database Tables Not Created

**Error:** `relation "catalog.Products" does not exist`

**Solution:**
The API automatically creates tables on startup. If this fails:

```bash
# Stop API
# Restart database
docker-compose restart

# Wait 10 seconds
sleep 10

# Start API again (Press F5)
```

---

## Configuration

### Database Connection

`src/API/Storefront.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=storefront;Username=postgres;Password=postgres"
  }
}
```

### JWT Settings

```json
{
  "Jwt": {
    "Secret": "YourSecretKeyHere-AtLeast32Characters",
    "Issuer": "Storefront.Api",
    "Audience": "Storefront.Web"
  }
}
```

### Catalog Settings (B2B Mode)

```json
{
  "CatalogSettings": {
    "PricingEnabled": false,
    "RequirePriceForProducts": false,
    "ShowPriceLabel": "Contact for Quote",
    "AllowPriceInquiry": true
  }
}
```

---

## Next Steps

✅ Installation complete!

Continue with:
- [Quick Reference](QUICK_REFERENCE.md) - Common commands
- [API Reference](API_REFERENCE.md) - Endpoint documentation
- [Architecture Guide](ARCHITECTURE.md) - System design
- [Testing Guide](TESTING.md) - Running tests

---

## Clean Uninstall

```bash
# Stop all containers
docker-compose down

# Remove volumes (deletes all data)
docker-compose down -v

# Remove Docker images
docker rmi postgres:16-alpine

# Delete project folder
cd ..
rm -rf Storefront
```

---

**Need help?** Check [Troubleshooting Guide](troubleshooting/COMMON_ISSUES.md) or open an issue on GitHub.
