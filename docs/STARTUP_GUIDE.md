# Quick Start Guide - Running the Backend

## Prerequisites

✅ Docker Desktop must be installed and running
✅ .NET 10 SDK installed
✅ VS Code with C# Dev Kit extension

---

## Step-by-Step Startup

### 1. Start Docker Desktop

**macOS**: Open Docker Desktop app from Applications
- Wait until you see "Docker Desktop is running" (green icon in menu bar)

### 2. Start PostgreSQL Database

Open a terminal in the project root and run:

```bash
cd /Users/yasincoskun/Projects/Storefront

# Start PostgreSQL container
docker-compose up -d

# Verify it's running (should show storefront-db)
docker ps
```

**Expected Output:**
```
[+] Running 2/2
 ✔ Network storefront_default  Created
 ✔ Container storefront-db     Started
```

### 3. Start the .NET API

**Option A: Using VS Code Debugger (RECOMMENDED)**
1. Open VS Code in the project folder
2. Press `F5`
3. Select **".NET Core Launch (API)"**
4. Swagger opens at `http://localhost:8080/swagger`

**Option B: Using Terminal**
```bash
cd src/API/Storefront.Api
dotnet run
```

---

## Troubleshooting

### ❌ Error: "Could not connect to PostgreSQL"

**Symptoms:**
```
Npgsql.NpgsqlException: Connection refused
```

**Solution:**
```bash
# Check if database container is running
docker ps

# If not running, start it
docker-compose up -d

# Check logs if it's failing
docker logs storefront-db
```

---

### ❌ Error: "Port 5432 already in use"

**Solution:**
```bash
# Stop any existing PostgreSQL instance
docker-compose down

# Or find what's using port 5432
lsof -i :5432

# Kill the process if needed
kill -9 <PID>

# Start again
docker-compose up -d
```

---

### ❌ Error: "Port 8080 already in use"

**Solution:**
```bash
# Kill orphaned dotnet processes
pkill -9 dotnet

# Or find specific process
lsof -ti:8080 | xargs kill -9
```

---

### ❌ Docker Desktop Not Running

**Symptoms:**
```
Cannot connect to the Docker daemon
```

**Solution:**
1. Open Docker Desktop application
2. Wait for it to fully start (green icon)
3. Try again: `docker-compose up -d`

---

## Verify Everything is Working

### 1. Check Database
```bash
docker ps
# Should show: storefront-db with STATUS "Up X seconds (healthy)"
```

### 2. Check API
Visit: `http://localhost:8080/swagger`

You should see the Swagger UI with all API endpoints.

### 3. Test Authentication
In Swagger:
1. Click on `/api/identity/auth/login`
2. Click "Try it out"
3. Use credentials:
   ```json
   {
     "email": "admin@storefront.com",
     "password": "AdminPassword123!"
   }
   ```
4. Click "Execute"
5. Should return 200 with JWT tokens

---

## Database Connection Details

The API automatically uses these settings from `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=storefront;Username=postgres;Password=postgres"
  }
}
```

---

## Useful Commands

```bash
# Start database
docker-compose up -d

# Stop database (keeps data)
docker-compose stop

# Stop and remove containers (keeps data in volume)
docker-compose down

# Stop and DELETE ALL DATA
docker-compose down -v

# View database logs
docker logs storefront-db -f

# Access PostgreSQL CLI
docker exec -it storefront-db psql -U postgres -d storefront

# List all schemas
docker exec -it storefront-db psql -U postgres -d storefront -c "\dn"

# List tables in a schema
docker exec -it storefront-db psql -U postgres -d storefront -c "\dt identity.*"
```

---

## Complete Startup Sequence

```bash
# 1. Start Docker Desktop (GUI)

# 2. Start Database
cd /Users/yasincoskun/Projects/Storefront
docker-compose up -d

# 3. Wait for database to be healthy (10-15 seconds)
docker ps

# 4. Start API (Press F5 in VS Code)
# OR
cd src/API/Storefront.Api
dotnet run

# 5. Open Swagger
# http://localhost:8080/swagger (should open automatically)
```

---

## What Happens on First Run?

The API will automatically:
1. ✅ Create the `storefront` database (if not exists)
2. ✅ Create schemas: `identity`, `catalog`, `content`
3. ✅ Create all tables
4. ✅ Seed default admin user
5. ✅ Enable PostgreSQL extensions (`uuid-ossp`, `pg_trgm`)

**Console Output:**
```
✅ Identity database created successfully
✅ Catalog database created successfully
✅ Content database created successfully
🌱 Seeding identity data...
✅ Role 'Admin' created successfully
✅ Default admin user created successfully
```

---

## Common Runtime Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| `Npgsql.NpgsqlException: Connection refused` | Database not running | `docker-compose up -d` |
| `EADDRINUSE: Port 8080 already in use` | Previous dotnet still running | `pkill -9 dotnet` |
| `Cannot connect to Docker daemon` | Docker Desktop not running | Start Docker Desktop |
| `role "identity.Roles" does not exist` | Database not initialized | Restart API (F5) |

---

## Next Steps After Successful Startup

1. **Login** in Swagger with admin credentials
2. **Copy the access token** from the response
3. **Click "Authorize"** button in Swagger (top right)
4. **Paste token** in format: `Bearer YOUR_TOKEN_HERE`
5. **Test all endpoints** (Products, Categories, Blog, etc.)
6. **Make a list** of missing features
7. **We'll implement** them together!

---

**Need help?** Share the exact error message you're seeing and I'll help you fix it! 🚀
