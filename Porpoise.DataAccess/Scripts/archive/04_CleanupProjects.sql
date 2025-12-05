-- Migration: Cleanup Projects table and add new fields
-- Date: 2025-11-30
-- Description: Remove obsolete folder fields, add project-level metadata

USE porpoise_dev;

-- First, clear existing data (as instructed)
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM SurveyData;
DELETE FROM Responses;
DELETE FROM Questions;
DELETE FROM Surveys;
DELETE FROM Projects;
SET FOREIGN_KEY_CHECKS = 1;

-- Remove obsolete folder/path columns
ALTER TABLE Projects DROP COLUMN BaseProjectFolder;
ALTER TABLE Projects DROP COLUMN ProjectFolder;
ALTER TABLE Projects DROP COLUMN FullFolder;
ALTER TABLE Projects DROP COLUMN FullPath;

-- Rename FileName to ProjectFile for clarity
ALTER TABLE Projects CHANGE COLUMN FileName ProjectFile VARCHAR(500) NULL;

-- Add new project-level metadata columns
ALTER TABLE Projects ADD COLUMN Description TEXT NULL AFTER ClientName;
ALTER TABLE Projects ADD COLUMN StartDate DATETIME NULL AFTER Description;
ALTER TABLE Projects ADD COLUMN EndDate DATETIME NULL AFTER StartDate;
ALTER TABLE Projects ADD COLUMN DefaultWeightingScheme VARCHAR(100) NULL AFTER EndDate;
ALTER TABLE Projects ADD COLUMN BrandingSettings JSON NULL AFTER DefaultWeightingScheme;

-- Rename existing CreatedOn/ModifiedOn to match new naming convention
ALTER TABLE Projects CHANGE COLUMN CreatedOn CreatedDate DATETIME NULL;
ALTER TABLE Projects CHANGE COLUMN ModifiedOn ModifiedDate DATETIME NULL;

-- Set defaults for audit fields
ALTER TABLE Projects MODIFY COLUMN CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE Projects MODIFY COLUMN ModifiedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

-- Add indexes for date range queries
CREATE INDEX idx_projects_dates ON Projects(StartDate, EndDate);
CREATE INDEX idx_projects_created ON Projects(CreatedDate);

-- Verify the changes
DESCRIBE Projects;
