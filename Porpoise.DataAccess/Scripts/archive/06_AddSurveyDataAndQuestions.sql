-- Add Questions and Survey Data to Test Data
-- Date: 2025-11-30
-- Description: Add questions and response data in correct format (survey#, q1, q2, ... with data matrix)

USE porpoise_dev;

-- Get survey IDs (we'll use the first survey from each project for demo data)
SET @survey1Id = (SELECT Id FROM Surveys WHERE SurveyName = 'Customer Satisfaction 2024' AND TenantId = 1 LIMIT 1);
SET @survey2Id = (SELECT Id FROM Surveys WHERE SurveyName = 'Q1 2024 - January' AND TenantId = 1 LIMIT 1);
SET @survey3Id = (SELECT Id FROM Surveys WHERE SurveyName = 'Employee Engagement 2024' AND TenantId = 1 LIMIT 1);

-- Add Questions for Survey 1: Customer Satisfaction
-- Question 1: Overall Satisfaction
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES (
    UUID(),
    @survey1Id,
    '1',
    'Overall satisfaction',
    1,  -- Column 1 in data file (after survey#)
    1   -- Single response
);

-- Question 2: Recommendation Likelihood (NPS)
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES (
    UUID(),
    @survey1Id,
    '2',
    'Recommendation likelihood',
    2,
    1
);

-- Question 3: Product Quality
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES (
    UUID(),
    @survey1Id,
    '3',
    'Product quality',
    3,
    1
);

-- Question 4: Customer Service
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES (
    UUID(),
    @survey1Id,
    '4',
    'Customer service',
    4,
    1
);

-- Question 5: Value for Money
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES (
    UUID(),
    @survey1Id,
    '5',
    'Value for money',
    5,
    1
);

-- Add Survey Data for Survey 1 in correct format
-- Format: [["survey#","q1","q2","q3","q4","q5"], ["1","4","8","4","5","4"], ["2","5","9","5","4","5"], ...]
INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(),
    @survey1Id,
    JSON_ARRAY(
        JSON_ARRAY('survey#', 'q1', 'q2', 'q3', 'q4', 'q5'),
        JSON_ARRAY('1', '4', '8', '4', '5', '4'),
        JSON_ARRAY('2', '5', '9', '5', '4', '5'),
        JSON_ARRAY('3', '3', '6', '3', '3', '3'),
        JSON_ARRAY('4', '4', '7', '4', '4', '4'),
        JSON_ARRAY('5', '5', '10', '5', '5', '5'),
        JSON_ARRAY('6', '2', '4', '2', '2', '2'),
        JSON_ARRAY('7', '4', '8', '4', '5', '4'),
        JSON_ARRAY('8', '5', '9', '5', '5', '5'),
        JSON_ARRAY('9', '3', '7', '3', '4', '3'),
        JSON_ARRAY('10', '4', '8', '4', '4', '4')
    )
);

-- Add Questions for Survey 2: Brand Tracker Q1
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey2Id, '1', 'Brand awareness', 1, 2),
    (UUID(), @survey2Id, '2', 'Preferred brand', 2, 1),
    (UUID(), @survey2Id, '3', 'Purchase intent', 3, 1),
    (UUID(), @survey2Id, '4', 'Brand quality rating', 4, 1),
    (UUID(), @survey2Id, '5', 'Brand innovation', 5, 1),
    (UUID(), @survey2Id, '6', 'Brand trust', 6, 1);

-- Add Survey Data for Survey 2
INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(),
    @survey2Id,
    JSON_ARRAY(
        JSON_ARRAY('survey#', 'q1', 'q2', 'q3', 'q4', 'q5', 'q6'),
        JSON_ARRAY('1', '1', '2', '4', '4', '3', '4'),
        JSON_ARRAY('2', '3', '1', '5', '5', '4', '5'),
        JSON_ARRAY('3', '2', '3', '3', '3', '3', '3'),
        JSON_ARRAY('4', '1', '2', '4', '4', '4', '4'),
        JSON_ARRAY('5', '3', '1', '5', '5', '5', '5'),
        JSON_ARRAY('6', '2', '2', '3', '3', '3', '4'),
        JSON_ARRAY('7', '1', '3', '4', '4', '3', '3'),
        JSON_ARRAY('8', '3', '1', '5', '5', '4', '5'),
        JSON_ARRAY('9', '2', '2', '4', '4', '4', '4'),
        JSON_ARRAY('10', '1', '3', '3', '3', '3', '3'),
        JSON_ARRAY('11', '3', '1', '5', '5', '5', '5'),
        JSON_ARRAY('12', '2', '2', '4', '4', '4', '4'),
        JSON_ARRAY('13', '1', '3', '3', '3', '3', '3'),
        JSON_ARRAY('14', '3', '1', '4', '4', '4', '4'),
        JSON_ARRAY('15', '2', '2', '5', '5', '5', '5')
    )
);

-- Add Questions for Survey 3: Employee Engagement
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey3Id, '1', 'Job satisfaction', 1, 1),
    (UUID(), @survey3Id, '2', 'Recommend as workplace', 2, 1),
    (UUID(), @survey3Id, '3', 'Tools and resources', 3, 1),
    (UUID(), @survey3Id, '4', 'Manager feedback', 4, 1);

-- Add Survey Data for Survey 3
INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(),
    @survey3Id,
    JSON_ARRAY(
        JSON_ARRAY('survey#', 'q1', 'q2', 'q3', 'q4'),
        JSON_ARRAY('1', '4', '4', '5', '4'),
        JSON_ARRAY('2', '5', '5', '5', '5'),
        JSON_ARRAY('3', '3', '3', '4', '3'),
        JSON_ARRAY('4', '4', '4', '4', '4'),
        JSON_ARRAY('5', '5', '5', '5', '5'),
        JSON_ARRAY('6', '2', '2', '3', '2'),
        JSON_ARRAY('7', '4', '4', '4', '4'),
        JSON_ARRAY('8', '5', '5', '5', '5')
    )
);

-- Verify data
SELECT 
    s.SurveyName,
    COUNT(DISTINCT q.Id) as QuestionCount,
    JSON_LENGTH(sd.DataList) as TotalRows,
    JSON_LENGTH(sd.DataList) - 1 as CaseCount
FROM Surveys s
LEFT JOIN Questions q ON s.Id = q.SurveyId
LEFT JOIN SurveyData sd ON s.Id = sd.SurveyId
WHERE s.TenantId = 1
GROUP BY s.Id, s.SurveyName, sd.DataList
ORDER BY s.SurveyName;
