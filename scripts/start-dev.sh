#!/bin/bash

# Storefront Development Startup Script

echo "🚀 Starting Storefront Development Environment..."
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop and try again."
    exit 1
fi

# Start PostgreSQL
echo "📦 Starting PostgreSQL..."
docker-compose up -d

# Wait for PostgreSQL to be ready
echo "⏳ Waiting for PostgreSQL to be ready..."
sleep 3

# Check if database is ready
until docker exec storefront-db pg_isready -U postgres > /dev/null 2>&1; do
    echo "⏳ Waiting for database..."
    sleep 2
done

echo "✅ PostgreSQL is ready!"
echo ""

# Kill any existing API processes
echo "🔄 Checking for existing API processes..."
pkill -9 -f "dotnet.*Storefront.Api" 2>/dev/null || true
lsof -ti:8080 | xargs kill -9 2>/dev/null || true
lsof -ti:7080 | xargs kill -9 2>/dev/null || true

echo ""
echo "✅ Environment ready!"
echo ""
echo "📋 Next steps:"
echo ""
echo "Terminal 1 (API):"
echo "  cd src/API/Storefront.Api"
echo "  dotnet run"
echo ""
echo "Terminal 2 (Frontend):"
echo "  cd web"
echo "  npm run dev"
echo ""
echo "Access:"
echo "  🌐 Frontend: http://localhost:3000"
echo "  🔧 API: http://localhost:8080"
echo "  📖 Swagger: https://localhost:7080/swagger"
echo ""
echo "Default Credentials:"
echo "  Admin: admin@storefront.com / Admin123!@#"
echo ""
