// Porpoise.Core/Application/Services/SelectPlusService.cs
#nullable enable
#pragma warning disable CS0618 // Type or member is obsolete - BlkLabel and BlkStem still in use during migration

using System;
using System.Linq;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Select Plus (Response Movement) feature — fully preserved, fully agnostic
/// Shows movement between two questions: "Who went from X → Y?"
/// Can be used in Blazor, MAUI, or anywhere — just pass in a shell
/// </summary>
public class SelectPlusService : ApplicationServiceBase
{
    private readonly ISelectPlusShell _view;

    public SelectPlusService(ISelectPlusShell view, IApplicationShell shell)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
        _view.OnDVSelected += OnDVSelected;
        _view.OnIVSelected += OnIVSelected;
        _view.OnClear += OnClear;
        _view.OnToggle += OnToggle;
        _view.OnConditionSelected += OnConditionSelected;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        _view.SetSurvey(_view.Survey);
        _view.ShowMovementGrid(_view.Survey.Data.GetMovementList(includeZeroes: true));
        _view.SetToggleState(_view.Survey.Data.SelectPlusOn);
    }

    private void OnDVSelected(Question? question)
    {
        if (question == null) return;

        _view.Survey.Data.SelectPlusQ1 = question;
        _view.SetQuestion1(question.QstLabel);
        UpdateMovementGrid();
    }

    private void OnIVSelected(Question? question)
    {
        if (question == null || _view.Survey.Data.SelectPlusQ1 == null) return;

        if (question.BlkLabel != _view.Survey.Data.SelectPlusQ1.BlkLabel)
        {
            Shell.ShowInfo("Questions must be in the same block", "Select Plus");
            return;
        }

        _view.Survey.Data.SelectPlusQ2 = question;
        _view.SetQuestion2(question.QstLabel);
        UpdateMovementGrid();
    }

    private void UpdateMovementGrid()
    {
        var q1 = _view.Survey.Data.SelectPlusQ1;
        var q2 = _view.Survey.Data.SelectPlusQ2;

        if (q1 == null || q2 == null)
        {
            _view.ShowMovementGrid(_view.Survey.Data.GetMovementList(includeZeroes: true));
            _view.EnableToggle(false);
            return;
        }

        _view.Survey.Data.AddMovementToDataList();
        _view.ShowMovementGrid(_view.Survey.Data.GetMovementList(includeZeroes: false));
        _view.EnableToggle(true);
    }

    private void OnToggle(bool enabled)
    {
        _view.Survey.Data.SelectPlusOn = enabled;
        _view.SetToggleState(enabled);
        _view.EnableTree(!enabled);
        Shell.ShowStatus(enabled ? "Select Plus: ON" : "Select Plus: OFF");
    }

    private void OnConditionSelected(SelectPlusConditionType condition)
    {
        _view.Survey.Data.SelectPlusCondition = condition;
        _view.HighlightCondition(condition);
        _view.ShowTotalN(_view.Survey.Data.GetMovementCount(condition));
    }

    private void OnClear(object? sender, EventArgs e)
    {
        _view.Survey.Data.SelectPlusQ1 = null;
        _view.Survey.Data.SelectPlusQ2 = null;
        _view.Survey.Data.SelectPlusCondition = SelectPlusConditionType.None;
        _view.ClearQuestions();
        UpdateMovementGrid();
        _view.EnableToggle(false);
    }
}