// Porpoise.Core/Application/Services/ApplyChangesService.cs
#nullable enable

using System;
using System.Linq;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Handles the "Apply Changes" dialog logic — bulk update of question properties
/// Fully decoupled from WinForms/Telerik
/// </summary>
public class ApplyChangesService : ApplicationServiceBase
{
    private readonly IApplyChangesShell _view;

    public ApplyChangesService(IApplyChangesShell view, IApplicationShell shell)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnShown += OnShown;
        _view.OnApplyClicked += OnApplyClicked;
        _view.OnSettingsChanged += (_) => UpdateApplyButtonState();
    }

    private void OnShown(object? sender, EventArgs e)
    {
        try
        {
            var q = _view.CurrentQuestion;
            if (q == null) return;

            // Sync UI → WhatToChange
            _view.SetCheck(ChangeType.MissingValues, _view.WhatToChange.MissingValues);
            _view.SetCheck(ChangeType.VariableType, _view.WhatToChange.VariableType);
            _view.SetCheck(ChangeType.DataType, _view.WhatToChange.DataType);
            _view.SetCheck(ChangeType.BlockLabel, _view.WhatToChange.BlkLabel);
            _view.SetCheck(ChangeType.BlockStem, _view.WhatToChange.BlkStem);
            _view.SetCheck(ChangeType.BlockStatus, _view.WhatToChange.BlkStatus);
            _view.SetCheck(ChangeType.Responses, _view.WhatToChange.Responses);

            // Populate fields
            _view.MissingValue1 = q.MissValue1;
            _view.MissingValue2 = q.MissValue2;
            _view.MissingValue3 = q.MissValue3;
            _view.VariableType = q.VariableType.ToString();
            _view.DataType = q.DataType.ToString();
            _view.BlockLabel = q.BlkLabel;
            _view.BlockStem = q.BlkStem;

            // Block status radio buttons
            _view.SetBlockStatus(q.BlkQstStatus);

            // Responses grid
            _view.BindResponses(q.Responses);
            q.Responses.ToList().ForEach(r => r.IsDirty = false);

            UpdateApplyButtonState();
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred loading the Apply Changes form.");
        }
    }

    private void OnApplyClicked(object? sender, EventArgs e)
    {
        try
        {
            var q = _view.CurrentQuestion;
            if (q == null) return;

            // Save values back to question
            q.MissValue1 = _view.MissingValue1;
            q.MissValue2 = _view.MissingValue2;
            q.MissValue3 = _view.MissingValue3;
            q.VariableType = EnumParse<QuestionVariableType>(_view.VariableType);
            q.DataType = EnumParse<QuestionDataType>(_view.DataType);
            q.BlkLabel = _view.BlockLabel;
            q.BlkStem = _view.BlockStem;

            // Block status
            q.BlkQstStatus = _view.GetBlockStatus();

            // Responses
            q.Responses = _view.GetResponses();

            // Build WhatToChange
            var wtc = new WhatToChange
            {
                MissingValues = _view.GetCheck(ChangeType.MissingValues),
                VariableType = _view.GetCheck(ChangeType.VariableType),
                DataType = _view.GetCheck(ChangeType.DataType),
                BlkLabel = _view.GetCheck(ChangeType.BlockLabel),
                BlkStem = _view.GetCheck(ChangeType.BlockStem),
                BlkStatus = _view.GetCheck(ChangeType.BlockStatus),
                Responses = _view.GetCheck(ChangeType.Responses)
            };

            _view.WhatToChange = wtc;
            _view.ChangesApplyTo = _view.GetApplyTo();
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred applying changes.");
        }
    }

    private void UpdateApplyButtonState()
    {
        var hasData =
            (_view.GetCheck(ChangeType.MissingValues) &&
             (!string.IsNullOrEmpty(_view.MissingValue1) ||
              !string.IsNullOrEmpty(_view.MissingValue2) ||
              !string.IsNullOrEmpty(_view.MissingValue3))) ||

            (_view.GetCheck(ChangeType.VariableType) && !string.IsNullOrEmpty(_view.VariableType)) ||
            (_view.GetCheck(ChangeType.DataType) && !string.IsNullOrEmpty(_view.DataType)) ||
            (_view.GetCheck(ChangeType.BlockLabel) && !string.IsNullOrEmpty(_view.BlockLabel)) ||
            (_view.GetCheck(ChangeType.BlockStem) && !string.IsNullOrEmpty(_view.BlockStem)) ||
            (_view.GetCheck(ChangeType.BlockStatus) && _view.GetBlockStatus() != BlkQuestionStatusType.None) ||
            (_view.GetCheck(ChangeType.Responses) && _view.GetResponses().Any(r => r.IsDirty));

        _view.SetApplyEnabled(hasData);
    }

    private static T EnumParse<T>(string value) where T : struct
        => Enum.TryParse<T>(value, true, out var result) ? result : default;
}