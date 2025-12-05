-- Migration 09: Remove calculated fields that should not be persisted
-- These fields are calculated dynamically from survey data, not stored

USE porpoise_dev;

-- Remove calculated fields from Questions table
ALTER TABLE Questions
    DROP COLUMN TotalIndex,
    DROP COLUMN TotalN,
    DROP COLUMN WeightOn;

-- Remove calculated fields from Responses table
ALTER TABLE Responses
    DROP COLUMN Frequency,
    DROP COLUMN Percentage;

-- Update ModifiedDate for tracking
-- (no rows to update since we're just removing columns)
