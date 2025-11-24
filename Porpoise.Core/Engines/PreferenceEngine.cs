#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// The legendary Preference Block engine — calculates pairwise preference scores,
/// preference matrix, summary tables, and crosstabs for 4, 5, or 6-choice preference blocks.
/// Used in the "Preference" tab and reports.
/// </summary>
public class PreferenceEngine
{
    private readonly Survey _survey;
    private readonly Question _dvQuestion;
    private readonly Question? _ivQuestion;

    private readonly ObjectListBase<Question> _questionsInBlock;
    private readonly List<List<decimal>> _arrPref = new();

    public Question DVQuestion => _dvQuestion;
    public Question? IVQuestion => _ivQuestion;
    public DataTable SummaryDataTable { get; private set; } = new();
    public DataTable SummaryDataForChart { get; private set; } = new();
    public DataTable MatrixDataTable { get; private set; } = new();
    public DataTable CrosstabDataTable { get; private set; } = new();
    public bool IsConsistent { get; private set; } = true;
    public string? IsConsistentMsg { get; private set; }

    public PreferenceEngine(Survey survey, Question dvQuestion)
        : this(survey, dvQuestion, null) { }

    public PreferenceEngine(Survey survey, Question dvQuestion, Question? ivQuestion)
    {
        _survey = survey ?? throw new ArgumentNullException(nameof(survey));
        _dvQuestion = dvQuestion ?? throw new ArgumentNullException(nameof(dvQuestion));
        _ivQuestion = ivQuestion;

        _questionsInBlock = QuestionEngine.GetQuestionsInBlock(_dvQuestion, _survey.QuestionList)
            ?? throw new InvalidOperationException("DV question must be part of a preference block.");

        CreatePreferenceSummaryDatatable();
        if (_ivQuestion != null)
            CreateCrosstabDataTable();
    }

    private void CreatePreferenceSummaryDatatable()
    {
        CreateInterimArray();

        // Summary table
        var table = new DataTable();
        table.Columns.Add("Blank1", typeof(string));
        table.Columns.Add("Preference Score", typeof(decimal));

        var chartTable = new DataTable();
        chartTable.Columns.Add("Blank1", typeof(string));
        chartTable.Columns.Add("Preference Score", typeof(decimal));

        int itemCount = _dvQuestion.PreferenceItems.Count;
        int scoreColumn = itemCount switch
        {
            4 => 6,
            5 => 7,
            6 => 8,
            _ => throw new InvalidOperationException($"Unsupported preference item count: {itemCount}")
        };

        for (int row = 0; row < itemCount; row++)
        {
            var newRow = table.NewRow();
            newRow["Blank1"] = _dvQuestion.PreferenceItems[row].ItemName;
            newRow["Preference Score"] = _arrPref[row][scoreColumn];
            table.Rows.Add(newRow);

            var chartRow = chartTable.NewRow();
            chartRow["Blank1"] = _dvQuestion.PreferenceItems[row].ItemName;
            chartRow["Preference Score"] = _arrPref[row][scoreColumn] * 100m;
            chartRow["Preference Score"] = Math.Round((decimal)chartRow["Preference Score"], 1);
            chartTable.Rows.Add(chartRow);
        }

        var view = new DataView(table) { Sort = "Preference Score DESC" };
        SummaryDataTable = view.ToTable();

        var chartView = new DataView(chartTable) { Sort = "Preference Score ASC" };
        SummaryDataForChart = chartView.ToTable();

        CreatePreferenceMatrixDataTable();
    }

    private void CreatePreferenceMatrixDataTable()
    {
        var table = new DataTable();
        table.Columns.Add("Blank1", typeof(string));

        foreach (var item in _dvQuestion.PreferenceItems)
            table.Columns.Add(item.ItemName, typeof(string));

        for (int row = 0; row < _dvQuestion.PreferenceItems.Count; row++)
        {
            var newRow = table.NewRow();
            newRow[0] = $"{_dvQuestion.PreferenceItems[row].ItemName} preferred";
            table.Rows.Add(newRow);
        }

        int itemCount = _dvQuestion.PreferenceItems.Count;

        for (int row = 0; row < itemCount; row++)
        {
            for (int col = 0; col < itemCount; col++)
            {
                if (row == col)
                    table.Rows[row][col + 1] = "***";
                else
                {
                    decimal wins = _arrPref[row][col];
                    decimal losses = _arrPref[col][row];
                    decimal total = wins + losses;
                    table.Rows[row][col + 1] = total > 0 ? $"{wins / total:P1}" : "0.0%";
                }
            }
        }

        MatrixDataTable = table;
    }

    private void CreateCrosstabDataTable()
    {
        if (_ivQuestion == null) return;

        var table = new DataTable();
        table.Columns.Add("Blank1", typeof(string));

        foreach (var r in _ivQuestion.Responses)
            table.Columns.Add(r.Label, typeof(decimal));

        // Save current select state
        var originalSelectOn = _survey.Data.SelectOn;
        var originalSelectedQuestion = _survey.Data.SelectedQuestion?.Clone();

        try
        {
            for (int respIndex = 0; respIndex < _ivQuestion.Responses.Count; respIndex++)
            {
                var selectedResponse = _ivQuestion.Responses[respIndex].Clone();
                var tempQuestion = _ivQuestion.Clone();
                tempQuestion.Responses = new ObjectListBase<Response> { selectedResponse };

                _survey.Data.SelectedQuestion = tempQuestion;
                _survey.Data.SelectOn = true;

                CreateInterimArray();

                int scoreColumn = _dvQuestion.PreferenceItems.Count switch
                {
                    4 => 6,
                    5 => 7,
                    6 => 8,
                    _ => throw new InvalidOperationException()
                };

                if (respIndex == 0)
                {
                    for (int row = 0; row < _dvQuestion.PreferenceItems.Count; row++)
                    {
                        var newRow = table.NewRow();
                        newRow[0] = $"{_dvQuestion.PreferenceItems[row].ItemName} preferred";
                        newRow[1] = _arrPref[row][scoreColumn];
                        table.Rows.Add(newRow);
                    }
                }
                else
                {
                    for (int row = 0; row < _dvQuestion.PreferenceItems.Count; row++)
                    {
                        table.Rows[row][respIndex + 1] = _arrPref[row][scoreColumn];
                    }
                }
            }
        }
        finally
        {
            // Restore original state
            _survey.Data.SelectOn = originalSelectOn;
            _survey.Data.SelectedQuestion = (Question?)originalSelectedQuestion;
        }

        CrosstabDataTable = table;
    }

