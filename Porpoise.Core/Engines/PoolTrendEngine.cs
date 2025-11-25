#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Core engine for Pooling and Trending — creates a "surrogate" survey that combines multiple surveys
/// into a single virtual dataset for analysis (crosstabs, topline, etc.).
/// One of the most powerful and complex features in Porpoise.
/// </summary>
public class PoolTrendEngine(PoolTrendList poolTrendList)
{
    private readonly PoolTrendList _poolTrendList = poolTrendList ?? throw new ArgumentNullException(nameof(poolTrendList));
    private readonly BlockQuestionValidator _validator = new();

    public PoolTrendItem? SurrogateSurveyItem { get; private set; }

    public void CreateSurrogateSurvey(PoolTrendType type)
    {
        SurrogateSurveyItem = type == PoolTrendType.Pool
            ? CreatePooledSurvey()
            : CreateTrendedSurvey();
    }

    #region Pooling

    private PoolTrendItem? CreatePooledSurvey()
    {
        if (_poolTrendList.PoolSurveyList.Count == 0 || !ValidatePoolQuestions()) return null;

        var lastItem = _poolTrendList.PoolSurveyList.Last();
        var pooled = BuildBasePooledSurvey(lastItem);

        var combinedData = new List<List<string>> { new() { "Case#", "q1DV", "q2IV" } };
        int caseId = 1;

        foreach (var item in _poolTrendList.PoolSurveyList)
        {
            if (item.Survey?.Data == null || 
                item.PoolDVQuestionSelected == null || 
                item.PoolIVQuestionSelected == null)
                continue;

            var dvResponses = item.Survey.Data.GetAllOriginalResponsesInColumn(item.PoolDVQuestionSelected.DataFileCol);
            var ivResponses = item.Survey.Data.GetAllOriginalResponsesInColumn(item.PoolIVQuestionSelected.DataFileCol);

            ValidateSameCaseCount(item, dvResponses.Count, ivResponses.Count);

            for (int i = 0; i < dvResponses.Count; i++)
                combinedData.Add([caseId++.ToString(), dvResponses[i].ToString(), ivResponses[i].ToString()]);
        }

        ApplyCombinedData(pooled, combinedData, lastItem);
        pooled.PoolDVQuestionSelected = pooled.Survey.QuestionList[0];
        pooled.PoolIVQuestionSelected = pooled.Survey.QuestionList[1];

        return pooled;
    }

    private bool ValidatePoolQuestions()
    {
        var dvQuestions = _poolTrendList.PoolSurveyList
            .Select(x => x.PoolDVQuestionSelected)
            .Where(q => q != null)
            .Cast<Question>()
            .ToList();
        var ivQuestions = _poolTrendList.PoolSurveyList
            .Select(x => x.PoolIVQuestionSelected)
            .Where(q => q != null)
            .Cast<Question>()
            .ToList();

        return _validator.IsQuestionListValid([.. dvQuestions]) &&
               _validator.IsQuestionListValid([.. ivQuestions]);
    }

    #endregion

    #region Trending

    private PoolTrendItem? CreateTrendedSurvey()
    {
        if (_poolTrendList.TrendSurveyList.Count == 0 || !ValidateTrendQuestions()) return null;

        var lastItem = _poolTrendList.TrendSurveyList.Last();
        var trended = BuildBaseTrendedSurvey(lastItem);

        var ivQuestion = CreateTrendIVQuestion();
        trended.Survey.QuestionList.Add(ivQuestion);

        var combinedData = new List<List<string>> { new() { "Case#", "q1DV", "q2IV" } };
        int caseId = 1;

        for (int surveyIndex = 0; surveyIndex < _poolTrendList.TrendSurveyList.Count; surveyIndex++)
        {
            var item = _poolTrendList.TrendSurveyList[surveyIndex];
            var dvResponses = item.Survey?.Data?.GetAllOriginalResponsesInColumn(item.TrendDVQuestionSelected?.DataFileCol ?? 0) ?? [];

            foreach (var resp in dvResponses)
                combinedData.Add([caseId++.ToString(), resp.ToString(), (surveyIndex + 1).ToString()]);
        }

        ApplyCombinedData(trended, combinedData, lastItem);
        trended.TrendDVQuestionSelected = trended.Survey.QuestionList[0];
        trended.TrendIVQuestionCreated = ivQuestion;

        return trended;
    }

