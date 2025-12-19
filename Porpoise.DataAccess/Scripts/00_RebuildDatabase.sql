-- =============================================================================
-- Porpoise Database Schema - Complete Rebuild
-- =============================================================================
-- This script creates a fresh database with the current schema.
-- It SUPERSEDES all previous migration scripts (01-09).
--
-- Version: 2.1
-- Date: December 2025
-- Changes included:
--   - Multi-tenant architecture with GUID-based TenantId (VARCHAR(36))
--   - Separated branding: ClientLogo (Projects) vs OrganizationLogo (Tenants)
--   - Normalized question blocks with proper relationships
--   - Complete survey data model with responses and analytics support
--   - Sample data for testing (5 projects, 8 surveys)
--
-- CAUTION: This script DROPS and RECREATES the entire database!
-- =============================================================================

DROP DATABASE IF EXISTS porpoise_dev;
CREATE DATABASE porpoise_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE porpoise_dev;

-- Table: Tenants (using GUID as primary key)
CREATE TABLE Tenants (
    TenantId VARCHAR(36) NOT NULL PRIMARY KEY,
    TenantKey VARCHAR(50) NOT NULL UNIQUE,
    Name VARCHAR(200) NOT NULL,
    IsActive TINYINT(1) DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    OrganizationName VARCHAR(255) NULL,
    OrganizationLogo LONGBLOB NULL,
    OrganizationTagline VARCHAR(500) NULL,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CreatedBy VARCHAR(100) NULL,
    ModifiedBy VARCHAR(100) NULL,
    INDEX idx_tenants_key (TenantKey)
) ENGINE=InnoDB;

