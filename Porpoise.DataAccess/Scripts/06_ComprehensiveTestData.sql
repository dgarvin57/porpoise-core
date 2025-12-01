-- Comprehensive Test Data with Questions and Responses
-- Clear all existing data first
SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE SurveyData;
TRUNCATE TABLE Responses;
TRUNCATE TABLE Questions;
TRUNCATE TABLE Surveys;
TRUNCATE TABLE Projects;
SET FOREIGN_KEY_CHECKS = 1;

-- Insert Projects
INSERT INTO Projects (Id, TenantId, ProjectName, ClientName, StartDate, EndDate, ProjectFile, CreatedDate, ModifiedDate)
VALUES
    -- Multi-survey project: Brand Tracker
    (UUID(), 1, 'Brand Tracker Q4 2024', 'Acme Corporation', 
     'Quarterly brand health tracking study measuring awareness, consideration, and perception across key demographics.',
     '2024-10-01', '2024-12-31', 'BrandTracker_Q4_2024.porp', NOW(), NOW()),
    
    -- Multi-survey project: Product Launch Research
    (UUID(), 1, 'Product Launch Research', 'TechStart Inc', 
     'Pre and post-launch research for new product line including concept testing and market response.',
     '2024-09-15', '2024-11-30', 'ProductLaunch_2024.porp', NOW(), NOW()),
    
    -- Standalone survey projects
    (UUID(), 1, 'Customer Satisfaction Survey', 'Retail Plus', 
     'Annual customer satisfaction measurement focusing on service quality and loyalty.',
     '2024-11-01', '2024-11-15', 'CustomerSat_2024.porp', NOW(), NOW()),
    
    (UUID(), 1, 'Employee Engagement Survey', 'Global Services Ltd', 
     'Bi-annual employee engagement survey measuring job satisfaction, management effectiveness, and workplace culture.',
     '2024-11-10', '2024-11-25', 'EmployeeEngagement_Fall2024.porp', NOW(), NOW());

-- Get Project IDs for reference
SET @brandTrackerId = (SELECT Id FROM Projects WHERE ProjectName = 'Brand Tracker Q4 2024' LIMIT 1);
SET @productLaunchId = (SELECT Id FROM Projects WHERE ProjectName = 'Product Launch Research' LIMIT 1);
SET @customerSatId = (SELECT Id FROM Projects WHERE ProjectName = 'Customer Satisfaction Survey' LIMIT 1);
SET @employeeEngId = (SELECT Id FROM Projects WHERE ProjectName = 'Employee Engagement Survey' LIMIT 1);

-- Insert Surveys
INSERT INTO Surveys (Id, TenantId, ProjectId, SurveyName CreatedDate, ModifiedDate)
VALUES
    -- Brand Tracker surveys (4 waves)
    (UUID(), 1, @brandTrackerId, 'Brand Tracker Wave 1 - October', '2024-10-05', '2024-10-05'),
    (UUID(), 1, @brandTrackerId, 'Brand Tracker Wave 2 - October', '2024-10-20', '2024-10-20'),
    (UUID(), 1, @brandTrackerId, 'Brand Tracker Wave 3 - November', '2024-11-05', '2024-11-05'),
    (UUID(), 1, @brandTrackerId, 'Brand Tracker Wave 4 - November', '2024-11-20', '2024-11-20'),
    
    -- Product Launch surveys (2)
    (UUID(), 1, @productLaunchId, 'Pre-Launch Concept Test', '2024-09-20', '2024-09-20'),
    (UUID(), 1, @productLaunchId, 'Post-Launch Market Response', '2024-11-15', '2024-11-15'),
    
    -- Standalone surveys
    (UUID(), 1, @customerSatId, 'Customer Satisfaction Survey', '2024-11-05', '2024-11-05'),
    (UUID(), 1, @employeeEngId, 'Employee Engagement Survey', '2024-11-12', '2024-11-12');

-- Get Survey IDs
SET @brandWave1 = (SELECT Id FROM Surveys WHERE SurveyName = 'Brand Tracker Wave 1 - October' LIMIT 1);
SET @brandWave2 = (SELECT Id FROM Surveys WHERE SurveyName = 'Brand Tracker Wave 2 - October' LIMIT 1);
SET @brandWave3 = (SELECT Id FROM Surveys WHERE SurveyName = 'Brand Tracker Wave 3 - November' LIMIT 1);
SET @brandWave4 = (SELECT Id FROM Surveys WHERE SurveyName = 'Brand Tracker Wave 4 - November' LIMIT 1);
SET @preLaunch = (SELECT Id FROM Surveys WHERE SurveyName = 'Pre-Launch Concept Test' LIMIT 1);
SET @postLaunch = (SELECT Id FROM Surveys WHERE SurveyName = 'Post-Launch Market Response' LIMIT 1);
SET @custSat = (SELECT Id FROM Surveys WHERE SurveyName = 'Customer Satisfaction Survey' LIMIT 1);
SET @empEng = (SELECT Id FROM Surveys WHERE SurveyName = 'Employee Engagement Survey' LIMIT 1);

-- ==================== QUESTIONS ====================
-- Questions use actual schema: SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType

