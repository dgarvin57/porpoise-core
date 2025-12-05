-- Test Data: Sample projects and surveys
-- Date: 2025-11-30
-- Description: Create sample data to demonstrate standalone surveys and multi-survey projects

USE porpoise_dev;

-- Create demo tenant if not exists
INSERT IGNORE INTO Tenants (TenantId, TenantKey, Name, IsActive)
VALUES (1, 'demo-tenant', 'Demo Tenant', 1);

-- Project 1: Standalone survey (auto-generated project for single survey)
SET @project1Id = UUID();
INSERT INTO Projects (
    Id, TenantId, ProjectName, ClientName, Description,
    ResearcherLabel
) VALUES (
    @project1Id,
    1,
    'Customer Satisfaction 2024',
    'Acme Corporation',
    'Annual customer satisfaction survey',
    'Research Inc.'
);

INSERT INTO Surveys (
    Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes
) VALUES (
    UUID(),
    @project1Id,
    1,
    'Customer Satisfaction 2024',
    2, -- Verified
    'Annual customer satisfaction tracking survey for all customers'
);

-- Project 2: Brand Tracker (Multi-wave project)
SET @project2Id = UUID();
INSERT INTO Projects (
    Id, TenantId, ProjectName, ClientName, Description,
    StartDate, EndDate, DefaultWeightingScheme,
    ResearcherLabel
) VALUES (
    @project2Id,
    1,
    'Brand Tracker 2024',
    'Global Brands Inc',
    'Quarterly brand awareness and perception tracking study',
    '2024-01-01',
    '2024-12-31',
    'PopulationWeight',
    'Market Research Partners'
);

-- Add 4 quarterly waves
INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES 
    (UUID(), @project2Id, 1, 'Q1 2024 - January', 2, 'First quarter wave - post-holiday season'),
    (UUID(), @project2Id, 1, 'Q2 2024 - April', 2, 'Second quarter wave - spring campaign'),
    (UUID(), @project2Id, 1, 'Q3 2024 - July', 2, 'Third quarter wave - summer season'),
    (UUID(), @project2Id, 1, 'Q4 2024 - October', 1, 'Fourth quarter wave - in progress');

-- Project 3: Product Launch Research (Pre/Post study)
SET @project3Id = UUID();
INSERT INTO Projects (
    Id, TenantId, ProjectName, ClientName, Description,
    StartDate, EndDate,
    ResearcherLabel
) VALUES (
    @project3Id,
    1,
    'New Product Launch 2024',
    'Tech Innovations Ltd',
    'Pre and post-launch research for new smartphone',
    '2024-06-01',
    '2024-09-30',
    'Product Insights Group'
);

INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes)
VALUES 
    (UUID(), @project3Id, 1, 'Pre-Launch Survey', 2, 'Baseline awareness and interest study'),
    (UUID(), @project3Id, 1, 'Post-Launch Survey', 2, 'Follow-up after 3 months in market');

-- Project 4: Another standalone survey
SET @project4Id = UUID();
INSERT INTO Projects (
    Id, TenantId, ProjectName, ClientName, Description,
    ResearcherLabel
) VALUES (
    @project4Id,
    1,
    'Employee Engagement 2024',
    'Acme Corporation',
    'Annual employee engagement and satisfaction survey',
    'HR Analytics'
);

INSERT INTO Surveys (
    Id, ProjectId, TenantId, SurveyName, Status, SurveyNotes
) VALUES (
    UUID(),
    @project4Id,
    1,
    'Employee Engagement 2024',
    2,
    'Company-wide employee satisfaction and engagement survey'
);

-- Verify the data
SELECT 
    p.ProjectName,
    p.ClientName,
    COUNT(s.Id) as SurveyCount
FROM Projects p
LEFT JOIN Surveys s ON p.Id = s.ProjectId
WHERE p.TenantId = 1
GROUP BY p.Id, p.ProjectName, p.ClientName
ORDER BY p.ProjectName;
