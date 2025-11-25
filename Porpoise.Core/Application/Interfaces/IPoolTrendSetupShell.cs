// Porpoise.Core/Application/Interfaces/IPoolTrendSetupShell.cs
#nullable enable

using Porpoise.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Porpoise.Core.Application.Interfaces;

public class WizardNextEventArgs : CancelEventArgs { }

public interface IPoolTrendSetupShell
{
    PoolTrendList PoolTrendList { get; }
    PoolTrendType Type { get; }
    DialogResult DialogResult { get; set; }
    int CurrentPage { get; }

    void SetTitle(string title);
    void SetIcon(Icon icon);
    void ShowIVResults(bool visible);
    void ShowReorderControls(bool visible);

    void BindSurveys(IEnumerable<PoolTrendItem> items);
    void RefreshSurveyList();
    IEnumerable<PoolTrendItem> GetSelectedSurveys();
    PoolTrendItem CurrentPoolTrendItem { get; }
    bool HasMoreSurveys { get; }
    bool HasPreviousSurvey { get; }
    void BindQuestionsPage(IEnumerable<PoolTrendItem> items);
    void MoveToNextSurvey();
    void MoveToPreviousSurvey();
    void UpdateSurveyInfo(string name, DateTime created, int cases, int questions, bool hasError);

    void SetSurveyItemState(PoolTrendItem item, bool enabled, bool hasError, bool locked);
    IEnumerable<object> GetAllSurveyItems();

    void SetNextEnabled(bool enabled);
    void SetClearSurveysEnabled(bool enabled);
    void ShowSummary(object dataSource);
    void SetCompleteMessage(string message);
    void SaveSurrogateSurvey(PoolTrendItem surrogate);

    event EventHandler OnLoad;
    event EventHandler<WizardNextEventArgs> OnNext;
    event EventHandler OnPrevious;
    event EventHandler OnFinish;
    event Action<PoolTrendItem, bool> OnSurveyChecked;
    event Action<int, int> OnSurveyMoved;
    event EventHandler OnResetOrder;
    event EventHandler OnClearSurveys;
    event Action<Question?> OnDVSelected;
    event Action<Question?> OnIVSelected;
    event EventHandler OnClearQuestions;
}