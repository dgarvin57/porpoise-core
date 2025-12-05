-- Migration 08: Convert TenantId from INT to GUID (CHAR(36))
-- This migration converts TenantId to use GUID/UUID format across all tables

USE porpoise_dev;

-- Step 1: Ensure TenantGuid columns exist
SET @exist := (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'Tenants' AND column_name = 'TenantGuid' AND table_schema = DATABASE());
SET @sqlstmt := IF(@exist = 0, 'ALTER TABLE Tenants ADD COLUMN TenantGuid CHAR(36) AFTER TenantId', 'SELECT "Column already exists"');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;

-- Step 2: Generate GUIDs for existing tenants
UPDATE Tenants SET TenantGuid = UUID() WHERE TenantGuid IS NULL OR TenantGuid = '';

-- Step 3: Add TenantGuid to Projects if needed
SET @exist := (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'Projects' AND column_name = 'TenantGuid' AND table_schema = DATABASE());
SET @sqlstmt := IF(@exist = 0, 'ALTER TABLE Projects ADD COLUMN TenantGuid CHAR(36) AFTER TenantId', 'SELECT "Column already exists"');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;

-- Step 4: Add TenantGuid to Surveys if needed
SET @exist := (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'Surveys' AND column_name = 'TenantGuid' AND table_schema = DATABASE());
SET @sqlstmt := IF(@exist = 0, 'ALTER TABLE Surveys ADD COLUMN TenantGuid CHAR(36) AFTER TenantId', 'SELECT "Column already exists"');
PREPARE stmt FROM @sqlstmt;
EXECUTE stmt;

-- Step 4: Copy GUID values to dependent tables using JOIN
UPDATE Projects p 
INNER JOIN Tenants t ON p.TenantId = t.TenantId 
SET p.TenantGuid = t.TenantGuid
WHERE p.TenantGuid IS NULL OR p.TenantGuid = '';

UPDATE Surveys s 
INNER JOIN Tenants t ON s.TenantId = t.TenantId 
SET s.TenantGuid = t.TenantGuid
WHERE s.TenantGuid IS NULL OR s.TenantGuid = '';

-- Step 5: Drop old foreign key constraints (ignore errors if not exists)
SET FOREIGN_KEY_CHECKS = 0;
ALTER TABLE Projects DROP FOREIGN KEY projects_ibfk_1;
ALTER TABLE Surveys DROP FOREIGN KEY surveys_ibfk_1;
ALTER TABLE Surveys DROP FOREIGN KEY surveys_ibfk_2;
SET FOREIGN_KEY_CHECKS = 1;

-- Step 6: Drop old TenantId columns from dependent tables
ALTER TABLE Projects DROP COLUMN TenantId;
ALTER TABLE Surveys DROP COLUMN TenantId;

-- Step 7: Rename TenantGuid to TenantId in dependent tables
ALTER TABLE Projects CHANGE COLUMN TenantGuid TenantId CHAR(36) NOT NULL;
ALTER TABLE Surveys CHANGE COLUMN TenantGuid TenantId CHAR(36) NOT NULL;

-- Step 8: Drop old primary key and auto_increment on Tenants table
ALTER TABLE Tenants DROP PRIMARY KEY;
ALTER TABLE Tenants MODIFY COLUMN TenantId INT NOT NULL;
ALTER TABLE Tenants DROP COLUMN TenantId;

-- Step 9: Rename TenantGuid to TenantId in Tenants table and set as primary key
ALTER TABLE Tenants CHANGE COLUMN TenantGuid TenantId CHAR(36) NOT NULL;
ALTER TABLE Tenants ADD PRIMARY KEY (TenantId);

-- Step 10: Add indexes on new TenantId GUID columns
ALTER TABLE Projects ADD INDEX idx_projects_tenantid (TenantId);
ALTER TABLE Surveys ADD INDEX idx_surveys_tenantid (TenantId);

-- Step 11: Add foreign key constraints back with new GUID TenantId
ALTER TABLE Projects ADD CONSTRAINT FK_Projects_TenantId 
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE;

ALTER TABLE Surveys ADD CONSTRAINT FK_Surveys_TenantId 
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE;
