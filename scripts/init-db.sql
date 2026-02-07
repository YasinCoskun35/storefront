-- Initialize PostgreSQL extensions required for Storefront

-- Enable UUID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable trigram similarity search for fuzzy matching
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create schemas for modules
CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS content;

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'Storefront database initialized with extensions: uuid-ossp, pg_trgm';
    RAISE NOTICE 'Schemas created: identity, catalog, content';
END $$;