-- Table: Projects
CREATE TABLE Projects (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    TenantId VARCHAR(36) NOT NULL,
    ProjectName VARCHAR(255) NOT NULL,
    ClientName VARCHAR(255) NULL,
    Description TEXT NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    DefaultWeightingScheme VARCHAR(100) NULL,
    ClientLogo LONGBLOB NULL,
    ProjectFile TEXT NULL,
    IsExported TINYINT(1) DEFAULT 0,
    IsDeleted TINYINT(1) DEFAULT 0,
    DeletedDate DATETIME NULL,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CreatedBy VARCHAR(100) NULL,
    ModifiedBy VARCHAR(100) NULL,
    INDEX idx_projects_tenantid (TenantId),
    INDEX idx_projects_name (ProjectName),
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Table: Surveys
CREATE TABLE Surveys (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    ProjectId CHAR(36) NULL,
    TenantId VARCHAR(36) NOT NULL,
    SurveyName VARCHAR(255) NOT NULL,
    Status INT DEFAULT 0,
    SurveyFileName VARCHAR(500) NULL,
    DataFileName VARCHAR(500) NULL,
    ErrorsExist TINYINT(1) DEFAULT 1,
    SurveyNotes TEXT NULL,
    IsDeleted TINYINT(1) DEFAULT 0,
    DeletedDate DATETIME NULL,
    LastAccessedDate DATETIME NULL,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    CreatedBy VARCHAR(100) NULL,
    ModifiedBy VARCHAR(100) NULL,
    INDEX idx_surveys_projectid (ProjectId),
    INDEX idx_surveys_tenantid (TenantId),
    INDEX idx_surveys_name (SurveyName),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Table: QuestionBlocks (renamed from Blocks for clarity)
CREATE TABLE QuestionBlocks (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    Label VARCHAR(255) NULL,
    Stem TEXT NULL,
    DisplayOrder INT DEFAULT 0,
    CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    INDEX idx_blocks_surveyid (SurveyId),
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Table: Questions
CREATE TABLE Questions (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    BlockId CHAR(36) NULL,
    QstNumber VARCHAR(50) NULL,
    QstLabel VARCHAR(255) NULL,
    QstStem TEXT NULL,
    DataFileColumn SMALLINT NULL,
    ColumnFilled TINYINT(1) DEFAULT 1,
    VariableType INT NULL,
    DataType INT NULL,
    MissValue1 VARCHAR(50) NULL,
    MissValue2 VARCHAR(50) NULL,
    MissValue3 VARCHAR(50) NULL,
    MissingLow DOUBLE DEFAULT 0,
    MissingHigh DOUBLE DEFAULT 0,
    BlkQstStatus INT NULL,
    IsPreferenceBlock TINYINT(1) DEFAULT 0,
    IsPreferenceBlockType TINYINT(1) DEFAULT 0,
    NumberOfPreferenceItems INT DEFAULT 0,
    PreserveDifferentResponsesInPreferenceBlock TINYINT(1) NULL,
    QuestionNotes TEXT NULL,
    UseAlternatePosNegLabels TINYINT(1) DEFAULT 0,
    AlternatePosLabel VARCHAR(100) NULL,
    AlternateNegLabel VARCHAR(100) NULL,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    INDEX idx_questions_surveyid (SurveyId),
    INDEX idx_questions_blockid (BlockId),
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE,
    FOREIGN KEY (BlockId) REFERENCES QuestionBlocks(Id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- Table: Responses
CREATE TABLE Responses (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    QuestionId CHAR(36) NOT NULL,
    RespValue INT NOT NULL,
    Label VARCHAR(255) NULL,
    IndexType VARCHAR(50) NULL,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    INDEX idx_responses_questionid (QuestionId),
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Table: SurveyData
CREATE TABLE SurveyData (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    DataFilePath VARCHAR(500) NULL,
    DataList LONGTEXT NULL,
    MissingResponseValues TEXT NULL,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    INDEX idx_surveydata_surveyid (SurveyId),
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- Insert Sample Data

-- Sample Tenant
INSERT INTO Tenants (TenantId, TenantKey, Name, IsActive, OrganizationName, OrganizationTagline, CreatedBy) 
VALUES 
    (UUID(), 'demo', 'Demo Organization', 1, 'Research & Analytics Corp', 'Insights that drive decisions', 'system');

-- Get the tenant ID for foreign keys
SET @tenantId = (SELECT TenantId FROM Tenants WHERE TenantKey = 'demo');

-- Sample Projects
INSERT INTO Projects (Id, TenantId, ProjectName, ClientName, Description, StartDate, EndDate, DefaultWeightingScheme, CreatedBy)
VALUES
    (UUID(), @tenantId, 'Customer Satisfaction 2024', 'Acme Corp', 'Annual customer satisfaction survey', '2024-01-01', '2024-12-31', 'Equal', 'system'),
    (UUID(), @tenantId, 'Employee Engagement Study', 'TechStart Inc', 'Q4 employee engagement assessment', '2024-10-01', '2024-12-31', 'Equal', 'system'),
    (UUID(), @tenantId, 'Product Market Research', 'GlobalRetail Ltd', 'New product line market research', '2024-06-01', '2024-09-30', 'Weighted', 'system'),
    (UUID(), @tenantId, 'Brand Awareness Campaign', 'BrandCo', 'Pre and post campaign brand tracking', '2024-03-01', '2024-08-31', 'Equal', 'system'),
    (UUID(), @tenantId, 'Healthcare Quality Assessment', 'MediCare Hospital', 'Patient experience and quality measures', '2024-02-01', '2024-11-30', 'Equal', 'system');

-- Sample Surveys for first project
SET @project1Id = (SELECT Id FROM Projects WHERE ProjectName = 'Customer Satisfaction 2024' LIMIT 1);
SET @project2Id = (SELECT Id FROM Projects WHERE ProjectName = 'Employee Engagement Study' LIMIT 1);
SET @project3Id = (SELECT Id FROM Projects WHERE ProjectName = 'Product Market Research' LIMIT 1);

INSERT INTO Surveys (Id, ProjectId, TenantId, SurveyName, Status, CreatedBy)
VALUES
    (UUID(), @project1Id, @tenantId, 'Q1 Customer Survey', 1, 'system'),
    (UUID(), @project1Id, @tenantId, 'Q2 Customer Survey', 1, 'system'),
    (UUID(), @project1Id, @tenantId, 'Q3 Customer Survey', 0, 'system'),
    (UUID(), @project1Id, @tenantId, 'Q4 Customer Survey', 0, 'system'),
    (UUID(), @project2Id, @tenantId, 'Employee Engagement Wave 1', 1, 'system'),
    (UUID(), @project2Id, @tenantId, 'Employee Engagement Wave 2', 0, 'system'),
    (UUID(), @project3Id, @tenantId, 'Product Concept Test', 1, 'system'),
    (UUID(), @project3Id, @tenantId, 'Price Sensitivity Analysis', 0, 'system');

-- Sample Questions for first survey
SET @survey1Id = (SELECT Id FROM Surveys WHERE SurveyName = 'Q1 Customer Survey' LIMIT 1);

INSERT INTO Questions (Id, SurveyId, QstLabel, QstStem, DataFileColumn, VariableType, DataType)
VALUES
    (UUID(), @survey1Id, 'Q1', 'How satisfied are you with our product overall?', 1, 0, 0),
    (UUID(), @survey1Id, 'Q2', 'How likely are you to recommend us to a friend?', 2, 0, 0),
    (UUID(), @survey1Id, 'Q3', 'Which features do you use most often?', 3, 0, 1),
    (UUID(), @survey1Id, 'Q4', 'What improvements would you suggest?', 4, 0, 2);

-- Sample Responses for rating questions
SET @q1Id = (SELECT Id FROM Questions WHERE QstLabel = 'Q1' AND SurveyId = @survey1Id LIMIT 1);
SET @q2Id = (SELECT Id FROM Questions WHERE QstLabel = 'Q2' AND SurveyId = @survey1Id LIMIT 1);

INSERT INTO Responses (Id, QuestionId, Label, RespValue, IndexType)
VALUES
    -- Q1 Responses
    (UUID(), @q1Id, 'Very Satisfied', 5, 'Positive'),
    (UUID(), @q1Id, 'Satisfied', 4, 'Positive'),
    (UUID(), @q1Id, 'Neutral', 3, 'None'),
    (UUID(), @q1Id, 'Dissatisfied', 2, 'Negative'),
    (UUID(), @q1Id, 'Very Dissatisfied', 1, 'Negative'),
    -- Q2 Responses (NPS)
    (UUID(), @q2Id, '10', 10, 'Positive'),
    (UUID(), @q2Id, '9', 9, 'Positive'),
    (UUID(), @q2Id, '8', 8, 'Neutral'),
    (UUID(), @q2Id, '7', 7, 'Neutral'),
    (UUID(), @q2Id, '6', 6, 'Negative'),
    (UUID(), @q2Id, '5', 5, 'Negative'),
    (UUID(), @q2Id, '4', 4, 'Negative'),
    (UUID(), @q2Id, '3', 3, 'Negative'),
    (UUID(), @q2Id, '2', 2, 'Negative'),
    (UUID(), @q2Id, '1', 1, 'Negative'),
    (UUID(), @q2Id, '0', 0, 'Negative');

SELECT 'Database rebuilt successfully!' as Status;
SELECT CONCAT('Tenant ID: ', TenantId) as Info FROM Tenants WHERE TenantKey = 'demo';
SELECT CONCAT('Created ', COUNT(*), ' projects') as Projects FROM Projects;
SELECT CONCAT('Created ', COUNT(*), ' surveys') as Surveys FROM Surveys;
SELECT CONCAT('Created ', COUNT(*), ' questions') as Questions FROM Questions;
