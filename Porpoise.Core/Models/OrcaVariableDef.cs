#nullable enable

using System;
using System.ComponentModel;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a variable/question definition in the legacy Orca export/import system.
/// Contains metadata about response types, counts, and unique response definitions.
/// </summary>
[Serializable]
public class OrcaVariableDef : ObjectBase
{
    #region P R I V A T E   M E M B E R S

    private int _projectId = -1;
    private string _questionNumber = string.Empty;
    private string _questionLabel = string.Empty;
    private readonly ObjectListBase<OrcaResponseDef> _uniqueResponses = new();
    private BlkQuestionStatusType _blockType;

    // Auto-generated members
    private int _responsesLabeled;
    private int _numResponsesHaveString;
    private int _numResponsesHaveNull;
    private int _numResponsesHaveDecimal;

    #endregion

    #region C O N S T R U C T O R S

    // Default constructor
    public OrcaVariableDef()
    {
    }

    /// <summary>
    /// Ctor with values passed in
    /// </summary>
    public OrcaVariableDef(int projectId, string questionNumber, string questionLabel)
    {
        _projectId = projectId;
        _questionNumber = questionNumber;
        _questionLabel = questionLabel;
    }

    #endregion

    #region P U B L I C   P R O P E R T I E S

    public int ProjectId
    {
        get => _projectId;
        set
        {
            if (value == _projectId) return;
            _projectId = value;
        }
    }

    public string QuestionNumber
    {
        get => _questionNumber;
        set
        {
            if (value == _questionNumber) return;
            _questionNumber = value;
        }
    }

    public string QuestionLabel
    {
        get => _questionLabel;
        set
        {
            if (value == _questionLabel) return;
            _questionLabel = value;
        }
    }

    public int ResponsesLabeled
    {
        get => _responsesLabeled;
        set
        {
            if (value == _responsesLabeled) return;
            _responsesLabeled = value;
        }
    }

    public int NumResponsesHaveString
    {
        // See if any responses are strings
        get => _numResponsesHaveString;
        set
        {
            if (value == _numResponsesHaveString) return;
            _numResponsesHaveString = value;
        }
    }

    public int NumResponsesHaveNull
    {
        // See if any responses are null
        get => _numResponsesHaveNull;
        set
        {
            if (value == _numResponsesHaveNull) return;
            _numResponsesHaveNull = value;
        }
    }

    public int NumResponsesHaveDecimal
    {
        // See how many responses are decimals
        get => _numResponsesHaveDecimal;
        set
        {
            if (value == _numResponsesHaveDecimal) return;
            _numResponsesHaveDecimal = value;
        }
    }

    public ObjectListBase<OrcaResponseDef> UniqueResponses
    {
        get => _uniqueResponses;
    }

    public BlkQuestionStatusType BlockType
    {
        get => _blockType;
        set
        {
            if (value == _blockType) return;
            _blockType = value;
        }
    }

    #endregion

    #region M E T H O D S
    // (empty in original)
    #endregion
}