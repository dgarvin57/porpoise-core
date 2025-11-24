#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Calculates percentage and index values for a single response across an entire block of questions.
/// Used in "One Response" analysis — e.g., "% Agree" across all questions in a block.
/// </summary>
public class SingleResponseEngine
{
    public List<OneResponseItem> Items { get; } = new();
    public bool IsConsistent { get; private set; } = true;
    public string? ConsistencyMessage { get; private set; }

    public SingleResponseEngine(Survey survey, Question dvQuestion, int responseValue)
    {
        if (survey == null) throw new ArgumentNullException(nameof(survey));
        if (dvQuestion == null) throw new ArgumentNullException(nameof(dvQuestion));
        if (responseValue <= 0) throw new ArgumentOutOfRangeException(nameof(responseValue));

        BuildResponseBlock(survey, dvQuestion, responseValue);
    }

    private void BuildResponseBlock(Survey survey, Question dvQuestion, int responseValue)
    {
        var blockQuestions = QuestionEngine.GetQuestionsInBlock(dvQuestion, survey.QuestionList);
        if (blockQuestions == null || !blockQuestions.Any()) return;

        // Pre-calculate statistics for all questions in the block
        foreach (var q in blockQuestions)
            QuestionEngine.CalculateQuestionAndResponseStatistics(survey, q);

        var items = new List<OneResponseItem>();

        foreach (var q in blockQuestions)
        {
            var item = new OneResponseItem
            {
                Id = q.Id,
                QuestionLabel = q.QstLabel,
                Index = q.TotalIndex
            };

            if (responseValue <= q.Responses.Count)
            {
                item.Percent = q.Responses[responseValue - 1].ResultPercent;
            }
            else
            {
                item.Percent = 0;
                IsConsistent = false;
                ConsistencyMessage =
                    $"All questions in block '{q.BlkLabel}' don't have the same number of responses, " +
                    $"which could skew results. See question '{q.QstLabel}', which has only {q.Responses.Count} responses. " +
                    $"This can be fixed in the Question Definition screen for this survey.";
            }

            items.Add(item);
        }

        // Sort by percentage descending (highest % first)
        Items.AddRange(items.OrderByDescending(x => x.Percent));
    }
}