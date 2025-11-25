// Porpoise.Core/Application/Services/MainApplicationService.cs
#nullable enable

using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using System;
using System.IO;
using System.Linq;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// The main application coordinator — orchestrates everything in the desktop app
/// This is the "main window presenter" — the most important service
/// Fully decoupled from WinForms/Telerik
/// </summary>
public class MainApplicationService : ApplicationServiceBase
{
    private readonly IMainShell _view;
    private bool _simIsOn;
    private bool _poolIsOn;
    private bool _trendIsOn;
    private readonly RecentProjectsService _recentProjects;

    public MainApplicationService(IMainShell view, IApplicationShell shell)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));

        // Initialize services
        _recentProjects = new RecentProjectsService(_view);

        // Wire up events
        RegisterEvents();

        // Subscribe to recent projects
        _recentProjects.ProjectSelected += OnRecentProjectSelected;
    }

    private void OnRecentProjectSelected(string projectPath)
    {
        try
        {
            Shell.Hourglass(true);
            SaveIfDirty("opening a recent project");

            var project = ProjectEngine.LoadProject(projectPath);
            _view.Project = project;
            _view.RefreshProjectView();
            UpdateMenuStates();
        }
        catch (Exception ex)
        {
            HandleError(ex, $"Failed to open recent project: {projectPath}");
            _recentProjects.Remove(projectPath);
        }
        finally
        {
            Shell.Hourglass(false);
        }
    }
    
    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
        _view.OnViewClosing += OnViewClosing;
        _view.OnCreateNewProject += OnCreateNewProject;
        _view.OnOpenProject += OnOpenProject;
        _view.OnCloseProject += OnCloseProject;
        _view.OnAddNewSurvey += OnAddNewSurvey;
        _view.OnSaveProject += OnSaveProject;
        _view.OnExportProject += OnExportProject;
        _view.OnImportProject += OnImportProject;
        _view.OnImportFromOrca += OnImportFromOrca;
        _view.OnOpenSurveyDefinition += OnOpenSurveyDefinition;
        _view.OnOpenSurveyAnalytics += OnOpenSurveyAnalytics;
        _view.OnRemoveSurvey += OnRemoveSurvey;
        _view.OnShowHelpContents += () => ShowHelp(HelpNavigator.TableOfContents);
        _view.OnShowHelpIndex += () => ShowHelp(HelpNavigator.Index);
        _view.OnActiveWindowChanged += OnActiveWindowChanged;
        _view.OnToggleSelect += OnToggleSelect;
        _view.OnToggleSelectPlus += OnToggleSelectPlus;
        _view.OnToggleSimulation += OnToggleSimulation;
        _view.OnTogglePool += OnTogglePool;
        _view.OnToggleTrend += OnToggleTrend;
        _view.OnShowPoolResults += item => _view.ShowPoolResults(item);
        _view.OnShowTrendResults += item => _view.ShowTrendResults(item);
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            _view.SetTitle("New Project - Porpoise Survey Analytics");
            UpdateMenuStates();
            EnsureDirectoriesExist();
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred loading the main application.");
        }
    }

    private void OnViewClosing(object? sender, FormClosingEventArgs e)
    {
        if (_view.Project?.IsDirty == true)
        {
            var result = Shell.AskYesNoCancel(
                $"Project '{_view.Project.ProjectName}' has changed. Save before closing?",
                "Save Project");

            switch (result)
            {
                case DialogResult.Yes:
                    OnSaveProject(sender, EventArgs.Empty);
                    if (_view.Project.IsDirty) e.Cancel = true;
                    break;
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }
    }

    private void OnCreateNewProject(object? sender, EventArgs e)
    {
        SaveIfDirty("creating a new project");
        var newProject = new Project(Shell.SettingsBaseProjectDir);
        var newSurvey = new Survey();
        newProject.SurveyList.Add(newSurvey);

        if (_view.ShowCreateWizard(newProject, newSurvey, CreateWizardType.CreateProjectAndSurvey, null) == DialogResult.OK)
        {
            _view.Project = newProject;
            OnSaveProject(sender, e);
            _view.RefreshProjectView();
            _view.OpenQuestionView(newSurvey);
        }

        UpdateMenuStates();
    }

    private void OnOpenProject(object? sender, EventArgs e)
    {
        SaveIfDirty("opening another project");

        if (_view.ShowOpenProjectDialog(out var path))
        {
            try
            {
                var project = ProjectEngine.LoadProject(path);
                _view.Project = project;
                _view.AddToMru(path);
                _view.RefreshProjectView();
                UpdateMenuStates();
            }
            catch (Exception ex)
            {
                HandleError(ex, $"Failed to open project: {path}");
            }
        }
    }

    private void OnCloseProject(object? sender, EventArgs e)
    {
        SaveIfDirty("closing project");
        _view.Project = null;
        _view.ClearSurveyList();
        UpdateMenuStates();
    }

    private void OnAddNewSurvey(object? sender, EventArgs e)
    {
        if (_view.Project == null)
        {
            Shell.ShowMessage("Create or open a project first.", "Add Survey");
            return;
        }

        var newSurvey = new Survey();
        _view.Project.SurveyList.Insert(0, newSurvey);

        if (_view.ShowCreateWizard(_view.Project, newSurvey, CreateWizardType.JustCreateSurvey, null) == DialogResult.OK)
        {
            _view.Project.IsDirty = true;
            _view.RefreshSurveyList();
            _view.OpenQuestionView(newSurvey);
        }
        else
        {
            _view.Project.SurveyList.Remove(newSurvey);
        }
    }

    private void OnSaveProject(object? sender, EventArgs e)
    {
        try
        {
            ProjectEngine.SaveProject(_view.Project);
            _view.AddToMru(_view.Project.FullPath);
            _view.RefreshProjectView();
            Shell.ShowStatus($"Project saved: {_view.Project.ProjectName}");
        }
        catch (Exception ex)
        {
            HandleError(ex, "Failed to save project.");
        }
    }

    private void OnExportProject(string exportPath)
    {
        // Implementation using ProjectEngine.ExportProject
        ProjectEngine.ExportProject(_view.Project, exportPath);
        Shell.ShowMessage($"Project exported to {exportPath}", "Export Complete");
    }

    private void OnImportProject(string importPath)
    {
        // Implementation using ProjectImportService
        var importer = new ProjectImportService();
        var project = importer.ImportPorpFile(importPath);
        _view.Project = project;
        _view.RefreshProjectView();
    }

    private void OnOpenSurveyDefinition(Guid surveyId)
    {
        var survey = _view.Project.SurveyList.FirstOrDefault(s => s.Id == surveyId);
        if (survey != null)
            _view.OpenQuestionView(survey);
    }

    private void OnOpenSurveyAnalytics(Guid surveyId)
    {
        var survey = _view.Project.SurveyList.FirstOrDefault(s => s.Id == surveyId);
        if (survey != null)
            _view.OpenAnalyticsView(survey);
    }

    private void OnRemoveSurvey(Guid surveyId)
    {
        var survey = _view.Project.SurveyList.FirstOrDefault(s => s.Id == surveyId);
        if (survey != null && Shell.AskYesNo($"Remove survey '{survey.SurveyName}'?", "Confirm Remove"))
        {
            _view.CloseSurveyViews(surveyId);
            _view.Project.SurveyList.Remove(survey);
            _view.Project.IsDirty = true;
            _view.RefreshSurveyList();
        }
    }

    private void OnToggleSelect(bool enabled) => UpdateFeatureState(ref _view.SelectOn, enabled, "SELECT");
    private void OnToggleSelectPlus(bool enabled) => UpdateFeatureState(ref _view.SelectPlusOn, enabled, "SELECT+");
    private void OnToggleSimulation(bool enabled) => UpdateFeatureState(ref _simIsOn, enabled, "SIMULATION");
    private void OnTogglePool(bool enabled) => UpdateFeatureState(ref _poolIsOn, enabled, "POOL");
    private void OnToggleTrend(bool enabled) => UpdateFeatureState(ref _trendIsOn, enabled, "TREND");

    private void UpdateFeatureState(ref bool flag, bool enabled, string label)
    {
        flag = enabled;
        _view.SetStatusIndicator(label, enabled);
        UpdateConflictingFeatures();
    }

    private void UpdateConflictingFeatures()
    {
        var anyOn = _simIsOn || _poolIsOn || _trendIsOn;
        _view.SetFeatureEnabled("SELECT", !anyOn);
        _view.SetFeatureEnabled("SIMULATION", !anyOn);
    }

    private void OnActiveWindowChanged(object? sender, DockWindowEventArgs e)
    {
        var isSurveyView = e.WindowName.Contains("SurveyView");
        _view.SetSurveyMenuEnabled(isSurveyView);
        UpdateConflictingFeatures();
    }

    private void SaveIfDirty(string action)
    {
        if (_view.Project?.IsDirty == true)
        {
            var result = Shell.AskYesNoCancel($"Save project before {action}?", "Save Project");
            if (result == DialogResult.Yes) OnSaveProject(null, EventArgs.Empty);
            if (result == DialogResult.Cancel) _view.CancelOperation();
        }
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(Shell.SettingsBaseProjectDir);
        Directory.CreateDirectory(Shell.SettingsBaseTemplateDir);
    }

    private void UpdateMenuStates()
    {
        var hasProject = _view.Project != null && !string.IsNullOrEmpty(_view.Project.ProjectName);
        _view.SetProjectMenuEnabled(hasProject);
        _view.SetTitle(hasProject ? $"{_view.Project.ProjectName} - Porpoise" : "New Project - Porpoise");
    }

    private void ShowHelp(HelpNavigator navigator)
    {
        ShowHelpFile(null, navigator, null);
    }
}