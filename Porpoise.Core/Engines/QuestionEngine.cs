#nullable enable

using Porpoise.Core.DataAccess;
using Porpoise.Core.Extensions;
using Porpoise.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Porpoise.Core.Engines;

/// <summary>
/// Core static helper engine for all question-level operations in Porpoise.
/// This is the workhorse behind the Question Definition screen and block logic.
/// </summary>
public static class QuestionEngine
{
    public static bool IsQuestionFilled(Question question)
    {
        ArgumentNullException.ThrowIfNull(question);

        return question.DataFileCol > 0 &&
               (!string.IsNullOrWhiteSpace(question.QstNumber?.Trim()) ||
                !string.IsNullOrWhiteSpace(question.QstLabel?.Trim()));
    }

    public static WhatChanged ChangeValuesInQuestions(
        Question currentQuestion,
        Survey survey,
        ChangesApplyTo applyTo,
        WhatToChange whatToChange,
        ObjectListBase<Question> selectedQuestions,
        bool forceApply)
    {
        var whatChanged = new WhatChanged(applyTo);

        if (applyTo == ChangesApplyTo.AllQuestions)
        {
            for (int i = 0; i < survey.QuestionList.Count; i++)
            {
                var q = survey.QuestionList[i];
                ChangeFields(currentQuestion, whatToChange, q, survey, i, forceApply, whatChanged);
            }
        }
        else if (applyTo == ChangesApplyTo.QuestionsInBlock && currentQuestion.BlkQstStatus != BlkQuestionStatusType.DiscreetQuestion)
        {
            var blockQuestions = GetQuestionsInBlock(currentQuestion, survey.QuestionList);
            if (blockQuestions != null)
            {
                foreach (var q in blockQuestions)
                {
                    int index = survey.QuestionList.IndexOf(q);
                    ChangeFields(currentQuestion, whatToChange, q, survey, index, forceApply, whatChanged);
                }
            }
        }
        else if (applyTo == ChangesApplyTo.SelectedQuestions)
        {
            foreach (var q in selectedQuestions)
            {
                int index = survey.QuestionList.IndexOf(q);
                ChangeFields(currentQuestion, whatToChange, q, survey, index, forceApply, whatChanged);
            }
        }
        else
        {
            int index = survey.QuestionList.IndexOf(currentQuestion);
            SyncResponsesAndMissingValues(currentQuestion, survey, index);
        }

        return whatChanged;
    }