-- Brand Tracker Wave 1 Questions
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType,CreatedDate, ModifiedDate)
VALUES
    (UUID(), @brandWave1, 'Q1', 'Which of the following brands are you aware of?', 1, 0, NOW(), NOW()),
    (UUID(), @brandWave1, 'Q2', 'Which brand would you most likely purchase?', 2, 0, NOW(), NOW()),
    (UUID(), @brandWave1, 'Q3', 'How would you rate the quality of [Brand]?', 3, 0, NOW(), NOW()),
    (UUID(), @brandWave1, 'Q4', 'What words come to mind when you think of [Brand]?', 4, 0, NOW(), NOW());

-- Brand Tracker Waves 2-4 use same questions
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType,CreatedDate, ModifiedDate)
SELECT UUID(), @brandWave2, QstNumber, QstLabel, DataFileColumn, VariableType,NOW(), NOW()
FROM Questions WHERE SurveyId = @brandWave1;

INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType,CreatedDate, ModifiedDate)
SELECT UUID(), @brandWave3, QstNumber, QstLabel, DataFileColumn, VariableType,NOW(), NOW()
FROM Questions WHERE SurveyId = @brandWave1;

INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType,CreatedDate, ModifiedDate)
SELECT UUID(), @brandWave4, QstNumber, QstLabel, DataFileColumn, VariableType,NOW(), NOW()
FROM Questions WHERE SurveyId = @brandWave1;

-- Pre-Launch Concept Test Questions
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType,CreatedDate, ModifiedDate)
VALUES
    (UUID(), @preLaunch, 'Q1', 'How appealing is this product concept?', 1, 0, NOW(), NOW()),
    (UUID(), @preLaunch, 'Q2', 'How likely are you to purchase this product?', 2, 0, NOW(), NOW()),
    (UUID(), @preLaunch, 'Q3', 'What price would you expect to pay?', 3, 0, NOW(), NOW()),
    (UUID(), @preLaunch, 'Q4', 'What features are most important to you?', 4, 0, NOW(), NOW()),
    (UUID(), @preLaunch, 'Q5', 'What improvements would you suggest?', 5, 0, NOW(), NOW());

-- Post-Launch Market Response Questions
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType,CreatedDate, ModifiedDate)
VALUES
    (UUID(), @postLaunch, 'Q1', 'Have you purchased this product?', 1, 0, NOW(), NOW()),
    (UUID(), @postLaunch, 'Q2', 'How satisfied are you with the product?', 2, 0, NOW(), NOW()),
    (UUID(), @postLaunch, 'Q3', 'How does it compare to alternatives?', 3, 0, NOW(), NOW()),
    (UUID(), @postLaunch, 'Q4', 'Would you recommend this product?', 4, 0, NOW(), NOW()),
    (UUID(), @postLaunch, 'Q5', 'What do you like most about it?', 5, 0, NOW(), NOW());

-- Customer Satisfaction Survey Questions
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType,CreatedDate, ModifiedDate)
VALUES
    (UUID(), @custSat, 'Q1', 'How satisfied are you with our service?', 1, 0, NOW(), NOW()),
    (UUID(), @custSat, 'Q2', 'How likely are you to recommend us? (NPS)', 2, 0, NOW(), NOW()),
    (UUID(), @custSat, 'Q3', 'How responsive was our staff?', 3, 0, NOW(), NOW()),
    (UUID(), @custSat, 'Q4', 'What could we improve?', 4, 0, NOW(), NOW()),
    (UUID(), @custSat, 'Q5', 'How long have you been a customer?', 5, 0, NOW(), NOW());

-- Employee Engagement Survey Questions
INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType,CreatedDate, ModifiedDate)
VALUES
    (UUID(), @empEng, 'Q1', 'I am satisfied with my job', 1, 0, NOW(), NOW()),
    (UUID(), @empEng, 'Q2', 'My manager supports my development', 2, 0, NOW(), NOW()),
    (UUID(), @empEng, 'Q3', 'I have the tools I need to succeed', 3, 0, NOW(), NOW()),
    (UUID(), @empEng, 'Q4', 'I would recommend this company as a great place to work', 4, 0, NOW(), NOW()),
    (UUID(), @empEng, 'Q5', 'What would improve your work experience?', 5, 0, NOW(), NOW());

-- ==================== RESPONSES ====================
-- Responses use schema: QuestionId, RespValue (int), Label, Frequency, Percentage

-- Get Question IDs for Brand Wave 1
SET @bw1q1 = (SELECT Id FROM Questions WHERE SurveyId = @brandWave1 AND QstNumber = 'Q1' LIMIT 1);
SET @bw1q2 = (SELECT Id FROM Questions WHERE SurveyId = @brandWave1 AND QstNumber = 'Q2' LIMIT 1);
SET @bw1q3 = (SELECT Id FROM Questions WHERE SurveyId = @brandWave1 AND QstNumber = 'Q3' LIMIT 1);

-- Brand Awareness Responses
INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @bw1q1, 1, 'Brand A', 45, 45.0, NOW(), NOW()),
    (UUID(), @bw1q1, 2, 'Brand B', 38, 38.0, NOW(), NOW()),
    (UUID(), @bw1q1, 3, 'Brand C', 32, 32.0, NOW(), NOW()),
    (UUID(), @bw1q1, 4, 'Brand D', 28, 28.0, NOW(), NOW()),
    (UUID(), @bw1q1, 5, 'Brand E', 15, 15.0, NOW(), NOW());