    private Question CreateTrendIVQuestion()
    {
        var iv = new Question("Surveys")
        {
            DataFileCol = 2,
            QstNumber = "q2IV",
            QstStem = "Multiple surveys being trended",
            BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion,
            DataType = QuestionDataType.Nominal,
            VariableType = QuestionVariableType.Independent,
            Responses = []
        };

        for (int i = 0; i < _poolTrendList.TrendSurveyList.Count; i++)
        {
            var resp = new Response
            {
                Label = _poolTrendList.TrendSurveyList[i].SurveyName ?? "Survey " + (i + 1),
                RespValue = i + 1,
                IndexType = ResponseIndexType.Neutral
            };
            iv.Responses.Add(resp);
        }

        return iv;
    }

    private bool ValidateTrendQuestions()
    {
        var questions = _poolTrendList.TrendSurveyList
            .Select(x => x.TrendDVQuestionSelected)
            .Where(q => q != null)
            .Cast<Question>()
            .ToList();

        return _validator.IsQuestionListValid([.. questions]);
    }

    #endregion

    #region Shared Helpers

    private static PoolTrendItem BuildBasePooledSurvey(PoolTrendItem lastItem)
    {
        var survey = new Survey
        {
            SurveyName = "Pooled Data",
            LockStatus = LockStatusType.Unlocked,
            Status = SurveyStatus.Verified,
            QuestionList = []
        };

        var pooled = new PoolTrendItem(survey);

        var dvClone = (Question)(lastItem.PoolDVQuestionSelected?.Clone() ?? throw new InvalidOperationException("DV Question is required"));
        dvClone.DataFileCol = 1;
        dvClone.QstNumber = "q1DV";
        pooled.Survey.QuestionList.Add(dvClone);

        var ivClone = (Question)(lastItem.PoolIVQuestionSelected?.Clone() ?? throw new InvalidOperationException("IV Question is required"));
        ivClone.DataFileCol = 2;
        ivClone.QstNumber = "q2IV";
        pooled.Survey.QuestionList.Add(ivClone);

        return pooled;
    }

    private static PoolTrendItem BuildBaseTrendedSurvey(PoolTrendItem lastItem)
    {
        var survey = new Survey
        {
            SurveyName = "Trended Data",
            LockStatus = LockStatusType.Unlocked,
            Status = SurveyStatus.Verified,
            QuestionList = []
        };

        var trended = new PoolTrendItem(survey);

        var dvClone = (Question)(lastItem.TrendDVQuestionSelected?.Clone() ?? throw new InvalidOperationException("DV Question is required"));
        dvClone.DataFileCol = 1;
        dvClone.QstNumber = "q1DV";
        trended.Survey.QuestionList.Add(dvClone);

        return trended;
    }

    private static void ApplyCombinedData(PoolTrendItem item, List<List<string>> combinedData, PoolTrendItem lastItem)
    {
        item.Survey.Data = new SurveyData();
        if (lastItem.Survey?.Data != null)
        {
            item.Survey.Data.DataList = lastItem.Survey.Data.DataList;
            item.Survey.Data.SelectedQuestion = lastItem.Survey.Data.SelectedQuestion;
            item.Survey.Data.SelectOn = lastItem.Survey.Data.SelectOn;
        }

        item.Survey.Data.DataList = combinedData;

        if (item.Survey.Data.SelectOn)
            item.Survey.Data.SelectOnDataList = combinedData;

        item.Survey.Id = lastItem.Survey?.Id ?? Guid.NewGuid();
    }

