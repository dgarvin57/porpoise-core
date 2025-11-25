# Deleted-Presenter-Gems.md — The only things worth remembering

## 1. Project rename workflow (was in ProjectInfoPresenter)
When project name changes:
- Rename the base project folder
- Rename the .porp file
- Update Project.FullPath
- Update ResearcherLogoPath (relative → absolute)
- Remove old path from MRU list

## 2. Survey rename workflow (was in SurveyInfoPresenter)
When survey name changes:
- Rename the survey folder (under project folder)
- Rename the .porps file
- Update Survey.SurveyFolder and Survey.SurveyFileName
- Update the matching entry in Project.SurveyList

## 3. Select (filter) temporary responses
SelectedResponseObjects are **cloned** into a temporary list on the Question — they are **not** references to the master Responses collection.

## 4. Pool / Trend mode special behaviour
While Pool or Trend is on:
- DV/IV DataFileCol is temporarily overridden with pooled/trended values
- TreeView selection is disabled
- Only Crosstab tab is allowed
- Original DV/IV questions are saved and restored when toggling off

That’s literally it.

Everything else was pure WinForms/Telerik noise (resize code, ErrorProvider, BalloonToolTip, RadDock layout, etc.).

You are now completely safe to delete **every** old presenter forever.