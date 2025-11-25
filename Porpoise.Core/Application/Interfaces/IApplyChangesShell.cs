// Porpoise.Core/Application/Interfaces/IApplyChangesShell.cs
#nullable enable

using System;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

public enum ChangeType
{
    MissingValues,
    VariableType,
    DataType,
    BlockLabel,
    BlockStem,
    BlockStatus,
    Responses
}

public interface IApplyChangesShell
{
    Question? CurrentQuestion { get; }
    WhatToChange WhatToChange { get; set; }
    ChangesApplyTo ChangesApplyTo { get; set; }

    // Values
    string MissingValue1 { get; set; }
    string MissingValue2 { get; set; }
    string MissingValue3 { get; set; }
    string VariableType { get; set; }
    string DataType { get; set; }
    string BlockLabel { get; set; }
    string BlockStem { get; set; }

    // Checkboxes
    void SetCheck(ChangeType type, bool value);
    bool GetCheck(ChangeType type);

    // Block status
    void SetBlockStatus(BlkQuestionStatusType status);
    BlkQuestionStatusType GetBlockStatus();

    // Responses
    void BindResponses(ObjectListBase<Response> responses);
    ObjectListBase<Response> GetResponses();

    // Apply button
    void SetApplyEnabled(bool enabled);

    // Events
    event EventHandler OnShown;
    event EventHandler OnApplyClicked;
    event EventHandler OnSettingsChanged;
}