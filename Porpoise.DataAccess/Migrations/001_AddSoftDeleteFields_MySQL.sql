-- Migration: Add soft delete fields to Projects and Surveys tables (MySQL)
-- Date: 2025-12-03

-- Add IsDeleted and DeletedDate to Projects table (if not exists)
ALTER TABLE Projects 
ADD COLUMN IF NOT EXISTS IsDeleted TINYINT NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS DeletedDate DATETIME NULL;

-- Add IsDeleted and DeletedDate to Surveys table (if not exists) 
ALTER TABLE Surveys 
ADD COLUMN IF NOT EXISTS IsDeleted TINYINT NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS DeletedDate DATETIME NULL;

-- Create index for faster queries on deleted items (if not exists)
CREATE INDEX IF NOT EXISTS IX_Projects_IsDeleted ON Projects(IsDeleted);
CREATE INDEX IF NOT EXISTS IX_Surveys_IsDeleted ON Surveys(IsDeleted);