-- Purchase Intent Responses
INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @bw1q2, 1, 'Brand A', 22, 22.0, NOW(), NOW()),
    (UUID(), @bw1q2, 2, 'Brand B', 18, 18.0, NOW(), NOW()),
    (UUID(), @bw1q2, 3, 'Brand C', 25, 25.0, NOW(), NOW()),
    (UUID(), @bw1q2, 4, 'Brand D', 15, 15.0, NOW(), NOW()),
    (UUID(), @bw1q2, 5, 'Brand E', 20, 20.0, NOW(), NOW());

-- Quality Rating Responses (Scale 1-5)
INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @bw1q3, 1, 'Very Poor', 5, 5.0, NOW(), NOW()),
    (UUID(), @bw1q3, 2, 'Poor', 8, 8.0, NOW(), NOW()),
    (UUID(), @bw1q3, 3, 'Average', 25, 25.0, NOW(), NOW()),
    (UUID(), @bw1q3, 4, 'Good', 42, 42.0, NOW(), NOW()),
    (UUID(), @bw1q3, 5, 'Excellent', 20, 20.0, NOW(), NOW());

-- Get Question IDs for Customer Satisfaction
SET @csq1 = (SELECT Id FROM Questions WHERE SurveyId = @custSat AND QstNumber = 'Q1' LIMIT 1);
SET @csq2 = (SELECT Id FROM Questions WHERE SurveyId = @custSat AND QstNumber = 'Q2' LIMIT 1);
SET @csq3 = (SELECT Id FROM Questions WHERE SurveyId = @custSat AND QstNumber = 'Q3' LIMIT 1);
SET @csq5 = (SELECT Id FROM Questions WHERE SurveyId = @custSat AND QstNumber = 'Q5' LIMIT 1);

-- Satisfaction Scale Responses
INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @csq1, 1, 'Very Dissatisfied', 8, 8.0, NOW(), NOW()),
    (UUID(), @csq1, 2, 'Dissatisfied', 12, 12.0, NOW(), NOW()),
    (UUID(), @csq1, 3, 'Neutral', 20, 20.0, NOW(), NOW()),
    (UUID(), @csq1, 4, 'Satisfied', 40, 40.0, NOW(), NOW()),
    (UUID(), @csq1, 5, 'Very Satisfied', 20, 20.0, NOW(), NOW());

-- NPS Scale Responses
INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @csq2, 0, '0 - Not at all likely', 5, 5.0, NOW(), NOW()),
    (UUID(), @csq2, 1, '1', 3, 3.0, NOW(), NOW()),
    (UUID(), @csq2, 2, '2', 4, 4.0, NOW(), NOW()),
    (UUID(), @csq2, 3, '3', 6, 6.0, NOW(), NOW()),
    (UUID(), @csq2, 4, '4', 7, 7.0, NOW(), NOW()),
    (UUID(), @csq2, 5, '5', 10, 10.0, NOW(), NOW()),
    (UUID(), @csq2, 6, '6', 8, 8.0, NOW(), NOW()),
    (UUID(), @csq2, 7, '7', 12, 12.0, NOW(), NOW()),
    (UUID(), @csq2, 8, '8', 15, 15.0, NOW(), NOW()),
    (UUID(), @csq2, 9, '9', 18, 18.0, NOW(), NOW()),
    (UUID(), @csq2, 10, '10 - Extremely likely', 12, 12.0, NOW(), NOW());

-- Responsiveness Responses
INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @csq3, 1, 'Very Poor', 6, 6.0, NOW(), NOW()),
    (UUID(), @csq3, 2, 'Poor', 10, 10.0, NOW(), NOW()),
    (UUID(), @csq3, 3, 'Average', 22, 22.0, NOW(), NOW()),
    (UUID(), @csq3, 4, 'Good', 42, 42.0, NOW(), NOW()),
    (UUID(), @csq3, 5, 'Excellent', 20, 20.0, NOW(), NOW());

-- Customer Tenure Responses
INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @csq5, 1, 'Less than 6 months', 15, 15.0, NOW(), NOW()),
    (UUID(), @csq5, 2, '6-12 months', 20, 20.0, NOW(), NOW()),
    (UUID(), @csq5, 3, '1-2 years', 25, 25.0, NOW(), NOW()),
    (UUID(), @csq5, 4, '2-5 years', 30, 30.0, NOW(), NOW()),
    (UUID(), @csq5, 5, '5+ years', 10, 10.0, NOW(), NOW());

-- Get Question IDs for Employee Engagement
SET @eeq1 = (SELECT Id FROM Questions WHERE SurveyId = @empEng AND QstNumber = 'Q1' LIMIT 1);
SET @eeq2 = (SELECT Id FROM Questions WHERE SurveyId = @empEng AND QstNumber = 'Q2' LIMIT 1);
SET @eeq3 = (SELECT Id FROM Questions WHERE SurveyId = @empEng AND QstNumber = 'Q3' LIMIT 1);
SET @eeq4 = (SELECT Id FROM Questions WHERE SurveyId = @empEng AND QstNumber = 'Q4' LIMIT 1);

