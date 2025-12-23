# Questions for Bereket & Val - Orca Integration

## Background
We're planning to integrate data preparation features from Orca (2014) into Porpoise Core. Porpoise already supports editing question/response labels, but Orca has:
- Better UX for response labeling (grid view)
- **Recoding functionality** (change data values across dataset)
- Other data prep features that may be valuable

**Key Document:** [ORCA_EXPLORATION.md](./ORCA_EXPLORATION.md)

---

## 🎯 Strategic Questions

### Priority & Scope
1. **What's the biggest pain point with the current question/data workflow?**
   - Is it labeling responses? Editing data? Something else?

2. **Is the current response labeling workflow sufficient?**
   - Do users find it easy enough?
   - Would Orca's grid view (all values at once) be better?
   - Any pain points with current approach?

3. **Which Orca features would add the most value?**
   Rank these (note: response labeling already exists in Porpoise):
   - [ ] Better response labeling UX (grid view like Orca)
   - [ ] **Recoding** (change data values across entire dataset) ⭐ Missing feature
   - [ ] Data quality metrics (% clean, validation alerts)
   - [ ] Data editing (fix values in spreadsheet view)
   - [ ] Block question management (first/continuing distinction)
   - [ ] Version control (snapshots for rollback)

### User Workflow
4. **What does a typical survey setup workflow look like?**
   - Import CSV → ... → Start analyzing
   - Where does data prep fit in?

5. **How often do users need to edit data AFTER import?**
   - Never / Rarely / Sometimes / Frequently / Always

6. **Do users import "dirty" data or is it usually clean?**
   - How common are: string values, nulls, decimals, typos

---

## 🔧 Technical Questions

### Current Schema
7. **Current response label schema - is it sufficient?**
   - We know labels are stored, but is structure optimal?
   - Any limitations with current approach?
   - Ready for recoding feature (tracking old→new values)?

8. **What's the current structure for question responses?**
   - Are unique values already stored?
   - Do we track counts per value?

### Data Editing Concerns
9. **If we allow data editing, what are the risks?**
   - Data integrity concerns?
   - Audit trail requirements?
   - Multi-user conflicts?

10. **Should data editing be:**
    - [ ] Always allowed
    - [ ] Only before first analysis
    - [ ] Only for admins
    - [ ] Never (read-only forever)

### Performance
11. **What's the typical survey size?**
    - Number of respondents (rows)?
    - Number of questions (columns)?
    - Largest dataset we need to support?

12. **Can we load full datasets in browser?**
    - Or do we need server-side pagination?
    - What's acceptable performance?

---

## 🎨 UX Questions

### Question Definition Screen
13. **Should question editing be:**
    - [ ] Modal dialog (like Orca)
    - [ ] Side panel / drawer
    - [ ] Full-page view
    - [ ] Inline in question list

14. **What info should be visible in the question list?**
    - Question number + label (current)
    - Response count?
    - "Needs attention" indicators?
    - Data quality metrics?

### Response Labeling
15. **Should we improve response labeling UX?**
    - [ ] Add Orca-style grid view (all values visible at once)
    - [ ] Keep current approach (it works)
    - [ ] Add bulk operations (label multiple at once)
    - [ ] Add AI-suggested labels

16. **Should labeling be required before analysis?**
    - Or optional enhancement?
    - Validation/warnings if unlabeled?

---

## 📊 Data Model Questions

17. **Block questions: Do we need the First/Continuing distinction?**
    - Orca had this to manage question number changes
    - Is visual grouping enough?
    - Use case for functional distinction?

18. **Should we support question renumbering?**
    - Change Q3a → Q4a (with cascade to data)?
    - Or question numbers are immutable after import?

19. **Missing value codes: How should we handle them?**
    - Global setting per survey (e.g., 99 = missing)?
    - Per-question override?
    - Multiple missing codes (99=Refused, 98=Don't Know)?

---

## 🚀 Implementation Questions

### Phase 1 Scope
20. **Is recoding a priority feature?**
    - How often do users need to change data values?
    - Use cases: fixing typos, collapsing categories, standardizing codes
    - Worth the complexity vs. just re-importing clean data?

21. **What's the MVP for Orca integration?**
    - Better response labeling UX?
    - Recoding functionality?
    - Data quality dashboard?
    - Full data editing?

21. **Timeline expectations?**
    - Quick win in days/weeks?
    - Proper solution in months?

### Testing
22. **Do you have sample "dirty" datasets for testing?**
    - Data with strings, nulls, decimals
    - Real-world examples to test against

23. **Any existing users we can beta test with?**
    - Get feedback early before full rollout

---

## 🤔 Open Questions

24. **Orca vs Porpoise PC - any features from PC we should keep/migrate?**
    - Original Porpoise had 11+ years of evolution
    - Anything we're missing from that era?

25. **Multi-language support?**
    - Do surveys need response labels in multiple languages?
    - Question text translation?

26. **Export functionality needed?**
    - Export clean data + labels?
    - SPSS format? Other analytics tools?

---

## 📝 Action Items Based on Answers

**After discussing above:**
- [ ] Prioritize feature list
- [ ] Define MVP scope
- [ ] Map current schema → required changes
- [ ] Create UI mockups for top features
- [ ] Estimate effort for Phase 1
- [ ] Plan migration for existing data

---

**Prepared:** December 23, 2025  
**Context:** Integrating Orca data prep features into Porpoise Core  
**Status:** Ready for team discussion
