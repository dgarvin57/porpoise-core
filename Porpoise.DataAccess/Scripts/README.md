# Database Scripts

## Current Schema

### `00_RebuildDatabase.sql` - **The ONLY Script You Need**

This is the **complete, authoritative database schema** that creates everything from scratch. All previous migrations (01-09) have been incorporated into this single file.

**What it includes:**
- All tables with proper schema (Tenants, Projects, Surveys, Questions, Responses, etc.)
- GUID-based TenantId (VARCHAR(36))
- Organization branding (OrganizationName, OrganizationLogo, OrganizationTagline)
- Client branding per project (ClientLogo)
- Proper foreign key relationships with CASCADE DELETE
- Indexes for performance
- Sample data for testing (5 projects, 8 surveys)

**When to use:**
- Setting up a new development environment
- Resetting your local database
- Creating test databases
- Initial deployment to a new environment

**How to use:**
```bash
mysql -u root -p < 00_RebuildDatabase.sql
```

## Archived Migrations

All migration scripts have been moved to `archive/` folder. They are kept for **historical reference only** and should **not be run** since `00_RebuildDatabase.sql` includes all their changes.

### Why archived?

The rebuild script is the single source of truth. Running old migrations could:
- Create inconsistencies
- Miss dependencies between migrations
- Cause errors with updated schema

## Creating New Migrations

When you need to modify the schema going forward:

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