-- Agreement Scale Responses for Employee Engagement
INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @eeq1, 1, 'Strongly Disagree', 8, 8.0, NOW(), NOW()),
    (UUID(), @eeq1, 2, 'Disagree', 12, 12.0, NOW(), NOW()),
    (UUID(), @eeq1, 3, 'Neutral', 18, 18.0, NOW(), NOW()),
    (UUID(), @eeq1, 4, 'Agree', 42, 42.0, NOW(), NOW()),
    (UUID(), @eeq1, 5, 'Strongly Agree', 20, 20.0, NOW(), NOW());

INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @eeq2, 1, 'Strongly Disagree', 10, 10.0, NOW(), NOW()),
    (UUID(), @eeq2, 2, 'Disagree', 15, 15.0, NOW(), NOW()),
    (UUID(), @eeq2, 3, 'Neutral', 20, 20.0, NOW(), NOW()),
    (UUID(), @eeq2, 4, 'Agree', 38, 38.0, NOW(), NOW()),
    (UUID(), @eeq2, 5, 'Strongly Agree', 17, 17.0, NOW(), NOW());

INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @eeq3, 1, 'Strongly Disagree', 12, 12.0, NOW(), NOW()),
    (UUID(), @eeq3, 2, 'Disagree', 18, 18.0, NOW(), NOW()),
    (UUID(), @eeq3, 3, 'Neutral', 22, 22.0, NOW(), NOW()),
    (UUID(), @eeq3, 4, 'Agree', 35, 35.0, NOW(), NOW()),
    (UUID(), @eeq3, 5, 'Strongly Agree', 13, 13.0, NOW(), NOW());

INSERT INTO Responses (Id, QuestionId, RespValue, Label, Frequency, Percentage, CreatedDate, ModifiedDate)
VALUES
    (UUID(), @eeq4, 1, 'Strongly Disagree', 7, 7.0, NOW(), NOW()),
    (UUID(), @eeq4, 2, 'Disagree', 11, 11.0, NOW(), NOW()),
    (UUID(), @eeq4, 3, 'Neutral', 17, 17.0, NOW(), NOW()),
    (UUID(), @eeq4, 4, 'Agree', 45, 45.0, NOW(), NOW()),
    (UUID(), @eeq4, 5, 'Strongly Agree', 20, 20.0, NOW(), NOW());

-- Verify data
SELECT 
    p.ProjectName,
    COUNT(DISTINCT s.Id) AS SurveyCount,
    COUNT(DISTINCT q.Id) AS QuestionCount,
    COUNT(DISTINCT r.Id) AS ResponseCount
FROM Projects p
LEFT JOIN Surveys s ON p.Id = s.ProjectId
LEFT JOIN Questions q ON s.Id = q.SurveyId
LEFT JOIN Responses r ON q.Id = r.QuestionId
WHERE p.TenantId = 1
GROUP BY p.ProjectName
ORDER BY p.ProjectName;

-- Clear all existing data first
SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE SurveyData;
TRUNCATE TABLE Responses;
TRUNCATE TABLE Questions;
TRUNCATE TABLE Surveys;
TRUNCATE TABLE Projects;
SET FOREIGN_KEY_CHECKS = 1;

-- Insert Projects
INSERT INTO Projects (Id, TenantId, ProjectName, ClientName, StartDate, EndDate, ProjectFile, CreatedDate, ModifiedDate)
VALUES
    -- Multi-survey project: Brand Tracker
    (UUID(), 1, 'Brand Tracker Q4 2024', 'Acme Corporation', 
     'Quarterly brand health tracking study measuring awareness, consideration, and perception across key demographics.',
     '2024-10-01', '2024-12-31', 'BrandTracker_Q4_2024.porp', NOW(), NOW()),
    
    -- Multi-survey project: Product Launch Research
    (UUID(), 1, 'Product Launch Research', 'TechStart Inc', 
     'Pre and post-launch research for new product line including concept testing and market response.',
     '2024-09-15', '2024-11-30', 'ProductLaunch_2024.porp', NOW(), NOW()),
    
    -- Standalone survey projects
    (UUID(), 1, 'Customer Satisfaction Survey', 'Retail Plus', 
     'Annual customer satisfaction measurement focusing on service quality and loyalty.',
     '2024-11-01', '2024-11-15', 'CustomerSat_2024.porp', NOW(), NOW()),
    
    (UUID(), 1, 'Employee Engagement Survey', 'Global Services Ltd', 
     'Bi-annual employee engagement survey measuring job satisfaction, management effectiveness, and workplace culture.',
     '2024-11-10', '2024-11-25', 'EmployeeEngagement_Fall2024.porp', NOW(), NOW());

-- Get Project IDs for reference
SET @brandTrackerId = (SELECT Id FROM Projects WHERE ProjectName = 'Brand Tracker Q4 2024' LIMIT 1);
SET @productLaunchId = (SELECT Id FROM Projects WHERE ProjectName = 'Product Launch Research' LIMIT 1);
SET @customerSatId = (SELECT Id FROM Projects WHERE ProjectName = 'Customer Satisfaction Survey' LIMIT 1);
SET @employeeEngId = (SELECT Id FROM Projects WHERE ProjectName = 'Employee Engagement Survey' LIMIT 1);

