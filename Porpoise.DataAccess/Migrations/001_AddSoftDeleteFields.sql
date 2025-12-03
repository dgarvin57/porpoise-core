-- Migration: Add soft delete fields to Projects and Surveys tables
-- Date: 2025-12-03

-- Add IsDeleted and DeletedDate to Projects table
ALTER TABLE Projects ADD COLUMN IsDeleted INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Projects ADD COLUMN DeletedDate TEXT;

-- Add IsDeleted and DeletedDate to Surveys table  
ALTER TABLE Surveys ADD COLUMN IsDeleted INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Surveys ADD COLUMN DeletedDate TEXT;

-- Create index for faster queries on deleted items
CREATE INDEX IF NOT EXISTS IX_Projects_IsDeleted ON Projects(IsDeleted);
CREATE INDEX IF NOT EXISTS IX_Surveys_IsDeleted ON Surveys(IsDeleted);
