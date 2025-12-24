# Orca Codebase Exploration & Analysis

**Purpose:** Data preparation tool for Porpoise survey analysis (written 2014)  
**Technology:** C# WPF Desktop Application  
**Architecture:** Classic N-tier (Client → Presenter → Business → Data Access → Model)

**CURRENT STATUS (2025):**
- **Orca is STILL IN USE** by existing Porpoise users for data preparation
- **Current Workflow:** Users prep data in Orca → Import to Porpoise web for analysis
- **Web App Goal:** Combine BOTH Orca and Porpoise into single integrated web application
- **Use Case:** Orca = primary data prep, Legacy Porpoise question def = last-minute adjustments (index values, etc.)
- **User Base:** Two active users still relying on this workflow today

---

## 🎯 Project Goal: Unified Data Prep + Analysis Platform

**What we're building:**
A modern web application that replaces BOTH legacy desktop apps with a seamless workflow:
1. **Import** messy survey data (CSV)
2. **Prepare** data (label questions/responses, clean values, handle blocks)
3. **Analyze** data (crosstabs, statistical significance, AI insights)
4. **Report** findings

**Why this matters:**
- Users currently juggle TWO separate desktop apps
- Data flows one direction: Orca → Porpoise (no round-trip)
- Export/import process is friction
- Web app can eliminate context switching and provide integrated experience

---

## 🚀 NEW WORKFLOW VISION (Dan's Intended Approach)

