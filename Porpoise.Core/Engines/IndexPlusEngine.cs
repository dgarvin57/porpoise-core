#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Validates and builds Index+ (side-by-side index comparison) for two questions in the same block.
/// Used exclusively in the Index Plus tab of the UI.
/// </summary>
public class IndexPlusEngine
{
    private readonly Survey _survey;
    private readonly Question _dvQuestion;
    private readonly Question _ivQuestion;

    public bool IsValid { get; private set; } = true;
    public string? ErrorTitle { get; private set; }
    public string? ErrorMessageLong { get; private set; }
    public string? ErrorMessageShort { get; private set; }
    public List<IndexPlusItem> Indexes { get; } = new();
    public bool IVNotInBlock { get; private set; }
    public bool DVNotInBlock { get; private set; }

    public IndexPlusEngine(Survey survey, Question dvQuestion, Question ivQuestion)
    {
        _survey = survey ?? throw new ArgumentNullException(nameof(survey));
        _dvQuestion = dvQuestion ?? throw new ArgumentNullException(nameof(dvQuestion));
        _ivQuestion = ivQuestion ?? throw new ArgumentNullException(nameof(ivQuestion));

        if (!ValidateQuestionsInSameBlock())
            return;

        BuildIndexes();
    }

    private bool ValidateQuestionsInSameBlock()
    {
        var blockQuestions = QuestionEngine.GetQuestionsInBlock(_dvQuestion, _survey.QuestionList);

        if (blockQuestions == null || !blockQuestions.Any())
        {
            ErrorTitle = "Select a DV within a block";
            ErrorMessageLong = $"Question 1 '{_dvQuestion.QstLabel}' is not within a block.";
            ErrorMessageShort = "You're in the Index Plus tab, which expects the DV question to be within a block.";
            IsValid = false;
            DVNotInBlock = true;
            return false;
        }

        bool dvInBlock = blockQuestions.Any(q => q.Id == _dvQuestion.Id);
        bool ivInBlock = blockQuestions.Any(q => q.Id == _ivQuestion.Id);

        if (dvInBlock && ivInBlock)
            return true;

        IsValid = false;
        ErrorTitle = "Select an IV in the same block";
        ErrorMessageLong = $"The first question '{_dvQuestion.QstLabel}' is not in the same block as the second question '{_ivQuestion.QstLabel}'.";
        ErrorMessageShort = "You're in the Index Plus tab, which expects two questions in the same block.";

        if (!ivInBlock) IVNotInBlock = true;
        if (!dvInBlock) DVNotInBlock = true;

        return false;
    }

    private void BuildIndexes()
    {
        var dvIndexes = new IndexEngine(_survey, _dvQuestion).Indexes
            .OrderBy(x => x.QuestionLabel)
            .ThenBy(x => x.ResponseLabel)
            .ToList();

        var ivIndexes = new IndexEngine(_survey, _ivQuestion).Indexes
            .OrderBy(x => x.QuestionLabel)
            .ThenBy(x => x.ResponseLabel)
            .ToList();

        // Assume both lists are identical in structure (validated earlier)
        for (int i = 0; i < dvIndexes.Count; i++)
        {
            Indexes.Add(new IndexPlusItem
            {
                Id = dvIndexes[i].Id,
                QuestionLabel = dvIndexes[i].QuestionLabel,
                ResponseLabel = dvIndexes[i].ResponseLabel,
                Index1 = dvIndexes[i].Index,
                Index2 = ivIndexes[i].Index
            });
        }

        // Final sort by Index+ descending
        Indexes.Sort((a, b) => b.IndexPlus.CompareTo(a.IndexPlus));
    }
}