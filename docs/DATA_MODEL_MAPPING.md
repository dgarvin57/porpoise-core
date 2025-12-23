# Data Model Mapping: Orca vs Porpoise Core

**Purpose:** Compare Orca's 2014 schema with current Porpoise Core to identify gaps and opportunities for integration.

**Date:** December 23, 2025

---

## Executive Summary

### ✅ Good News: Schema is Well Aligned

Porpoise Core **already supports** the core data structures needed:
- Question metadata (number, label, stem)
- Response definitions (value, label, index type)
- Block management (normalized in QuestionBlocks table)
- Missing value codes (multiple codes + range support)

### ⚠️ Key Differences

1. **Response Storage:** 
   - Orca: Stores unique values found in data with counts
   - Porpoise: Stores defined responses, calculates frequencies at analysis time

2. **Data Editing:**
   - Orca: Full in-memory dataset editing with recoding
   - Porpoise: Read-only data once imported

3. **Data Quality:**
   - Orca: Tracks string/null/decimal responses, % clean metric
   - Porpoise: Basic validation at import time

---

## Detailed Model Comparison

### 1. Question/Variable Definition

| Feature | Orca (OrcaVariableDef) | Porpoise Core (Question) | Status |
|---------|------------------------|--------------------------|--------|
| **Primary Key** | INTEGER (auto-increment) | GUID | ✅ Different but equivalent |
| **Question Number** | QuestionNumber (NVARCHAR 50) | QstNumber (string) | ✅ Supported |
| **Question Label** | QuestionLabel (NVARCHAR 50) | QstLabel (string) | ✅ Supported |
| **Question Stem** | ❌ Not in Orca model | QstStem (string) | ✅ Porpoise advantage |
| **Data File Column** | ❌ Not in Orca | DataFileCol (short) | ✅ Porpoise tracks |
| **Block Status** | BlockType (enum: None/First/Continuing) | BlkQstStatus (enum) | ✅ Supported |
| **Block Reference** | ❌ Embedded in question | BlockId (FK to QuestionBlocks) | ✅ Porpoise normalized |
| **Missing Values** | ❌ Not in Orca variable model | MissValue1/2/3 (string) | ✅ Porpoise supports 3 codes |
| **Missing Range** | ❌ Not supported | MissingLow/High (double) | ✅ Porpoise advantage |
| **Variable Type** | ❌ Not in Orca | VariableType (Independent/Dependent) | ✅ Porpoise for analysis |
| **Data Type** | ❌ Not in Orca | DataType (Nominal/Ordinal/Scale) | ✅ Porpoise for analysis |
| **Response Collection** | UniqueResponses (in-memory) | Responses (FK relationship) | ✅ Both support |
| **Metadata Tracking** | ResponsesLabeled, NumResponsesHaveString, etc. | ❌ Not tracked | ⚠️ Orca advantage |

**Orca SQL Schema:**
```sql
CREATE TABLE OrcaVariableDef (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ProjectId INTEGER NOT NULL,
    QuestionNumber NVARCHAR(50) NOT NULL,
    QuestionLabel NVARCHAR(50) NULL,
    BlockType INTEGER NOT NULL,
    -- Calculated fields (auto-generated from data):
    ResponsesLabeled INTEGER,
    NumResponsesHaveString INTEGER,
    NumResponsesHaveNull INTEGER,
    NumResponsesHaveDecimal INTEGER
)
```

**Porpoise Core SQL Schema:**
```sql
CREATE TABLE Questions (
    Id CHAR(36) PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    QstNumber VARCHAR(255),
    QstLabel VARCHAR(255),
    QstStem TEXT,
    DataFileColumn SMALLINT,
    ColumnFilled BOOLEAN,
    VariableType INT,
    DataType INT,
    MissValue1 VARCHAR(50),
    MissValue2 VARCHAR(50),
    MissValue3 VARCHAR(50),
    MissingLow DOUBLE,
    MissingHigh DOUBLE,
    BlkQstStatus INT,
    BlockId CHAR(36),  -- FK to QuestionBlocks
    IsPreferenceBlock BOOLEAN,
    IsPreferenceBlockType BOOLEAN,
    NumberOfPreferenceItems INT,
    PreserveDifferentResponsesInPreferenceBlock BOOLEAN,
    QuestionNotes TEXT,
    UseAlternatePosNegLabels BOOLEAN,
    AlternatePosLabel VARCHAR(255),
    AlternateNegLabel VARCHAR(255),
    CreatedDate DATETIME,
    ModifiedDate DATETIME,
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id),
    FOREIGN KEY (BlockId) REFERENCES QuestionBlocks(Id)
)
```

### 2. Response Definition