    private static void ValidateSameCaseCount(PoolTrendItem item, int dvCount, int ivCount)
    {
        if (dvCount != ivCount)
        {
            throw new InvalidOperationException(
                $"DV question '{item.PoolDVQuestionSelected?.QstLabel}' ({dvCount} cases) and " +
                $"IV question '{item.PoolIVQuestionSelected?.QstLabel}' ({ivCount} cases) " +
                $"from survey '{item.SurveyName}' have different case counts.");
        }
    }

    #endregion

    #region Static Validation Helpers

    public static bool IsSelectValid(List<PoolTrendItem> items, PoolTrendType type, out string errorMessage)
    {
        int selectOnCount = items.Count(x => x.Survey?.Data?.SelectOn == true);
        if (selectOnCount == 0 || selectOnCount == items.Count)
        {
            errorMessage = string.Empty;
            return true;
        }

        string mode = type == PoolTrendType.Pool ? "Pooled" : "Trended";
        errorMessage = $"You turned Select On for {selectOnCount} but not all {items.Count} {mode} surveys.{Environment.NewLine}{Environment.NewLine}" +
                       "You must set Select On for ALL surveys in a Pool/Trend.";
        return false;
    }

    public static bool IsSelectPlusValid(List<PoolTrendItem> items, PoolTrendType type, out string errorMessage)
    {
        int selectPlusOnCount = items.Count(x => x.Survey?.Data?.SelectPlusOn == true);
        if (selectPlusOnCount == 0 || selectPlusOnCount == items.Count)
        {
            errorMessage = string.Empty;
            return true;
        }

        string mode = type == PoolTrendType.Pool ? "Pooled" : "Trended";
        errorMessage = $"You turned Select Plus On for {selectPlusOnCount} but not all {items.Count} {mode} surveys.{Environment.NewLine}{Environment.NewLine}" +
                       "You must set Select Plus On for ALL surveys in a Pool/Trend.";
        return false;
    }

    #endregion

    #region Summary DataTable

    public DataTable GetSummaryDataTable(PoolTrendType type)
    {
        var dt = new DataTable();
        dt.Columns.Add("DV/IV", typeof(string));
        dt.Columns.Add("Survey", typeof(string));
        dt.Columns.Add("Selected", typeof(string));
        dt.Columns.Add("Question", typeof(string));
        dt.Columns.Add("Responses", typeof(int));
        dt.Columns.Add("TotalN", typeof(int));

        var list = type == PoolTrendType.Pool ? _poolTrendList.PoolSurveyList : _poolTrendList.TrendSurveyList;
        if (list == null || list.Count == 0) return dt;

        foreach (var item in list)
        {
            var dvQuestion = type == PoolTrendType.Pool ? item.PoolDVQuestionSelected : item.TrendDVQuestionSelected;

            string selectedText = string.Empty;
            if (item.Survey?.Data?.SelectOn == true && item.Survey.Data.SelectedQuestion != null)
            {
                var responses = SurveyEngine.FormatSelectedResponsesToString(item.Survey.Data.SelectedQuestion)
                    .Replace("Selected responses:", "").Trim();
                selectedText = $"{item.Survey.Data.SelectedQuestion.QstLabel}: {responses}";
            }
            if (item.Survey?.Data?.SelectPlusOn == true)
            {
                var plusText = SurveyEngine.FormatSelectPlusSelectionToString(item.Survey.Data, false);
                selectedText += (string.IsNullOrEmpty(selectedText) ? "" : Environment.NewLine) + plusText;
            }

            dt.Rows.Add("DV", item.SurveyName, selectedText, dvQuestion?.QstLabel, dvQuestion?.Responses.Count ?? 0, dvQuestion?.TotalN ?? 0);
        }

        if (type == PoolTrendType.Pool)
        {
            foreach (var item in list)
            {
                dt.Rows.Add("IV", item.SurveyName, string.Empty, item.PoolIVQuestionSelected?.QstLabel,
                    item.PoolIVQuestionSelected?.Responses.Count ?? 0, item.PoolIVQuestionSelected?.TotalN ?? 0);
            }
        }

        return dt;
    }

    #endregion
}