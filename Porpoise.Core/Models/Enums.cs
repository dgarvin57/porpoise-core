#nullable enable

using System.ComponentModel;

namespace Porpoise.Core.Models;

public enum BlkQuestionStatusType
{
    [Description("None")] None = 0,
    [Description("First question in block")] FirstQuestionInBlock = 1,
    [Description("Continuation question")] ContinuationQuestion = 2,
    [Description("Discreet question")] DiscreetQuestion = 3
}

public enum QuestionDataType
{
    [Description("None")] None = 0,
    [Description("Nominal")] Nominal = 1,
    [Description("Interval")] Interval = 2,
    [Description("Both")] Both = 3
}

public enum QuestionVariableType
{
    [Description("None")] None = 0,
    [Description("Independent (IV)")] Independent = 1,
    [Description("Dependent (DV)")] Dependent = 2
}

public enum ChangesApplyTo
{
    [Description("This question only")] ThisQuestionOnly,
    [Description("All questions in block")] QuestionsInBlock,
    [Description("Selected questions")] SelectedQuestions,
    [Description("All questions")] AllQuestions
}

public enum WhatChangedEnum
{
    [Description("Missing values")] MissingValues,
    [Description("Variable type")] VariableType,
    [Description("Data type")] DataType,
    [Description("Block status")] BlkStatus,
    [Description("Block label")] BlkLabel,
    [Description("Block stem")] BlkStem,
    [Description("Response label")] ResponseLabel,
    [Description("Response index type")] ResponseIndexType,
    [Description("Is Preference block")] IsPreferenceBlock,
    [Description("Number of Preference Items")] NumberOfPreferenceItems,
    [Description("Preference item")] PreferenceItem,
    [Description("Use Alternate Pos/Neg Labels")] UseAltPosNegLabels,
    [Description("Alternate Positive Label")] AltPosLabel,
    [Description("Alternate Negative Label")] AltNegLabel
}

public enum DVOrIV { DV, IV, Both }
public enum NotesType { Question, Survey }

public enum SurveyStatus
{
    Initial,
    Verified
}

public enum LockStatusType
{
    Locked,
    Unlocked
}

public enum ShowResultsColumnType
{
    TotalN,
    Cumulative,
    InverseCumulative,
    SamplingError,
    Blank
}

public enum ReportProcessRunning
{
    TopLine,
    SurveyNotes
}

public struct WhatToChange
{
    public bool MissingValues;
    public bool VariableType;
    public bool DataType;
    public bool BlkStatus;
    public bool BlkLabel;
    public bool BlkStem;
    public bool Responses;
    public bool IsPreferenceBlock;
    public bool NumberOfPreferenceItems;
    public bool PreferenceItemsList;
    public bool UseAltPosNegLabels;
    public bool AltPosLabel;
    public bool AltNegLabel;
}