// Porpoise.Core/UI/Helpers/ResultsStatusHelper.cs
using Porpoise.Core.Models;
using System.Text;

namespace Porpoise.Core.UI.Helpers;

public static class ResultsStatusHelper
{
    public static (string Label, string Tooltip) GetSelectStatus(SurveyData data)
    {
        if (!data.SelectOn && !data.SelectPlusOn)
            return ("", "");

        var label = new StringBuilder();
        var tooltip = new StringBuilder();

        if (data.SelectOn)
        {
            label.Append($"Select: {data.SelectedQuestion.QstLabel}");
            tooltip.Append(SurveyManager.FormatSelectedResponsesToString(data.SelectedQuestion));
        }

        if (data.SelectPlusOn)
        {
            var plusLabel = SurveyManager.FormatSelectPlusSelectionToString(data, shortForm: true);
            var plusTooltip = SurveyManager.FormatSelectPlusSelectionToString(data, shortForm: false);

            if (label.Length > 0)
            {
                label.AppendLine().Append(plusLabel);
                tooltip.AppendLine().AppendLine().Append(plusTooltip);
            }
            else
            {
                label.Append(plusLabel);
                tooltip.Append(plusTooltip);
            }
        }

        return (label.ToString(), tooltip.ToString());
    }

    public static (string Label, string Tooltip) GetWeightStatus(SurveyData data, bool weightOn)
    {
        if (!weightOn || data.WeightedQuestion == null)
            return ("", "");

        return (
            $"Weight: {data.WeightedQuestion.QstLabel}",
            SurveyManager.FormatWeightedResponsesToString(data.WeightedQuestion)
        );
    }
}