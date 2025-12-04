// Porpoise.Core/Application/Services/PoolTrendSetupService.cs
#nullable enable

using System;
using System.Linq;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Handles the entire Pool/Trend setup wizard
/// Select surveys, order them, select DV/IV questions, validate consistency
/// Fully decoupled from WinForms/Telerik
/// </summary>
public class PoolTrendSetupService : ApplicationServiceBase
{
    private readonly IPoolTrendSetupShell _view;
    private readonly PoolTrendManager _manager;
    private readonly IQuestionTreeShell _questionTree;
    private readonly IResultsShell _dvResults;
    private readonly IResultsShell _ivResults;

    public PoolTrendSetupService(
        IPoolTrendSetupShell view,
        IApplicationShell shell,
        IQuestionTreeShell questionTree,
        IResultsShell dvResults,
        IResultsShell ivResults)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _questionTree = questionTree;
        _dvResults = dvResults;
        _ivResults = ivResults;

        _manager = new PoolTrendManager(_view.PoolTrendList);
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
        _view.OnNext += OnNext;
        _view.OnPrevious += OnPrevious;
        _view.OnFinish += OnFinish;
        _view.OnSurveyChecked += OnSurveyChecked;
        _view.OnSurveyMoved += OnSurveyMoved;
        _view.OnResetOrder += OnResetOrder;
        _view.OnClearSurveys += OnClearSurveys;
        _view.OnDVSelected += OnDVSelected;
        _view.OnIVSelected += OnIVSelected;
        _view.OnClearQuestions += OnClearQuestions;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            Shell.Hourglass(true);
            ConfigureWizard();
            LoadSurveys();
            _questionTree.Initialize(_view.SelectOnEnabled);
            ButtonsState();
        }
        catch (Exception ex)
        {
            HandleError(ex, "Error loading Pool/Trend setup.");
        }
        finally
        {
            Shell.Hourglass(false);
        }
    }

    private void ConfigureWizard()
    {
        _view.SetTitle(_view.Type == PoolTrendType.Pool ? "Pool Setup" : "Trend Setup");
        _view.SetIcon(_view.Type == PoolTrendType.Pool ? _view.PoolIcon : _view.TrendIcon);

        if (_view.Type == PoolTrendType.Trend)
        {
            _view.ShowIVResults(false);
            _view.ShowReorderControls(true);
        }
    }

    private void LoadSurveys()
    {
        _view.BindSurveys(_view.PoolTrendList.SurveyList);
        _view.ResetSurveyOrder(); // Sort by CreatedOn
        FormatSurveyItems();
    }

    private void OnNext(object? sender, WizardNextEventArgs e)
    {
        if (_view.CurrentPage == 0) // Survey selection
        {
            if (!ValidateSurveySelection()) { e.Cancel = true; return; }
            _view.BindQuestionsPage(_view.GetSelectedSurveys());
            LoadCurrentSurveyQuestions();
        }
        else if (_view.CurrentPage == 1) // Question selection
        {
            if (_view.HasMoreSurveys)
            {
                _view.MoveToNextSurvey();
                LoadCurrentSurveyQuestions();
                e.Cancel = true;
            }
            else
            {
                ShowSummary();
            }
        }
    }

    private void OnPrevious(object? sender, EventArgs e)
    {
        if (_view.HasPreviousSurvey)
        {
            _view.MoveToPreviousSurvey();
            LoadCurrentSurveyQuestions();
        }
    }

    private void OnFinish(object? sender, EventArgs e)
    {
        if (!ValidateFinalSetup(out var error))
        {
            Shell.ShowMessage(error, "Setup Invalid");
            _view.DialogResult = DialogResult.None;
            return;
        }

        _manager.CreateSurrogateSurvey(_view.Type);
        _view.SaveSurrogateSurvey(_manager.SurrogateSurveyItem!);
        _view.DialogResult = DialogResult.OK;
    }

    private void OnSurveyChecked(PoolTrendItem item, bool selected)
    {
        if (_view.Type == PoolTrendType.Pool)
            item.PoolSurveySelected = selected;
        else
            item.TrendSurveySelected = selected;

        _view.PoolTrendList.IsDirty = true;
        ButtonsState();
    }

    private void OnSurveyMoved(int oldIndex, int newIndex)
    {
        var item = _view.PoolTrendList.SurveyList[oldIndex];
        _view.PoolTrendList.SurveyList.RemoveAt(oldIndex);
        _view.PoolTrendList.SurveyList.Insert(newIndex, item);
        _view.RefreshSurveyList();
        _view.PoolTrendList.IsDirty = true;
    }

    private void OnResetOrder(object? sender, EventArgs e)
    {
        _view.PoolTrendList.SurveyList.Sort((x, y) => x.Survey.CreatedDate.CompareTo(y.Survey.CreatedDate));
        _view.RefreshSurveyList();
        FormatSurveyItems();
    }

    private void OnClearSurveys(object? sender, EventArgs e)
    {
        _view.ClearAllSurveys();
        _view.PoolTrendList.ClearSetup(_view.Type);
        ButtonsState();
    }

    private void OnDVSelected(Question? question)
    {
        var item = _view.CurrentPoolTrendItem;
        if (_view.Type == PoolTrendType.Pool)
            item.PoolDVQuestionSelected = question;
        else
            item.TrendDVQuestionSelected = question;

        if (question != null) _dvResults.LoadQuestion(question);
        else _dvResults.Clear("DV:");
    }

    private void OnIVSelected(Question? question)
    {
        if (_view.Type != PoolTrendType.Pool) return;

        var item = _view.CurrentPoolTrendItem;
        item.PoolIVQuestionSelected = question;

        if (question != null) _ivResults.LoadQuestion(question);
        else _ivResults.Clear("IV:");
    }

    private void OnClearQuestions(object? sender, EventArgs e)
    {
        _questionTree.Clear();
        var item = _view.CurrentPoolTrendItem;
        if (_view.Type == PoolTrendType.Pool)
        {
            item.PoolDVQuestionSelected = null;
            item.PoolIVQuestionSelected = null;
        }
        else
        {
            item.TrendDVQuestionSelected = null;
        }
        _dvResults.Clear("DV:");
        _ivResults.Clear("IV:");
    }

    private void LoadCurrentSurveyQuestions()
    {
        var item = _view.CurrentPoolTrendItem;
        _questionTree.LoadQuestions(item.Survey.QuestionList);

        if (_view.Type == PoolTrendType.Pool)
        {
            _questionTree.SelectDV(item.PoolDVQuestionSelected);
            _questionTree.SelectIV(item.PoolIVQuestionSelected);
        }
        else
        {
            _questionTree.SelectDV(item.TrendDVQuestionSelected);
        }

        UpdateSurveyInfoLabels(item.Survey);
    }

    private void UpdateSurveyInfoLabels(Survey survey)
    {
        _view.UpdateSurveyInfo(
            survey.SurveyName,
            survey.CreatedDate,
            survey.Data.DataList.Count,
            survey.QuestionList.Count,
            survey.ErrorsExist || survey.LockStatus == LockStatusType.Locked);
    }

    private void FormatSurveyItems()
    {
        foreach (var item in _view.GetAllSurveyItems())
        {
            var survey = item.Survey;
            var enabled = !survey.ErrorsExist && survey.LockStatus != LockStatusType.Locked;
            _view.SetSurveyItemState(item, enabled, survey.ErrorsExist, survey.LockStatus == LockStatusType.Locked);
        }
    }

    private bool ValidateSurveySelection()
    {
        var selected = _view.GetSelectedSurveys();
        if (selected.Count < 2)
        {
            Shell.ShowMessage("Select at least two surveys.", "Select Surveys");
            return false;
        }
        return true;
    }

    private bool ValidateFinalSetup(out string error)
    {
        error = "";
        if (!_manager.Checker.IsValid)
        {
            error = $"Selected questions are inconsistent:\n{_manager.Checker.InvalidMsg}";
            return false;
        }

        foreach (var item in _view.GetSelectedSurveys())
        {
            if (_view.Type == PoolTrendType.Pool &&
                (item.PoolDVQuestionSelected == null || item.PoolIVQuestionSelected == null))
            {
                error = $"Survey '{item.SurveyName}' is missing DV or IV.";
                return false;
            }

            if (_view.Type == PoolTrendType.Trend && item.TrendDVQuestionSelected == null)
            {
                error = $"Survey '{item.SurveyName}' is missing DV.";
                return false;
            }
        }

        return true;
    }

    private void ButtonsState()
    {
        var anySelected = _view.GetSelectedSurveys().Any();
        _view.SetNextEnabled(anySelected);
        _view.SetClearSurveysEnabled(anySelected);
    }

    private void ShowSummary()
    {
        _view.ShowSummary(_manager.GetSummaryData(_view.Type));
        _view.SetCompleteMessage(_view.Type == PoolTrendType.Pool
            ? "Click Finish to see Pooled results."
            : "Click Finish to see Trend results.");
    }
}