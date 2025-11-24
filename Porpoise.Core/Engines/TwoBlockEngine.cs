#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Two Block Index engine — compares two separate blocks of questions side-by-side.
/// Validates block structure and builds paired index data for charting.
/// Used exclusively in the "Two Block" tab.
/// </summary>
public class TwoBlockEngine
{
    private readonly Survey _survey;
    private readonly Question _dvQuestion;
    private readonly Question _ivQuestion;

    private readonly ObjectListBase<Question> _block1Questions;
    private readonly ObjectListBase<Question> _block2Questions;

    public bool IsValid { get; private set; } = true;
    public string? ErrorTitle { get; private set; }
    public string? ErrorMessageLong { get; private set; }
    public string? ErrorMessageShort { get; private set; }
    public List<TwoBlockIndex> TwoBlockIndexes { get; } = new();

    public TwoBlockEngine(Survey survey, Question dvQuestion, Question ivQuestion)
    {
        _survey = survey ?? throw new ArgumentNullException(nameof(survey));
        _dvQuestion = dvQuestion ?? throw new ArgumentNullException(nameof(dvQuestion));
        _ivQuestion = ivQuestion ?? throw new ArgumentNullException(nameof(ivQuestion));

        _block1Questions = QuestionEngine.GetQuestionsInBlock(_dvQuestion, _survey.QuestionList)
            ?? throw new ArgumentException($"Unexpected null returned for questions in block for DV '{_dvQuestion.QstLabel}'");

        _block2Questions = QuestionEngine.GetQuestionsInBlock(_ivQuestion, _survey.QuestionList)
            ?? throw new ArgumentException($"Unexpected null returned for questions in block for IV '{_ivQuestion.QstLabel}'");

        if (!ValidateQuestions())
            return;

        CreateTwoBlockIndexes();
    }

    private bool ValidateQuestions()
    {
        // DV must be in a block
        if (_dvQuestion.BlkQstStatus == BlkQuestionStatusType.DiscreetQuestion ||
            _dvQuestion.BlkQstStatus == BlkQuestionStatusType.None)
        {
            ErrorTitle = "Select a Block DV";
            ErrorMessageShort = "You're in the Two Block tab, which expects both questions within separate blocks.";
            ErrorMessageLong = $"The DV question '{_dvQuestion.QstLabel}' is not in a block.";
            IsValid = false;
            return false;
        }

        // IV must be in a block
        if (_ivQuestion.BlkQstStatus == BlkQuestionStatusType.DiscreetQuestion ||
            _ivQuestion.BlkQstStatus == BlkQuestionStatusType.None)
        {
            ErrorTitle = "Select a Block IV";
            ErrorMessageShort = "You're in the Two Block tab, which expects that both questions selected are in matching blocks.";
            ErrorMessageLong = $"The IV question '{_ivQuestion.QstLabel}' is not in a block.";
            IsValid = false;
            return false;
        }

        // Must be different blocks
        if (_dvQuestion.BlkLabel == _ivQuestion.BlkLabel)
        {
            ErrorTitle = "Select Different Blocks";
            ErrorMessageShort = "You're in the Two Block tab, which expects two questions from two different and matching blocks.";
            ErrorMessageLong = $"The DV question '{_dvQuestion.QstLabel}' and IV question '{_ivQuestion.QstLabel}' selected must be in SEPARATE blocks with DIFFERENT block labels.";
            IsValid = false;
            return false;
        }

        // Blocks must have same number of questions
        if (_block1Questions.Count != _block2Questions.Count)
        {
            ErrorTitle = "Select Matching Blocks";
            ErrorMessageShort = "You're on the Two Block tab, which expects two questions from matching blocks";
            ErrorMessageLong = $"Block 1 (DV question) '{_dvQuestion.BlkLabel}' and block 2 (IV question) '{_ivQuestion.BlkLabel}' do not have the same number of questions.";
            IsValid = false;
            return false;
        }

        return true;
    }

    private void CreateTwoBlockIndexes()
    {
        var indexList = new List<TwoBlockIndex>();

        for (int i = 0; i < _block1Questions.Count; i++)
        {
            var q1 = _block1Questions[i];
            var q2 = _block2Questions[i];

            QuestionEngine.CalculateQuestionAndResponseStatistics(_survey, q1);
            QuestionEngine.CalculateQuestionAndResponseStatistics(_survey, q2);

            indexList.Add(new TwoBlockIndex
            {
                QBlock1Label = q1.QstLabel,
                QBlock1Index = q1.TotalIndex,
                QBlock2Index = q2.TotalIndex
            });
        }

        // Sort by Block 1 index ascending (default chart order)
        TwoBlockIndexes.AddRange(indexList.OrderBy(x => x.QBlock1Index));
    }
}