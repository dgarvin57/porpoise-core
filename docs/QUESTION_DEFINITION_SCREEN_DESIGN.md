# Question Definition Screen - Design Specification

**Date:** December 23, 2025  
**Purpose:** Design comprehensive question editing interface for Porpoise Core  
**Status:** Design Phase - Ready for Implementation

---

## Executive Summary

Build a master-detail interface for viewing and editing all question metadata, responses, and block information. Use existing survey data (no import workflow yet).

### Key Design Decisions

✅ **Master-Detail Layout** (not inline editing)  
✅ **Responses in detail panel** (not separate screen)  
✅ **Drag-and-drop reordering** (nice-to-have, Phase 2)  
✅ **Block grouping** in master list  
❌ **NO raw data grid** (keep that separate for future Data View screen)

---

## Screen Layout

### Overall Structure

```
┌─────────────────────────────────────────────────────────────────┐
│ HEADER: "Questions - Survey Name"                    [?] [Save] │
├──────────────┬──────────────────────────────────────────────────┤
│              │                                                   │
│  MASTER      │              DETAIL PANEL                        │
│  QUESTION    │                                                   │
│  LIST        │   ┌─────────────────────────────────────────┐   │
│              │   │ Question Info                            │   │
│  [Search]    │   │ • Label, Stem, Number                   │   │
│              │   │ • Variable Type, Data Type               │   │
│  ▶ Block 1   │   │ • Missing Values                        │   │
│    Q1        │   └─────────────────────────────────────────┘   │
│    Q2  ←     │                                                   │
│    Q3        │   ┌─────────────────────────────────────────┐   │
│              │   │ Block Info (if in block)                │   │
│  ▶ Block 2   │   │ • Block Label, Stem                     │   │
│    Q4        │   └─────────────────────────────────────────┘   │
│    Q5        │                                                   │
│              │   ┌─────────────────────────────────────────┐   │
│  Q6 (alone)  │   │ Responses                               │   │
│  Q7          │   │                                          │   │
│              │   │ Value │ Label      │ Index │ Weight │ N │   │
│              │   │   1   │ [Str Agr…] │  [+]  │  [1.0] │ 45│   │
│              │   │   2   │ [Agree   ] │  [+]  │  [1.0] │ 78│   │
│              │   │   3   │ [Neutral ] │  [/]  │  [1.0] │ 23│   │
│              │   │   4   │ [Disagr…]  │  [-]  │  [1.0] │ 12│   │
│              │   │   5   │ [Str Dis…] │  [-]  │  [1.0] │  8│   │
│              │   │                              [+ Add Row]│   │
│  [166 total] │   └─────────────────────────────────────────┘   │
│              │                                                   │
│              │   Stats: Total N: 166 | Missing: 0               │
│              │                                                   │
└──────────────┴──────────────────────────────────────────────────┘
```

---

## Component Breakdown

### 1. Master List (Left Panel - 30% width)

**Components:**
- Search/filter input
- Collapsible block groups
- Question items
- Total count

**Features:**
- Click question → show in detail
- Show question number + label
- Visual block grouping (indent, connector lines)
- Highlight selected question
- Sticky search at top
- Show data type icon (red/blue/gray for Dep/Ind/Neither)

**Block Display:**
```
▼ Vital Signs (Block)           ← Collapsible header
│  Q3a: Ralph Republican        ← Block Header
│  Q3b: Debbie Democrat         ← Block Member
│  Q3c: Alan Smith              ← Block Member
```

**Standalone Question:**
```
  Q9: 2nd Ballot                 ← No block indent
```

**Question Item Details:**
- Show question number (Q3a)
- Show label (truncated if long)
- Show data type icon
- Show selection state (blue background)
- Missing data indicator (if applicable)

### 2. Detail Panel (Right Panel - 70% width)

**Three Sections (tabs or stacked):**

#### A. Question Information

**Fields (all editable):**
- **Question Number:** `Q3a` [input field]
- **Question Label:** `Ralph Republican` [input field, multi-line if needed]
- **Question Stem:** `How favorable is your opinion of...` [textarea]
- **Variable Type:** [Dropdown: Independent / Dependent / Neither]
- **Data Type:** [Dropdown: Nominal / Ordinal / Scale]
- **Missing Values:** 
  - Value 1: `99` [input]
  - Value 2: `98` [input]  
  - Value 3: `_` [input]
  - Range: Low `_` to High `_` [inputs]

#### B. Block Information (only if question is in a block)

**Fields (all editable):**
- **Block Label:** `Vital Signs` [input field]
- **Block Stem:** `Now I'd like to get your opinion of some people and organizations...` [textarea]
- **Block Status:** [Dropdown: Block Header / Block Member]

_Note: "Block Header" is the first question in a block (shows full block stem), "Block Member" continues the same block._

**Note:** Editing block label/stem affects all questions in the block

#### C. Responses Grid

