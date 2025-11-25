#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Validates that the raw survey data file matches the current question definitions.
/// Detects added/removed questions and responses, and can auto-fix when requested.
/// Critical for data integrity when opening or saving surveys.
/// </summary>
public class SurveyDataValidator(Survey survey)
{
    private readonly Survey _survey = survey ?? throw new ArgumentNullException(nameof(survey));

    public SurveyDataValidationResults DoesSurveyDataAndQuestionsMatch(ref string message, bool fix)
    {
        if (_survey?.Data?.DataList == null || _survey.Data.DataList.Count == 0 ||
            _survey.QuestionList == null || _survey.QuestionList.Count == 0)
        {
            return SurveyDataValidationResults.OK;
        }

        var questionResult = IsQuestionsChanged(ref message, false);
        if (questionResult != SurveyDataValidationResults.OK)
            return questionResult;

        return IsResponsesChanged(ref message, fix);
    }

    public void RemoveQuestionDefinitionToMatchDataFile(ref string message)
    {
        IsQuestionsChanged(ref message, true);
    }

    private SurveyDataValidationResults IsQuestionsChanged(ref string message, bool justRemoveQuestions)
    {
        var questionNumbers = _survey.QuestionList.Select(q => q.QstNumber).OrderBy(x => x).ToList();
        var dataColumns = _survey.Data?.DataList?[0].Skip(1).OrderBy(x => x).ToList() ?? []; // Skip case number

        if (questionNumbers.SequenceEqual(dataColumns))
        {
            _survey.ResequenceColumnNumbers();
            return SurveyDataValidationResults.OK;
        }

        var removedQuestions = questionNumbers.Except(dataColumns).ToList();
        var addedQuestions = dataColumns.Except(questionNumbers)
            .Where(x => x != "#?SIM_WEIGHT/?#" && x != "WEIGHT" && x != "#?/MOVEMENT/?#")
            .ToList();

        if (removedQuestions.Count != 0)
        {
            if (!justRemoveQuestions)
            {
                message = $"{(removedQuestions.Count == 1 ? "A question has" : "Questions have")} been removed from the data file: " +
                          $"{string.Join(", ", removedQuestions)}{Environment.NewLine}{Environment.NewLine}" +
                          "Click Yes to automatically remove the matching question definition(s), No to ignore, or Cancel to fix manually.";
                return SurveyDataValidationResults.QuestionRemoved;
            }

            foreach (var qNum in removedQuestions)
                _survey.QuestionList.RemoveAll(q => q.QstNumber == qNum);

            _survey.ResequenceColumnNumbers();
            message = "";
        }

        if (addedQuestions.Count != 0 && _survey.Data?.DataList != null && _survey.Data.DataList.Count > 0)
        {
            foreach (var qNum in addedQuestions)
            {
                var colIndex = _survey.Data.DataList[0].IndexOf(qNum);
                if (colIndex <= 0) continue;

                var newQuestion = new Question
                {
                    QstNumber = qNum,
                    DataFileCol = (short)colIndex,
                    Responses = _survey.Data.GetQuestionResponses(colIndex, [])
                };

                _survey.QuestionList.Insert(colIndex - 1, newQuestion);
            }

            _survey.ResequenceColumnNumbers();
            message = $"{(addedQuestions.Count == 1 ? "A question has" : "Questions have")} been added to the data file '{_survey.DataFileName}': {string.Join(", ", addedQuestions)}";
            return SurveyDataValidationResults.QuestionAdded;
        }

        _survey.ResequenceColumnNumbers();
        return SurveyDataValidationResults.OK;
    }

    private SurveyDataValidationResults IsResponsesChanged(ref string message, bool fix)
    {
        var changes = new List<string>();

        for (int i = 0; i < _survey.QuestionList.Count; i++)
        {
            var question = _survey.QuestionList[i];
            if (question.Responses == null || !question.Responses.Any()) continue;

            var currentResponses = _survey.Data?.GetUniqueResponsesForQuestion(i + 1, true, question.MissingValues).ToList() ?? [];
            var definedResponses = question.Responses.Select(r => r.RespValue).ToList();

            if (currentResponses.Count < definedResponses.Count)
            {
                var missing = definedResponses.Except(currentResponses).ToList();
                var stillMissing = missing.Where(m => !IsResponseExistsInBlock(question, _survey, m)).ToList();

                if (stillMissing.Count != 0)
                {
                    if (fix)
                    {
                        foreach (var resp in stillMissing)
                            question.Responses.RemoveAll(r => r.RespValue == resp);
                        question.MarkDirty();
                    }

                    changes.Add($"In {question.QstNumber}{(string.IsNullOrEmpty(question.QstLabel) ? "" : $" ({question.QstLabel})")}, (col {i + 1}): " +
                                $"{(stillMissing.Count == 1 ? "Response" : "Responses")} '{string.Join(", ", stillMissing)}' {(stillMissing.Count == 1 ? "is" : "are")} missing.");
                }
            }
            else if (currentResponses.Count > definedResponses.Count)
            {
                var added = currentResponses.Except(definedResponses).ToList();

                if (fix)
                {
                    foreach (var resp in added)
                        question.Responses.Add(new Response { RespValue = resp });
                    question.Responses.Sort((x, y) => x.RespValue.CompareTo(y.RespValue));
                    question.MarkDirty();
                }

                changes.Add($"In {question.QstNumber}{(string.IsNullOrEmpty(question.QstLabel) ? "" : $" ({question.QstLabel})")}, (col {i + 1}): " +
                            $"New {(added.Count == 1 ? "response" : "responses")} '{string.Join(", ", added)}' exists but {(added.Count == 1 ? "is" : "are")} not defined.");
            }
        }

        if (changes.Count != 0)
        {
            var fixMsg = fix
                ? "The response definitions have been automatically fixed to match the raw data. Review in Question Definition."
                : "The response definitions do not match the data file. Fix automatically?";

            message = $"The response definitions and data file '{_survey.DataFileName}' do not match:{Environment.NewLine}{Environment.NewLine}" +
                      $"{string.Join(Environment.NewLine, changes)}{Environment.NewLine}{Environment.NewLine}{fixMsg}";

            return fix ? SurveyDataValidationResults.ResponsesFixed : SurveyDataValidationResults.ResponsesOutOfSync;
        }

        return SurveyDataValidationResults.OK;
    }

    private static bool IsResponseExistsInBlock(Question question, Survey survey, int responseValue)
    {
        if (question.BlkQstStatus == BlkQuestionStatusType.DiscreetQuestion)
            return false;

        var blockQuestions = QuestionEngine.GetQuestionsInBlock(question, survey.QuestionList);
        if (!blockQuestions.Any() || (blockQuestions.Count == 1 && blockQuestions[0].QstNumber == question.QstNumber))
            return false;

        return blockQuestions.Any(q =>
            survey.Data?.GetUniqueResponsesForQuestion(q.DataFileCol, true, q.MissingValues).Contains(responseValue) == true);
    }
}

public enum SurveyDataValidationResults
{
    OK,
    QuestionAdded,
    QuestionRemoved,
    ResponsesOutOfSync,
    ResponsesFixed
}