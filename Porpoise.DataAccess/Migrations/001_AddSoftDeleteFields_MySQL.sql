-- Migration: Add soft delete fields to Projects and Surveys tables (MySQL)
-- Date: 2025-12-03

-- Add IsDeleted and DeletedDate to Projects table
ALTER TABLE Projects 
ADD COLUMN IsDeleted TINYINT NOT NULL DEFAULT 0,
ADD COLUMN DeletedDate DATETIME NULL;

-- Add IsDeleted and DeletedDate to Surveys table  
ALTER TABLE Surveys 
ADD COLUMN IsDeleted TINYINT NOT NULL DEFAULT 0,
ADD COLUMN DeletedDate DATETIME NULL;

-- Create index for faster queries on deleted items
CREATE INDEX IX_Projects_IsDeleted ON Projects(IsDeleted);
CREATE INDEX IX_Surveys_IsDeleted ON Surveys(IsDeleted);
