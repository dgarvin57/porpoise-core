// Porpoise.Core/Application/Services/SelectService.cs
#nullable enable

using System;
using System.Linq;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Select (Filter) feature — "Show only respondents who answered X to this question"
/// Fully preserved, fully agnostic
/// Works in Blazor, MAUI, WinUI — anywhere
/// </summary>
public class SelectService : ApplicationServiceBase
{
    private readonly ISelectShell _view;

    public SelectService(ISelectShell view, IApplicationShell shell)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
        _view.OnQuestionSelected += OnQuestionSelected;
        _view.OnResponseToggled += OnResponseToggled;
        _view.OnToggleSelect += OnToggleSelect;
        _view.OnClear += OnClear;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        _view.SetSurvey(_view.Survey);
        _view.SetToggleState(_view.Survey.Data.SelectOn);

        if (_view.Survey.Data.SelectedQuestion != null)
        {
            OnQuestionSelected(_view.Survey.Data.SelectedQuestion);
            _view.SelectResponses(_view.Survey.Data.SelectedResponses);
        }
    }

    private void OnQuestionSelected(Question? question)
    {
        if (question == null) return;

        _view.Survey.Data.SelectedQuestion = question;
        _view.SetQuestionLabel(question.QstLabel);
        _view.SetColumnLabel($"Q{question.QstNumber}");
        _view.BindResponses(question.Responses);

        var total = _view.Survey.Data.GetAllResponsesInColumn(question.DataFileCol, true, question.MissingValues);
        _view.SetTotalN(total.Count);

        // Auto-select previously selected responses
        var selected = _view.Survey.Data.SelectedResponses;
        _view.SelectResponses(selected);
        UpdateSelectEnabled();
    }

    private void OnResponseToggled(Response response, bool selected)
    {
        if (selected)
            _view.Survey.Data.SelectedResponses.Add(response);
        else
            _view.Survey.Data.SelectedResponses.Remove(response);

        var count = _view.Survey.Data.GetSelectedResponsesForQuestion(
            _view.Survey.Data.SelectedQuestion!.DataFileCol,
            _view.Survey.Data.SelectedResponses,
            _view.Survey.Data.SelectedQuestion.MissingValues).Count;

        _view.SetSelectedN(count);
        UpdateSelectEnabled();
    }

    private void OnToggleSelect(bool enabled)
    {
        _view.Survey.Data.SelectOn = enabled;
        _view.SetToggleState(enabled);
        _view.EnableTree(!enabled);
        _view.EnableResponseList(!enabled);

        Shell.ShowStatus(enabled ? "Select: ON" : "Select: OFF");
    }

    private void OnClear(object? sender, EventArgs e)
    {
        _view.Survey.Data.SelectedQuestion = null;
        _view.Survey.Data.SelectedResponses.Clear();
        _view.ClearAll();
        UpdateSelectEnabled();
    }

    private void UpdateSelectEnabled()
    {
        var hasResponses = _view.Survey.Data.SelectedResponses.Any();
        _view.EnableToggle(hasResponses);
    }
}