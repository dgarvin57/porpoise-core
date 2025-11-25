// Porpoise.Core/Application/Interfaces/ISelectPlusShell.cs
#nullable enable

using System;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

public interface ISelectPlusShell
{
    Survey Survey { get; }

    void SetSurvey(Survey survey);
    void SetQuestion1(string label);
    void SetQuestion2(string label);
    void ClearQuestions();
    void ShowMovementGrid(object dataSource);
    void HighlightCondition(SelectPlusConditionType condition);
    void ShowTotalN(int count);
    void EnableToggle(bool enabled);
    void EnableTree(bool enabled);
    void SetToggleState(bool on);

    event EventHandler OnLoad;
    event Action<Question?> OnDVSelected;
    event Action<Question?> OnIVSelected;
    event EventHandler OnClear;
    event Action<bool> OnToggle;
    event Action<SelectPlusConditionType> OnConditionSelected;
}