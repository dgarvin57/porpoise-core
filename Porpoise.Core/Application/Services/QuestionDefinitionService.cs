// Porpoise.Core/Application/Services/QuestionDefinitionService.cs
#nullable enable

using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using System;
using System.Linq;

namespace Porpoise.Core.Application.Services;

public class QuestionDefinitionService : ApplicationServiceBase
{
    private readonly IQuestionDefinitionShell _view;
    private readonly RecentProjectsService _mru;
    private int _lastValidPosition = 0;

    public QuestionDefinitionService(
        IQuestionDefinitionShell view,
        IApplicationShell shell,
        RecentProjectsService mru)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _mru = mru;
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
        _view.OnClosing += OnClosing;
        _view.OnSave += OnSave;
        _view.OnQuestionChanged += OnQuestionChanged;
        _view.OnBlockStatusChanged += OnBlockStatusChanged;
        _view.OnSeeData += OnSeeData;
        _view.OnApplyChanges += OnApplyChanges;
        _view.OnSaveTemplate += OnSaveTemplate;
        _view.OnLoadTemplate += OnLoadTemplate;
        _view.OnUseAltPosNegLabels += OnUseAltPosNegLabels;
        _view.OnPreferenceBlockChanged += OnPreferenceBlockChanged;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            Shell.Hourglass(true);

            _view.Survey.Data.SelectOn = false;
            _view.Survey.Data.SelectPlusOn = false;

            _view.BindSurvey(_view.Survey);
            _view.BindQuestions(_view.Survey.QuestionList);

            _view.SetTitle(_view.Survey.SurveyName);
            _view.SetDataFilePath(_view.Survey.Data.DataFilePath);
            _view.ShowExported(_view.Project.IsExported);