**Grid Columns:**
1. **Value** (read-only, gray text) - the integer code
2. **Label** (editable) - response text
3. **Index Type** (editable dropdown) - `+ / - / / (blank)`
4. **Weight** (editable number) - default 1.0
5. **N** (read-only, calculated) - frequency count from data

**Grid Features:**
- Inline editing (click to edit)
- Auto-save on blur or debounced
- Add new response row
- Delete response (with confirmation)
- Reorder responses (drag-and-drop optional)
- Validation (no duplicate values)

**Below Grid:**
- **Stats Row:** `Total N: 166 | Missing: 0 | Valid: 166`
- **Add Response Button:** `+ Add Response Value`

---

## Data Model - Do We Need Changes?

### Current Schema (From DATA_MODEL_MAPPING.md)

**Questions Table:**
```sql
✅ QstNumber, QstLabel, QstStem - HAVE IT
✅ VariableType, DataType - HAVE IT
✅ MissValue1/2/3, MissingLow/High - HAVE IT
✅ BlockId (FK to QuestionBlocks) - HAVE IT
✅ BlkQstStatus (First/Continuing) - HAVE IT
```

**Responses Table:**
```sql
✅ RespValue (int) - HAVE IT
✅ Label - HAVE IT
✅ IndexType - HAVE IT
✅ Weight - HAVE IT (default 1.0)
❌ OriginalValue - DON'T HAVE (for import conversion)
❌ FrequencyInData (N count) - DON'T HAVE (calculated on-demand)
```

**QuestionBlocks Table:**
```sql
✅ Label, Stem - HAVE IT
✅ DisplayOrder - HAVE IT
```

### Proposed Schema Additions

**Option 1: Minimum (Do Now)**
```sql
-- Add OriginalValue for import conversion history
ALTER TABLE Responses ADD COLUMN OriginalValue VARCHAR(255);

-- Add ValueType to track original data type
ALTER TABLE Responses ADD COLUMN ValueType VARCHAR(20); -- 'Integer', 'String', 'Decimal'
```

**Option 2: Full (Do Later with Data Quality)**
```sql
-- All from Option 1, plus:

-- Cache frequency counts for performance
ALTER TABLE Responses ADD COLUMN FrequencyInData INT DEFAULT 0;
ALTER TABLE Responses ADD COLUMN LastCountedAt DATETIME;

-- Track question reordering
ALTER TABLE Questions ADD COLUMN DisplayOrder INT;
```

### Recommendation: Start with Current Schema

**Why:** 
- Current schema has everything needed for editing
- OriginalValue can be added later (only needed for import)
- Frequency can be calculated on-demand (query SurveyData table)
- No blockers to build the UI now

**Action:** Proceed with current schema, add fields in Phase 2 (Import)

---

## UX Flow

### Opening the Screen

1. User clicks "Questions" in sidebar
2. Load all questions for survey
3. Auto-select first question (or last edited)
4. Display detail panel

### Editing a Question

1. User clicks question in list
2. Detail panel loads with question data
3. User edits field (label, stem, etc.)
4. Auto-save on blur (or debounced)
5. Show save indicator (checkmark or loading)
6. Update list if question number/label changed

### Editing Responses

1. User scrolls to responses grid in detail panel
2. Click label cell → inline edit
3. Change index type dropdown
4. Change weight value
5. Auto-save each change
6. Recalculate stats (Total N, etc.)

### Editing Block

1. User clicks question that's in a block
2. Block info section appears in detail panel
3. Edit block label/stem
4. **Warning:** "This will update all 3 questions in this block"
5. Confirm and save
6. Update all questions in block

### Adding a Response

1. Click "+ Add Response Value"
2. New row appears with defaults:
   - Value: Next sequential integer (max + 1)
   - Label: Empty
   - Index: None
   - Weight: 1.0
3. User fills in label
4. Save

---

## Visual Design

### Color Coding

- **Question Types:**
  - � Blue icon: Dependent variable
  - 🔴 Red icon: Independent variable
  - ⚫ Gray icon: Neither

- **Block Questions:**
  - Indented with subtle background
  - Connector line showing hierarchy
  - Block header with collapse icon

- **Selected Question:**
  - Blue background highlight
  - Bold text

### Index Type Visual

```
Dropdown options:
┌──────────────────┐
│ + Positive       │
│ - Negative       │
│ / Neutral        │
│ (blank) None     │
└──────────────────┘
```

### Missing Value Input

```
Missing Value Codes:
[99___] [98___] [____]

Missing Value Range:
Low [____] to High [____]
```

---

## API Endpoints Needed

### Read Operations

```
GET /api/surveys/{surveyId}/questions
Response: Array of questions with blocks and responses

GET /api/surveys/{surveyId}/questions/{questionId}
Response: Single question with full details

GET /api/surveys/{surveyId}/questions/{questionId}/response-frequency
Response: Frequency count per response value (calculated from SurveyData)
```

