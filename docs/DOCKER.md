# Docker Guide

Complete guide to running Storefront with Docker.

---

## Quick Start

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes (deletes data)
docker-compose down -v
```

---

## Services

### Development (`docker-compose.yml`)

```yaml
services:
  db:         PostgreSQL 16 database
  pgadmin:    Database admin UI (optional)
```

**Backend and frontend run natively** for hot-reload during development.

### Production (`docker-compose.prod.yml`)

```yaml
services:
  db:         PostgreSQL 16 database
  api:        .NET 10 API (Alpine)
  web:        Next.js 15 admin panel
  nginx:      Reverse proxy + static files
```

---

## Development Setup

### docker-compose.yml

```yaml
version: '3.8'

services:
  db:
    image: postgres:16-alpine
    container_name: storefront-db
    environment:
      POSTGRES_DB: storefront
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
```

### Start Development

```bash
# 1. Start database
docker-compose up -d

# 2. Run API natively (hot reload)
cd src/API/Storefront.Api
dotnet watch run

# 3. Run frontend natively (hot reload)
cd web
npm run dev
```

**Benefits:**
- ✅ Fast rebuilds (no Docker layer caching)
- ✅ Hot reload for code changes
- ✅ Direct debugging
- ✅ Native IDE integration

---

## Production Setup

### docker-compose.prod.yml

Full production stack with optimized builds.

### Build Production Images

```bash
# Build all services
docker-compose -f docker-compose.prod.yml build

# Build specific service
docker-compose -f docker-compose.prod.yml build api
docker-compose -f docker-compose.prod.yml build web
```

### Start Production

```bash
# Start all services
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Check status
docker-compose -f docker-compose.prod.yml ps
```

**Access:**
- Web: `http://localhost` (Nginx routes)
- API: `http://localhost/api` (proxied through Nginx)
- Direct API: `http://localhost:8080` (if exposed)

---

## Multi-Stage Dockerfile (API)

### Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Storefront.Api.dll"]
```

**Benefits:**
- ✅ 70% smaller images (Alpine Linux)
- ✅ No SDK in production
- ✅ Faster startup
- ✅ Better security

**Image Sizes:**
- SDK image: ~900MB
- Runtime image: ~220MB

---

## Multi-Stage Dockerfile (Next.js)

### Dockerfile

```dockerfile
# Dependencies stage
FROM node:20-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci

# Build stage
FROM node:20-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build

