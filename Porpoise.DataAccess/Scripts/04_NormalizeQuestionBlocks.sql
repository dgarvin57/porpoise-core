-- Migration: Normalize Question Blocks into separate table
-- This script:
-- 1. Creates QuestionBlocks table
-- 2. Migrates existing block data from Questions
-- 3. Adds BlockId foreign key to Questions
-- 4. Removes redundant block columns from Questions

-- Step 1: Create the QuestionBlocks table
CREATE TABLE IF NOT EXISTS QuestionBlocks (
    Id VARCHAR(36) PRIMARY KEY,
    SurveyId VARCHAR(36) NOT NULL,
    Label VARCHAR(255) NOT NULL,
    Stem TEXT,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE,
    INDEX idx_questionblocks_survey (SurveyId),
    INDEX idx_questionblocks_label (Label),
    UNIQUE INDEX idx_questionblocks_survey_label (SurveyId, Label)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Step 2: Add BlockId column to Questions (nullable for now)
ALTER TABLE Questions 
ADD COLUMN BlockId VARCHAR(36) NULL AFTER SurveyId,
ADD INDEX idx_questions_block (BlockId);

-- Step 3: Migrate existing block data
-- For each unique (SurveyId, BlkLabel) combination where BlkLabel is not empty,
-- create a QuestionBlock record with the stem from the first question in that block
INSERT INTO QuestionBlocks (Id, SurveyId, Label, Stem, DisplayOrder)
SELECT 
    UUID() as Id,
    q.SurveyId,
    q.BlkLabel as Label,
    -- Get the stem from the first question in the block (BlkQstStatus = 1)
    (SELECT q2.BlkStem 
     FROM Questions q2 
     WHERE q2.SurveyId = q.SurveyId 
       AND q2.BlkLabel = q.BlkLabel 
       AND q2.BlkQstStatus = 1 
     LIMIT 1) as Stem,
    -- Use the minimum DataFileColumn as the display order
    MIN(q.DataFileColumn) as DisplayOrder
FROM Questions q
WHERE q.BlkLabel IS NOT NULL 
  AND q.BlkLabel != ''
  AND (q.BlkQstStatus = 1 OR q.BlkQstStatus = 2)
GROUP BY q.SurveyId, q.BlkLabel;

-- Step 4: Update Questions.BlockId to reference the new QuestionBlocks
UPDATE Questions q
INNER JOIN QuestionBlocks qb 
    ON q.SurveyId = qb.SurveyId 
    AND q.BlkLabel = qb.Label
SET q.BlockId = qb.Id
WHERE q.BlkLabel IS NOT NULL 
  AND q.BlkLabel != ''
  AND (q.BlkQstStatus = 1 OR q.BlkQstStatus = 2);

-- Step 5: Add foreign key constraint now that BlockId is populated
ALTER TABLE Questions
ADD CONSTRAINT fk_questions_block
FOREIGN KEY (BlockId) REFERENCES QuestionBlocks(Id) ON DELETE SET NULL;

-- Step 6: Remove redundant block columns from Questions
-- Keep BlkQstStatus as it indicates the question's position within a block
-- Remove BlkLabel and BlkStem as they're now in QuestionBlocks
ALTER TABLE Questions
DROP COLUMN BlkLabel,
DROP COLUMN BlkStem;

-- Verification queries (optional - comment out for production)
-- SELECT 'QuestionBlocks created' as Status, COUNT(*) as Count FROM QuestionBlocks;
-- SELECT 'Questions with BlockId' as Status, COUNT(*) as Count FROM Questions WHERE BlockId IS NOT NULL;
-- SELECT 'Questions without BlockId (discrete)' as Status, COUNT(*) as Count FROM Questions WHERE BlockId IS NULL;