-- Insert Surveys
INSERT INTO Surveys (Id, TenantId, ProjectId, SurveyName CreatedDate, ModifiedDate)
VALUES
    -- Brand Tracker surveys (4 waves)
    (UUID(), 1, @brandTrackerId, 'Brand Tracker Wave 1 - October', '2024-10-05', '2024-10-05'),
    (UUID(), 1, @brandTrackerId, 'Brand Tracker Wave 2 - October', '2024-10-20', '2024-10-20'),
    (UUID(), 1, @brandTrackerId, 'Brand Tracker Wave 3 - November', '2024-11-05', '2024-11-05'),
    (UUID(), 1, @brandTrackerId, 'Brand Tracker Wave 4 - November', '2024-11-20', '2024-11-20'),
    
    -- Product Launch surveys (2)
    (UUID(), 1, @productLaunchId, 'Pre-Launch Concept Test', '2024-09-20', '2024-09-20'),
    (UUID(), 1, @productLaunchId, 'Post-Launch Market Response', '2024-11-15', '2024-11-15'),
    
    -- Standalone surveys
    (UUID(), 1, @customerSatId, 'Customer Satisfaction Survey', '2024-11-05', '2024-11-05'),
    (UUID(), 1, @employeeEngId, 'Employee Engagement Survey', '2024-11-12', '2024-11-12');

-- Get Survey IDs
SET @brandWave1 = (SELECT Id FROM Surveys WHERE SurveyName = 'Brand Tracker Wave 1 - October' LIMIT 1);
SET @brandWave2 = (SELECT Id FROM Surveys WHERE SurveyName = 'Brand Tracker Wave 2 - October' LIMIT 1);
SET @brandWave3 = (SELECT Id FROM Surveys WHERE SurveyName = 'Brand Tracker Wave 3 - November' LIMIT 1);
SET @brandWave4 = (SELECT Id FROM Surveys WHERE SurveyName = 'Brand Tracker Wave 4 - November' LIMIT 1);
SET @preLaunch = (SELECT Id FROM Surveys WHERE SurveyName = 'Pre-Launch Concept Test' LIMIT 1);
SET @postLaunch = (SELECT Id FROM Surveys WHERE SurveyName = 'Post-Launch Market Response' LIMIT 1);
SET @custSat = (SELECT Id FROM Surveys WHERE SurveyName = 'Customer Satisfaction Survey' LIMIT 1);
SET @empEng = (SELECT Id FROM Surveys WHERE SurveyName = 'Employee Engagement Survey' LIMIT 1);

-- ==================== QUESTIONS ====================

-- Brand Tracker Wave 1 Questions
INSERT INTO Questions (Id, TenantId, SurveyId, QuestionText, QuestionType, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @brandWave1, 'Which of the following brands are you aware of?', 'MultipleChoice', 1, NOW(), NOW()),
    (UUID(), 1, @brandWave1, 'Which brand would you most likely purchase?', 'SingleChoice', 2, NOW(), NOW()),
    (UUID(), 1, @brandWave1, 'How would you rate the quality of [Brand]?', 0, 3, NOW(), NOW()),
    (UUID(), 1, @brandWave1, 'What words come to mind when you think of [Brand]?', 'OpenEnded', 4, NOW(), NOW());

-- Brand Tracker Wave 2-4 use same questions (abbreviated for brevity)
INSERT INTO Questions (Id, TenantId, SurveyId, QuestionText, QuestionType, SortOrder, CreatedDate, ModifiedDate)
SELECT UUID(), 1, @brandWave2, QuestionText, QuestionType, SortOrder, NOW(), NOW()
FROM Questions WHERE SurveyId = @brandWave1;

INSERT INTO Questions (Id, TenantId, SurveyId, QuestionText, QuestionType, SortOrder, CreatedDate, ModifiedDate)
SELECT UUID(), 1, @brandWave3, QuestionText, QuestionType, SortOrder, NOW(), NOW()
FROM Questions WHERE SurveyId = @brandWave1;

INSERT INTO Questions (Id, TenantId, SurveyId, QuestionText, QuestionType, SortOrder, CreatedDate, ModifiedDate)
SELECT UUID(), 1, @brandWave4, QuestionText, QuestionType, SortOrder, NOW(), NOW()
FROM Questions WHERE SurveyId = @brandWave1;

-- Pre-Launch Concept Test Questions
INSERT INTO Questions (Id, TenantId, SurveyId, QuestionText, QuestionType, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @preLaunch, 'How appealing is this product concept?', 0, 1, NOW(), NOW()),
    (UUID(), 1, @preLaunch, 'How likely are you to purchase this product?', 0, 2, NOW(), NOW()),
    (UUID(), 1, @preLaunch, 'What price would you expect to pay?', 'OpenEnded', 3, NOW(), NOW()),
    (UUID(), 1, @preLaunch, 'What features are most important to you?', 'MultipleChoice', 4, NOW(), NOW()),
    (UUID(), 1, @preLaunch, 'What improvements would you suggest?', 'OpenEnded', 5, NOW(), NOW());

-- Post-Launch Market Response Questions
INSERT INTO Questions (Id, TenantId, SurveyId, QuestionText, QuestionType, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @postLaunch, 'Have you purchased this product?', 'SingleChoice', 1, NOW(), NOW()),
    (UUID(), 1, @postLaunch, 'How satisfied are you with the product?', 0, 2, NOW(), NOW()),
    (UUID(), 1, @postLaunch, 'How does it compare to alternatives?', 0, 3, NOW(), NOW()),
    (UUID(), 1, @postLaunch, 'Would you recommend this product?', 0, 4, NOW(), NOW()),
    (UUID(), 1, @postLaunch, 'What do you like most about it?', 'OpenEnded', 5, NOW(), NOW());

