#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Builds the "Full Block" crosstab and index data for a block of questions sharing the same response scale.
/// Used heavily in Two-Block Index and Preference Block analysis.
/// </summary>
public class FullBlockEngine
{
    private readonly Survey _survey;
    private readonly Question _dvQuestion;
    private readonly ObjectListBase<Question> _questionsInBlock = new();

    public Question DVQuestion => _dvQuestion;
    public DataTable DataTable { get; private set; } = new();
    public List<CxIVIndex> IVIndexes { get; } = new();
    public bool IsConsistent { get; private set; } = true;
    public string? ConsistencyMessage { get; private set; }

    public FullBlockEngine(Survey survey, Question dvQuestion)
    {
        _survey = survey ?? throw new ArgumentNullException(nameof(survey));
        _dvQuestion = dvQuestion ?? throw new ArgumentNullException(nameof(dvQuestion));

        BuildFullBlockDataTable();
        BuildIVIndexes();
    }

    private void BuildFullBlockDataTable()
    {
        _questionsInBlock.Clear();
        _questionsInBlock.AddRange(QuestionEngine.GetQuestionsInBlock(_dvQuestion, _survey.QuestionList));

        if (!_questionsInBlock.Any()) return;

        var table = new DataTable();
        table.Columns.Add("Response", typeof(string));

        // Add column for each question in the block
        foreach (var q in _questionsInBlock)
        {
            QuestionEngine.CalculateQuestionAndResponseStatistics(_survey, q);
            table.Columns.Add(q.QstLabel, typeof(string));
        }

        int expectedResponseCount = _dvQuestion.Responses.Count;

        // Build one row per response
        for (int r = 0; r < expectedResponseCount; r++)
        {
            var row = table.NewRow();
            row[0] = _dvQuestion.Responses[r].Label;

            for (int qIndex = 0; qIndex < _questionsInBlock.Count; qIndex++)
            {
                var q = _questionsInBlock[qIndex];
                if (r < q.Responses.Count)
                {
                    row[qIndex + 1] = q.Responses[r].ResultPercent.ToString("#0.0%");
                }
                else
                {
                    // Inconsistent response count detected
                    row[qIndex + 1] = "0.0%";
                    IsConsistent = false;
                    ConsistencyMessage =
                        $"All questions in block '{q.BlkLabel}' don't have the same number of responses, " +
                        $"which could skew results. See question '{q.QstLabel}', which has only {q.Responses.Count} responses. " +
                        $"This can be fixed in the Question Definition screen for this survey.";
                }
            }
            table.Rows.Add(row);
        }

        // Add Index row at the bottom
        var indexRow = table.NewRow();
        indexRow[0] = "Index";
        for (int i = 0; i < _questionsInBlock.Count; i++)
            indexRow[i + 1] = _questionsInBlock[i].TotalIndex;

        table.Rows.Add(indexRow);
        DataTable = table;
    }

    private void BuildIVIndexes()
    {
        IVIndexes.Clear();

        foreach (var q in _questionsInBlock)
        {
            double positive = 0;
            double negative = 0;

            foreach (var resp in q.Responses)
            {
                double percent = resp.ResultPercent * 100;

                switch (resp.IndexType)
                {
                    case ResponseIndexType.Positive:
                        positive += percent;
                        break;
                    case ResponseIndexType.Negative:
                        negative += percent;
                        break;
                }
            }

            IVIndexes.Add(new CxIVIndex
            {
                IVLabel = q.QstLabel,
                Index = q.TotalIndex,
                PosIndex = positive,
                NegIndex = negative
            });
        }
    }

    /// <summary>
    /// Determines if the given question is already part of the currently processed block.
    /// Used to avoid double-processing the same block.
    /// </summary>
    public bool IsAlreadyInBlock(Question q)
        => _questionsInBlock.Any(x => x.Id == q.Id);
}