### Write Operations

```
PUT /api/surveys/{surveyId}/questions/{questionId}
Body: { qstNumber, qstLabel, qstStem, variableType, dataType, missValue1, ... }
Response: Updated question

PUT /api/surveys/{surveyId}/questions/{questionId}/responses/{responseId}
Body: { label, indexType, weight }
Response: Updated response

POST /api/surveys/{surveyId}/questions/{questionId}/responses
Body: { respValue, label, indexType, weight }
Response: New response

DELETE /api/surveys/{surveyId}/questions/{questionId}/responses/{responseId}
Response: 204 No Content

PUT /api/surveys/{surveyId}/blocks/{blockId}
Body: { label, stem }
Response: Updated block (affects all questions in block)
```

---

## Implementation Phases

### Phase 1: Core Editing (MVP - This Week)

**Build:**
- Master list with search
- Detail panel with question info
- Response grid with inline editing
- Block info display
- Auto-save functionality

**Skip:**
- Drag-and-drop reordering
- Response reordering
- Bulk operations
- Advanced validation

### Phase 2: Enhanced UX (Next Week)

**Add:**
- Keyboard navigation (arrow keys, Enter to edit)
- Undo/redo
- Validation warnings (duplicate values, missing labels)
- Bulk edit (change index type for multiple responses)
- Copy response labels to clipboard
- Drag-and-drop question reordering

### Phase 3: Polish (Later)

**Add:**
- Response preview (show distribution chart)
- Smart suggestions (common response labels)
- History/audit trail
- Export question definitions to CSV

---

## Technical Architecture

### Component Structure

```
QuestionsView.vue (main container)
├── QuestionMasterList.vue (left panel)
│   ├── QuestionSearch.vue
│   ├── QuestionBlockGroup.vue (collapsible)
│   └── QuestionListItem.vue
│
└── QuestionDetailPanel.vue (right panel)
    ├── QuestionInfoForm.vue
    ├── BlockInfoForm.vue (conditional)
    └── ResponsesGrid.vue
        └── ResponseRow.vue
```

### State Management

**Use Composable:**
```javascript
// useQuestionEditor.js
export function useQuestionEditor(surveyId) {
  const questions = ref([])
  const selectedQuestion = ref(null)
  const loading = ref(false)
  const saving = ref(false)
  
  async function loadQuestions() { ... }
  async function updateQuestion(id, data) { ... }
  async function updateResponse(questionId, responseId, data) { ... }
  async function addResponse(questionId, data) { ... }
  async function deleteResponse(questionId, responseId) { ... }
  async function updateBlock(blockId, data) { ... }
  
  return {
    questions,
    selectedQuestion,
    loading,
    saving,
    loadQuestions,
    updateQuestion,
    updateResponse,
    addResponse,
    deleteResponse,
    updateBlock
  }
}
```

---

## Design Decisions (CONFIRMED)

1. **Response Value Editing:** ✅ **READ-ONLY**
   - Response values (integers) cannot be edited in this screen
   - Use recoding feature for value changes (prevents accidental data corruption)

2. **Block Editing Scope:** ✅ **WARNING + CONFIRMATION**
   - When editing block label/stem, show warning that it affects all questions in block
   - Require confirmation before saving

3. **Auto-save vs Manual Save:** ✅ **AUTO-SAVE**
   - Auto-save each field on blur with visual indicator (checkmark/spinner)
   - Similar to Google Docs UX

4. **Question Reordering:** ✅ **PHASE 2**
   - Drag-and-drop reordering deferred to Phase 2
   - Phase 1 focuses on editing existing question order

5. **Response Frequency (N column):** ✅ **PER-QUESTION CALCULATION**
   - Calculate N counts when question is selected
   - Cache briefly to avoid redundant queries
   - Show loading state while calculating

6. **Color Coding:** ✅ **BLUE DV, RED IV**
   - Blue icon: Dependent variable (consistent with analysis screens)
   - Red icon: Independent variable
   - Gray icon: Neither

---

## Success Metrics

**User can:**
- ✅ View all questions in survey
- ✅ Edit question labels and stems
- ✅ Edit response labels
- ✅ Change index types (+/-/neutral)
- ✅ Edit block information
- ✅ Add new response values
- ✅ See response distribution (N count)
- ✅ Changes persist to database
- ✅ Changes reflect immediately in UI

**Performance:**
- Load questions list < 1 second
- Update single field < 500ms
- Smooth scrolling in large question lists (100+ questions)

---

## Next Steps

1. **Review this design** - confirm approach
2. **Answer open questions** - make decisions
3. **Build API endpoints** - backend first
4. **Implement Phase 1 components** - frontend
5. **Test with real survey data** - validate UX
6. **Iterate based on feedback** - polish

---

**Ready to proceed?** Let me know which design decisions need adjustment!
