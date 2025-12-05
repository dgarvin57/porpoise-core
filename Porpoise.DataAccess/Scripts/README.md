# Database Scripts

## Current Schema

### `00_RebuildDatabase.sql` - **Use This for Fresh Installations**

This is the **authoritative database schema** that creates a complete, up-to-date database from scratch. It includes:
- All tables with proper schema (Tenants, Projects, Surveys, Questions, Responses, etc.)
- GUID-based TenantId (VARCHAR(36))
- Proper foreign key relationships
- Indexes for performance
- Sample data for testing

**When to use:**
- Setting up a new development environment
- Resetting your local database
- Creating test databases
- Initial deployment to a new environment

**How to use:**
```bash
mysql -u root -p < 00_RebuildDatabase.sql
```

## Active Migrations

These migrations should be applied **in order** to an existing database:

### `07_RefactorTenantAndProjectLogos.sql`
- Adds organization branding to Tenants table (OrganizationName, OrganizationLogo, OrganizationTagline)
- Renames ResearcherLogo → ClientLogo in Projects
- Removes ResearcherLabel, ResearcherSubLabel, BrandingSettings from Projects
- **Status:** Applied to current schema

### `08_ConvertTenantIdToGuid.sql`
- Converts TenantId from INT to GUID (VARCHAR(36))
- Updates all foreign key relationships
- **Status:** Superseded by 00_RebuildDatabase.sql

### `08b_FinishTenantIdMigration.sql`
- Cleanup script for TenantId migration
- **Status:** Superseded by 00_RebuildDatabase.sql

### `08c_CompleteTenantMigration.sql`
- Final steps for TenantId migration
- **Status:** Superseded by 00_RebuildDatabase.sql

## Archived Migrations

Old migration scripts have been moved to `archive/` folder. These are kept for historical reference but are **not needed** for new installations since `00_RebuildDatabase.sql` includes all their changes.

Archived scripts:
- `01_CreateDatabase.sql` - Initial database creation
- `02_RecreateTables.sql` - Early table definitions
- `03_AddMultiTenancy.sql` - Added tenant support
- `04_CleanupProjects.sql` - Project table cleanup
- `04_NormalizeQuestionBlocks.sql` - Question block normalization
- `04b_FixBlockLinks.sql` - Block relationship fixes
- `05_TestData.sql` - Early test data
- `05_UpdateResearcherLogo.sql` - Logo field updates
- `06_*` - Various data and schema updates
- `07_ResetAndLoadCleanTestData.sql` - Test data reset
- `08_ExpandQuestionFields.sql` - Question field additions
- `09_RemoveCalculatedFields.sql` - Field cleanup

## Migration Strategy Going Forward

### For New Environments
1. Run `00_RebuildDatabase.sql`
2. Done! You have the latest schema with sample data

### For Existing Production Databases
1. Create new migration scripts numbered sequentially (10, 11, 12, etc.)
2. Test migrations on a copy of production data
3. Apply migrations in order
4. Update `00_RebuildDatabase.sql` to include the changes for future fresh installs

### Creating New Migrations

When you need to modify the schema:

1. **Create a new numbered script** (e.g., `10_AddNewFeature.sql`)
2. **Write forward migration** (ALTER TABLE, ADD COLUMN, etc.)
3. **Update `00_RebuildDatabase.sql`** to include the changes
4. **Test both paths:**
   - Fresh install with 00_RebuildDatabase.sql
   - Incremental migration from previous version

Example new migration:
```sql
-- Migration 10: Add Survey Templates
-- Date: 2025-12-XX
-- Description: Add template support for surveys

USE porpoise_dev;

ALTER TABLE Surveys 
    ADD COLUMN IsTemplate TINYINT(1) DEFAULT 0 AFTER SurveyNotes,
    ADD COLUMN TemplateCategory VARCHAR(100) NULL AFTER IsTemplate;

-- Add index for template lookups
ALTER TABLE Surveys ADD INDEX idx_surveys_template (IsTemplate, TemplateCategory);
```

## Database Connection

Default development database:
- **Host:** localhost:3306
- **Database:** porpoise_dev
- **User:** root
- **Charset:** utf8mb4
- **Collation:** utf8mb4_unicode_ci

## Important Notes

- ⚠️ **`00_RebuildDatabase.sql` drops and recreates the entire database** - use with caution!
- All GUIDs are stored as VARCHAR(36) strings in MySQL
- TenantId uses the default tenant key 'demo' for development
- Sample data includes 5 projects and 8 surveys for testing
- Foreign keys use CASCADE DELETE for proper cleanup

## Troubleshooting

**Error: "Table already exists"**
- Solution: Run `DROP DATABASE porpoise_dev;` first, or use `00_RebuildDatabase.sql` which does this

**Error: "Foreign key constraint fails"**
- Solution: Ensure migrations run in order and all referenced tables exist

**Error: "GUID format incorrect"**
- Solution: GUIDs must be 36 characters (e.g., '550e8400-e29b-41d4-a716-446655440000')

## Version History

- **v1.0** - Initial multi-tenant architecture with INT TenantId
- **v2.0** - Migrated to GUID-based TenantId (current)
- **v2.1** - Separated Project/Tenant branding (ClientLogo vs OrganizationLogo)
