-- Migration Script: Add Multi-Tenancy Support
-- Created: 2025-11-28
-- Purpose: Add Tenants table and TenantId columns to existing tables

USE porpoise_dev;

-- Drop existing tables to recreate with tenant support
SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS SurveyData;
DROP TABLE IF EXISTS Responses;
DROP TABLE IF EXISTS Questions;
DROP TABLE IF EXISTS Surveys;
DROP TABLE IF EXISTS Projects;
DROP TABLE IF EXISTS Tenants;
SET FOREIGN_KEY_CHECKS = 1;

-- Create Tenants table
CREATE TABLE Tenants (
    TenantId INT AUTO_INCREMENT PRIMARY KEY,
    TenantKey VARCHAR(50) UNIQUE NOT NULL,
    Name VARCHAR(200) NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_tenantkey (TenantKey)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert default tenant
INSERT INTO Tenants (TenantKey, Name, IsActive) 
VALUES ('demo-tenant', 'Demo Tenant', TRUE);

-- Recreate Projects table with TenantId
CREATE TABLE Projects (
    Id VARCHAR(36) PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectName VARCHAR(500),
    ClientName VARCHAR(500),
    ResearcherLabel VARCHAR(500),
    ResearcherSubLabel VARCHAR(500),
    ResearcherLogo TEXT,
    ResearcherLogoFilename VARCHAR(500),
    ResearcherLogoPath VARCHAR(1000),
    BaseProjectFolder VARCHAR(1000),
    ProjectFolder VARCHAR(1000),
    FullFolder VARCHAR(1000),
    FullPath VARCHAR(1000),
    FileName VARCHAR(500),
    IsExported BOOLEAN DEFAULT FALSE,
    CreatedBy VARCHAR(100),
    CreatedOn DATETIME,
    ModifiedBy VARCHAR(100),
    ModifiedOn DATETIME,
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE,
    INDEX idx_projects_tenant (TenantId),
    INDEX idx_projects_name (ProjectName)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Recreate Surveys table with TenantId
CREATE TABLE Surveys (
    Id VARCHAR(36) PRIMARY KEY,
    ProjectId VARCHAR(36),
    TenantId INT NOT NULL,
    SurveyName VARCHAR(500) NOT NULL,
    Status INT DEFAULT 0,
    LockStatus INT DEFAULT 0,
    UnlockKeyName VARCHAR(200),
    UnlockKeyType INT DEFAULT 0,
    SaveAlteredString VARCHAR(200),
    SurveyFileName VARCHAR(500),
    DataFileName VARCHAR(500),
    OrigDataFilePath VARCHAR(1000),
    SurveyPath VARCHAR(1000),
    SurveyFolder VARCHAR(1000),
    FullProjectFolder VARCHAR(1000),
    ErrorsExist BOOLEAN DEFAULT TRUE,
    SurveyNotes TEXT,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
    FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId) ON DELETE CASCADE,
    INDEX idx_surveys_tenant (TenantId),
    INDEX idx_surveys_project (ProjectId),
    INDEX idx_surveys_name (SurveyName)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Recreate Questions table (inherits tenant through Survey)
CREATE TABLE Questions (
    Id VARCHAR(36) PRIMARY KEY,
    SurveyId VARCHAR(36) NOT NULL,
    QstNumber VARCHAR(50) NOT NULL,
    QstLabel TEXT NOT NULL,
    DataFileColumn INT,
    VariableType INT,
    MissingLow DOUBLE DEFAULT 0,
    MissingHigh DOUBLE DEFAULT 0,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE,
    INDEX idx_questions_survey (SurveyId),
    INDEX idx_questions_number (QstNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Recreate Responses table (inherits tenant through Question->Survey)
CREATE TABLE Responses (
    Id VARCHAR(36) PRIMARY KEY,
    QuestionId VARCHAR(36) NOT NULL,
    RespValue INT NOT NULL,
    Label TEXT NOT NULL,
    Percentage DOUBLE DEFAULT 0,
    Frequency INT DEFAULT 0,
    IndexType VARCHAR(50),
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE,
    INDEX idx_responses_question (QuestionId),
    INDEX idx_responses_value (RespValue)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Recreate SurveyData table (inherits tenant through Survey)
CREATE TABLE SurveyData (
    Id VARCHAR(36) PRIMARY KEY,
    SurveyId VARCHAR(36) NOT NULL,
    DataFilePath VARCHAR(1000),
    DataList JSON,
    MissingResponseValues JSON,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE,
    INDEX idx_surveydata_survey (SurveyId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Verify the schema
SELECT 'Multi-tenancy schema created successfully' AS Status;
SELECT * FROM Tenants;