    private static void ChangeFields(
        Question source,
        WhatToChange whatToChange,
        Question target,
        Survey survey,
        int questionIndex,
        bool forceApply,
        WhatChanged changeLog)
    {
        // Missing Values
        if (whatToChange.MissingValues)
        {
            if (target.MissValue1 != source.MissValue1)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.MissingValues, target.MissValue1, source.MissValue1));
                target.MissValue1 = source.MissValue1;
            }
            if (target.MissValue2 != source.MissValue2)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.MissingValues, target.MissValue2, source.MissValue2));
                target.MissValue2 = source.MissValue2;
            }
            if (target.MissValue3 != source.MissValue3)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.MissingValues, target.MissValue3, source.MissValue3));
                target.MissValue3 = source.MissValue3;
            }
            SyncResponsesAndMissingValues(target, survey, questionIndex);
        }

        // Variable Type
        if (whatToChange.VariableType && target.VariableType != source.VariableType)
        {
            if (target.VariableType == QuestionVariableType.None && source.VariableType != QuestionVariableType.None)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.VariableType, target.VariableType.Description(), source.VariableType.Description()));
                target.VariableType = source.VariableType;
            }
            else if (target.VariableType != QuestionVariableType.None && source.VariableType == QuestionVariableType.None)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.VariableType, source.VariableType.Description(), target.VariableType.Description()));
                source.VariableType = target.VariableType;
            }
            else if (source.VariableType != QuestionVariableType.None && forceApply)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.VariableType, target.VariableType.Description(), source.VariableType.Description()));
                target.VariableType = source.VariableType;
            }

            if (target.VariableType == QuestionVariableType.Independent)
            {
                foreach (var r in target.Responses.Where(r => r.IndexType == ResponseIndexType.None))
                {
                    r.IndexType = ResponseIndexType.Neutral;
                    changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.ResponseIndexType, ResponseIndexType.None.Description(), ResponseIndexType.Neutral.Description()));
                }
            }
        }

        // Data Type
        if (whatToChange.DataType && target.DataType != source.DataType)
        {
            if (target.DataType == QuestionDataType.None && source.DataType != QuestionDataType.None)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.DataType, target.DataType.ToString(), source.DataType.ToString()));
                target.DataType = source.DataType;
            }
            else if (target.DataType != QuestionDataType.None && source.DataType == QuestionDataType.None)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.DataType, source.DataType.ToString(), target.DataType.ToString()));
                source.DataType = target.DataType;
            }
            else if (source.DataType != QuestionDataType.None && forceApply)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.DataType, target.DataType.ToString(), source.DataType.ToString()));
                target.DataType = source.DataType;
            }
        }

        // Block Status
        if (whatToChange.BlkStatus && target.BlkQstStatus != source.BlkQstStatus)
        {
            if (target.BlkQstStatus == BlkQuestionStatusType.None && source.BlkQstStatus != BlkQuestionStatusType.None)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.BlkStatus, target.BlkQstStatus.Description(), source.BlkQstStatus.Description()));
                target.BlkQstStatus = source.BlkQstStatus;
            }
            else if (target.BlkQstStatus != BlkQuestionStatusType.None && source.BlkQstStatus == BlkQuestionStatusType.None)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.BlkStatus, source.BlkQstStatus.Description(), target.BlkQstStatus.Description()));
                source.BlkQstStatus = target.BlkQstStatus;
            }
            else if (source.BlkQstStatus != BlkQuestionStatusType.None && forceApply)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.BlkStatus, target.BlkQstStatus.Description(), source.BlkQstStatus.Description()));
                target.BlkQstStatus = source.BlkQstStatus;
            }
        }

        // Block Label
        if (whatToChange.BlkLabel && target.BlkLabel != source.BlkLabel)
        {
            if (string.IsNullOrEmpty(target.BlkLabel) && !string.IsNullOrEmpty(source.BlkLabel))
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.BlkLabel, target.BlkLabel, source.BlkLabel));
                target.BlkLabel = source.BlkLabel;
            }
            else if (!string.IsNullOrEmpty(target.BlkLabel) && string.IsNullOrEmpty(source.BlkLabel))
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.BlkLabel, source.BlkLabel, target.BlkLabel));
                source.BlkLabel = target.BlkLabel;
            }
            else if (!string.IsNullOrEmpty(source.BlkLabel) && forceApply)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.BlkLabel, target.BlkLabel, source.BlkLabel));
                target.BlkLabel = source.BlkLabel;
            }
        }

        // Block Stem
        if (whatToChange.BlkStem && target.BlkStem != source.BlkStem)
        {
            if (string.IsNullOrEmpty(target.BlkStem) && !string.IsNullOrEmpty(source.BlkStem))
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.BlkStem, target.BlkStem, source.BlkStem));
                target.BlkStem = source.BlkStem;
            }
            else if (!string.IsNullOrEmpty(target.BlkStem) && string.IsNullOrEmpty(source.BlkStem))
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.BlkStem, source.BlkStem, target.BlkStem));
                source.BlkStem = target.BlkStem;
            }
            else if (!string.IsNullOrEmpty(source.BlkStem) && forceApply)
            {
                changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.BlkStem, target.BlkStem, source.BlkStem));
                target.BlkStem = source.BlkStem;
            }
        }

        // IsPreferenceBlock
        if (whatToChange.IsPreferenceBlock && target.IsPreferenceBlock != source.IsPreferenceBlock)
        {
            changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.IsPreferenceBlock, target.IsPreferenceBlock.ToString(), source.IsPreferenceBlock.ToString()));
            target.IsPreferenceBlock = source.IsPreferenceBlock;
        }

        // NumberOfPreferenceItems
        if (whatToChange.NumberOfPreferenceItems && target.NumberOfPreferenceItems != source.NumberOfPreferenceItems)
        {
            changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.NumberOfPreferenceItems, target.NumberOfPreferenceItems.ToString(), source.NumberOfPreferenceItems.ToString()));
            target.NumberOfPreferenceItems = source.NumberOfPreferenceItems;
        }

        // Use Alternate Pos/Neg Labels
        if (whatToChange.UseAltPosNegLabels && target.UseAlternatePosNegLabels != source.UseAlternatePosNegLabels)
        {
            changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.UseAltPosNegLabels, target.UseAlternatePosNegLabels.ToString(), source.UseAlternatePosNegLabels.ToString()));
            target.UseAlternatePosNegLabels = source.UseAlternatePosNegLabels;
            target.AlternatePosLabel = source.AlternatePosLabel;
            target.AlternateNegLabel = source.AlternateNegLabel;
        }

        // Alternate Pos Label
        if (whatToChange.AltPosLabel && target.AlternatePosLabel != source.AlternatePosLabel)
        {
            changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.AltPosLabel, target.AlternatePosLabel, source.AlternatePosLabel));
            target.AlternatePosLabel = source.AlternatePosLabel;
        }

        // Alternate Neg Label
        if (whatToChange.AltNegLabel && target.AlternateNegLabel != source.AlternateNegLabel)
        {
            changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.AltNegLabel, target.AlternateNegLabel, source.AlternateNegLabel));
            target.AlternateNegLabel = source.AlternateNegLabel;
        }

        // Responses
        if (whatToChange.Responses && target.Responses.Count == source.Responses.Count)
        {
            for (int i = 0; i < target.Responses.Count; i++)
            {
                var targetResp = target.Responses[i];
                var sourceResp = source.Responses[i];

                if (targetResp.RespValue != sourceResp.RespValue) continue;

                // Label
                if (targetResp.Label != sourceResp.Label)
                {
                    if (string.IsNullOrEmpty(targetResp.Label) && !string.IsNullOrEmpty(sourceResp.Label))
                    {
                        changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.ResponseLabel, targetResp.Label, sourceResp.Label));
                        targetResp.Label = sourceResp.Label;
                    }
                    else if (!string.IsNullOrEmpty(targetResp.Label) && string.IsNullOrEmpty(sourceResp.Label))
                    {
                        changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.ResponseLabel, sourceResp.Label, targetResp.Label));
                        sourceResp.Label = targetResp.Label;
                    }
                    else if (!string.IsNullOrEmpty(sourceResp.Label) && forceApply)
                    {
                        changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.ResponseLabel, targetResp.Label, sourceResp.Label));
                        targetResp.Label = sourceResp.Label;
                    }
                }

                // IndexType
                if (targetResp.IndexType != sourceResp.IndexType)
                {
                    if (targetResp.IndexType == ResponseIndexType.None && sourceResp.IndexType != ResponseIndexType.None)
                    {
                        changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.ResponseIndexType, targetResp.IndexType.Description(), sourceResp.IndexType.Description()));
                        targetResp.IndexType = sourceResp.IndexType;
                    }
                    else if (targetResp.IndexType != ResponseIndexType.None && sourceResp.IndexType == ResponseIndexType.None)
                    {
                        changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.ResponseIndexType, sourceResp.IndexType.Description(), targetResp.IndexType.Description()));
                        sourceResp.IndexType = targetResp.IndexType;
                    }
                    else if (sourceResp.IndexType != ResponseIndexType.None && forceApply)
                    {
                        changeLog.AddItem(new WhatChangedItem(WhatChangedEnum.ResponseIndexType, targetResp.IndexType.Description(), sourceResp.IndexType.Description()));
                        targetResp.IndexType = sourceResp.IndexType;
                    }
                }
            }
        }
    }

    private static void SyncResponsesAndMissingValues(Question question, Survey survey, int questionIndex)
    {
        ObjectListBase<Response> newResponses = [];
        var validResponses = survey.Data?.GetUniqueResponsesForQuestion(questionIndex + 1, true, question.MissingValues) ?? [];

        foreach (int respValue in validResponses)
        {
            Response newResponse = new() { RespValue = respValue };

            var existing = question.Responses.FirstOrDefault(r => r.RespValue == respValue);
            if (existing != null)
            {
                newResponse.Label = existing.Label;
                newResponse.IndexType = existing.IndexType;
            }

            newResponses.Add(newResponse);
        }

        question.Responses = newResponses;
        survey.IsDirty = true;
    }

    public static ObjectListBase<Question> GetQuestionsInBlock(Question question, ObjectListBase<Question> allQuestions)
    {
        if (question.BlkQstStatus == BlkQuestionStatusType.DiscreetQuestion)
            return [];

        ObjectListBase<Question> block = [];
        int index = allQuestions.IndexOf(question);
        int start = index;
        int end = index;

        // Find start
        for (int i = index; i >= 0; i--)
        {
            if (allQuestions[i].BlkQstStatus == BlkQuestionStatusType.FirstQuestionInBlock)
            {
                start = i;
                break;
            }
        }

        // Find end
        for (int i = index; i < allQuestions.Count; i++)
        {
            // Don't end the block on the starting FirstQuestion itself
            if (i == start && i == index && allQuestions[i].BlkQstStatus == BlkQuestionStatusType.FirstQuestionInBlock)
                continue;
                
            if (allQuestions[i].BlkQstStatus == BlkQuestionStatusType.FirstQuestionInBlock ||
                allQuestions[i].BlkQstStatus == BlkQuestionStatusType.DiscreetQuestion)
            {
                end = i - 1;
                break;
            }
            
            if (i == allQuestions.Count - 1)
            {
                end = i;
                break;
            }
        }

        for (int i = start; i <= end; i++)
            block.Add(allQuestions[i]);

        return block;
    }

    public static void CalculateQuestionAndResponseStatistics(Survey survey, Question question)
    {
        var allResponses = survey.GetAllResponsesForQuestion(question, true);
        int unweightedN = allResponses.Count;

        survey.Data?.GetResponseFrequencyAndTotalN(question);
        CalculateStatisticsHelper(question, unweightedN);
    }

    public static void CalculateStatisticsHelper(Question question, int unweightedN)
    {
        decimal positive = 0;
        decimal negative = 0;
        decimal cumPercent = 0;
        decimal inverseCumPercent = 1;

        foreach (var response in question.Responses)
        {
            if (question.MissingValues.Contains(response.RespValue)) continue;

            decimal unweightedPercent = unweightedN > 0 ? (decimal)response.ResultFrequency / unweightedN : 0m;
            response.ResultPercent = question.TotalN > 0 ? (decimal)response.ResultFrequency / question.TotalN : 0m;

            cumPercent += response.ResultPercent;
            response.CumPercent = cumPercent;
            response.InverseCumPercent = inverseCumPercent;
            inverseCumPercent -= response.ResultPercent;

            if (response.IndexType == ResponseIndexType.Positive)
                positive += response.ResultPercent * 100m;
            else if (response.IndexType == ResponseIndexType.Negative)
                negative += response.ResultPercent * 100m;

            decimal pVar = unweightedPercent * 100m;
            decimal qVar = 100m - pVar;
            response.SamplingError = unweightedN > 0 ? Math.Sqrt((double)(pVar * qVar / unweightedN)) * 1.96 : 0;
        }

        question.TotalIndex = (int)Math.Round(positive - negative + 100, 0);
    }

    public static bool IsTemplateXMLFilesExist(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        return Directory.GetFiles(path, "*.xml").Length > 0;
    }

    public static List<Question>? GetQuestionTemplate(string path, out bool validTemplate, out string errorMessage)
    {
        var template = LegacyDataAccess.LoadXMLFileToList<Question>(path);

        validTemplate = template != null && template.Count > 0 && template[0] is not null;
        errorMessage = validTemplate ? string.Empty :
            template == null ? "Unable to retrieve template for unknown reason." :
            template.Count == 0 ? "Template contains no question definitions" :
            "Template does not include valid question definitions";

        return validTemplate ? template : null;
    }

    public static bool SaveQuestionTemplate(List<Question> questions, string path)
    {
        var copy = questions.Select(q => (Question)q.Clone()).ToList();
        return LegacyDataAccess.SaveListToXMLFile(ResetQuestionData(copy), path);
    }

    private static List<Question> ResetQuestionData(List<Question> questionList)
    {
        foreach (var q in questionList)
        {
            q.Id = Guid.NewGuid();
            q.CreatedOn = DateTime.Now;
            q.CreatedBy = Environment.UserName;
            q.ModifiedBy = string.Empty;
            q.ModifiedOn = DateTime.MinValue;

            foreach (var r in q.Responses)
            {
                r.Id = Guid.NewGuid();
                r.CreatedOn = DateTime.Now;
                r.CreatedBy = Environment.UserName;
                r.ModifiedBy = string.Empty;
                r.ModifiedOn = DateTime.MinValue;
                r.IsDirty = false;
                r.ResultFrequency = 0;
                r.ResultPercent = 0;
                r.CumPercent = 0;
                r.InverseCumPercent = 0;
                r.SamplingError = 0;
                r.Weight = 1;
            }
            q.IsDirty = false;
        }
        return questionList;
    }

    public static void SetQuestionModified(List<Question> questions)
    {
        foreach (var q in questions.Where(q => q.IsDirty))
        {
            q.ModifiedBy = Environment.UserName;
            q.ModifiedOn = DateTime.Now;

            foreach (var r in q.Responses.Where(r => r.IsDirty))
            {
                r.ModifiedBy = Environment.UserName;
                r.ModifiedOn = DateTime.Now;
            }
        }
    }

    public static void DefaultResponseIndex(Question dvQuestion, ObjectListBase<Question> questionList)
    {
        var blockQuestions = GetQuestionsInBlock(dvQuestion, questionList);
        if (blockQuestions == null) return;

        foreach (var q in blockQuestions)
        {
            foreach (var r in q.Responses.Where(r => r.IndexType == ResponseIndexType.None))
                r.IndexType = ResponseIndexType.Neutral;
        }
    }

    public static bool IsIVInSameBlockAsDV(Question dvQuestion, Question ivQuestion, ObjectListBase<Question> questionList)
    {
        var blockQuestions = GetQuestionsInBlock(dvQuestion, questionList);
        return blockQuestions?.Any(q => q.Id == ivQuestion.Id) == true;
    }
}