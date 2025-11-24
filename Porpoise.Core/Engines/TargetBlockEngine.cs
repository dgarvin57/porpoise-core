#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Target Block engine — calculates normal and filtered (target-selected) indexes
/// for a block of questions. Used in the Target tab for "Target Block" analysis.
/// </summary>
public class TargetBlockEngine
{
    private readonly Survey _survey;
    private readonly Question _blockQuestion;
    private readonly Question _targetQuestion;

    private readonly ObjectListBase<Question> _questionsInBlock;
    private readonly ObjectListBase<Question> _questionsInBlockCopy = new();

    private readonly List<List<string>> _targetData = new();

    public List<CxIVIndex> IVIndexes { get; } = new();
    public List<CxIVIndex> IVIndexesSelected { get; } = new();
    public List<Response>? Responses { get; set; }
    public bool IsConsistent { get; private set; } = true;
    public string? IsConsistentMsg { get; private set; }

    public TargetBlockEngine(Survey survey, Question blockQuestion, Question targetQuestion)
    {
        _survey = survey ?? throw new ArgumentNullException(nameof(survey));
        _blockQuestion = blockQuestion ?? throw new ArgumentNullException(nameof(blockQuestion));
        _targetQuestion = targetQuestion ?? throw new ArgumentNullException(nameof(targetQuestion));

        CreateTargetBlockIVIndexesNormal();
    }

    private void CreateTargetBlockIVIndexesNormal()
    {
        _questionsInBlock = QuestionEngine.GetQuestionsInBlock(_blockQuestion, _survey.QuestionList)
            ?? new ObjectListBase<Question>();

        if (!_questionsInBlock.Any()) return;

        IVIndexes.Clear();

        foreach (var q in _questionsInBlock)
        {
            QuestionEngine.CalculateQuestionAndResponseStatistics(_survey, q);

            double positive = 0;
            double negative = 0;

            foreach (var r in q.Responses)
            {
                double percent = r.ResultPercent * 100;

                if (r.IndexType == ResponseIndexType.Positive)
                    positive += percent;
                else if (r.IndexType == ResponseIndexType.Negative)
                    negative += percent;
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

    public void CreateTargetBlockIVIndexesSelectedGraph()
    {
        if (Responses == null || !Responses.Any()) return;

        _questionsInBlockCopy.Clear();
        _questionsInBlockCopy.AddRange(QuestionEngine.GetQuestionsInBlock(_blockQuestion, _survey.QuestionList)
            ?? new ObjectListBase<Question>());

        if (!_questionsInBlockCopy.Any()) return;

        BuildTargetData();

        IVIndexesSelected.Clear();

        foreach (var q in _questionsInBlockCopy)
        {
            CalculateTargetQuestionAndResponseStatistics(q);

            double positive = 0;
            double negative = 0;

            foreach (var r in q.Responses)
            {
                double percent = r.ResultPercent * 100;

                if (r.IndexType == ResponseIndexType.Positive)
                    positive += percent;
                else if (r.IndexType == ResponseIndexType.Negative)
                    negative += percent;
            }

            IVIndexesSelected.Add(new CxIVIndex
            {
                IVLabel = q.QstLabel,
                Index = q.TotalIndex,
                PosIndex = positive,
                NegIndex = negative
            });
        }
    }

    private void BuildTargetData()
    {
        _targetData.Clear();

        int targetCol = _targetQuestion.DataFileCol;

        for (int row = 0; row < _survey.Data.DataList.Count; row++)
        {
            if (int.TryParse(_survey.Data.DataList[row][targetCol], out int responseValue) &&
                Responses.Any(r => r.RespValue == responseValue))
            {
                _targetData.Add(new List<string>(_survey.Data.DataList[row]));
            }
        }
    }

    private void CalculateTargetQuestionAndResponseStatistics(Question q)
    {
        var allResponses = GetAllResponsesInColumn(q.DataFileCol, q.MissingValues);
        int unweightedN = allResponses.Count;

        GetResponseFrequencyAndTotalN(q);
        QuestionEngine.CalculateStatisticsHelper(q, unweightedN);
    }

    private List<int> GetAllResponsesInColumn(int colNumber, List<int> missingValues)
    {
        var responses = new List<int>();

        for (int row = 1; row < _targetData.Count; row++)
        {
            if (int.TryParse(_targetData[row][colNumber], out int value) &&
                (missingValues.Count == 0 || !missingValues.Contains(value)))
            {
                responses.Add(value);
            }
        }

        return responses;
    }

    private void GetResponseFrequencyAndTotalN(Question q)
    {
        double totalN = 0;

        foreach (var r in q.Responses)
            r.ResultFrequency = 0;

        for (int row = 1; row < _targetData.Count; row++)
        {
            if (!int.TryParse(_targetData[row][q.DataFileCol], out int respValue))
                continue;

            double simWeight = _survey.Data.WeightOn ? GetResponseSimWeight(row) : 1.0;
            double staticWeight = _survey.Data.UseStaticWeight ? GetStaticWeight(row, false) : 1.0;

            int index = q.Responses.FindIndex(r => r.RespValue == respValue);
            if (index >= 0)
            {
                q.Responses[index].ResultFrequency += simWeight * staticWeight;
                totalN += simWeight * staticWeight;
            }
        }

        q.TotalN = Math.Ceiling(totalN);
    }

    private double GetResponseSimWeight(int rowNumber)
    {
        if (!_survey.Data.WeightOn) return 1.0;

        int weightCol = _survey.Data.GetSimWeightColumnNumber();
        if (weightCol <= 0) return 1.0;

        return double.TryParse(_targetData[rowNumber][weightCol], out double weight) ? weight : 1.0;
    }

    private double GetStaticWeight(int rowNumber, bool ignoreUseStaticWeight)
    {
        if (!_survey.Data.UseStaticWeight && !ignoreUseStaticWeight) return 1.0;

        int staticWeightCol = _survey.Data.GetStaticWeightColumnNumber();
        if (staticWeightCol <= 0) return 1.0;

        return double.TryParse(_targetData[rowNumber][staticWeightCol], out double weight) ? weight : 1.0;
    }

    public bool IsAlreadyInBlock(Question q)
        => _questionsInBlock?.Any(x => x.Id == q.Id) == true;
}