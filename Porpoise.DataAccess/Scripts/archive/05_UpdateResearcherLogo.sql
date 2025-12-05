-- Migration: Update ResearcherLogo to use BLOB storage
-- Drop unused filename and path columns
-- Change ResearcherLogo from TEXT to LONGBLOB

ALTER TABLE Projects 
    DROP COLUMN ResearcherLogoFilename,
    DROP COLUMN ResearcherLogoPath,
    MODIFY COLUMN ResearcherLogo LONGBLOB;
