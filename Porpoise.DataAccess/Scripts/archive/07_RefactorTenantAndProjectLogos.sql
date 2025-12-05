-- Migration: Refactor Tenant and Project tables for proper logo separation
-- Date: 2025-12-04
-- Description: 
--   1. Enhance Tenants table with organization info (name, logo, tagline, audit fields)
--   2. Rename ResearcherLogo to ClientLogo in Projects table
--   3. Remove ResearcherLabel, ResearcherSubLabel from Projects table
--   4. Remove BrandingSettings from Projects table

-- Step 1: Enhance Tenants table
ALTER TABLE Tenants 
    ADD COLUMN OrganizationName VARCHAR(255) NULL AFTER TenantId,
    ADD COLUMN OrganizationLogo LONGBLOB NULL AFTER OrganizationName,
    ADD COLUMN OrganizationTagline VARCHAR(500) NULL AFTER OrganizationLogo,
    ADD COLUMN CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) AFTER OrganizationTagline,
    ADD COLUMN ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) AFTER CreatedDate,
    ADD COLUMN CreatedBy VARCHAR(100) NULL AFTER ModifiedDate,
    ADD COLUMN ModifiedBy VARCHAR(100) NULL AFTER CreatedBy;

-- Step 2: Update Projects table - rename and remove columns
ALTER TABLE Projects
    CHANGE COLUMN ResearcherLogo ClientLogo LONGBLOB NULL,
    DROP COLUMN ResearcherLabel,
    DROP COLUMN ResearcherSubLabel,
    DROP COLUMN BrandingSettings;

-- Step 3: Add index on OrganizationName for search
ALTER TABLE Tenants
    ADD INDEX idx_organization_name (OrganizationName);
