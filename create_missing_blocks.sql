-- Manually create QuestionBlocks for f365bf23-a366-4ff1-9f04-16c1d85a7a87 (newly imported Demo 2015)
-- Based on the first questions found: Bipartisan, Fiscally conservative, 1st Ballot, Ralph Republican

USE porpoise_dev;

-- Create blocks with explicit GUIDs
SET @survey_id = 'f365bf23-a366-4ff1-9f04-16c1d85a7a87';
SET @block1_id = UUID();
SET @block2_id = UUID();
SET @block3_id = UUID();
SET @block4_id = UUID();

-- Block 1: Bipartisan (q8a, DataFileColumn 18)
INSERT INTO QuestionBlocks (Id, SurveyId, Label, Stem, DisplayOrder)
VALUES (
    @block1_id,
    @survey_id,
    'Bipartisan',
    '',
    0
);

-- Block 2: Fiscally conservative (q6a, DataFileColumn 11)  
INSERT INTO QuestionBlocks (Id, SurveyId, Label, Stem, DisplayOrder)
VALUES (
    @block2_id,
    @survey_id,
    'Fiscally conservative',
    '',
    1
);

-- Block 3: 1st Ballot (q5, DataFileColumn 1)
INSERT INTO QuestionBlocks (Id, SurveyId, Label, Stem, DisplayOrder)
VALUES (
    @block3_id,
    @survey_id,
    '1st Ballot',
    '',
    2
);

-- Block 4: Ralph Republican (q3a, DataFileColumn 15)
INSERT INTO QuestionBlocks (Id, SurveyId, Label, Stem, DisplayOrder)
VALUES (
    @block4_id,
    @survey_id,
    'Ralph Republican',
    '',
    3
);

-- Now link questions to blocks based on DataFileColumn order
-- Block 1: Bipartisan (DataFileColumn 18) - link first question and its continuations
UPDATE Questions 
SET BlockId = @block1_id
WHERE SurveyId = @survey_id
  AND Id = '38bcbbce-bde1-422d-95ab-68ab670768c3'; -- Bipartisan first question

-- Link continuation questions for Bipartisan (DataFileColumn > 18)
UPDATE Questions
SET BlockId = @block1_id
WHERE SurveyId = @survey_id
  AND BlkQstStatus = 2
  AND DataFileColumn > 18;

-- Block 2: Fiscally conservative (DataFileColumn 11)
UPDATE Questions
SET BlockId = @block2_id
WHERE SurveyId = @survey_id
  AND Id = '87f9835d-d00c-41ba-b16d-b41397222d70'; -- Fiscally conservative first question

UPDATE Questions
SET BlockId = @block2_id
WHERE SurveyId = @survey_id
  AND BlkQstStatus = 2
  AND DataFileColumn > 11
  AND DataFileColumn < 15;

-- Block 3: 1st Ballot (DataFileColumn 1)
UPDATE Questions
SET BlockId = @block3_id
WHERE SurveyId = @survey_id
  AND Id = 'b4e1da18-3ed4-4b0b-807b-6fdd91a62683'; -- 1st Ballot first question

UPDATE Questions
SET BlockId = @block3_id
WHERE SurveyId = @survey_id
  AND BlkQstStatus = 2
  AND DataFileColumn > 1
  AND DataFileColumn < 11;

-- Block 4: Ralph Republican (DataFileColumn 15)
UPDATE Questions
SET BlockId = @block4_id
WHERE SurveyId = @survey_id
  AND Id = 'e2ece432-2632-4c33-ab15-88f1caab129f'; -- Ralph Republican first question

UPDATE Questions
SET BlockId = @block4_id
WHERE SurveyId = @survey_id
  AND BlkQstStatus = 2
  AND DataFileColumn > 15
  AND DataFileColumn < 18;

-- Show results
SELECT 'Created blocks:' as Result;
SELECT * FROM QuestionBlocks WHERE SurveyId = @survey_id ORDER BY DisplayOrder;

SELECT 'Questions with BlockId assigned:' as Result;
SELECT QstLabel, QstNumber, BlkQstStatus, DataFileColumn, BlockId
FROM Questions
WHERE SurveyId = @survey_id
ORDER BY DataFileColumn;
