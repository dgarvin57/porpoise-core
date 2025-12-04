// Porpoise.Core/Application/Services/CreateSurveyWizardService.cs
#nullable enable

using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Orchestrates the entire "Create New Survey" wizard
/// Handles project creation, data file selection, Orca import, templates, missing values
/// Fully decoupled from WinForms/Telerik
/// </summary>
public class CreateSurveyWizardService : ApplicationServiceBase
{
    private readonly ICreateSurveyWizardShell _view;
    private List<Question>? _questionTemplate;
    private bool _templatesExist;

    public CreateSurveyWizardService(ICreateSurveyWizardShell view, IApplicationShell shell)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
        _view.OnPageChanged += OnPageChanged;
        _view.OnNext += OnNext;
        _view.OnFinish += OnFinish;
        _view.OnSeeData += OnSeeData;
        _view.OnUseNewDefinition += (_) => UpdateTemplateControls();
        _view.OnUseTemplate += (_) => UpdateTemplateControls();
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            ConfigureWizardStart();
            _templatesExist = QuestionEngine.IsTemplateXMLFilesExist(Shell.SettingsBaseTemplateDir);
            UpdateSeeSurveyDataLink();
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred loading the new survey wizard.");
        }
    }

    private void ConfigureWizardStart()
    {
        switch (_view.WizardType)
        {
            case CreateWizardType.CreateProjectAndSurvey:
                _view.GoToPage(0);
                break;
            case CreateWizardType.JustCreateSurvey:
                _view.GoToPage(2);
                _view.HideBackButton();
                break;
            case CreateWizardType.ImportFromOrcaIntoExistingProject:
                _view.GoToPage(2);
                _view.HideBackButton();
                SetupOrcaImport();
                break;
            case CreateWizardType.ImportFromOrcaIntoNewProject:
                _view.GoToPage(0);
                SetupOrcaImport();
                break;
        }
    }

    private void SetupOrcaImport()
    {
        if (_view.OrcaExportObject != null)
        {
            _view.SurveyName = _view.OrcaExportObject.FileName;
            _view.Survey.DataFileName = Path.GetFileName(_view.OrcaExportObject.CSVPath);
            _view.DataFilePath = _view.OrcaExportObject.CSVPath;
            _view.OrcaXmlPath = _view.OrcaExportObject.XMLPath;
            // Initialize survey data if needed
            if (_view.Survey.Data == null)
                _view.Survey.Data = new SurveyData();
            _view.Survey.Data.DataFilePath = _view.OrcaExportObject.CSVPath;
            _view.DisableDataFileBrowser();
        }
    }

    private void OnPageChanged(object? sender, WizardPageChangedEventArgs e)
    {
        // Focus management
        if (e.PageIndex == 1) _view.FocusProjectName();
        else if (e.PageIndex == 2) _view.FocusSurveyName();
    }

    private async void OnNext(object? sender, WizardNextEventArgs e)
    {
        try
        {
            if (_view.CurrentPage == 1) // Project page
            {
                if (!ValidateProject()) { e.Cancel = true; return; }
                CreateProjectFromInput();
            }
            else if (_view.CurrentPage == 2) // Survey page
            {
                if (!ValidateSurvey()) { e.Cancel = true; return; }
                if (!await LoadSurveyDataAsync()) { e.Cancel = true; return; }

                if (!_templatesExist || _view.IsOrcaImport)
                    _view.GoToPage(4); // Skip template
            }
            else if (_view.CurrentPage == 3) // Template page
            {
                if (_view.UseTemplate && string.IsNullOrEmpty(_view.TemplatePath))
                {
                    Shell.ShowMessage("Please select a template file.", "Template Required");
                    e.Cancel = true;
                    return;
                }

                if (_view.UseTemplate)
                    await LoadTemplateAsync();
            }
            else if (_view.CurrentPage == 4) // Missing values / final data page
            {
                ApplyMissingValues();
                await FinalizeQuestionsAsync();
            }
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred advancing in the wizard.");
            e.Cancel = true;
        }
    }

    private void OnFinish(object? sender, EventArgs e)
    {
        try
        {
            // Update survey in project
            var index = _view.Project.SurveyList.FindIndex(s => s.Id == _view.Survey.Id);
            if (index >= 0)
                _view.Project.SurveyList[index] = _view.Survey;
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred finalizing the survey.");
        }
    }

    private void OnSeeData(object? sender, EventArgs e)
    {
        if (File.Exists(_view.DataFilePath))
            CommonApplicationMethods.SeeSurveyData(_view.DataFilePath, Shell, _view.Project.IsExported);
        else
            Shell.ShowMessage($"Data file not found: {_view.DataFilePath}", "View Data");
    }

    private void UpdateTemplateControls()
    {
        _view.SetTemplateEnabled(_view.UseTemplate);
    }

    private void UpdateSeeSurveyDataLink()
    {
        if (!string.IsNullOrEmpty(_view.DataFilePath))
        {
            var label = $"View survey data ({Path.GetFileName(_view.DataFilePath)})";
            _view.UpdateDataLinks(label, _view.DataFilePath);
        }
    }

    private bool ValidateProject() => ValidateRequired(_view.ProjectName, "Project name") && ValidateRequired(_view.ClientName, "Client name");
    private bool ValidateSurvey() => ValidateRequired(_view.SurveyName, "Survey name") && ValidateDataFile();

    private bool ValidateRequired(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Shell.ShowMessage($"'{field}' is required.", "Validation");
            return false;
        }
        return true;
    }

    private bool ValidateDataFile()
    {
        if (string.IsNullOrWhiteSpace(_view.DataFilePath) || !File.Exists(_view.DataFilePath))
        {
            Shell.ShowMessage("Please select a valid survey data file (.csv).", "Data File Required");
            return false;
        }
        return true;
    }

    private void CreateProjectFromInput()
    {
        _view.Project.ProjectName = _view.ProjectName;
        _view.Project.ProjectFolder = _view.ProjectName;
        _view.Project.FullFolder = Path.Combine(Shell.SettingsBaseProjectDir, _view.ProjectName);
        _view.Project.FileName = $"{_view.ProjectName}.porp";
        _view.Project.FullPath = Path.Combine(_view.Project.FullFolder, _view.Project.FileName);
    }

    private async Task<bool> LoadSurveyDataAsync()
    {
        // Initialize survey data if needed
        if (_view.Survey.Data == null)
            _view.Survey.Data = new SurveyData();
        _view.Survey.Data.DataFilePath = _view.DataFilePath;

        return await Task.Run(() => SurveyEngine.LoadSurveyData(_view.Survey, true, _view.Project.IsExported));
    }

    private async Task LoadTemplateAsync()
    {
        var (valid, template, error) = await Task.Run(() => QuestionEngine.GetQuestionTemplate(_view.TemplatePath));
        if (!valid)
        {
            Shell.ShowMessage(error ?? "Invalid template file.", "Template Error");
            return;
        }
        _questionTemplate = template;
    }

    private void ApplyMissingValues()
    {
        var misses = new List<int>();
        if (int.TryParse(_view.MissValue1, out var m1)) misses.Add(m1);
        if (int.TryParse(_view.MissValue2, out var m2)) misses.Add(m2);
        if (int.TryParse(_view.MissValue3, out var m3)) misses.Add(m3);
        _view.Survey.Data.MissingResponseValues = misses;
    }

    private async Task FinalizeQuestionsAsync()
    {
        await Task.Run(() =>
        {
            SurveyEngine.LoadDataIntoQuestions(_view.Survey, _view.OrcaXmlPath);

            if (_questionTemplate != null)
            {
                var msg = "";
                SurveyEngine.LoadTemplateIntoQuestion(_questionTemplate, Path.GetFileName(_view.TemplatePath), _view.Survey, ref msg);
                if (!string.IsNullOrEmpty(msg))
                    Shell.ShowMessage(msg, "Template Applied");
            }
        });
    }
}