-- Customer Satisfaction Survey Questions
INSERT INTO Questions (Id, TenantId, SurveyId, QuestionText, QuestionType, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @custSat, 'How satisfied are you with our service?', 0, 1, NOW(), NOW()),
    (UUID(), 1, @custSat, 'How likely are you to recommend us?', 0, 2, NOW(), NOW()),
    (UUID(), 1, @custSat, 'How responsive was our staff?', 0, 3, NOW(), NOW()),
    (UUID(), 1, @custSat, 'What could we improve?', 'OpenEnded', 4, NOW(), NOW()),
    (UUID(), 1, @custSat, 'How long have you been a customer?', 'SingleChoice', 5, NOW(), NOW());

-- Employee Engagement Survey Questions
INSERT INTO Questions (Id, TenantId, SurveyId, QuestionText, QuestionType, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @empEng, 'I am satisfied with my job', 0, 1, NOW(), NOW()),
    (UUID(), 1, @empEng, 'My manager supports my development', 0, 2, NOW(), NOW()),
    (UUID(), 1, @empEng, 'I have the tools I need to succeed', 0, 3, NOW(), NOW()),
    (UUID(), 1, @empEng, 'I would recommend this company as a great place to work', 0, 4, NOW(), NOW()),
    (UUID(), 1, @empEng, 'What would improve your work experience?', 'OpenEnded', 5, NOW(), NOW());

-- ==================== RESPONSES ====================

-- Get Question IDs for Brand Wave 1
SET @bw1q1 = (SELECT Id FROM Questions WHERE SurveyId = @brandWave1 AND QuestionText LIKE 'Which of the following%' LIMIT 1);
SET @bw1q2 = (SELECT Id FROM Questions WHERE SurveyId = @brandWave1 AND QuestionText LIKE 'Which brand would%' LIMIT 1);
SET @bw1q3 = (SELECT Id FROM Questions WHERE SurveyId = @brandWave1 AND QuestionText LIKE 'How would you rate%' LIMIT 1);

-- Brand Awareness Responses
INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @bw1q1, 'Brand A', '1', 1, NOW(), NOW()),
    (UUID(), 1, @bw1q1, 'Brand B', '2', 2, NOW(), NOW()),
    (UUID(), 1, @bw1q1, 'Brand C', '3', 3, NOW(), NOW()),
    (UUID(), 1, @bw1q1, 'Brand D', '4', 4, NOW(), NOW()),
    (UUID(), 1, @bw1q1, 'Brand E', '5', 5, NOW(), NOW());

-- Purchase Intent Responses
INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @bw1q2, 'Brand A', '1', 1, NOW(), NOW()),
    (UUID(), 1, @bw1q2, 'Brand B', '2', 2, NOW(), NOW()),
    (UUID(), 1, @bw1q2, 'Brand C', '3', 3, NOW(), NOW()),
    (UUID(), 1, @bw1q2, 'Brand D', '4', 4, NOW(), NOW()),
    (UUID(), 1, @bw1q2, 'Brand E', '5', 5, NOW(), NOW());

-- Quality Rating Responses (Scale 1-5)
INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @bw1q3, 'Very Poor', '1', 1, NOW(), NOW()),
    (UUID(), 1, @bw1q3, 'Poor', '2', 2, NOW(), NOW()),
    (UUID(), 1, @bw1q3, 'Average', '3', 3, NOW(), NOW()),
    (UUID(), 1, @bw1q3, 'Good', '4', 4, NOW(), NOW()),
    (UUID(), 1, @bw1q3, 'Excellent', '5', 5, NOW(), NOW());

-- Get Question IDs for Customer Satisfaction
SET @csq1 = (SELECT Id FROM Questions WHERE SurveyId = @custSat AND QuestionText LIKE 'How satisfied%' LIMIT 1);
SET @csq2 = (SELECT Id FROM Questions WHERE SurveyId = @custSat AND QuestionText LIKE 'How likely%' LIMIT 1);
SET @csq3 = (SELECT Id FROM Questions WHERE SurveyId = @custSat AND QuestionText LIKE 'How responsive%' LIMIT 1);
SET @csq5 = (SELECT Id FROM Questions WHERE SurveyId = @custSat AND QuestionText LIKE 'How long have%' LIMIT 1);

-- Satisfaction Scale Responses
INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @csq1, 'Very Dissatisfied', '1', 1, NOW(), NOW()),
    (UUID(), 1, @csq1, 'Dissatisfied', '2', 2, NOW(), NOW()),
    (UUID(), 1, @csq1, 'Neutral', '3', 3, NOW(), NOW()),
    (UUID(), 1, @csq1, 'Satisfied', '4', 4, NOW(), NOW()),
    (UUID(), 1, @csq1, 'Very Satisfied', '5', 5, NOW(), NOW());

