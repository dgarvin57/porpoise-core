-- Fix: Link continuation questions (BlkQstStatus = 2) to their blocks
-- This handles cases where continuation questions have empty BlkLabel

-- Strategy: For each continuation question with NULL BlockId, find the most recent
-- first question (BlkQstStatus = 1) in the same survey and assign its BlockId

-- Create a temporary table to store the mapping
CREATE TEMPORARY TABLE BlockMapping AS
SELECT 
    q2.Id as ContinuationQuestionId,
    (
        SELECT q1.BlockId
        FROM Questions q1
        WHERE q1.SurveyId = q2.SurveyId
          AND q1.BlkQstStatus = 1
          AND q1.BlockId IS NOT NULL
          AND q1.DataFileColumn < q2.DataFileColumn
        ORDER BY q1.DataFileColumn DESC
        LIMIT 1
    ) as TargetBlockId
FROM Questions q2
WHERE q2.BlkQstStatus = 2 
  AND q2.BlockId IS NULL;

-- Update continuation questions with the correct BlockId
UPDATE Questions q
INNER JOIN BlockMapping bm ON q.Id = bm.ContinuationQuestionId
SET q.BlockId = bm.TargetBlockId
WHERE bm.TargetBlockId IS NOT NULL;

-- Clean up
DROP TEMPORARY TABLE BlockMapping;

-- Verification query
SELECT 
    'Fixed continuation questions' as Status,
    COUNT(*) as Count
FROM Questions
WHERE BlkQstStatus = 2 AND BlockId IS NOT NULL;
