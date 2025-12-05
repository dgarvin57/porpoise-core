-- Rollback and Redo TenantId GUID Migration

USE porpoise_dev;

-- Rollback: Add back INT TenantId to Projects and Surveys
ALTER TABLE Projects ADD COLUMN TenantId INT AFTER Id;
ALTER TABLE Surveys ADD COLUMN TenantId INT AFTER ProjectId;

-- Copy from Tenants back to dependent tables
UPDATE Projects p 
INNER JOIN Tenants t ON p.TenantGuid = t.TenantGuid 
SET p.TenantId = t.TenantId;

UPDATE Surveys s 
INNER JOIN Tenants t ON s.TenantGuid = t.TenantGuid 
SET s.TenantId = t.TenantId;

-- Now run the proper migration forward
-- Step 1: Drop old columns from Projects and Surveys
ALTER TABLE Projects DROP COLUMN TenantId;
ALTER TABLE Surveys DROP COLUMN TenantId;

-- Step 2: Rename TenantGuid to TenantId
ALTER TABLE Projects CHANGE COLUMN TenantGuid TenantId CHAR(36) NOT NULL;
ALTER TABLE Surveys CHANGE COLUMN TenantGuid TenantId CHAR(36) NOT NULL;

-- Step 3: Update Tenants table
ALTER TABLE Tenants DROP PRIMARY KEY;
ALTER TABLE Tenants MODIFY COLUMN TenantId INT NOT NULL;
ALTER TABLE Tenants DROP COLUMN TenantId;
ALTER TABLE Tenants CHANGE COLUMN TenantGuid TenantId CHAR(36) NOT NULL;
ALTER TABLE Tenants ADD PRIMARY KEY (TenantId);

-- Step 4: Add indexes
ALTER TABLE Projects ADD INDEX idx_projects_tenantid (TenantId);
ALTER TABLE Surveys ADD INDEX idx_surveys_tenantid (TenantId);

-- Step 5: Add foreign key constraints
ALTER TABLE Projects ADD CONSTRAINT FK_Projects_TenantId 
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE;

ALTER TABLE Surveys ADD CONSTRAINT FK_Surveys_TenantId 
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE;
