-- Migration: Remove unused fields from Surveys table
-- Date: 2025-12-03
-- Description: Remove licensing fields (LockStatus, UnlockKeyName, UnlockKeyType),
--              SaveAlteredString, and path fields (OrigDataFilePath, SurveyPath, 
--              SurveyFolder, FullProjectFolder) that are no longer used

-- Remove licensing fields
ALTER TABLE Surveys DROP COLUMN IF EXISTS LockStatus;
ALTER TABLE Surveys DROP COLUMN IF EXISTS UnlockKeyName;
ALTER TABLE Surveys DROP COLUMN IF EXISTS UnlockKeyType;

-- Remove legacy field
ALTER TABLE Surveys DROP COLUMN IF EXISTS SaveAlteredString;

-- Remove path fields (runtime-only, not needed in DB)
ALTER TABLE Surveys DROP COLUMN IF EXISTS OrigDataFilePath;
ALTER TABLE Surveys DROP COLUMN IF EXISTS SurveyPath;
ALTER TABLE Surveys DROP COLUMN IF EXISTS SurveyFolder;
ALTER TABLE Surveys DROP COLUMN IF EXISTS FullProjectFolder;
