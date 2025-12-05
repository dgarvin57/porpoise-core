-- Reset Database and Load Clean Test Data
-- Date: 2025-11-30
-- Description: Clear all data and load fresh test data with correct DataList format

USE porpoise_dev;

-- Disable foreign key checks for cleanup
SET FOREIGN_KEY_CHECKS=0;

-- Clear all data (cascade should handle most, but being explicit)
TRUNCATE TABLE SurveyData;
TRUNCATE TABLE Questions;
TRUNCATE TABLE Surveys;
TRUNCATE TABLE Projects;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS=1;

-- Create demo tenant if not exists
INSERT IGNORE INTO Tenants (TenantId, TenantKey, Name, IsActive)
VALUES (1, 'demo-tenant', 'Demo Tenant', 1);

-- ============================================================================
-- Project 1: Customer Satisfaction (Standalone Survey)
-- ============================================================================
SET @project1Id = UUID();
INSERT INTO Projects (
    Id, TenantId, ProjectName, ClientName, Description, ResearcherLabel
) VALUES (
    @project1Id, 1, 'Customer Satisfaction 2024', 'Acme Corporation',
    'Annual customer satisfaction survey', 'Research Inc.'
);

SET @survey1Id = UUID();
INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES (
    @survey1Id, @project1Id, 1, 'Customer Satisfaction 2024', 2,
    'Annual customer satisfaction tracking survey for all customers'
);

-- Add Questions
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey1Id, '1', 'Overall satisfaction', 1, 1),
    (UUID(), @survey1Id, '2', 'Recommendation likelihood (NPS)', 2, 1),
    (UUID(), @survey1Id, '3', 'Product quality', 3, 1),
    (UUID(), @survey1Id, '4', 'Customer service', 4, 1),
    (UUID(), @survey1Id, '5', 'Value for money', 5, 1);

-- Add Survey Data (header row + 10 data rows = 11 total, CaseCount = 10)
INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(), @survey1Id,
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

-- ============================================================================
-- Project 2: Brand Tracker 2024 (Multi-Survey Project - 4 Quarterly Waves)
-- ============================================================================
SET @project2Id = UUID();
INSERT INTO Projects (
    Id, TenantId, ProjectName, ClientName, Description,
    StartDate, EndDate, DefaultWeightingScheme, ResearcherLabel
) VALUES (
    @project2Id, 1, 'Brand Tracker 2024', 'Global Brands Inc',
    'Quarterly brand awareness and perception tracking study',
    '2024-01-01', '2024-12-31', 'PopulationWeight', 'Market Research Partners'
);

-- Q1 Wave
SET @survey2aId = UUID();
INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES (@survey2aId, @project2Id, 1, 'Q1 2024 - January', 2, 'First quarter wave - post-holiday season');

INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey2aId, '1', 'Brand awareness', 1, 2),
    (UUID(), @survey2aId, '2', 'Preferred brand', 2, 1),
    (UUID(), @survey2aId, '3', 'Purchase intent', 3, 1),
    (UUID(), @survey2aId, '4', 'Brand quality rating', 4, 1),
    (UUID(), @survey2aId, '5', 'Brand innovation', 5, 1),
    (UUID(), @survey2aId, '6', 'Brand trust', 6, 1);

INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(), @survey2aId,
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

-- Q2 Wave
SET @survey2bId = UUID();
INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES (@survey2bId, @project2Id, 1, 'Q2 2024 - April', 2, 'Second quarter wave - spring campaign');

INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey2bId, '1', 'Brand awareness', 1, 2),
    (UUID(), @survey2bId, '2', 'Preferred brand', 2, 1),
    (UUID(), @survey2bId, '3', 'Purchase intent', 3, 1),
    (UUID(), @survey2bId, '4', 'Brand quality rating', 4, 1),
    (UUID(), @survey2bId, '5', 'Brand innovation', 5, 1),
    (UUID(), @survey2bId, '6', 'Brand trust', 6, 1);

INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(), @survey2bId,
    JSON_ARRAY(
        JSON_ARRAY('survey#', 'q1', 'q2', 'q3', 'q4', 'q5', 'q6'),
        JSON_ARRAY('1', '2', '1', '5', '4', '4', '5'),
        JSON_ARRAY('2', '1', '3', '4', '5', '3', '4'),
        JSON_ARRAY('3', '3', '2', '4', '4', '5', '5'),
        JSON_ARRAY('4', '2', '1', '5', '5', '4', '5'),
        JSON_ARRAY('5', '1', '3', '3', '3', '3', '3'),
        JSON_ARRAY('6', '3', '2', '4', '4', '4', '4'),
        JSON_ARRAY('7', '2', '1', '5', '5', '5', '5'),
        JSON_ARRAY('8', '1', '3', '3', '3', '3', '4'),
        JSON_ARRAY('9', '3', '2', '4', '4', '4', '4'),
        JSON_ARRAY('10', '2', '1', '5', '5', '5', '5'),
        JSON_ARRAY('11', '1', '3', '4', '4', '3', '3'),
        JSON_ARRAY('12', '3', '2', '3', '3', '4', '4')
    )
);

-- Q3 Wave
SET @survey2cId = UUID();
INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES (@survey2cId, @project2Id, 1, 'Q3 2024 - July', 2, 'Third quarter wave - summer season');

INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey2cId, '1', 'Brand awareness', 1, 2),
    (UUID(), @survey2cId, '2', 'Preferred brand', 2, 1),
    (UUID(), @survey2cId, '3', 'Purchase intent', 3, 1),
    (UUID(), @survey2cId, '4', 'Brand quality rating', 4, 1),
    (UUID(), @survey2cId, '5', 'Brand innovation', 5, 1),
    (UUID(), @survey2cId, '6', 'Brand trust', 6, 1);

INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(), @survey2cId,
    JSON_ARRAY(
        JSON_ARRAY('survey#', 'q1', 'q2', 'q3', 'q4', 'q5', 'q6'),
        JSON_ARRAY('1', '3', '1', '5', '5', '5', '5'),
        JSON_ARRAY('2', '2', '2', '4', '4', '4', '4'),
        JSON_ARRAY('3', '1', '3', '3', '3', '3', '3'),
        JSON_ARRAY('4', '3', '1', '5', '5', '4', '5'),
        JSON_ARRAY('5', '2', '2', '4', '4', '5', '4'),
        JSON_ARRAY('6', '1', '3', '3', '3', '3', '3'),
        JSON_ARRAY('7', '3', '1', '4', '5', '5', '5'),
        JSON_ARRAY('8', '2', '2', '4', '4', '4', '4')
    )
);

-- Q4 Wave
SET @survey2dId = UUID();
INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES (@survey2dId, @project2Id, 1, 'Q4 2024 - October', 1, 'Fourth quarter wave - in progress');

INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey2dId, '1', 'Brand awareness', 1, 2),
    (UUID(), @survey2dId, '2', 'Preferred brand', 2, 1),
    (UUID(), @survey2dId, '3', 'Purchase intent', 3, 1),
    (UUID(), @survey2dId, '4', 'Brand quality rating', 4, 1),
    (UUID(), @survey2dId, '5', 'Brand innovation', 5, 1),
    (UUID(), @survey2dId, '6', 'Brand trust', 6, 1);

INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(), @survey2dId,
    JSON_ARRAY(
        JSON_ARRAY('survey#', 'q1', 'q2', 'q3', 'q4', 'q5', 'q6'),
        JSON_ARRAY('1', '2', '1', '5', '5', '4', '5'),
        JSON_ARRAY('2', '3', '2', '4', '4', '5', '4'),
        JSON_ARRAY('3', '1', '3', '3', '3', '3', '3'),
        JSON_ARRAY('4', '2', '1', '5', '5', '5', '5'),
        JSON_ARRAY('5', '3', '2', '4', '4', '4', '4')
    )
);

-- ============================================================================
-- Project 3: New Product Launch (Pre/Post Study)
-- ============================================================================
SET @project3Id = UUID();
INSERT INTO Projects (
    Id, TenantId, ProjectName, ClientName, Description,
    StartDate, EndDate, ResearcherLabel
) VALUES (
    @project3Id, 1, 'New Product Launch 2024', 'Tech Innovations Ltd',
    'Pre and post-launch research for new smartphone',
    '2024-06-01', '2024-09-30', 'Product Insights Group'
);

-- Pre-Launch Survey
SET @survey3aId = UUID();
INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES (@survey3aId, @project3Id, 1, 'Pre-Launch Survey', 2, 'Baseline awareness and interest study');

INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey3aId, '1', 'Brand awareness', 1, 1),
    (UUID(), @survey3aId, '2', 'Purchase intent', 2, 1),
    (UUID(), @survey3aId, '3', 'Price expectation', 3, 1);

INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(), @survey3aId,
    JSON_ARRAY(
        JSON_ARRAY('survey#', 'q1', 'q2', 'q3'),
        JSON_ARRAY('1', '2', '3', '4'),
        JSON_ARRAY('2', '3', '4', '5'),
        JSON_ARRAY('3', '2', '3', '3'),
        JSON_ARRAY('4', '1', '2', '3'),
        JSON_ARRAY('5', '3', '4', '5'),
        JSON_ARRAY('6', '2', '3', '4')
    )
);

-- Post-Launch Survey
SET @survey3bId = UUID();
INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES (@survey3bId, @project3Id, 1, 'Post-Launch Survey', 2, 'Follow-up after 3 months in market');

INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey3bId, '1', 'Brand awareness', 1, 1),
    (UUID(), @survey3bId, '2', 'Product satisfaction', 2, 1),
    (UUID(), @survey3bId, '3', 'Recommendation likelihood', 3, 1),
    (UUID(), @survey3bId, '4', 'Value for money', 4, 1);

INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(), @survey3bId,
    JSON_ARRAY(
        JSON_ARRAY('survey#', 'q1', 'q2', 'q3', 'q4'),
        JSON_ARRAY('1', '4', '4', '8', '4'),
        JSON_ARRAY('2', '5', '5', '9', '5'),
        JSON_ARRAY('3', '4', '4', '7', '4'),
        JSON_ARRAY('4', '5', '5', '10', '5'),
        JSON_ARRAY('5', '3', '3', '6', '3'),
        JSON_ARRAY('6', '4', '4', '8', '4'),
        JSON_ARRAY('7', '5', '5', '9', '5')
    )
);

-- ============================================================================
-- Project 4: Employee Engagement (Standalone Survey)
-- ============================================================================
SET @project4Id = UUID();
INSERT INTO Projects (
    Id, TenantId, ProjectName, ClientName, Description, ResearcherLabel
) VALUES (
    @project4Id, 1, 'Employee Engagement 2024', 'Acme Corporation',
    'Annual employee engagement and satisfaction survey', 'HR Analytics'
);

SET @survey4Id = UUID();
INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES (
    @survey4Id, @project4Id, 1, 'Employee Engagement 2024', 2,
    'Company-wide employee satisfaction and engagement survey'
);

INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType)
VALUES 
    (UUID(), @survey4Id, '1', 'Job satisfaction', 1, 1),
    (UUID(), @survey4Id, '2', 'Recommend as workplace', 2, 1),
    (UUID(), @survey4Id, '3', 'Tools and resources', 3, 1),
    (UUID(), @survey4Id, '4', 'Manager feedback', 4, 1);

INSERT INTO SurveyData (Id, SurveyId, DataList)
VALUES (
    UUID(), @survey4Id,
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

-- ============================================================================
-- Verification Query
-- ============================================================================
SELECT 
    p.ProjectName,
    COUNT(DISTINCT s.Id) as SurveyCount,
    COUNT(DISTINCT q.Id) as TotalQuestions,
    SUM(JSON_LENGTH(sd.DataList) - 1) as TotalCases
FROM Projects p
LEFT JOIN Surveys s ON p.Id = s.ProjectId
LEFT JOIN Questions q ON s.Id = q.SurveyId
LEFT JOIN SurveyData sd ON s.Id = sd.SurveyId
WHERE p.TenantId = 1
GROUP BY p.Id, p.ProjectName
ORDER BY p.ProjectName;

-- Detailed Survey View
SELECT 
    p.ProjectName,
    s.SurveyName,
    COUNT(DISTINCT q.Id) as QuestionCount,
    JSON_LENGTH(sd.DataList) - 1 as CaseCount
FROM Projects p
JOIN Surveys s ON p.Id = s.ProjectId
LEFT JOIN Questions q ON s.Id = q.SurveyId
LEFT JOIN SurveyData sd ON s.Id = sd.SurveyId
WHERE p.TenantId = 1
GROUP BY p.Id, p.ProjectName, s.Id, s.SurveyName, sd.DataList
ORDER BY p.ProjectName, s.SurveyName;
