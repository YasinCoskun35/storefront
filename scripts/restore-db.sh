#!/bin/bash

# Database restore script for Storefront
# Usage: ./scripts/restore-db.sh <backup_file.sql.gz> [container_name]

set -e

if [ -z "$1" ]; then
    echo "❌ Error: Backup file not specified"
    echo "Usage: ./scripts/restore-db.sh <backup_file.sql.gz> [container_name]"
    exit 1
fi

BACKUP_FILE=$1
CONTAINER_NAME=${2:-storefront-db-prod}

if [ ! -f "$BACKUP_FILE" ]; then
    echo "❌ Error: Backup file not found: $BACKUP_FILE"
    exit 1
fi

echo "🗄️  Starting database restore..."
echo "Container: $CONTAINER_NAME"
echo "Backup file: $BACKUP_FILE"

# Decompress if needed
if [[ "$BACKUP_FILE" == *.gz ]]; then
    echo "📦 Decompressing backup..."
    TEMP_FILE=$(mktemp)
    gunzip -c "$BACKUP_FILE" > "$TEMP_FILE"
    SQL_FILE="$TEMP_FILE"
else
    SQL_FILE="$BACKUP_FILE"
fi

# Warning
echo "⚠️  WARNING: This will overwrite the current database!"
read -p "Continue? (y/N) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Restore cancelled."
    [ -n "$TEMP_FILE" ] && rm "$TEMP_FILE"
    exit 0
fi

# Drop existing database and recreate
echo "🔄 Recreating database..."
docker exec -t "$CONTAINER_NAME" psql -U postgres -c "DROP DATABASE IF EXISTS storefront;"
docker exec -t "$CONTAINER_NAME" psql -U postgres -c "CREATE DATABASE storefront;"

# Restore from backup
echo "📥 Restoring backup..."
docker exec -i "$CONTAINER_NAME" psql -U postgres -d storefront < "$SQL_FILE"

# Cleanup temp file
[ -n "$TEMP_FILE" ] && rm "$TEMP_FILE"

echo "✅ Database restored successfully!"

