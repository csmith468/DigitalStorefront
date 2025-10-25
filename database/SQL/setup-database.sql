-- Master Database Setup Script
-- Run this in SQLCMD mode in SSMS or via sqlcmd CLI
-- In SSMS: Query Menu > SQLCMD Mode (or Ctrl+Shift+M)
-- CI/CD: `sqlcmd -S your-server.database.windows.net -U admin -P password -i setup-database.sql`

PRINT 'Starting database setup...';
GO

PRINT 'Step 1: Creating dsf schema tables (user, auth)...';
:r scripts/01-dsf-table-schema.sql
GO

PRINT 'Step 2: Creating database and dbo schema tables (products, categories, etc)...';
:r scripts/02-dbo-table-schema.sql
GO

PRINT 'Step 3: Seeding initial data...';
:r scripts/03-dbo-seed-data.sql
GO

PRINT 'Step 4: Creating indexes...';
:r scripts/04-indexes.sql
GO

PRINT 'Database setup complete!';
GO
