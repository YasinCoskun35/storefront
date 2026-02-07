#!/bin/bash

# Database backup script for Storefront
# Usage: ./scripts/backup-db.sh [container_name]

set -e

CONTAINER_NAME=${1:-storefront-db-prod}
BACKUP_DIR="./backups"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="${BACKUP_DIR}/storefront_backup_${TIMESTAMP}.sql"

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

echo "🗄️  Starting database backup..."
echo "Container: $CONTAINER_NAME"
echo "Backup file: $BACKUP_FILE"

# Run pg_dump inside the container
docker exec -t "$CONTAINER_NAME" pg_dump -U postgres -d storefront > "$BACKUP_FILE"

# Compress the backup
gzip "$BACKUP_FILE"
COMPRESSED_FILE="${BACKUP_FILE}.gz"

echo "✅ Backup completed successfully!"
echo "📦 Compressed backup: $COMPRESSED_FILE"
echo "📊 Backup size: $(du -h "$COMPRESSED_FILE" | cut -f1)"

# Keep only last 7 backups
echo "🧹 Cleaning old backups (keeping last 7)..."
ls -t "$BACKUP_DIR"/storefront_backup_*.sql.gz | tail -n +8 | xargs -r rm

echo "✨ Backup complete!"

