#!/bin/bash

# Database Reset Script
# This script drops and recreates the PostgreSQL database

echo "🔄 Resetting Storefront Database..."

# Stop the API if running
echo "Stopping any running API instances..."
pkill -f "dotnet.*Storefront.Api" 2>/dev/null || true

# Drop and recreate database using Docker
echo "Dropping existing database..."
docker exec storefront-db psql -U postgres -c "DROP DATABASE IF EXISTS storefront;"

echo "Creating fresh database..."
docker exec storefront-db psql -U postgres -c "CREATE DATABASE storefront;"

echo "Enabling extensions..."
docker exec storefront-db psql -U postgres -d storefront -c "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";"
docker exec storefront-db psql -U postgres -d storefront -c "CREATE EXTENSION IF NOT EXISTS pg_trgm;"

echo "✅ Database reset complete!"
echo ""
echo "Next steps:"
echo "1. cd src/API/Storefront.Api"
echo "2. dotnet run"
echo ""
echo "The application will automatically create all schemas and tables on startup."
