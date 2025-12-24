# Session Bootstrap Checklist

Purpose: Quickly bring a new AI/dev session up to speed after context resets or memory limits.

## Quick Boot (3–5 minutes)
- Read: `.copilot-instructions.md` (workflow, CI/CD, design philosophy)
- Scan: `README.md`, `TASKS.md`, `DEPLOYMENT.md`, `RAILWAY_DEPLOYMENT.md`
- Open current focus files:
  - UI: `porpoise-ui/src/components/Analytics/QuestionsView.vue`
  - UI: `porpoise-ui/src/components/Analytics/QuestionListSelector.vue`
  - UI: `porpoise-ui/src/components/Analytics/QuestionBlockText.vue`
  - API: `Porpoise.Api` controllers/services involved in questions
  - Core/Data: `Porpoise.Core` models and `Porpoise.DataAccess` repos for `Question`, `Response`, `QuestionBlock`
- Check docs relevant to current feature:
  - `docs/QUESTION_DEFINITION_SCREEN_DESIGN.md`
  - `docs/DATA_MODEL_MAPPING.md`
  - `docs/ORCA_EXPLORATION.md`

## Current Focus (fill this as you work)
- Feature: Question Definition screen & naming consistency
- Decision context: Prefer commercial-friendly terminology; progressive disclosure for advanced stats
- Active files: [add links as you touch them]
- Open questions: [list unresolved items]
- Next actions: [small, verifiable steps]

## Commands & Tasks
- Tests (all):
```bash
dotnet test
```
- VS Code tasks:
  - `test: all` → runs full test suite
  - `test: Core` / `test: DataAccess` → scoped tests
  - `test: all with coverage` → collect coverage
  - `coverage: generate report` → HTML/text summary
  - `build: all` → build solution

## Git Workflow
- Work on `develop`; commit normally
- Use GitHub PR to merge `develop → main`
- Do not edit version files; CI/CD bumps versions & tags
- If tag conflicts: "Replace Local Tag(s)"; let hooks skip version checks

## Handoff Notes Template (paste into PR description or update here)
- What changed: [summary]
- Why: [user value]
- Where: [file links]
- Decisions: [naming/UX choices]
- Follow-ups: [next steps]

## Optional Extras
- Capture ADRs for naming in `docs/decisions/` if helpful
- Keep a short worklog per feature (e.g., `docs/worklogs/question-definition.md`)

## Branding Snapshot (keep current)
- Company Name: Porpoise Analytics
- Product Names: Porpoise Core (engine), Porpoise Analytics (web app)
- Domain Plan: porpoiseanalytics.com as primary; consider also purposeanalytics.com + redirects
- Messaging Angle: Practitioner-friendly insights; "Simple surface, rigorous depth"

Notes:
- If choosing a company name distinct from product names, prefer clear, pronounceable company brand (e.g., "Purpose Analytics") (probably not going to name the company "purpose analytics") and keep product identity ("Porpoise" and "Porpoise Analytics").
- Secure related domains and common misspellings; set up redirects.