# Runtime stage
FROM node:20-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/public ./public
EXPOSE 3000
CMD ["node", "server.js"]
```

**Benefits:**
- ✅ Standalone mode (smaller)
- ✅ No `node_modules` in production
- ✅ Optimized for deployment

---

## Nginx Configuration

### nginx.conf

```nginx
server {
    listen 80;
    server_name localhost;

    # Frontend
    location / {
        proxy_pass http://web:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    # Backend API
    location /api/ {
        proxy_pass http://api:8080/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    # Static files (images)
    location /uploads/ {
        alias /app/uploads/;
        expires 7d;
        add_header Cache-Control "public, immutable";
    }

    # Gzip compression
    gzip on;
    gzip_types text/plain application/json;
}
```

---

## Docker Commands

### Container Management

```bash
# List running containers
docker ps

# List all containers
docker ps -a

# Stop container
docker stop storefront-db

# Start container
docker start storefront-db

# Restart container
docker restart storefront-db

# Remove container
docker rm storefront-db

# View logs
docker logs storefront-db
docker logs -f storefront-db  # Follow logs
docker logs --tail 100 storefront-db  # Last 100 lines
```

### Image Management

```bash
# List images
docker images

# Remove image
docker rmi postgres:16-alpine

# Remove unused images
docker image prune

# Remove all unused data
docker system prune -a
```

### Volume Management

```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect storefront_postgres_data

# Remove volume (deletes data!)
docker volume rm storefront_postgres_data

# Remove all unused volumes
docker volume prune
```

---

## Database Access

### Using Docker Exec

```bash
# Connect to database
docker exec -it storefront-db psql -U postgres -d storefront

# List schemas
docker exec -it storefront-db psql -U postgres -d storefront -c "\dn"

# List tables in catalog schema
docker exec -it storefront-db psql -U postgres -d storefront -c "\dt catalog.*"

# Run SQL query
docker exec -it storefront-db psql -U postgres -d storefront -c "SELECT * FROM catalog.\"Products\" LIMIT 10;"

# Dump database
docker exec storefront-db pg_dump -U postgres storefront > backup.sql

# Restore database
docker exec -i storefront-db psql -U postgres storefront < backup.sql
```

### Using pgAdmin

Add pgAdmin service to `docker-compose.yml`:

```yaml
pgadmin:
  image: dpage/pgadmin4
  container_name: storefront-pgadmin
  environment:
    PGADMIN_DEFAULT_EMAIL: admin@storefront.com
    PGADMIN_DEFAULT_PASSWORD: admin
  ports:
    - "5050:80"
  depends_on:
    - db
```

**Access:** `http://localhost:5050`

**Connect to database:**
- Host: `db` (Docker service name)
- Port: `5432`
- Database: `storefront`
- Username: `postgres`
- Password: `postgres`

---

## Environment Variables

### .env File

Create `.env` file in project root:

```bash
# Database
POSTGRES_DB=storefront
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change-this-in-production

# API
JWT_SECRET=your-super-secret-key-at-least-32-characters
ASPNETCORE_ENVIRONMENT=Production

# Next.js
NEXT_PUBLIC_API_URL=http://localhost:8080
NODE_ENV=production
```

### Load in Docker Compose

```yaml
services:
  api:
    env_file:
      - .env
    environment:
      ConnectionStrings__DefaultConnection: Host=db;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
```

---

## Health Checks

### Database

```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U postgres"]
  interval: 10s
  timeout: 5s
  retries: 5
  start_period: 10s
```

### API

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

### Check Status

```bash
# View health status
docker ps

# Inspect health
docker inspect storefront-db | grep -A 10 Health
```

---

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker logs storefront-db

# Inspect container
docker inspect storefront-db

# Remove and recreate
docker-compose down
docker-compose up -d
```

### Port Already in Use

```bash
# Find process using port 5432
lsof -i :5432

# Kill process
kill -9 <PID>

# Or change port in docker-compose.yml
ports:
  - "5433:5432"  # Host:Container
```

### Database Connection Failed

```bash
# Check database is running
docker ps | grep storefront-db

# Check health
docker inspect storefront-db | grep Health

# Test connection
docker exec storefront-db pg_isready -U postgres

# View database logs
docker logs storefront-db
```

### Out of Disk Space

```bash
# Check Docker disk usage
docker system df

# Remove unused images
docker image prune -a

# Remove unused volumes
docker volume prune

# Remove everything
docker system prune -a --volumes
```

---

## Performance Tuning

### PostgreSQL

Edit `docker-compose.yml`:

```yaml
db:
  command:
    - "postgres"
    - "-c"
    - "shared_buffers=256MB"
    - "-c"
    - "effective_cache_size=1GB"
    - "-c"
    - "max_connections=100"
```

### Resource Limits

```yaml
api:
  deploy:
    resources:
      limits:
        cpus: '2'
        memory: 2G
      reservations:
        cpus: '0.5'
        memory: 512M
```

---

## Backup & Restore

### Manual Backup

```bash
# Backup database
docker exec storefront-db pg_dump -U postgres storefront > backup_$(date +%Y%m%d).sql

# Backup with compression
docker exec storefront-db pg_dump -U postgres storefront | gzip > backup_$(date +%Y%m%d).sql.gz
```

### Automated Backups

Add to `docker-compose.yml`:

```yaml
backup:
  image: prodrigestivill/postgres-backup-local
  environment:
    POSTGRES_HOST: db
    POSTGRES_DB: storefront
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: postgres
    SCHEDULE: "@daily"
    BACKUP_KEEP_DAYS: 7
  volumes:
    - ./backups:/backups
  depends_on:
    - db
```

### Restore

```bash
# Restore from backup
docker exec -i storefront-db psql -U postgres storefront < backup_20260208.sql

# Restore from compressed backup
gunzip -c backup_20260208.sql.gz | docker exec -i storefront-db psql -U postgres storefront
```

---

## CI/CD Integration

### GitHub Actions

`.github/workflows/docker.yml`:

```yaml
name: Docker Build

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Docker images
        run: docker-compose -f docker-compose.prod.yml build
      
      - name: Run tests
        run: docker-compose -f docker-compose.prod.yml run --rm api dotnet test
```

---

## Production Deployment

### Option 1: Docker Compose on VPS

```bash
# 1. Copy files to server
scp -r . user@server:/opt/storefront

# 2. SSH to server
ssh user@server

# 3. Start services
cd /opt/storefront
docker-compose -f docker-compose.prod.yml up -d

# 4. Set up Nginx reverse proxy (host level)
# 5. Configure SSL (Let's Encrypt)
```

### Option 2: Docker Swarm

```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.prod.yml storefront

# View services
docker stack services storefront

# Scale service
docker service scale storefront_api=3
```

### Option 3: Kubernetes

Convert with Kompose:

```bash
kompose convert -f docker-compose.prod.yml
kubectl apply -f .
```

---

## Best Practices

### Security

- ✅ Don't expose database ports in production
- ✅ Use secrets for sensitive data
- ✅ Run containers as non-root user
- ✅ Use specific image versions (not `latest`)
- ✅ Scan images for vulnerabilities

### Optimization

- ✅ Use Alpine base images
- ✅ Multi-stage builds
- ✅ Minimize layers
- ✅ Leverage build cache
- ✅ Use `.dockerignore`

### Monitoring

- ✅ Health checks on all services
- ✅ Container logs to centralized logging
- ✅ Resource usage monitoring
- ✅ Automated backups

---

## Further Reading

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
- [Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Multi-Stage Builds](https://docs.docker.com/build/building/multi-stage/)

---

**Next:** [Deployment Guide](DEPLOYMENT.md) | [Startup Guide](STARTUP_GUIDE.md)
