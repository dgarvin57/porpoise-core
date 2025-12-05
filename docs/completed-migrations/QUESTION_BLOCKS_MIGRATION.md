# Question Blocks Normalization - Implementation Summary

## Overview
Successfully normalized block information from the Questions table into a separate QuestionBlocks table, eliminating data redundancy and improving data integrity.

## Changes Made

### 1. Database Schema
- **Created `QuestionBlocks` table** with columns: Id, SurveyId, Label, Stem, DisplayOrder
- **Added `BlockId` column** to Questions table (foreign key to QuestionBlocks)
- **Removed redundant columns** from Questions: BlkLabel, BlkStem

### 2. Data Migration
**Migration Scripts:**
- `04_NormalizeQuestionBlocks.sql` - Main migration script
  - Created QuestionBlocks table
  - Migrated block data from first questions in each block
  - Linked questions to blocks via BlockId
  - Removed redundant columns

- `04b_FixBlockLinks.sql` - Fix orphaned continuation questions
  - Linked continuation questions that had empty BlkLabel values
  - Handled forward-looking references (continuation before first question)

**Data Fixes:**
- Fixed "2nd Ballot" DataFileColumn from 0 to 2 (was reading wrong data column)
- Linked all 40 orphaned continuation questions to their proper blocks

### 3. Code Changes

**New Files:**
- `Porpoise.Core/Models/QuestionBlock.cs` - Block model
- `Porpoise.Core/Application/Interfaces/IQuestionBlockRepository.cs` - Repository interface
- `Porpoise.DataAccess/Repositories/QuestionBlockRepository.cs` - Repository implementation

**Modified Files:**
- `Question.cs` - Added BlockId property, marked BlkLabel/BlkStem as [Obsolete]
- `QuestionRepository.cs` - Updated queries to LEFT JOIN with QuestionBlocks
- `SurveyPersistenceService.cs` - Import logic now creates QuestionBlock records
- `Program.cs` - Registered IQuestionBlockRepository in DI container

### 4. Import Logic Update
When importing surveys, the system now:
1. Identifies first questions in blocks (BlkQstStatus = 1)
2. Creates QuestionBlock records with Label and Stem
3. Assigns BlockId to first questions and their continuations
4. Discrete questions (status 0 or 3) have BlockId = NULL

### 5. API Behavior
- GET `/api/surveys/{id}/questions` returns block info via LEFT JOIN
- BlkLabel and BlkStem are populated from QuestionBlocks table for backward compatibility
- Frontend receives identical response structure (no changes needed)

## Results

**Database:**
- 13 QuestionBlocks created across all surveys
- 38 questions linked to blocks
- 127 discrete questions (not in blocks)

**Benefits:**
- ✅ Block stem stored only once per block (not duplicated in every question)
- ✅ Easier to update block information (single location)
- ✅ Cleaner data model with proper normalization
- ✅ Maintains backward compatibility with deprecated properties

## Testing Verified
- ✅ Demo 2015 survey displays all block stems correctly
- ✅ Porpoise Demo survey works correctly
- ✅ Response counts are accurate
- ✅ Question ordering is correct
- ✅ Import functionality ready for new surveys

## Future Work
The deprecated BlkLabel and BlkStem properties in Question.cs can be removed once all code is updated to use BlockId and the Block navigation property instead.
