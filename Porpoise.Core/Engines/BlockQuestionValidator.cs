#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Validates that all questions within a block are consistent:
/// - Same number of responses
/// - Same IndexType for each response in the same position
/// 
/// Used heavily when building Two-Block Index, Preference Blocks, etc.
/// </summary>
public class BlockQuestionValidator
{
    private readonly ObjectListBase<Question> _questionList;

    public ObjectListBase<Question> QuestionList => _questionList;

    public bool IsValid { get; private set; } = true;
    public string? InvalidMessage { get; private set; }
    public BlockErrorType ErrorType { get; private set; }

    public enum BlockErrorType
    {
        NumberOfResponses,
        IndexType
    }

    public BlockQuestionValidator()
    {
        _questionList = [];
    }

    public BlockQuestionValidator(Survey survey)
        : this()
    {
        if (survey.QuestionList is not null)
            _questionList.AddRange(survey.QuestionList);

        ValidateAllBlocks();
    }

    public BlockQuestionValidator(ObjectListBase<Question> questionList)
        : this()
    {
        _questionList.AddRange(questionList ?? throw new ArgumentNullException(nameof(questionList)));
        IsValid = IsQuestionListValid(_questionList);
    }

    /// <summary>
    /// Validates all blocks in the current question list.
    /// Sets IsValid = false and fills InvalidMessage on first error.
    /// </summary>
    public void ValidateAllBlocks()
    {
        IsValid = true;
        InvalidMessage = null;

        var firstQuestionsInBlocks = _questionList
            .Where(q => q.BlkQstStatus == BlkQuestionStatusType.FirstQuestionInBlock)
            .ToList();

        if (firstQuestionsInBlocks.Count == 0)
        {
            IsValid = false;
            return;
        }

        foreach (var blockStart in firstQuestionsInBlocks)
        {
            var blockQuestions = QuestionEngine.GetQuestionsInBlock(blockStart, _questionList);
            if (!IsQuestionListValid(blockQuestions))
            {
                IsValid = false;
                return; // Stop on first invalid block
            }
        }
    }

    /// <summary>
    /// Validates that all questions in the given list have identical response counts and IndexTypes.
    /// Returns false on first inconsistency and populates InvalidMessage/ErrorType.
    /// </summary>
    public bool IsQuestionListValid(ObjectListBase<Question> questions)
    {
        ArgumentNullException.ThrowIfNull(questions);

        if (questions.Count <= 1) return true;

        IsValid = true;
        InvalidMessage = null;

        var baseQuestion = questions[0];
        var baseResponses = baseQuestion.Responses.ToList();
        int expectedCount = baseResponses.Count;

        for (int i = 1; i < questions.Count; i++)
        {
            var q = questions[i];

            // Check response count
            if (q.Responses.Count != expectedCount)
            {
                InvalidMessage = $"The number of responses are different for questions '{baseQuestion.QstLabel}' and '{q.QstLabel}'.";
                ErrorType = BlockErrorType.NumberOfResponses;
                IsValid = false;
                return false;
            }

            // Check IndexType for each response
            for (int r = 0; r < expectedCount; r++)
            {
                var currentResp = q.Responses[r];
                var baseResp = baseResponses[r];

                if (currentResp.IndexType != baseResp.IndexType)
                {
                    InvalidMessage = $"The Index Type '{currentResp.IndexTypeDesc}' for response '{currentResp.Label}' " +
                                           $"in question '{q.QstLabel}' doesn't match Index Type '{baseResp.IndexTypeDesc}' " +
                                           $"for response '{baseResp.Label}' in question '{baseQuestion.QstLabel}'.";
                    ErrorType = BlockErrorType.IndexType;
                    IsValid = false;
                    return false;
                }
            }
        }

        return true;
    }
}