            ValidateAllQuestions();
            _view.FocusFirstQuestion();
        }
        catch (Exception ex)
        {
            HandleError(ex, "Error loading question definition.");
        }
        finally
        {
            Shell.Hourglass(false);
        }
    }

    private void OnClosing(object? sender, FormClosingEventArgs e)
    {
        if (_view.IsDirty)
        {
            var result = Shell.AskYesNoCancel("Save changes before closing?", "Question Definition");
            if (result == DialogResult.Yes)
                OnSave(sender, EventArgs.Empty);
            else if (result == DialogResult.Cancel)
                e.Cancel = true;
        }
    }

    private void OnSave(object? sender, EventArgs e)
    {
        try
        {
            Shell.Hourglass(true);

            SyncBlockData();
            if (!ValidateAllQuestions()) return;

            if (!TestForAllMissingValues()) return;

            QuestionEngine.SetModifiedFlags(_view.Survey.QuestionList);
            SurveyEngine.SaveSurvey(_view.Survey, _view.Project.IsExported);

            ProjectEngine.UpdateSurvey(_view.Project, _view.Survey);
            _view.SaveProject();

            _view.MarkClean();
            _view.EnableSaveTemplate(!_view.Survey.ErrorsExist);
        }
        catch (Exception ex)
        {
            HandleError(ex, "Error saving question definition.");
        }
        finally
        {
            Shell.Hourglass(false);
        }
    }

    private void OnQuestionChanged(object? sender, EventArgs e)
    {
        var question = _view.CurrentQuestion;
        if (question == null) return;

        _lastValidPosition = _view.CurrentQuestionIndex;

        _view.BindResponses(question.Responses);
        _view.SetVariableType(question.VariableType);
        _view.SetDataType(question.DataType);
        _view.SetBlockStatus(question.BlkQstStatus);
        _view.SetColumnNumber(question.DataFileCol, _view.QuestionCount);

        HighlightBlock();
        UpdatePreferenceBlockUI();
        ValidateCurrentQuestion();
    }

    private void OnBlockStatusChanged(object? sender, EventArgs e)
    {
        SyncBlockData();
        HighlightBlock();
    }

    private void OnSeeData(object? sender, EventArgs e)
    {
        CommonApplicationMethods.SeeSurveyData(_view.Survey.OrigDataFilePath, Shell, _view.Project.IsExported);
    }

    private void OnApplyChanges(WhatToChange changes)
    {
        var current = _view.CurrentQuestion;
        var selected = _view.GetSelectedQuestions();
        var inBlock = current.BlkQstStatus != BlkQuestionStatusType.DiscreetQuestion;

        var result = _view.ShowApplyChangesDialog(current, inBlock, selected.Count > 1, changes);
        if (result != DialogResult.OK) return;

        var changed = QuestionEngine.ApplyChanges(current, _view.Survey, result.ApplyTo, changes, selected);
        _view.RefreshQuestions();
        ShowChangesMessage(changed);
    }

    private void OnSaveTemplate(object? sender, EventArgs e)
    {
        if (_view.Survey.ErrorsExist)
        {
            Shell.ShowMessage("Fix all errors before saving as template.", "Save Template");
            return;
        }

        if (_view.ShowSaveFileDialog("Template files|*.xml", out var path))
        {
            QuestionEngine.SaveTemplate(_view.Survey.QuestionList, path);
            Shell.ShowStatus($"Template saved: {Path.GetFileName(path)}");
        }
    }

    private void OnLoadTemplate(object? sender, EventArgs e)
    {
        if (_view.ShowOpenFileDialog("Template files|*.xml", out var path))
        {
            QuestionEngine.ApplyTemplate(_view.Survey, path);
            _view.RefreshQuestions();
            Shell.ShowStatus($"Template loaded: {Path.GetFileName(path)}");
        }
    }

    private void OnUseAltPosNegLabels(object? sender, bool enabled)
    {
        _view.SetAltLabelsEnabled(enabled);
    }

    private void OnPreferenceBlockChanged(object? sender, bool isPreference)
    {
        if (isPreference)
            QuestionEngine.DefaultResponseIndexes(_view.CurrentQuestion, _view.Survey.QuestionList);

        UpdatePreferenceBlockUI();
    }

    private void SyncBlockData()
    {
        var current = _view.CurrentQuestion;
        if (current == null) return;

        var changed = QuestionEngine.SyncBlockData(current, _view.Survey.QuestionList);
        if (changed != null && Shell.SettingsConfirmBlockDataChanges)
        {
            var result = _view.ShowApplyChangesDialog(current, true, false, changed.WhatToChange);
            if (result == DialogResult.OK)
            {
                QuestionEngine.ApplyChanges(current, _view.Survey, result.ApplyTo, changed.WhatToChange, null);
                _view.RefreshQuestions();
            }
        }
    }

    private void HighlightBlock()
    {
        var inBlock = _view.CurrentQuestion?.BlkQstStatus != BlkQuestionStatusType.DiscreetQuestion;
        _view.ShowBlockMode(inBlock);
    }

    private void UpdatePreferenceBlockUI()
    {
        var q = _view.CurrentQuestion;
        if (q == null) return;

        var block = QuestionEngine.GetQuestionsInBlock(q, _view.Survey.QuestionList);
        var eligible = block != null && (block.Count == 6 || block.Count == 10 || block.Count == 15);

        _view.SetPreferenceEligible(eligible, block?.Count ?? 0);
        if (eligible) _view.BindPreferenceItems(q.PreferenceItems);
    }

    private bool ValidateAllQuestions()
    {
        var valid = QuestionEngine.ValidateAllQuestions(_view.Survey.QuestionList, out var errors);
        _view.ShowValidationErrors(errors);
        _view.Survey.ErrorsExist = !valid;
        return valid;
    }

    private void ValidateCurrentQuestion()
    {
        var errors = QuestionEngine.ValidateQuestion(_view.CurrentQuestion);
        _view.ShowValidationErrors(errors);
    }

    private bool TestForAllMissingValues()
    {
        try
        {
            return _view.Survey.IsAllResponsesInQuestionMissingValuesOK();
        }
        catch (SurveyColumnAllMissingValuesException ex)
        {
            Shell.ShowMessage(ex.Message, "Data Error");
            return false;
        }
    }

    private void ShowChangesMessage(WhatChanged? changes)
    {
        if (changes?.HasChanges == true)
        {
            Shell.ShowInfo(changes.MessageTitle, changes.MessageDetail ?? "");
        }
    }
}