| Feature | Orca (OrcaResponseDef) | Porpoise Core (Response) | Status |
|---------|------------------------|--------------------------|--------|
| **Primary Key** | ❌ No explicit PK | Id (GUID) | ✅ Porpoise better |
| **Response Value** | RespValue (NVARCHAR 50) | RespValue (int) | ⚠️ Orca allows strings, Porpoise int only |
| **Response Label** | Label (NVARCHAR 50) | Label (string) | ✅ Supported |
| **Index Type** | ❌ Not in Orca | IndexType (+/-/Neutral/None) | ✅ Porpoise for analysis |
| **Data Type Flags** | ResponseIsString, ResponseIsNull, ResponseIsDecimal | ❌ Not tracked | ⚠️ Orca tracks data quality |
| **Labeled Status** | ResponseIsLabeled (bool) | ❌ Derived from Label.IsEmpty | ✅ Both support |
| **Frequency/Count** | ❌ Calculated from data | ResultFrequency (double) | ✅ Porpoise calculates |
| **Percentage** | ❌ Not stored | ResultPercent (decimal) | ✅ Porpoise calculates |
| **Weighting** | ❌ Not in Orca | Weight (double) | ✅ Porpoise advantage |

**Orca SQL Schema:**
```sql
-- Orca didn't have a separate ResponseDef table
-- Responses were stored as a collection in memory
-- Tracked as unique values found in the data
```

**Porpoise Core SQL Schema:**
```sql
CREATE TABLE Responses (
    Id CHAR(36) PRIMARY KEY,
    QuestionId CHAR(36) NOT NULL,
    RespValue INT NOT NULL,
    Label VARCHAR(255),
    IndexType VARCHAR(50),  -- 'Positive', 'Negative', 'Neutral', 'None'
    CreatedDate DATETIME,
    ModifiedDate DATETIME,
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id)
)
```

### 3. Question Blocks

| Feature | Orca | Porpoise Core (QuestionBlock) | Status |
|---------|------|-------------------------------|--------|
| **Separate Table** | ❌ No | ✅ Yes - QuestionBlocks | ✅ Porpoise normalized |
| **Block Label** | Embedded in Question.BlkLabel | Label (string) | ✅ Porpoise better architecture |
| **Block Stem** | Embedded in Question.BlkStem | Stem (string) | ✅ Porpoise better architecture |
| **Display Order** | ❌ No | DisplayOrder (int) | ✅ Porpoise advantage |
| **Question Collection** | ❌ No navigation | Questions (ICollection) | ✅ Porpoise has relationships |

**Orca Approach:**
- Block label and stem duplicated across all questions in the block
- Changed one question → must update all questions in block
- No way to query all blocks

**Porpoise Core SQL Schema:**
```sql
CREATE TABLE QuestionBlocks (
    Id CHAR(36) PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    Label VARCHAR(255) NOT NULL,
    Stem TEXT,
    DisplayOrder INT,
    CreatedAt DATETIME,
    UpdatedAt DATETIME,
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id)
)
```

### 4. Project/Survey Metadata

| Feature | Orca (Project) | Porpoise Core (Survey) | Status |
|---------|----------------|------------------------|--------|
| **Project Name** | Name (NVARCHAR 50) | Name (string) | ✅ Supported |
| **Client Name** | ClientName (NVARCHAR 50) | ❌ Not tracked | ⚠️ Could add |
| **Notes** | Notes (text) | ❌ Not tracked | ⚠️ Could add |
| **Data File Path** | OriginalDataFilePath (string) | ❌ Not persisted after import | Different approach |
| **Delimiter** | Delimiter (char) | ❌ Not stored | Different approach |
| **Default Missing Value** | MissingValue (int) | ❌ Not at survey level | ⚠️ Porpoise per-question |
| **Export State** | ExportState (enum) | ❌ Not tracked | Not needed (integrated app) |
| **Data Table** | Data (DataTable in memory) | Stored in SurveyData table | ✅ Both support |
| **Statistics** | NumCases, NumVariables, NumUniqueResponses | ❌ Calculated on demand | Different approach |
| **Quality Metrics** | NumResponseStrings, NumResponseNulls, NumResponseDecimals, PctClean | ❌ Not tracked | ⚠️ Orca advantage |

---

## Gap Analysis

### Features Porpoise Core Already Has (No Action Needed)

✅ **Fully Supported:**
1. Question number, label, stem editing
2. Response value and label definitions
3. Block management (better than Orca - normalized)
4. Multiple missing value codes (3 codes + range)
5. Variable type and data type classification
6. Index type assignment (+/-/Neutral)
7. Response weighting
8. Preference blocks

### Features from Orca to Consider Adding

#### HIGH PRIORITY

