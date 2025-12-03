-- Manually create QuestionBlocks for newly imported survey f365bf23-a366-4ff1-9f04-16c1d85a7a87
-- This survey was imported after normalization but blocks weren't created

USE porpoise_dev;

-- First, let's see what first questions exist
SELECT 
    Id,
    QstLabel,
    QstNumber,
    BlkQstStatus,
    DataFileColumn
FROM Questions
WHERE SurveyId = 'f365bf23-a366-4ff1-9f04-16c1d85a7a87'
  AND BlkQstStatus = 1
ORDER BY DataFileColumn;

-- Now create the blocks manually
-- We need to find the block info from an old Demo 2015 survey since BlkLabel/BlkStem don't exist in Questions anymore

-- Get block info from original Demo 2015
SELECT DISTINCT
    qb.Label,
    qb.Stem,
    qb.DisplayOrder
FROM QuestionBlocks qb
INNER JOIN Questions q ON q.BlockId = qb.Id
WHERE qb.SurveyId IN (
    SELECT Id FROM Surveys WHERE SurveyName = 'Demo 2015' AND Id != 'f365bf23-a366-4ff1-9f04-16c1d85a7a87'
)
ORDER BY qb.DisplayOrder;