    private void CreateInterimArray()
    {
        _arrPref.Clear();

        int itemCount = _dvQuestion.PreferenceItems.Count;
        int expectedQuestions = itemCount switch
        {
            4 => 6,
            5 => 10,
            6 => 15,
            _ => throw new InvalidOperationException($"Unsupported preference item count: {itemCount}")
        };

        if (_questionsInBlock.Count != expectedQuestions)
            throw new InvalidOperationException(
                $"Expected {expectedQuestions} questions in {itemCount}-item preference block, found {_questionsInBlock.Count}");

        // Initialize matrix
        for (int i = 0; i < itemCount; i++)
            _arrPref.Add(Enumerable.Repeat(0m, itemCount + 3).ToList());

        // Count pairwise wins
        for (int q = 0; q < _questionsInBlock.Count; q++)
        {
            QuestionEngine.CalculateQuestionAndResponseStatistics(_survey, _questionsInBlock[q]);

            int resp1 = GetResponseFrequency(_questionsInBlock[q], 1);
            int resp2 = GetResponseFrequency(_questionsInBlock[q], 2);
            int resp3 = GetResponseFrequency(_questionsInBlock[q], 3);

            (int row, int col) = (q, itemCount) switch
            {
                (0, 4) => (0, 1),
                (1, 4) => (0, 2),
                (2, 4) => (0, 3),
                (3, 4) => (1, 2),
                (4, 4) => (1, 3),
                (5, 4) => (2, 3),
                (0, 5) => (0, 1),
                (1, 5) => (0, 2),
                (2, 5) => (0, 3),
                (3, 5) => (0, 4),
                (4, 5) => (1, 2),
                (5, 5) => (1, 3),
                (6, 5) => (1, 4),
                (7, 5) => (2, 3),
                (8, 5) => (2, 4),
                (9, 5) => (3, 4),
                (0, 6) => (0, 1),
                (1, 6) => (0, 2),
                (2, 6) => (0, 3),
                (3, 6) => (0, 4),
                (4, 6) => (0, 5),
                (5, 6) => (1, 2),
                (6, 6) => (1, 3),
                (7, 6) => (1, 4),
                (8, 6) => (1, 5),
                (9, 6) => (2, 3),
                (10, 6) => (2, 4),
                (11, 6) => (2, 5),
                (12, 6) => (3, 4),
                (13, 6) => (3, 5),
                (14, 6) => (4, 5),
                _ => (-1, -1)
            };

            if (row >= 0)
            {
                _arrPref[row][col] += resp1 + (resp3 * 0.5m);
                _arrPref[col][row] += resp2 + (resp3 * 0.5m);
            }
        }

        // Calculate possible wins and final score
        for (int i = 0; i < itemCount; i++)
        {
            decimal actualWins = _arrPref[i].Take(itemCount).Where((_, j) => i != j).Sum();
            decimal possibleWins = CountPossibleWinsForItem(i, itemCount);

            _arrPref[i][itemCount] = actualWins;
            _arrPref[i][itemCount + 1] = possibleWins;
            _arrPref[i][itemCount + 2] = possibleWins > 0 ? actualWins / possibleWins : 0m;
        }
    }

    private decimal CountPossibleWinsForItem(int itemIndex, int itemCount)
    {
        var questions = itemCount switch
        {
            4 => itemIndex switch
            {
                0 => new[] { 0, 1, 2 },
                1 => new[] { 0, 3, 4 },
                2 => new[] { 1, 3, 5 },
                3 => new[] { 2, 4, 5 },
                _ => Array.Empty<int>()
            },
            5 => itemIndex switch
            {
                0 => new[] { 0, 1, 2, 3 },
                1 => new[] { 0, 4, 5, 6 },
                2 => new[] { 1, 4, 7, 8 },
                3 => new[] { 2, 5, 7, 9 },
                4 => new[] { 3, 6, 8, 9 },
                _ => Array.Empty<int>()
            },
            6 => itemIndex switch
            {
                0 => new[] { 0, 1, 2, 3, 4 },
                1 => new[] { 0, 5, 6, 7, 8 },
                2 => new[] { 1, 5, 9, 10, 11 },
                3 => new[] { 2, 6, 9, 12, 13 },
                4 => new[] { 3, 7, 10, 12, 14 },
                5 => new[] { 4, 8, 11, 13, 14 },
                _ => Array.Empty<int>()
            },
            _ => Array.Empty<int>()
        };

        return questions.Sum(qi => CountPossibleWins(_questionsInBlock[qi]));
    }

    private static int GetResponseFrequency(Question q, int respValue)
        => q.Responses.FirstOrDefault(r => r.RespValue == respValue)?.ResultFrequency ?? 0;

    private static decimal CountPossibleWins(Question q)
    {
        QuestionEngine.CalculateQuestionAndResponseStatistics(q.Survey!, q);
        return q.Responses.Sum(r => r.ResultFrequency);
    }
}