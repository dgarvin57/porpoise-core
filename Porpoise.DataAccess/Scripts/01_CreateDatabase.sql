-- Create Porpoise database
CREATE DATABASE IF NOT EXISTS porpoise_dev
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE porpoise_dev;

-- Surveys table
CREATE TABLE IF NOT EXISTS surveys (
    id CHAR(36) PRIMARY KEY,
    survey_name VARCHAR(255) NOT NULL,
    status VARCHAR(50) NOT NULL,
    survey_file_name VARCHAR(255),
    data_file_name VARCHAR(255),
    survey_path VARCHAR(500),
    survey_folder VARCHAR(500),
    survey_notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_survey_name (survey_name),
    INDEX idx_status (status)
) ENGINE=InnoDB;

-- Questions table
CREATE TABLE IF NOT EXISTS questions (
    id CHAR(36) PRIMARY KEY,
    survey_id CHAR(36) NOT NULL,
    qst_number VARCHAR(50),
    qst_label VARCHAR(255),
    qst_stem TEXT,
    data_file_col SMALLINT,
    column_filled BOOLEAN DEFAULT TRUE,
    variable_type VARCHAR(20),
    data_type VARCHAR(20),
    blk_qst_status VARCHAR(50),
    blk_label VARCHAR(255),
    blk_stem TEXT,
    miss_value_1 VARCHAR(50),
    miss_value_2 VARCHAR(50),
    miss_value_3 VARCHAR(50),
    total_index INT,
    total_n INT,
    selected BOOLEAN DEFAULT FALSE,
    weight_on BOOLEAN DEFAULT FALSE,
    question_notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (survey_id) REFERENCES surveys(id) ON DELETE CASCADE,
    INDEX idx_survey_id (survey_id),
    INDEX idx_qst_number (qst_number),
    INDEX idx_variable_type (variable_type)
) ENGINE=InnoDB;

-- Responses table
CREATE TABLE IF NOT EXISTS responses (
    id CHAR(36) PRIMARY KEY,
    question_id CHAR(36) NOT NULL,
    resp_value INT NOT NULL,
    label VARCHAR(255),
    result_percent DECIMAL(10, 4),
    cumulative_percent DECIMAL(10, 4),
    result_frequency INT,
    unweighted_frequency INT,
    index_type VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (question_id) REFERENCES questions(id) ON DELETE CASCADE,
    INDEX idx_question_id (question_id),
    INDEX idx_resp_value (resp_value)
) ENGINE=InnoDB;

-- Survey data table (for raw survey data storage)
CREATE TABLE IF NOT EXISTS survey_data (
    id CHAR(36) PRIMARY KEY,
    survey_id CHAR(36) NOT NULL,
    data_file_path VARCHAR(500),
    data_list LONGTEXT, -- JSON array of arrays
    missing_response_values JSON,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (survey_id) REFERENCES surveys(id) ON DELETE CASCADE,
    INDEX idx_survey_id (survey_id)
) ENGINE=InnoDB;
