-- Drop existing tables (order matters due to foreign keys)
DROP TABLE IF EXISTS SurveyData;
DROP TABLE IF EXISTS SurveyResponses;
DROP TABLE IF EXISTS Responses;
DROP TABLE IF EXISTS Questions;
DROP TABLE IF EXISTS Surveys;

-- Create Surveys table matching C# Survey model
CREATE TABLE Surveys (
    Id CHAR(36) PRIMARY KEY,
    SurveyName VARCHAR(255) NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    LockStatus INT NOT NULL DEFAULT 0,
    UnlockKeyName VARCHAR(255),
    UnlockKeyType INT NOT NULL DEFAULT 0,
    SaveAlteredString VARCHAR(500),
    SurveyFileName VARCHAR(255),
    DataFileName VARCHAR(255),
    OrigDataFilePath VARCHAR(500),
    SurveyPath VARCHAR(500),
    SurveyFolder VARCHAR(500),
    FullProjectFolder VARCHAR(500),
    ErrorsExist BOOLEAN NOT NULL DEFAULT TRUE,
    SurveyNotes TEXT,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    INDEX idx_survey_name (SurveyName),
    INDEX idx_status (Status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create Questions table
CREATE TABLE Questions (
    Id CHAR(36) PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    QstNumber VARCHAR(50) NOT NULL,
    QstLabel VARCHAR(255),
    DataFileColumn INT,
    VariableType VARCHAR(50),
    MissingLow DOUBLE,
    MissingHigh DOUBLE,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE,
    INDEX idx_survey_id (SurveyId),
    INDEX idx_qst_number (QstNumber),
    INDEX idx_variable_type (VariableType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create Responses table
CREATE TABLE Responses (
    Id CHAR(36) PRIMARY KEY,
    QuestionId CHAR(36) NOT NULL,
    RespValue INT NOT NULL,
    Label VARCHAR(500),
    Percentage DECIMAL(10,4),
    Frequency INT,
    IndexType VARCHAR(50),
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id) ON DELETE CASCADE,
    INDEX idx_question_id (QuestionId),
    INDEX idx_resp_value (RespValue),
    INDEX idx_index_type (IndexType)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create SurveyResponses table (for response tracking - used in GetResponseCountAsync)
CREATE TABLE SurveyResponses (
    Id CHAR(36) PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    ResponseData JSON,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE,
    INDEX idx_survey_id (SurveyId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create SurveyData table
CREATE TABLE SurveyData (
    Id CHAR(36) PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    DataFilePath VARCHAR(500),
    DataList LONGTEXT,
    MissingResponseValues JSON,
    CreatedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ModifiedDate DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id) ON DELETE CASCADE,
    INDEX idx_survey_id (SurveyId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
