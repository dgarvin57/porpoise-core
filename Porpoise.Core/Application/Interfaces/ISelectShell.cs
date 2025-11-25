// Porpoise.Core/Application/Interfaces/ISelectShell.cs
#nullable enable

using System;
using System.Collections.Generic;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

public interface ISelectShell
{
    Survey Survey { get; }

    void SetSurvey(Survey survey);
    void SetQuestionLabel(string label);
    void SetColumnLabel(string label);
    void SetTotalN(int count);
    void SetSelectedN(int count);
    void BindResponses(ObjectListBase<Response> responses);
    void SelectResponses(IEnumerable<Response> responses);
    void ClearAll();
    void EnableTree(bool enabled);
    void EnableResponseList(bool enabled);
    void EnableToggle(bool enabled);
    void SetToggleState(bool on);

    event EventHandler OnLoad;
    event Action<Question?> OnQuestionSelected;
    event Action<Response, bool> OnResponseToggled;
    event Action<bool> OnToggleSelect;
    event EventHandler OnClear;
}