**CRITICAL SHIFT:** Instead of manual data entry (Orca's approach), leverage existing survey definition files

### The Intended Import Flow:

**User starts with:**
1. **Survey definition file** (from survey platform/tool)
   - Contains: Question numbers, question text, response options, response labels
   - May include: Block definitions, question groupings, metadata
   - Format: TBD - waiting on sample from Bereket
   
2. **Data file** (CSV or Excel)
   - First row: Question numbers (matches survey definition)
   - Subsequent rows: Survey responses (one row per respondent)
   - Example: Q1, Q2, Q3a, Q3b, Q4... as column headers

**Auto-matching process:**
1. User uploads BOTH files
2. System matches question numbers from data columns to survey definitions
3. Auto-populate:
   - Question text from survey file
   - Response value labels (1="Strongly Agree", 2="Agree", etc.)
   - Block groupings (if present in survey file)
   - Question metadata (type, data type, etc.)
4. User reviews/validates matches
5. User fixes any mismatches or missing definitions
6. Ready to analyze

**Value Proposition:**
- ✅ Eliminate tedious manual typing from Orca workflow
- ✅ Reduce transcription errors
- ✅ Import from existing survey tools (Qualtrics, SurveyMonkey exports)
- ✅ Get to analysis faster
- ✅ Make survey definition the "exception" not the "rule"

**Key Assumption:** Most users already have survey definitions from their survey platform

---

## ⚠️ Open Questions & Validation Needed

### About the Block Concept (CRITICAL - Needs User Validation)

**Dan's Concern:** Not sure users understand or value blocks

**Questions to validate with users:**
1. ❓ **Do questions in a block HAVE to have same number of responses?**
   - Need to verify this constraint
   - If yes, why? What breaks if they don't?
   - Is this a statistical requirement or implementation detail?

2. ❓ **Do users understand what blocks are for?**
   - Can they explain the purpose in their own words?
   - How do they decide what should be a block vs separate questions?
   - Do they use blocks correctly, or make mistakes?

3. ❓ **Do users see value in blocks?**
   - What analysis does blocking enable that they couldn't do otherwise?
   - How often do they actually use block-specific features?
   - What would they lose if blocks didn't exist?

4. ❓ **Is this a Val-ism (academic preference) or market need?**
   - Do other survey analysis tools have this concept?
   - What do Qualtrics, SPSS, Tableau call this?
   - Is "block" the right term, or confusing jargon?

**Potential Alternatives to "Blocks":**
- Simple visual grouping (cosmetic, no statistical implications)
- "Question Groups" or "Question Sets" (less academic terminology)
- Auto-detect from survey structure (same response scale = auto-group)
- Progressive disclosure: Hide block features until user needs them

**Red Flag:** If blocks are complex and users don't understand them, they'll avoid the feature or use it incorrectly

---

## 📄 Waiting on Sample Files from Bereket

**Need to see:**
1. **Survey definition file:**
   - What format? (XML, JSON, CSV, proprietary?)
   - What fields are included?
   - How are blocks represented?
   - How are response labels stored?

2. **Data file:**
   - Actual survey response data
   - How question numbers appear in first row
   - What response values look like (integers, strings, mixed?)
   - How many questions/responses typical?

**Once we have samples, we can:**
- Design the import/matching algorithm
- Build parser for survey definition format
- Test auto-matching accuracy
- Identify edge cases and errors
- Design the validation/fix UI

---

## 📊 Architecture Overview

### Layer Structure
```
0-Client/         → WPF Views (UI)
1-Presenter/      → MVP Pattern (Business Logic)
2-Model/          → Domain Models (POCOs)
3-BusinessLayer/  → Business Operations
4-DataAccessLayer/ → SQLite Database Operations
```

### Key Technologies
- **UI Framework:** Telerik WinControls (RadRibbonBar, RadGridView, etc.)
- **Database:** SQLite (local file-based)
- **Pattern:** Model-View-Presenter (MVP)
- **Data Binding:** WPF BindingSource
- **Persistence:** SQLite + XML export

---

## 🗂️ Core Data Model

### Primary Entities

**1. Project** (Project.cs - 1195 lines)
```csharp
Properties:
- Id (int, auto-increment PK)
- Name (string, required)
- ClientName (string)
- Notes (string)
- OriginalDataFilePath (string) - path to CSV file
- Delimiter (char) - CSV delimiter
- MissingValue (int) - code for missing data
- ExportState (enum) - None/Exported/ExportedAndVerified
- Data (DataTable) - actual survey data loaded in memory
- VariableDefs (collection) - question definitions
- NumCases (int) - number of survey responses
- NumVariables (int) - number of questions
- NumUniqueResponses (int)
- NumQuestionsWithLabels (int)
- NumResponsesWithLabels (int)
- NumResponseStrings (int) - non-numeric responses
- NumResponseNulls (int) - null responses
- NumResponseDecimals (int) - decimal responses
- PctClean (decimal) - percentage of "clean" data
```

**2. OrcaVariableDef** (Question Definition - 235 lines)
```csharp
Properties:
- Id (int, auto-increment PK)
- ProjectId (int, FK)
- QuestionNumber (string, 50 chars) - e.g., "Q1", "Q3a"
- QuestionLabel (string, 50 chars) - descriptive label
- UniqueResponses (collection) - response values for this question
- BlockType (enum) - None/FirstInBlock/ContinuingInBlock
- ResponsesLabeled (int) - how many responses have labels
- NumResponsesHaveString (int)
- NumResponsesHaveNull (int)
- NumResponsesHaveDecimal (int)
```

**3. OrcaResponseDef** (Response Definition - 163 lines)
```csharp
Properties:
- RespValue (string, 50 chars) - the actual value (e.g., "1", "99", "Male")
- Label (string, 50 chars) - human-readable label (e.g., "Very Satisfied")
- ResponseIsLabeled (bool) - whether label is assigned
- ResponseIsString (bool) - value is non-numeric
- ResponseIsNull (bool) - value is null/empty
- ResponseIsDecimal (bool) - value is decimal (not integer)
```

**4. Snapshot** (Version Control)
```csharp
- Id, ProjectId
- Name, Description
- CreatedOn, CreatedBy
- Associated data snapshot for rollback capability
```

---

## 🎯 Core Features & Workflows

### 1. **Project Creation & Data Import**

**Workflow:**
- User selects CSV file (or Excel, or creates blank)
- Orca reads CSV, auto-detects:
  - Number of cases (rows)
  - Number of variables (columns)
  - Question numbers from first row
  - Response values from data
  - Data types (integer, decimal, string, null)
- Creates Project with VariableDefs collection
- Loads entire dataset into memory (DataTable)

**Key Files:**
- `NewProjectView.cs` - New project wizard
- `ProjectManager.LoadCSVDataFile()` - CSV parsing
- `ProjectDB.ReadSurveyDataFromCSV()` - File reading

### 2. **Question Definition Screen** (PRIMARY FEATURE)

**QuestionDetailView** - The most complex screen (439 lines + 1739 lines presenter)

**Capabilities:**
- View/edit question label
- View all unique response values for that question across all cases
- Label each response (e.g., "1" → "Strongly Agree")
- Recode responses (change values in dataset)
- Mark question as part of a block
- Navigate between questions with trackbar

**UI Components:**
- Question label textbox
- Response grid (DataGridView):
  - Column 1: Response Value (read-only from data)
  - Column 2: Label (editable)
  - Column 3: Count (how many times value appears)
- "Part of Block" checkbox
- "First in Block" vs "Continuing in Block" radio buttons
- Recode functionality
- Navigation: trackbar, first/last buttons

**Key Operations:**
```csharp
// Question navigation
QNavInitialize() - Setup trackbar for question count
OnTrkQNav_ValueChanged() - Navigate to different question
OnBtnNavFirst/Last_Click() - Jump to first/last question

// Response labeling
OnGrdResponses_CellValueChanged() - User edits response label
SaveQuestion() - Persist changes to database

// Recoding
OnBtnRecode_Click() - Open recode dialog
RecodeResponseValues() - Change data values across entire dataset

// Block management
OnChbPartOfBlock_ToggleStateChanged() - Mark as block question
ValidateBlockState() - Ensure block consistency
```

### 3. **Data Sheet View** (DataFileView.cs - 628 lines)

**Features:**
- **Spreadsheet-like grid** showing entire dataset
- **Question List** sidebar (similar to current Porpoise)
- **Context menus:**
  - Copy/Paste cells
  - Sort columns
  - Insert/Move columns
  - Delete columns
- **Real-time data editing** with validation
- **Undo/Redo** support

**Key Components:**
```csharp
DataSheetGrid - Main spreadsheet (custom control)
QuestionGrid - Question list on left
Context menus - Right-click operations
PasteRowsWorker - Background worker for large paste operations
```

### 4. **Export to Porpoise**

**Workflow:**
1. Validate project is "clean":
   - All responses are integers
   - All questions have labels
   - All responses have labels (optional but recommended)
2. Export creates:
   - `{ProjectName}.csv` - clean data file
   - `{ProjectName}.xml` - question/response definitions
   - `OrcaExport.xml` - interface file for Porpoise to auto-import
3. Marks project as "Exported"

**Key Files:**
- `OrcaExport.cs` - Export metadata model
- `ProjectManager.SaveProjectDataToCSVFile()`
- `ProjectManager.SaveVariableDefsToXMLFile()`
- `ProjectManager.SaveExportInterfaceFile()`

### 5. **Snapshot System** (Version Control)

**Features:**
- Create named snapshots of project state
- Store complete data + variable definitions
- Restore to previous snapshot
- View snapshot history

**Use Case:** Before major recoding operations, create snapshot to allow rollback

### 6. **Validation & Quality Metrics**

**"Percent Clean" Calculation:**
```csharp
PctClean = (TotalResponses - Strings - Nulls - Decimals) / TotalResponses * 100
```

**Tracked Metrics:**
- Questions with labels
- Responses with labels
- String responses (need conversion)
- Null responses (need handling)
- Decimal responses (need rounding decision)

**Visual Feedback:**
- Progress bar showing % clean
- "Needs Attention" labels (red) for problem areas
- Grid cell highlighting for non-integer values

---

## 🎨 UI Patterns & Interactions

### Question List
- Hierarchical view (blocks expand/collapse)
- Question number + label display
- Icon indicators for variable type
- Click to open QuestionDetailView

### Response Grid Editing
- Inline editing with validation
- Auto-save on cell change
- Row highlighting for selected response
- Context menu for batch operations

### Navigation
- Ribbon-based menu system (Telerik)
- Backstage view for New/Open/Save
- Docking panels for question list
- Modal dialogs for detail editing

### Data Entry Helpers
- Auto-complete for response labels
- Copy labels to entire column
- Recode by range (e.g., 1-5 → "Low", 6-10 → "High")
- Change question number (with block awareness)

---

## 🔧 Key Business Rules

### Question Numbers
- Can contain letters (Q3a, Q3b for block questions)
- Must be unique within project
- Changing number updates throughout dataset

### Blocks
- First question marked as "FirstInBlock"
- Subsequent questions marked as "ContinuingInBlock"
- All must have same base number (Q3a, Q3b, Q3c)
- Optional "change entire block" mode

### Response Values
- Original values preserved until explicit recode
- Can be any string, but export requires integers
- Null/empty treated as missing (can set missing value code)

### Data Integrity
- Changes to question definitions immediately affect data view
- Recoding is permanent (use snapshots for safety)
- Export validates entire dataset before allowing export

---

## 📋 Feature Comparison: Orca vs Legacy Porpoise vs Current Porpoise

| Feature | Orca (2014) | Legacy Porpoise PC (2013) | Current Porpoise Web | Gap/Opportunity |
|---------|-------------|---------------------------|----------------------|-----------------|
| **Data Import** | CSV, Excel, Blank | Import from Orca | Upload CSV | ⚠️ Lost Orca integration |
| **Question Definition** | Full editor | **Survey Definition screen** | Basic metadata only | ❌ **MAJOR REGRESSION** |
| **Response Labeling** | Per-question grid | **Response grid editor** | Only at import | ❌ **MAJOR REGRESSION** |
| **Data Editing** | Full spreadsheet | Limited/None? | None (read-only) | ❌ Gap |
| **Recoding** | Interactive with preview | Unknown | None | ❌ Gap |
| **Blocks** | First/Continuing distinction | **Block Question Mode** | Only visual grouping | ❌ **Feature regression** |
| **Batch Operations** | Limited | **Apply Changes dialog** | None | ❌ **Missing valuable feature** |
| **Validation** | Real-time % clean metrics | Unknown | Import-time only | ⚠️ Gap |
| **Snapshots** | Full version control | Unknown | None | ❌ Missing |
| **Analysis** | None (export to Porpoise) | Full suite (crosstab, statsig) | Full suite (crosstab, statsig) | ✓ Maintained |

**CRITICAL FINDING:** Legacy Porpoise PC (2013) **already had** a "Survey Definition" screen with response labeling capability! The current web version represents a **regression** in functionality, not just missing Orca features.

---

## 🎯 Recommended Integration Strategy

### Phase 0: Understanding Current User Workflow (CRITICAL FIRST STEP)

**Before coding, validate with users:**
1. **Current Orca usage patterns**
   - Which Orca features do they use most?
   - Which features are rarely/never used?
   - What's their typical workflow sequence?
   - Where do they spend most time?

2. **Current pain points**
   - What's frustrating about the two-app workflow?
   - What errors/issues happen during export/import?
   - What data quality problems are common?
   - What takes too long?

3. **Porpoise adjustment usage**
   - What adjustments do they make after importing from Orca?
   - Why not make those changes in Orca?
   - Are index values the only adjustment, or more?

4. **Migration concerns**
   - What are they worried about losing?
   - What would they miss from desktop apps?
   - What new capabilities would they want?

**Deliverable:** User interview notes → Updated workflow diagram → Feature priority list

---

### Phase 1: Survey Definition Screen (FOUNDATION - Merge Both Apps' Capabilities)

**Goal:** Replace the primary Orca workflow with integrated web interface

**Core Features (Must-Have):**

1. **Question List Sidebar** (Already exists in current app ✓)
   - Shows all questions
   - Indicates labeled/unlabeled status (⚠️ warnings)
   - Click to open detail panel

2. **Question Detail Panel** (New - slide-over or right panel)
   
   **Question Metadata:**
   - Question number (auto from column, read-only)
   - Question label (editable)
   - Question stem (text area - for blocks)
   - Variable type dropdown (Dependent/Independent)
   - Data type dropdown (Nominal/Ordinal/Scale)
   
   **Block Configuration:**
   - "Block Question Mode" toggle
   - When enabled:
     - Block stem (text area)
     - Block label (text field)
     - Block status radio buttons: First / Continuing / Stand-alone
   
   **Response Labeling Grid** (PRIMARY FEATURE - from Orca):
   - Auto-populate with unique values found in data
   - Columns:
     - **Value** (from data, read-only) - e.g., 1, 2, 3, 99
     - **Label** (editable) - e.g., "Strongly Agree"
     - **N** (count, read-only) - how many times this value appears
     - **%** (percentage, read-only) - 117 responses = 24.8%
     - **Index** (optional, for preference blocks) - weighting value
     - **Omitted** (checkbox) - mark as missing value
   - Show warning icons for unlabeled values
   - "ELSE" row for values not explicitly labeled
   - Inline editing (click to edit label)
   
   **Actions:**
   - Save button
   - Save & Next button (navigate to next question)
   - Cancel button
   - Recode button (future - for data cleaning)

3. **Navigation** (from Orca)
   - Previous/Next question buttons
   - Question counter (e.g., "Question 4 of 15")
   - Optional: Slider/trackbar for quick navigation

4. **Data Quality Indicator** (from Orca)
   - Show at top: "% Clean: 1.5%" with progress bar
   - Click to see details (what needs attention)
   - Total cases count

**Technical Implementation:**

```typescript
// API Endpoints needed
GET /api/surveys/{id}/questions/{questionId}/detail
  Returns: question metadata + unique response values with frequencies

PUT /api/surveys/{id}/questions/{questionId}
  Body: question metadata, response labels, block settings

GET /api/surveys/{id}/data-quality
  Returns: % clean, counts of unlabeled items, validation issues

// New Vue component structure
components/
  SurveyDefinition/
    QuestionListSidebar.vue (reuse existing)
    QuestionDetailPanel.vue (new)
    ResponseLabelingGrid.vue (new)
    BlockConfigSection.vue (new)
    DataQualityBadge.vue (new)
    NavigationControls.vue (new)
```

**DB Schema Changes:**
```sql
-- Add missing question fields
ALTER TABLE questions ADD COLUMN question_stem TEXT;
ALTER TABLE questions ADD COLUMN data_type VARCHAR(20) DEFAULT 'Nominal';
ALTER TABLE questions ADD COLUMN block_stem TEXT;
ALTER TABLE questions ADD COLUMN block_label VARCHAR(255);
ALTER TABLE questions ADD COLUMN block_status VARCHAR(20) DEFAULT 'StandAlone';

-- Response labels with frequencies
CREATE TABLE question_response_labels (
    id INT AUTO_INCREMENT PRIMARY KEY,
    question_id INT NOT NULL,
    response_value VARCHAR(255) NOT NULL,
    response_label VARCHAR(255),
    response_count INT DEFAULT 0, -- Calculated from data
    response_percent DECIMAL(5,2) DEFAULT 0.00,
    index_value DECIMAL(10,2), -- For preference blocks
    is_omitted BOOLEAN DEFAULT FALSE, -- Missing value flag
    display_order INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (question_id) REFERENCES questions(id) ON DELETE CASCADE,
    UNIQUE KEY unique_response (question_id, response_value)
);

-- Data quality metrics (calculated on import/change)
CREATE TABLE survey_data_quality (
    survey_id INT PRIMARY KEY,
    total_cases INT,
    total_questions INT,
    total_unique_responses INT,
    questions_labeled INT,
    responses_labeled INT,
    integer_responses INT,
    string_responses INT,
    null_responses INT,
    decimal_responses INT,
    percent_clean DECIMAL(5,2),
    last_calculated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (survey_id) REFERENCES surveys(id) ON DELETE CASCADE
);
```

---

### Phase 2: Batch Operations (HIGH VALUE - Efficiency for Users)

**Goal:** Allow applying changes to multiple questions at once (from Legacy Porpoise PC)

**"Apply Changes" Dialog:**

1. **Select what to change:**
   - ☐ Omitted values (enter codes: 99, 999, -1)
   - ☐ Variable type (Dependent/Independent)
   - ☐ Data type (Nominal/Ordinal/Scale)
   - ☐ Block settings (stem, label, status)
   - ☐ Response labels (add/update response definitions)

2. **Select scope:**
   - ⚪ Current question only
   - ⚪ Current block
   - ⚪ Selected questions (multi-select from list)
   - ⚪ All questions

3. **Preview changes:**
   - Show which questions will be affected
   - Count: "This will update 12 questions"

**Use Cases:**
- Set 99 as missing value across all questions
- Apply same 1-5 Likert scale labels to multiple rating questions
- Mark all questions in a block as "Continuing"
- Set all questions as "Independent" variable type

**Implementation:**
```typescript
// Component
components/SurveyDefinition/BatchOperationsDialog.vue

// API
POST /api/surveys/{id}/batch-update
Body: {
  changes: { omittedValues: [99], variableType: 'Independent' },
  scope: { type: 'selected', questionIds: [1,2,3] }
}
```

---

### Phase 3: Data Quality Dashboard (MEDIUM PRIORITY - Orca Visibility)

**Goal:** Show data quality metrics like Orca's "% Clean" dashboard

**Location:** Survey detail page (new card/section)

**Metrics to Display:**
- **Overall Quality Score:** % Clean (large number with progress bar)
- **Questions:** Total | Labeled | Unlabeled (with counts)
- **Responses:** Total unique | Labeled | Unlabeled
- **Data Issues:**
  - String values: 0 (0.0%)
  - Null values: 0 (0.0%)
  - Decimal values: 0 (0.0%)
  - Integer values: 513 (100.0%)

**Actions:**
- "View Details" button → opens question list filtered to problems
- "Fix Issues" button → navigates to first question needing attention

**Implementation:**
```typescript
// Component
components/Survey/DataQualityCard.vue

// API (reuse from Phase 1)
GET /api/surveys/{id}/data-quality
```

---

### Phase 4: Auto-Detection & Smart Defaults (MEDIUM PRIORITY - Reduce Manual Work)

**Goal:** Reduce tedious labeling work with intelligent defaults

**Features:**

1. **Question Label Auto-Suggestion**
   - If question number is "Q1_Gender", suggest label "Gender"
   - If question number is "Satisfaction", suggest label "Satisfaction"
   - Parse common patterns from column names

2. **Response Label Templates**
   - Detect common scales:
     - 1-5 → "Strongly Disagree" to "Strongly Agree"
     - 1-10 → "1 (Low)" to "10 (High)"
     - 0-1 → "No" / "Yes"
   - Offer to apply template to selected questions
   - User can customize templates

3. **Smart Missing Value Detection**
   - Auto-detect common missing codes (99, 999, -1, -99)
   - Suggest marking as omitted
   - "Click to mark all 99 values as missing"

4. **AI-Powered Labeling** (Future Enhancement)
   - Send question number + data sample to LLM
   - Get suggested question label and response labels
   - User reviews and accepts/edits

**Implementation:**
```typescript
// Utilities
utils/questionLabelSuggester.ts
utils/responseScaleTemplates.ts
utils/missingValueDetector.ts

// API
POST /api/surveys/{id}/suggest-labels
Body: { questionId, responseSample }
Returns: { suggestedQuestionLabel, suggestedResponseLabels }
```

---

### Phase 5: Data Viewing (LOWER PRIORITY - Nice to Have)

**Goal:** Show actual survey data like Orca's spreadsheet view

**Features:**
- Spreadsheet-style grid (read-only initially)
- Shows respondent rows × question columns
- Highlight data quality issues (strings in red, nulls in yellow)
- Filter/sort by question or respondent
- Export view to CSV

**Technical:**
- Use AG Grid or Handsontable
- Paginate (don't load all 1000+ rows at once)
- Server-side filtering/sorting for performance

---

### Phase 6: Data Editing & Recoding (FUTURE - Advanced Orca Features)

**Goal:** Full data manipulation like Orca (only if users actually need it)

**Features:**
- Edit individual cell values
- Recode entire question (change all 1→5, 2→4, etc.)
- Find & replace across dataset
- Add/delete cases (rows)
- Merge cases or variables
- Create calculated variables

**Considerations:**
- Requires audit trail (who changed what when)
- Potential for data corruption
- May not be needed if import is clean
- Validate with users before building

---

### Phase 7: Import from Orca (TRANSITIONAL - Bridge to New System)

**Goal:** Help users migrate from Orca workflow

**Features:**
- Import Orca's XML export files
- Parse question definitions and response labels
- Map to new web app schema
- Validate import, show preview
- "I have Orca XML files" button on import screen

**Timeline:** Implement early to ease migration, deprecate once users comfortable with web app

---

---

## 💡 Modernization Opportunities

### Things to KEEP from Orca:
- **Recoding functionality** (change data values across dataset)
- Navigation between questions (trackbar is great UX)
- Validation feedback (% clean metric)
- Grid view showing all response values at once

### Things Porpoise Already Has (Keep):
- Question label editing
- Block label editing
- Response label editing
- Inline editing approach

### Things to IMPROVE:
- **UX for response labeling** (Orca's grid view is more efficient)
- **Bulk operations** (label all responses at once)
- **Smart defaults** (auto-suggest labels based on common patterns)
- **Real-time collaboration** (multiple users editing)
- **AI-powered labeling** (suggest labels based on context)

### Things to SKIP:
- Desktop-only features (ribbon, docking panels)
- SQLite local storage (use server DB)
- XML export format (use JSON/API)
- Separate export step (integrated app)

---

## 🖼️ UI Analysis from Screenshots

### Orca Question Detail View (Images 1-2)

**Layout:**
- **Top bar:** Navigation controls (First, Prev, slider showing "4 of 15", Next, Last)
- **Left panel:** Question list with Label/Resp Status columns
- **Right panel:** Question detail form
  - Question # field (large number display)
  - Question label textbox
  - Block section with checkboxes and radio buttons
  - Total cases count
  - **Response grid** (primary feature):
    - Columns: Value | Label | Recode | N | %
    - Expandable rows (▶ indicator)
    - Shows unique values found in data
    - Editable Label column
    - Warning icons (⚠️) for unlabeled responses
    - "ELSE" row for catch-all
  - Buttons: Refresh, Save & Next, Save & Close, Close, Recode

**Key Insights:**
- Navigation slider is excellent UX for moving between questions
- Response grid shows both raw values AND their frequency (N and %)
- Warning indicators make it obvious what needs attention
- "Recode by range of values" checkbox suggests batch operations

### Orca Data Spreadsheet View (Images 3, 5)

**Layout:**
- **Ribbon menu:** HOME and QUESTIONS tabs with extensive toolbar
- **Left sidebar:** Question list (collapsible, shows Qst/Label/Resp Status)
- **Main area:** Full spreadsheet view of raw data
- **Status bar:** "% Clean: 1.5%", row/column counts
- **Toolbar actions:**
  - Editing: Copy Selected, Copy All, Paste, Merge Cases, Merge Variables
  - Sort & Find: Clear, Find
  - Move Columns: First, Last, Move Left, Move Right
  - Insert & Delete: Insert After, Insert Case, Delete, Add Weight
  - Backup: Snapshot, View All, Restore
  - Export: To .csv File, To Excel, Porpoise

**Key Insights:**
- Question list sidebar is identical pattern to current Porpoise web app ✓
- Shows ALL data in memory (472 rows visible)
- Rich toolbar for data manipulation
- Snapshot feature prominently featured

### Orca File Info Screen (Image 4)

**Left menu:**
- Info (selected)
- New
- Open
- Save As
- Rename File
- Delete File
- Options
- Close

**Main panels:**
1. **File Info**
   - File name, Client name, Notes
   - Original data file path
   - Percent clean: 1.5% (progress bar)
   - Missing value: 99

2. **File Activity**
   - Project id, Created on/by
   - Last modified on/by
   - "Change File Info..." button

3. **File Statistics**
   - Total number of cases: 472
   - Total number of variables: 15 (No questions labels - "Needs attention")
   - Total response values: 7080

4. **Unique Responses**
   - Total: 513 (8 or 1.6% responses - "Needs attention")
   - Integer: 513 (100.0%)
   - String: 0 (0.0%)
   - NULL: 0 (0.0%)
   - Decimal: 0 (0.0%)

**Key Insights:**
- Dashboard-style metrics show data quality at a glance
- "Needs attention" labels guide user to problem areas
- Clear percentage-based quality score

### Orca Start Screen (Image 6)

**Options:**
- "From CSV file" or "Blank Data File" (two large buttons)
- **Data Files** list below (test1, test2)
- Preview pane shows: Client name, # Cases: 472, Notes, Pct clean: 0.0%, # Variables: 15

**Key Insights:**
- Recent files list with preview (similar to current Porpoise web! ✓)
- Quality metrics shown even before opening

### Legacy Porpoise Survey Definition Screen (Image 7) ⭐ **CRITICAL**

**Layout:**
- **Top menu:** Project, Survey, View, Results, Help
- **Toolbar:** Test results definition, Results, Export, Data (with icons)
- **Left panel:** Question list (Qst, Label, Block columns)
  - Questions grouped/hierarchical
  - Selected question highlighted (q2e)
- **Right panel:** Question detail form
  - Column number: 6 of 12
  - Question number: q2e
  - Question label: [text field]
  - Question stem: [text area]
  - Variable type: Dependent (dropdown)
  - Data type: [dropdown]
  - **Responses section:**
    - Grid with Response | Label | Index Value columns
    - Rows: ■ 1, ▷ 3, ▷ 4, ▷ 8, ▷ 99
    - "Click to add a response alternative" prompt
  - **Block Question Mode ✦** checkbox
    - Block stem: [text area]
    - Block label: [text field]
  - **Block status radio buttons:**
    - ⚪ First question in block
    - ⚪ Continuing block
    - ⚪ Stand-alone question
  - **Preference Block Definition** section
  - Data file information: TestTest7_porpd, Locked
  - "Save Survey" and "Cancel" buttons

**KEY REVELATION:** 
- Legacy Porpoise **already had response labeling**!
- Block question mode with stem/label distinction
- Similar grid-based response editor to Orca
- "Index Value" column suggests preference block weighting

### Legacy Porpoise Apply Changes Dialog (Image 8) ⭐ **CRITICAL**

**Dialog title:** "Apply Missing Values"

**Sections:**

1. **Change Fields**
   - ☑ Omitted values: [___] [___] [___]
   - ☐ Variable type: Dependent (dropdown)
   - ☐ Data type: Nominal (dropdown)

2. **Block**
   - ☐ Block label: [text field]
   - ☐ Block stem: [text area]
   - ☐ Block status:
     - ⚪ First question in block
     - ⚪ Continuing block
     - ⚪ Stand-alone question

3. **Responses**
   - Grid: Resp... | Label | Index Value
   - Rows: ⊙ [blank], ▷ 1, 2, 3, 99
   - "Click to add a response alternative"

4. **Select how to apply these changes:**
   - ⚪ This question only
   - ⚪ Current block (selected)
   - ⚪ Selected questions
   - ⚪ All questions

**Buttons:** Apply, Cancel

**KEY REVELATION:**
- **Batch operation capability** to apply changes to multiple questions!
- Can apply to: single question, current block, selected questions, or all
- Can set omitted values (missing data codes) in bulk
- Shows that users needed efficiency tools for repetitive tasks

---

## 🎯 Revised Understanding: The Complete Picture

### The Current Two-App Workflow (STILL IN USE TODAY - 2025)

**User's Current Process:**
1. **Orca (Desktop):** Primary data preparation
   - Import messy CSV with raw survey data
   - Auto-detect questions from column headers
   - Label each question
   - View unique response values per question
   - Label each response (1="Strongly Agree", 2="Agree", etc.)
   - Clean data (convert strings to integers, handle nulls, fix decimals)
   - Validate data quality (% Clean metric)
   - Export clean CSV + XML definition files

2. **Porpoise Web (Current):** Analysis + Minor Adjustments
   - Import Orca's clean data files
   - Make last-minute adjustments to question definitions
   - Adjust index values for preference blocks
   - Run crosstabs and statistical significance tests
   - Generate AI analysis
   - View/share results

**The Pain Points:**
- **Context switching** between two apps
- **One-way data flow** (Orca → Porpoise, can't go back to edit data)
- **Manual export/import** process (friction)
- **Desktop dependency** (Orca only runs on Windows)
- **No collaboration** (one user per desktop app)
- **Workflow fragmentation** (hard to remember which app does what)

### What the Unified Web App Must Do

**Core Workflow to Replace:**
1. **Data Import & Discovery** (Orca Phase)
   - Upload CSV
   - Auto-detect questions from columns
   - Scan data for unique response values
   - Calculate data quality metrics

2. **Question Definition** (Orca Primary, Porpoise Secondary)
   - Label questions
   - Define question type (Dependent/Independent)
   - Define data type (Nominal/Ordinal/Scale)
   - Set question stem (for blocks)
   - Mark block status (First/Continuing/Stand-alone)

3. **Response Labeling** (Orca Primary)
   - Show all unique values found in data
   - Allow labeling each value
   - Show frequency (N and %) for each value
   - Set index values (for preference blocks)
   - Mark omitted values (missing data codes)

4. **Data Quality & Validation** (Orca)
   - % Clean metric
   - Identify non-integer values
   - Identify unlabeled questions/responses
   - Guide user to problems

5. **Data Cleaning/Recoding** (Orca Advanced - Lower Priority)
   - Recode values across entire dataset
   - Convert strings to integers
   - Handle null values
   - Data editing (spreadsheet view)

6. **Analysis** (Already Implemented in Web App ✓)
   - Crosstabs
   - Statistical significance
   - AI analysis
   - Charts and visualizations

### What Current Porpoise Web Lost

**From Orca (ACTIVELY USED TODAY):**
- ❌ Data quality metrics (% clean)
- ❌ Response value discovery (show unique values per question)
- ❌ Response frequency display (N and % for each value)
- ❌ Data editing/recoding
- ❌ Snapshot/version control
- ❌ Validation feedback (warnings for unlabeled items)

**From Legacy Porpoise PC (USED FOR ADJUSTMENTS):**
- ❌ Survey Definition screen (question/response editor)
- ❌ Block question mode with stem/label
- ❌ Preference blocks with index values
- ❌ **Batch operations** (Apply Changes dialog)
- ❌ Question stem (separate from label)
- ❌ Data type field (Nominal/Ordinal/Scale)

---

## 🗺️ Next Steps

### Immediate Actions (Before Writing Code):

1. **✅ Get Sample Files from Bereket** (BLOCKING - Do This First)
   - Request survey definition file (actual format they use)
   - Request corresponding data file
   - Analyze structure and identify parsing requirements
   - **This determines the entire import workflow design**

2. **✅ User Interviews** (CRITICAL - Validate Assumptions)
   - Schedule time with both active users
   - Walk through their current workflow (Porpoise already has label editing)
   - Screen share and observe them working
   - Ask: "Show me how you currently edit question/response labels"
   - Document: Is current UX sufficient or should we adopt Orca's approach?
   - Ask: "What features from Orca do you miss most?"
   - Ask: "How often do you need to recode data values?"
   - Ask: "What takes the longest/is most tedious?"
   
   **SPECIFIC QUESTIONS ABOUT EXISTING FEATURES:**
   - "Is the current response labeling workflow easy enough?"
   - "Would Orca's grid view (seeing all values at once) be better?"
   - "Do you ever need to change data values after import? (recoding)"
   - "Do you need data quality metrics (% clean, validation warnings)?"
   
   **SPECIFIC QUESTIONS ABOUT BLOCKS:**
   - "Can you explain what a block is and why you use them?"
   - "Do all questions in a block need the same number of responses? Why?"
   - "How often do you use blocks vs standalone questions?"
   - "What analysis can you do with blocks that you can't do without?"
   - "If we called them 'Question Groups' instead, would that be clearer?"
   - "Show me an example of a block in one of your surveys"

3. **📊 Current Schema Analysis**
   - Review existing database schema in Porpoise Core
   - Identify which fields already exist (question metadata)
   - Identify what needs to be added (stems, response labels, etc.)
   - Create migration plan for existing surveys

4. **🎨 Design Mockup - REVISED APPROACH**
   - Instead of "Question Definition UI", design "Import & Validation UI"
   - Show: Survey file upload → Auto-matching preview → Validation screen
   - Focus on: Confirming matches, fixing mismatches, filling gaps
   - Manual definition becomes edge case, not primary workflow
   - Consider mobile/tablet (is responsive needed?)
   - Share mockup with users for feedback

5. **📝 Technical Spike**
   - How to parse survey definition files (format TBD based on Bereket's samples)?
   - How to match question numbers with fuzzy logic (Q1 vs q1 vs Q01)?
   - How to handle mismatches (data has Q5, survey doesn't)?
   - How to calculate response frequencies efficiently?
   - How to handle large datasets (1000+ respondents)?
   - How to store response labels (normalized vs denormalized)?
   - API performance considerations

6. **🚀 Build MVP (Minimum Viable Product) - REVISED SCOPE**
   - Phase 1: Smart Import
     - Upload survey definition + data file
     - Auto-match questions
     - Preview matches/mismatches
     - Simple validation screen to confirm/fix
     - Save to database
   - Phase 2: Manual Definition (Fallback)
     - Question detail panel for fixing mismatches
     - Response labeling for missing definitions
     - Basic navigation (prev/next)
   - Get in users' hands quickly
   - Iterate based on feedback

### Key Questions to Answer with Users:

**Workflow:**
- ❓ Do you always use Orca first, or sometimes skip it?
- ❓ What percentage of questions need response labeling?
- ❓ Do you ever go back to Orca after importing to Porpoise?
- ❓ How often do you adjust index values in Porpoise?

**Feature Priorities:**
- ❓ Which Orca features do you use every time vs rarely?
- ❓ Do you use snapshots/version control? How often?
- ❓ Do you use the recode feature? For what purpose?
- ❓ Do you use batch operations? What kind?

**Pain Points:**
- ❓ What errors/issues happen during export/import?
- ❓ What data quality problems are most common?
- ❓ What do you wish you could do but can't?

**Migration:**
- ❓ How many active surveys do you have in Orca right now?
- ❓ Would you want to import old Orca projects to the web app?
- ❓ What would make you comfortable moving from desktop to web?

---

## 📸 Screenshots Reference

**Priority screens captured from legacy apps:**
1. ✅ Orca QuestionDetailView - Response grid with labeling
2. ✅ Orca DataFileView - Spreadsheet with question list
3. ✅ Orca Project Info - Stats dashboard
4. ✅ Orca Start Screen - Recent files with preview
5. ✅ Legacy Porpoise Survey Definition - Question metadata editor
6. ✅ Legacy Porpoise Apply Changes - Batch operations dialog

**Additional screenshots needed:**
- [ ] Orca Recode dialog
- [ ] Orca Snapshot/Restore UI
- [ ] Legacy Porpoise preference block setup
- [ ] User's actual survey data (anonymized)

---

## 🎯 Success Criteria

**Phase 1 is successful when:**
- ✓ Users can import a CSV survey
- ✓ Users can see all unique response values per question
- ✓ Users can label each response value
- ✓ Users can see frequency (N and %) for each value
- ✓ Users can save changes and navigate between questions
- ✓ **Users say:** "I can prep a survey without needing Orca"

**Long-term success:**
- ✓ Both users migrate from desktop apps to web app
- ✓ Orca and legacy Porpoise PC are retired
- ✓ New users can onboard without learning two separate tools
- ✓ Survey prep time is reduced (fewer steps, less friction)
- ✓ Data quality improves (better validation, fewer errors)

---

**Generated:** December 22, 2025  
**Author:** GitHub Copilot  
**Purpose:** Foundation for merging Orca + Porpoise into unified web application  
**Status:** Ready for user validation and Phase 1 implementation
