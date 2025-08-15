-- Pet Love Community Database Initialization Script
-- This script is executed when the PostgreSQL container starts

-- Create the main database if it doesn't exist
SELECT 'CREATE DATABASE petlove_dev'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'petlove_dev')\gexec

-- Connect to the database
\c petlove_dev;

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Create indexes for full-text search
-- (These will be managed by EF Core migrations, this is just for reference)

-- Set timezone
SET timezone = 'UTC';

-- Create a simple health check function
CREATE OR REPLACE FUNCTION database_health_check()
RETURNS TABLE(status text, timestamp timestamptz) AS $$
BEGIN
    RETURN QUERY SELECT 'healthy'::text, now();
END;
$$ LANGUAGE plpgsql;

-- Grant permissions to the application user
-- (User creation is handled by environment variables in docker-compose)

COMMENT ON DATABASE petlove_dev IS 'Pet Love Community main database for development';

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'Pet Love Community database initialized successfully at %', now();
END $$;