# Survey Analytics Screen – Everything the UI Must Connect To

This is the complete list of properties, events, and methods the old WinForms UI expected.
Use this as a checklist when building the new Blazor/MAUI version — nothing important gets forgotten.

## Core Objects
- ProjectObject
- SurveyObject
- SelectedQuestionObject (DV)
- SelectedResponseObjects

## Main Windows / Tabs
- ResultsTab
- CrossTab
- Index, IndexPlus, StatSig
- Profile, OneResp
- FullBlock, TargetBlock, TwoBlock
- Preference
- Univariate, Bivariate

## Key Events the UI Must Raise
- LoadFormView / OnSurveyViewShown
- SelectDVEvent / SelectIVEvent
- ToggleSelectState / ToggleSelectPlusState / ToggleWeightState
- TogglePoolState / ToggleTrendState
- ShowSelectOnSetup / ShowSelectPlusOnSetup / ShowWeightOnSetup
- TopLineReportEvent / NotesReportEvent
- etc.

## Methods the UI Must Call
- OpenSelectOnSetupView()
- OpenSelectPlusOnSetupView()
- OpenWeightOnSetupView()
- OpenTopLineProgressIndicatorView()