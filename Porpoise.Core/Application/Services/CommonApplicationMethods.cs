// Porpoise.Core/Application/Services/CommonApplicationMethods.cs
#nullable enable

using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Shared application-level helper methods — converted from CommonPresenterMethods.vb
/// UI-agnostic. Pure logic. Ready for Blazor, WinUI, MAUI, or API.
/// </summary>
public static class CommonApplicationMethods
{
    public static void SeeSurveyData(string path, IApplicationShell shell, bool exported)
    {
        if (string.IsNullOrEmpty(path))
        {
            shell.ShowMessage("Please select a survey data file using the Browse... button and try again", "View Survey Data");
            return;
        }

        try
        {
            shell.Hourglass(true);

            var dummySurvey = new Survey
            {
                OrigDataFilePath = path,
                DataFileName = Path.GetFileName(path)
            };

            if (SurveyEngine.LoadSurveyData(dummySurvey, true, exported))
            {
                // In future UI: show data viewer
                shell.ShowMessage($"Survey data loaded: {dummySurvey.Data.DataList.Count} rows", "View Survey Data");
            }
            else
            {
                shell.ShowMessage($"Unable to view survey data for data file '{dummySurvey.DataFileName}'", "View Survey Data");
            }
        }
        catch (Exception ex)
        {
            shell.ShowErrorMessage("An error occurred viewing the survey data file.", ex);
        }
        finally
        {
            shell.Hourglass(false);
        }
    }

    public static void SetBlockQuestionStatus(Question question, bool isFirst, bool isContinuation, bool isStandalone)
    {
        if (question == null) return;

        question.BlkQstStatus = isFirst ? BlkQuestionStatusType.FirstQuestionInBlock :
                                isContinuation ? BlkQuestionStatusType.ContinuationQuestion :
                                isStandalone ? BlkQuestionStatusType.DiscreetQuestion :
                                BlkQuestionStatusType.None;
    }

    public static (bool isFirst, bool isContinuation, bool isStandalone) GetBlockQuestionStatus(Question question)
    {
        return question?.BlkQstStatus switch
        {
            BlkQuestionStatusType.FirstQuestionInBlock => (true, false, false),
            BlkQuestionStatusType.ContinuationQuestion => (false, true, false),
            BlkQuestionStatusType.DiscreetQuestion => (false, false, true),
            _ => (false, false, false)
        };
    }

    public static string LimitTitle(string title, int maxLength)
        => title.Length <= maxLength ? title : title[..maxLength] + "...";

    public static string ShrinkText(string text, Font font, int maxWidth)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (TextRenderer.MeasureText(text, font).Width <= maxWidth) return text;

        const string ellipsis = "...";
        for (int i = text.Length - 1; i > 0; i--)
        {
            var test = text[..i] + ellipsis;
            if (TextRenderer.MeasureText(test, font).Width <= maxWidth)
                return test;
        }
        return text[..1] + ellipsis;
    }

    public static string FileTitle(string fileName, int maxLength)
    {
        if (string.IsNullOrEmpty(fileName)) return "";
        if (fileName.Length <= maxLength) return fileName;

        var firstSeparator = fileName.IndexOf(Path.DirectorySeparatorChar);
        if (firstSeparator <= 0) return "..." + fileName[^Math.Min(maxLength - 3, fileName.Length)..];

        return fileName[..(firstSeparator + 1)] + "..." +
               fileName[^Math.Min(maxLength - firstSeparator - 4, fileName.Length - firstSeparator - 1)..];
    }

    public static string FileTitle(string fileName, Font font, int targetPixelWidth)
    {
        if (string.IsNullOrEmpty(fileName)) return "";
        if (TextRenderer.MeasureText(fileName, font).Width <= targetPixelWidth) return fileName;

        const string ellipsis = "...";
        var firstSeparator = fileName.IndexOf(Path.DirectorySeparatorChar);
        if (firstSeparator <= 0) return ShrinkText(fileName, font, targetPixelWidth);

        var leftPart = fileName[..(firstSeparator + 1)] + ellipsis;
        var leftWidth = TextRenderer.MeasureText(leftPart, font).Width;
        var rightTarget = targetPixelWidth - leftWidth;

        var rightPart = fileName[(firstSeparator + 1)..];
        for (int i = 0; i < rightPart.Length; i++)
        {
            var test = rightPart[i..];
            if (TextRenderer.MeasureText(test, font).Width <= rightTarget)
                return leftPart + test;
        }

        return fileName;
    }
}