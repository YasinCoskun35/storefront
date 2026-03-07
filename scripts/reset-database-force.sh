#!/bin/bash

# Database Reset Script - Force version
# This script terminates all connections and resets the database

echo "🔄 Resetting Storefront Database (Force Mode)..."

# Stop the API if running
echo "Stopping any running API instances..."
pkill -f "dotnet.*Storefront.Api" 2>/dev/null || true
sleep 2

# Terminate all connections to the database
echo "Terminating all database connections..."
docker exec storefront-db psql -U postgres -c "
SELECT pg_terminate_backend(pg_stat_activity.pid)
FROM pg_stat_activity
WHERE pg_stat_activity.datname = 'storefront'
  AND pid <> pg_backend_pid();"

# Drop and recreate database
echo "Dropping existing database..."
docker exec storefront-db psql -U postgres -c "DROP DATABASE IF EXISTS storefront;"

echo "Creating fresh database..."
docker exec storefront-db psql -U postgres -c "CREATE DATABASE storefront;"

echo "Enabling extensions..."
docker exec storefront-db psql -U postgres -d storefront -c "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";"
docker exec storefront-db psql -U postgres -d storefront -c "CREATE EXTENSION IF NOT EXISTS pg_trgm;"

echo "✅ Database reset complete!"
echo ""
echo "All schemas (identity, catalog, content, orders) will be created automatically when you run the API."
echo ""
echo "Next steps:"
echo "1. cd src/API/Storefront.Api"
echo "2. dotnet run"
echo ""
