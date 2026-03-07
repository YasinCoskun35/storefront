# Database Reset Script (PowerShell)
# This script drops and recreates the PostgreSQL database

Write-Host "🔄 Resetting Storefront Database..." -ForegroundColor Cyan

# Stop the API if running
Write-Host "Stopping any running API instances..." -ForegroundColor Yellow
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*" -and $_.CommandLine -like "*Storefront.Api*"} | Stop-Process -Force -ErrorAction SilentlyContinue

# Drop and recreate database using Docker
Write-Host "Dropping existing database..." -ForegroundColor Yellow
docker exec storefront-db psql -U postgres -c "DROP DATABASE IF EXISTS storefront;"

Write-Host "Creating fresh database..." -ForegroundColor Yellow
docker exec storefront-db psql -U postgres -c "CREATE DATABASE storefront;"

Write-Host "Enabling extensions..." -ForegroundColor Yellow
docker exec storefront-db psql -U postgres -d storefront -c "CREATE EXTENSION IF NOT EXISTS \`"uuid-ossp\`";"
docker exec storefront-db psql -U postgres -d storefront -c "CREATE EXTENSION IF NOT EXISTS pg_trgm;"

Write-Host "✅ Database reset complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. cd src/API/Storefront.Api"
Write-Host "2. dotnet run"
Write-Host ""
Write-Host "The application will automatically create all schemas and tables on startup."