1. **Data Quality Metrics** ⭐
   - Track response data types (string, null, decimal, integer)
   - Calculate "% Clean" metric
   - Flag questions/responses needing attention
   
   **Implementation:**
   ```sql
   -- Add to Response table or calculate at runtime
   ALTER TABLE Responses ADD COLUMN IsString BOOLEAN DEFAULT FALSE;
   ALTER TABLE Responses ADD COLUMN IsNull BOOLEAN DEFAULT FALSE;
   ALTER TABLE Responses ADD COLUMN IsDecimal BOOLEAN DEFAULT FALSE;
   
   -- Or add survey-level metrics table
   CREATE TABLE SurveyQualityMetrics (
       Id CHAR(36) PRIMARY KEY,
       SurveyId CHAR(36) NOT NULL,
       TotalResponseValues INT,
       NumStringValues INT,
       NumNullValues INT,
       NumDecimalValues INT,
       NumIntegerValues INT,
       PercentClean DECIMAL(5,2),
       CalculatedAt DATETIME,
       FOREIGN KEY (SurveyId) REFERENCES Surveys(Id)
   )
   ```

2. **Recoding/Data Editing** ⭐⭐⭐
   - Change data values across entire dataset
   - Track recoding operations for audit trail
   
   **Implementation:**
   ```sql
   -- Audit trail for recodes
   CREATE TABLE RecodeHistory (
       Id CHAR(36) PRIMARY KEY,
       SurveyId CHAR(36) NOT NULL,
       QuestionId CHAR(36) NOT NULL,
       OldValue VARCHAR(255),
       NewValue VARCHAR(255),
       AffectedRows INT,
       CreatedBy CHAR(36),
       CreatedAt DATETIME,
       FOREIGN KEY (SurveyId) REFERENCES Surveys(Id),
       FOREIGN KEY (QuestionId) REFERENCES Questions(Id)
   )
   ```

3. **Response Value Flexibility**
   - Currently Porpoise only supports integer response values
   - Orca allowed string values (e.g., "Male", "Female")
   
   **Implementation:**
   ```sql
   -- Option 1: Change RespValue from INT to VARCHAR
   ALTER TABLE Responses MODIFY COLUMN RespValue VARCHAR(255);
   
   -- Option 2: Add separate field for original value
   ALTER TABLE Responses ADD COLUMN OriginalValue VARCHAR(255);
   ALTER TABLE Responses ADD COLUMN ValueType VARCHAR(20); -- 'Integer', 'String', 'Decimal'
   ```

#### MEDIUM PRIORITY

4. **Client Name & Notes**
   ```sql
   ALTER TABLE Surveys ADD COLUMN ClientName VARCHAR(255);
   ALTER TABLE Surveys ADD COLUMN Notes TEXT;
   ```

5. **Response Count Tracking**
   - Store count of how many times each response value appears in data
   - Useful for data quality review
   
   ```sql
   ALTER TABLE Responses ADD COLUMN FrequencyInData INT DEFAULT 0;
   ALTER TABLE Responses ADD COLUMN LastCountedAt DATETIME;
   ```

#### LOW PRIORITY

6. **Snapshot/Version Control**
   - Ability to save project state before major changes
   - Rollback capability
   
   **Implementation:** Complex - requires full data + definition versioning

7. **Original File Tracking**
   ```sql
   ALTER TABLE Surveys ADD COLUMN OriginalFileName VARCHAR(255);
   ALTER TABLE Surveys ADD COLUMN ImportedAt DATETIME;
   ALTER TABLE Surveys ADD COLUMN ImportDelimiter VARCHAR(5);
   ```

---

## Data Type Comparison

### Orca Data Types
- **Questions:** INTEGER, NVARCHAR(50), BIT
- **Responses:** NVARCHAR(50) - flexible, allows strings
- **Storage:** SQLite (local file)
- **Approach:** In-memory DataTable for active data

### Porpoise Core Data Types
- **Questions:** CHAR(36) GUIDs, VARCHAR, INT, BOOLEAN, DATETIME
- **Responses:** INT only for values - more restrictive
- **Storage:** MySQL/SQLite (server-based)
- **Approach:** Database-backed with lazy loading

---

## Migration Considerations

### If Adding String Response Values

**Problem:** Current Porpoise stores `Response.RespValue` as `INT`

**Solutions:**

1. **Conservative (Recommended):** Add `OriginalValue` column
   - Keep `RespValue` as INT for backwards compatibility
   - Add `OriginalValue VARCHAR(255)` for flexibility
   - Add `ValueType` enum to track type
   - Migration: Copy `RespValue` to `OriginalValue` for existing records

2. **Aggressive:** Change `RespValue` to VARCHAR
   - Requires updating all analysis code
   - Could break existing crosstab/index calculations
   - Need to handle conversion logic everywhere

