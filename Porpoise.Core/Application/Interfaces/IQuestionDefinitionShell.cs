// Porpoise.Core/Application/Interfaces/IQuestionDefinitionShell.cs
#nullable enable

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

public interface IQuestionDefinitionShell
{
    Project Project { get; }
    Survey Survey { get; }
    Question? CurrentQuestion { get; }
    int CurrentQuestionIndex { get; }
    int QuestionCount => Survey.QuestionList.Count;
    bool IsDirty { get; }

    void BindSurvey(Survey survey);
    void BindQuestions(ObjectListBase<Question> questions);
    void BindResponses(ObjectListBase<Response> responses);
    void BindPreferenceItems(ObjectListBase<PreferenceItem> items);

    void SetTitle(string title);
    void SetDataFilePath(string path);
    void SetProjectPath(string path);
    void ShowExported(bool exported);
    void ShowBlockMode(bool inBlock);
    void SetVariableType(QuestionVariableType type);
    void SetDataType(QuestionDataType type);
    void SetBlockStatus(BlkQuestionStatusType status);
    void SetColumnNumber(int col, int total);
    void SetPreferenceEligible(bool eligible, int blockSize);
    void SetAltLabelsEnabled(bool enabled);
    void SetReadOnly(bool readOnly);
    void EnableSaveTemplate(bool enabled);

    void FocusFirstQuestion();
    void RefreshQuestions();
    void RefreshQuestionGrid();
    void ShowValidationErrors(List<string> errors);

    DialogResult ShowApplyChangesDialog(Question current, bool inBlock, bool multipleSelected, WhatToChange changes);
    IEnumerable<Question> GetSelectedQuestions();

    bool ShowSaveFileDialog(string filter, out string path);
    bool ShowOpenFileDialog(string filter, out string path);

    void SaveProject();
    void MarkClean();

    event EventHandler OnLoad;
    event EventHandler<FormClosingEventArgs> OnClosing;
    event EventHandler OnSave;
    event EventHandler OnQuestionChanged;
    event EventHandler OnBlockStatusChanged;
    event EventHandler OnSeeData;
    event Action<WhatToChange> OnApplyChanges;
    event EventHandler OnSaveTemplate;
    event EventHandler OnLoadTemplate;
    event Action<bool> OnUseAltPosNegLabels;
    event Action<bool> OnPreferenceBlockChanged;
}