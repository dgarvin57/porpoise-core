-- Complete TenantId GUID Migration - Final Steps

USE porpoise_dev;

-- Ensure all GUIDs are populated
UPDATE Projects p 
INNER JOIN Tenants t ON p.TenantId = t.TenantId 
SET p.TenantGuid = t.TenantGuid
WHERE p.TenantGuid IS NULL OR p.TenantGuid = '';

UPDATE Surveys s 
INNER JOIN Tenants t ON s.TenantId = t.TenantId 
SET s.TenantGuid = t.TenantGuid
WHERE s.TenantGuid IS NULL OR s.TenantGuid = '';

-- Drop foreign keys that reference old TenantId
SET FOREIGN_KEY_CHECKS = 0;

-- Drop Projects constraints
SET @query = (SELECT CONCAT('ALTER TABLE Projects DROP FOREIGN KEY ', CONSTRAINT_NAME, ';')
FROM information_schema.TABLE_CONSTRAINTS 
WHERE TABLE_NAME = 'Projects' AND CONSTRAINT_TYPE = 'FOREIGN KEY' 
AND CONSTRAINT_NAME LIKE '%tenant%' LIMIT 1);
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Drop Surveys constraints related to Tenants
SET @query = (SELECT CONCAT('ALTER TABLE Surveys DROP FOREIGN KEY ', CONSTRAINT_NAME, ';')
FROM information_schema.TABLE_CONSTRAINTS 
WHERE TABLE_NAME = 'Surveys' AND CONSTRAINT_TYPE = 'FOREIGN KEY' 
AND CONSTRAINT_NAME IN ('surveys_ibfk_1', 'surveys_ibfk_2') LIMIT 1);
PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET FOREIGN_KEY_CHECKS = 1;

-- Drop old INT TenantId columns
ALTER TABLE Projects DROP COLUMN TenantId;
ALTER TABLE Surveys DROP COLUMN TenantId;

-- Rename TenantGuid to TenantId in Projects and Surveys
ALTER TABLE Projects CHANGE COLUMN TenantGuid TenantId CHAR(36) NOT NULL;
ALTER TABLE Surveys CHANGE COLUMN TenantGuid TenantId CHAR(36) NOT NULL;

-- Update Tenants table: drop old INT primary key, use GUID
ALTER TABLE Tenants DROP PRIMARY KEY;
ALTER TABLE Tenants DROP COLUMN TenantId;
ALTER TABLE Tenants CHANGE COLUMN TenantGuid TenantId CHAR(36) NOT NULL;
ALTER TABLE Tenants ADD PRIMARY KEY (TenantId);

-- Add indexes
CREATE INDEX idx_projects_tenantid ON Projects(TenantId);
CREATE INDEX idx_surveys_tenantid ON Surveys(TenantId);

-- Re-add foreign key constraints
ALTER TABLE Projects ADD CONSTRAINT FK_Projects_TenantId 
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE;

ALTER TABLE Surveys ADD CONSTRAINT FK_Surveys_TenantId 
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE;

-- Re-add ProjectId foreign key that was dropped
ALTER TABLE Surveys ADD CONSTRAINT FK_Surveys_ProjectId
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE;