**Recommendation:** Option 1 - safer, maintains compatibility

### If Adding Data Quality Metrics

**Options:**

1. **Real-time Calculation:** No schema changes, calculate on demand
   - Slower but no storage overhead
   - Good for small datasets

2. **Cached Metrics Table:** Store calculated metrics
   - Fast retrieval
   - Need to recalculate on data changes
   - Good for large datasets

**Recommendation:** Option 2 for better UX, with background recalculation

---

## Proposed Schema Changes

### Phase 1: Data Quality Enhancement (Low Risk)

```sql
-- Add survey-level metadata
ALTER TABLE Surveys ADD COLUMN ClientName VARCHAR(255);
ALTER TABLE Surveys ADD COLUMN Notes TEXT;
ALTER TABLE Surveys ADD COLUMN OriginalFileName VARCHAR(255);
ALTER TABLE Surveys ADD COLUMN ImportedAt DATETIME;

-- Add quality metrics table
CREATE TABLE SurveyQualityMetrics (
    Id CHAR(36) PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    TotalResponseValues INT,
    NumStringValues INT,
    NumNullValues INT,
    NumDecimalValues INT,
    NumIntegerValues INT,
    PercentClean DECIMAL(5,2),
    LastCalculatedAt DATETIME,
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id)
);

-- Add response value metadata
ALTER TABLE Responses ADD COLUMN OriginalValue VARCHAR(255);
ALTER TABLE Responses ADD COLUMN ValueType VARCHAR(20); -- 'Integer', 'String', 'Decimal', 'Null'
ALTER TABLE Responses ADD COLUMN FrequencyInData INT DEFAULT 0;
```

### Phase 2: Recoding Support (Medium Risk)

```sql
-- Audit trail for data modifications
CREATE TABLE RecodeHistory (
    Id CHAR(36) PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    QuestionId CHAR(36) NOT NULL,
    OperationType VARCHAR(50), -- 'SingleValue', 'Range', 'BulkReplace'
    OldValue VARCHAR(255),
    NewValue VARCHAR(255),
    AffectedResponseCount INT,
    Description TEXT,
    CreatedBy CHAR(36),
    CreatedAt DATETIME,
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id),
    FOREIGN KEY (QuestionId) REFERENCES Questions(Id)
);

-- Optional: Snapshot support
CREATE TABLE DataSnapshots (
    Id CHAR(36) PRIMARY KEY,
    SurveyId CHAR(36) NOT NULL,
    Name VARCHAR(255),
    Description TEXT,
    SnapshotData LONGBLOB, -- Serialized data state
    CreatedBy CHAR(36),
    CreatedAt DATETIME,
    FOREIGN KEY (SurveyId) REFERENCES Surveys(Id)
);
```

---

## Recommendations

### Immediate Actions (This Week)

1. ✅ **Document current schema** (this document)
2. 🔲 **Review with Bereket & Val** - validate gaps
3. 🔲 **Prototype quality metrics** - can we calculate from existing data?
4. 🔲 **Test response value flexibility** - do we need strings or can we standardize?

### Short Term (Next Sprint)

1. **Add Data Quality Metrics**
   - Implement `SurveyQualityMetrics` table
   - Background job to calculate metrics
   - Display on survey detail page

2. **Enhance Response Model**
   - Add `OriginalValue` and `ValueType` columns
   - Backward-compatible migration
   - Update import logic to populate both fields

3. **Add Client Name & Notes**
   - Simple ALTER TABLE commands
   - Update UI to expose fields

### Long Term (Future)

1. **Recoding Functionality**
   - Design UX for data editing
   - Implement `RecodeHistory` audit trail
   - Build preview/validation logic
   - Add undo capability

2. **Snapshot/Versioning** (Optional)
   - Only if recoding is heavily used
   - Full project state capture
   - Rollback capability

---

## Conclusion

**Good News:** Porpoise Core's schema is actually more advanced than Orca's in many ways:
- Normalized block structure
- GUID primary keys (better for distributed systems)
- Rich analysis metadata (variable type, data type, index type, weighting)
- Preference block support

**Key Gaps:**
1. **Data quality metrics** - Orca tracked this, Porpoise doesn't
2. **Recoding** - Orca could change data values, Porpoise can't
3. **Response value flexibility** - Orca allowed strings, Porpoise requires integers

**Recommendation:** Focus on #1 and #2. Response value flexibility (#3) is nice-to-have but not critical if import process standardizes values upfront.

---

**Next Steps:** 
1. Review this document with team
2. Prioritize which gaps to address
3. Design UI mockups for selected features
4. Create migration scripts for chosen schema changes

**Generated:** December 23, 2025  
**Status:** Draft - Awaiting Team Review