-- NPS Scale Responses
INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @csq2, '0 - Not at all likely', '0', 1, NOW(), NOW()),
    (UUID(), 1, @csq2, '1', '1', 2, NOW(), NOW()),
    (UUID(), 1, @csq2, '2', '2', 3, NOW(), NOW()),
    (UUID(), 1, @csq2, '3', '3', 4, NOW(), NOW()),
    (UUID(), 1, @csq2, '4', '4', 5, NOW(), NOW()),
    (UUID(), 1, @csq2, '5', '5', 6, NOW(), NOW()),
    (UUID(), 1, @csq2, '6', '6', 7, NOW(), NOW()),
    (UUID(), 1, @csq2, '7', '7', 8, NOW(), NOW()),
    (UUID(), 1, @csq2, '8', '8', 9, NOW(), NOW()),
    (UUID(), 1, @csq2, '9', '9', 10, NOW(), NOW()),
    (UUID(), 1, @csq2, '10 - Extremely likely', '10', 11, NOW(), NOW());

-- Responsiveness Responses
INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @csq3, 'Very Poor', '1', 1, NOW(), NOW()),
    (UUID(), 1, @csq3, 'Poor', '2', 2, NOW(), NOW()),
    (UUID(), 1, @csq3, 'Average', '3', 3, NOW(), NOW()),
    (UUID(), 1, @csq3, 'Good', '4', 4, NOW(), NOW()),
    (UUID(), 1, @csq3, 'Excellent', '5', 5, NOW(), NOW());

-- Customer Tenure Responses
INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
VALUES
    (UUID(), 1, @csq5, 'Less than 6 months', '1', 1, NOW(), NOW()),
    (UUID(), 1, @csq5, '6-12 months', '2', 2, NOW(), NOW()),
    (UUID(), 1, @csq5, '1-2 years', '3', 3, NOW(), NOW()),
    (UUID(), 1, @csq5, '2-5 years', '4', 4, NOW(), NOW()),
    (UUID(), 1, @csq5, '5+ years', '5', 5, NOW(), NOW());

-- Get Question IDs for Employee Engagement
SET @eeq1 = (SELECT Id FROM Questions WHERE SurveyId = @empEng AND QuestionText LIKE 'I am satisfied%' LIMIT 1);
SET @eeq2 = (SELECT Id FROM Questions WHERE SurveyId = @empEng AND QuestionText LIKE 'My manager%' LIMIT 1);
SET @eeq3 = (SELECT Id FROM Questions WHERE SurveyId = @empEng AND QuestionText LIKE 'I have the tools%' LIMIT 1);
SET @eeq4 = (SELECT Id FROM Questions WHERE SurveyId = @empEng AND QuestionText LIKE 'I would recommend%' LIMIT 1);

-- Agreement Scale Responses for Employee Engagement
INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
SELECT UUID(), 1, @eeq1, ResponseText, ResponseValue, SortOrder, NOW(), NOW()
FROM (
    SELECT 'Strongly Disagree' AS ResponseText, '1' AS ResponseValue, 1 AS SortOrder
    UNION SELECT 'Disagree', '2', 2
    UNION SELECT 'Neutral', '3', 3
    UNION SELECT 'Agree', '4', 4
    UNION SELECT 'Strongly Agree', '5', 5
) AS scale_responses;

INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
SELECT UUID(), 1, @eeq2, ResponseText, ResponseValue, SortOrder, NOW(), NOW()
FROM (
    SELECT 'Strongly Disagree' AS ResponseText, '1' AS ResponseValue, 1 AS SortOrder
    UNION SELECT 'Disagree', '2', 2
    UNION SELECT 'Neutral', '3', 3
    UNION SELECT 'Agree', '4', 4
    UNION SELECT 'Strongly Agree', '5', 5
) AS scale_responses;

INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
SELECT UUID(), 1, @eeq3, ResponseText, ResponseValue, SortOrder, NOW(), NOW()
FROM (
    SELECT 'Strongly Disagree' AS ResponseText, '1' AS ResponseValue, 1 AS SortOrder
    UNION SELECT 'Disagree', '2', 2
    UNION SELECT 'Neutral', '3', 3
    UNION SELECT 'Agree', '4', 4
    UNION SELECT 'Strongly Agree', '5', 5
) AS scale_responses;

INSERT INTO Responses (Id, TenantId, QuestionId, ResponseText, ResponseValue, SortOrder, CreatedDate, ModifiedDate)
SELECT UUID(), 1, @eeq4, ResponseText, ResponseValue, SortOrder, NOW(), NOW()
FROM (
    SELECT 'Strongly Disagree' AS ResponseText, '1' AS ResponseValue, 1 AS SortOrder
    UNION SELECT 'Disagree', '2', 2
    UNION SELECT 'Neutral', '3', 3
    UNION SELECT 'Agree', '4', 4
    UNION SELECT 'Strongly Agree', '5', 5
) AS scale_responses;

-- ==================== SURVEY DATA (Sample Respondent Answers) ====================

-- Brand Tracker Wave 1 - 50 respondents
INSERT INTO SurveyData (Id, TenantId, SurveyId, RespondentId, QuestionId, AnswerText, AnswerValue, CreatedDate)
SELECT 
    UUID(),
    1,
    @brandWave1,
    CONCAT('R', LPAD(n, 3, '0')),
    @bw1q1,
    CASE WHEN RAND() < 0.8 THEN '1' WHEN RAND() < 0.6 THEN '2' WHEN RAND() < 0.4 THEN '3' ELSE '4' END,
    CASE WHEN RAND() < 0.8 THEN '1' WHEN RAND() < 0.6 THEN '2' WHEN RAND() < 0.4 THEN '3' ELSE '4' END,
    NOW()
