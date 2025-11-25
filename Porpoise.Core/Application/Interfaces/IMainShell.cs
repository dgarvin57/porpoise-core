// Porpoise.Core/Application/Interfaces/IMainShell.cs
#nullable enable

using System;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

public class DockWindowEventArgs : EventArgs
{
    public string WindowName { get; }
    public DockWindowEventArgs(string name) => WindowName = name;
}

public interface IMainShell
{
    Project? Project { get; set; }
    bool SelectOn { get; }
    bool SelectPlusOn { get; }

    void SetTitle(string title);
    void SetProjectMenuEnabled(bool enabled);
    void SetSurveyMenuEnabled(bool enabled);
    void SetFeatureEnabled(string feature, bool enabled);
    void SetStatusIndicator(string label, bool active);
    void RefreshProjectView();
    void RefreshSurveyList();
    void AddToMru(string path);
    void ShowStatus(string message);
    void OpenQuestionView(Survey survey);
    void OpenAnalyticsView(Survey survey);
    void CloseSurveyViews(Guid surveyId);
    void CancelOperation();

    bool ShowOpenProjectDialog(out string path);
    bool ShowCreateWizard(Project project, Survey survey, CreateWizardType type, OrcaExport? orca);
    void ShowPoolResults(PoolTrendItem item);
    void ShowTrendResults(PoolTrendItem item);
    void RefreshRecentProjects();

    event EventHandler OnLoad;
    event EventHandler<FormClosingEventArgs> OnViewClosing;
    event EventHandler OnCreateNewProject;
    event EventHandler OnOpenProject;
    event EventHandler OnCloseProject;
    event EventHandler OnAddNewSurvey;
    event EventHandler OnSaveProject;
    event Action<string> OnExportProject;
    event Action<string> OnImportProject;
    event EventHandler OnImportFromOrca;
    event Action<Guid> OnOpenSurveyDefinition;
    event Action<Guid> OnOpenSurveyAnalytics;
    event Action<Guid> OnRemoveSurvey;
    event EventHandler OnShowHelpContents;
    event EventHandler OnShowHelpIndex;
    event EventHandler<DockWindowEventArgs> OnActiveWindowChanged;
    event Action<bool> OnToggleSelect;
    event Action<bool> OnToggleSelectPlus;
    event Action<bool> OnToggleSimulation;
    event Action<bool> OnTogglePool;
    event Action<bool> OnToggleTrend;
    event Action<PoolTrendItem> OnShowPoolResults;
    event Action<PoolTrendItem> OnShowTrendResults;
    event Action<string> OnRecentProjectSelected;
}