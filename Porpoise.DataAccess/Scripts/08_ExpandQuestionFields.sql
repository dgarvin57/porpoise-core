-- Migration 08: Expand Questions table with all Question model fields
-- This migration adds missing fields from the Question model that are needed for full feature parity with PC version

USE porpoise_dev;

-- Add new columns to Questions table
ALTER TABLE Questions
    -- Question text/stem
    ADD COLUMN QstStem TEXT AFTER QstLabel,
    
    -- Missing values (replace MissingLow/MissingHigh range with discrete values)
    ADD COLUMN MissValue1 VARCHAR(10) AFTER VariableType,
    ADD COLUMN MissValue2 VARCHAR(10) AFTER MissValue1,
    ADD COLUMN MissValue3 VARCHAR(10) AFTER MissValue2,
    
    -- Data type classification (Nominal=0, Ordinal=1, Interval=2, Ratio=3)
    ADD COLUMN DataType INT AFTER MissValue3,
    
    -- Column state
    ADD COLUMN ColumnFilled BOOLEAN DEFAULT FALSE AFTER DataFileColumn,
    
    -- Block question fields
    ADD COLUMN BlkQstStatus INT AFTER DataType,
    ADD COLUMN BlkLabel VARCHAR(255) AFTER BlkQstStatus,
    ADD COLUMN BlkStem TEXT AFTER BlkLabel,
    
    -- Preference block fields
    ADD COLUMN IsPreferenceBlock BOOLEAN DEFAULT FALSE AFTER BlkStem,
    ADD COLUMN IsPreferenceBlockType BOOLEAN DEFAULT FALSE AFTER IsPreferenceBlock,
    ADD COLUMN NumberOfPreferenceItems INT DEFAULT 0 AFTER IsPreferenceBlockType,
    ADD COLUMN PreserveDifferentResponsesInPreferenceBlock BOOLEAN AFTER NumberOfPreferenceItems,
    
    -- Statistics fields
    ADD COLUMN TotalIndex INT DEFAULT 0 AFTER PreserveDifferentResponsesInPreferenceBlock,
    ADD COLUMN TotalN INT DEFAULT 0 AFTER TotalIndex,
    
    -- UI state
    ADD COLUMN WeightOn BOOLEAN DEFAULT FALSE AFTER TotalN,
    ADD COLUMN QuestionNotes TEXT AFTER WeightOn,
    
    -- Alternate labels
    ADD COLUMN UseAlternatePosNegLabels BOOLEAN DEFAULT FALSE AFTER QuestionNotes,
    ADD COLUMN AlternatePosLabel VARCHAR(255) AFTER UseAlternatePosNegLabels,
    ADD COLUMN AlternateNegLabel VARCHAR(255) AFTER AlternatePosLabel;

-- Add indexes for new searchable fields
CREATE INDEX idx_data_type ON Questions(DataType);
CREATE INDEX idx_variable_type_data_type ON Questions(VariableType, DataType);
CREATE INDEX idx_is_preference_block ON Questions(IsPreferenceBlock);

-- Note: MissingLow and MissingHigh columns are kept for backward compatibility but should not be used
-- The application should use MissValue1, MissValue2, MissValue3 instead
-- Future migration can drop MissingLow and MissingHigh after data verification

-- Migrate existing MissingLow values to MissValue1 if they exist
UPDATE Questions 
SET MissValue1 = CAST(MissingLow AS CHAR(10))
WHERE MissingLow IS NOT NULL AND MissingLow > 0;

-- Update ModifiedDate for all affected rows
UPDATE Questions SET ModifiedDate = NOW();