FROM (
    SELECT 1 n UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5
    UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9 UNION SELECT 10
    UNION SELECT 11 UNION SELECT 12 UNION SELECT 13 UNION SELECT 14 UNION SELECT 15
    UNION SELECT 16 UNION SELECT 17 UNION SELECT 18 UNION SELECT 19 UNION SELECT 20
    UNION SELECT 21 UNION SELECT 22 UNION SELECT 23 UNION SELECT 24 UNION SELECT 25
    UNION SELECT 26 UNION SELECT 27 UNION SELECT 28 UNION SELECT 29 UNION SELECT 30
    UNION SELECT 31 UNION SELECT 32 UNION SELECT 33 UNION SELECT 34 UNION SELECT 35
    UNION SELECT 36 UNION SELECT 37 UNION SELECT 38 UNION SELECT 39 UNION SELECT 40
    UNION SELECT 41 UNION SELECT 42 UNION SELECT 43 UNION SELECT 44 UNION SELECT 45
    UNION SELECT 46 UNION SELECT 47 UNION SELECT 48 UNION SELECT 49 UNION SELECT 50
) numbers;

-- Customer Satisfaction - 100 respondents
INSERT INTO SurveyData (Id, TenantId, SurveyId, RespondentId, QuestionId, AnswerText, AnswerValue, CreatedDate)
SELECT 
    UUID(),
    1,
    @custSat,
    CONCAT('CS', LPAD(n, 3, '0')),
    @csq1,
    CASE 
        WHEN RAND() < 0.1 THEN 'Very Dissatisfied'
        WHEN RAND() < 0.2 THEN 'Dissatisfied'
        WHEN RAND() < 0.4 THEN 'Neutral'
        WHEN RAND() < 0.7 THEN 'Satisfied'
        ELSE 'Very Satisfied'
    END,
    CASE 
        WHEN RAND() < 0.1 THEN '1'
        WHEN RAND() < 0.2 THEN '2'
        WHEN RAND() < 0.4 THEN '3'
        WHEN RAND() < 0.7 THEN '4'
        ELSE '5'
    END,
    NOW()
FROM (
    SELECT 1 n UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5
    UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9 UNION SELECT 10
    UNION SELECT 11 UNION SELECT 12 UNION SELECT 13 UNION SELECT 14 UNION SELECT 15
    UNION SELECT 16 UNION SELECT 17 UNION SELECT 18 UNION SELECT 19 UNION SELECT 20
    UNION SELECT 21 UNION SELECT 22 UNION SELECT 23 UNION SELECT 24 UNION SELECT 25
    UNION SELECT 26 UNION SELECT 27 UNION SELECT 28 UNION SELECT 29 UNION SELECT 30
    UNION SELECT 31 UNION SELECT 32 UNION SELECT 33 UNION SELECT 34 UNION SELECT 35
    UNION SELECT 36 UNION SELECT 37 UNION SELECT 38 UNION SELECT 39 UNION SELECT 40
    UNION SELECT 41 UNION SELECT 42 UNION SELECT 43 UNION SELECT 44 UNION SELECT 45
    UNION SELECT 46 UNION SELECT 47 UNION SELECT 48 UNION SELECT 49 UNION SELECT 50
    UNION SELECT 51 UNION SELECT 52 UNION SELECT 53 UNION SELECT 54 UNION SELECT 55
    UNION SELECT 56 UNION SELECT 57 UNION SELECT 58 UNION SELECT 59 UNION SELECT 60
    UNION SELECT 61 UNION SELECT 62 UNION SELECT 63 UNION SELECT 64 UNION SELECT 65
    UNION SELECT 66 UNION SELECT 67 UNION SELECT 68 UNION SELECT 69 UNION SELECT 70
    UNION SELECT 71 UNION SELECT 72 UNION SELECT 73 UNION SELECT 74 UNION SELECT 75
    UNION SELECT 76 UNION SELECT 77 UNION SELECT 78 UNION SELECT 79 UNION SELECT 80
    UNION SELECT 81 UNION SELECT 82 UNION SELECT 83 UNION SELECT 84 UNION SELECT 85
    UNION SELECT 86 UNION SELECT 87 UNION SELECT 88 UNION SELECT 89 UNION SELECT 90
    UNION SELECT 91 UNION SELECT 92 UNION SELECT 93 UNION SELECT 94 UNION SELECT 95
    UNION SELECT 96 UNION SELECT 97 UNION SELECT 98 UNION SELECT 99 UNION SELECT 100
) numbers;

-- Verify data
SELECT 
    p.ProjectName,
    COUNT(DISTINCT s.Id) AS SurveyCount,
    COUNT(DISTINCT q.Id) AS QuestionCount,
    COUNT(DISTINCT r.Id) AS ResponseCount,
    COUNT(sd.Id) AS SurveyDataCount
FROM Projects p
LEFT JOIN Surveys s ON p.Id = s.ProjectId
LEFT JOIN Questions q ON s.Id = q.SurveyId
LEFT JOIN Responses r ON q.Id = r.QuestionId
LEFT JOIN SurveyData sd ON s.Id = sd.SurveyId
WHERE p.TenantId = 1
GROUP BY p.ProjectName
ORDER BY p.ProjectName;
