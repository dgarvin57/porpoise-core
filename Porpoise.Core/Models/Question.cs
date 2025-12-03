#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace Porpoise.Core.Models;

/// <summary>
/// This is not just a "question". This is:
/// • A fully weighted, indexed, block-aware survey item
/// • The foundation of Two-Block Index, Preference Blocks, Select Plus, Pooling, Trending
///
/// Every response, every index, every preference item, every weight — all flow through here.
/// </summary>
[Serializable]
public class Question : ObjectBase
{
    #region Private Members

    private Guid _id = Guid.NewGuid();
    private bool _columnFilled = true;
    private short _dataFileCol;
    private string _qstNumber = string.Empty;
    private string _qstLabel = string.Empty;
    private string _qstStem = string.Empty;
    private string _missValue1 = string.Empty;
    private string _missValue2 = string.Empty;
    private string _missValue3 = string.Empty;
    private double _missingLow = 0;
    private double _missingHigh = 0;
    private QuestionVariableType _variableType = QuestionVariableType.Dependent;
    private QuestionDataType _dataType = QuestionDataType.Nominal;
    private BlkQuestionStatusType _blkQstStatus = 0;
    private Guid? _blockId; // FK to QuestionBlocks table
    private string _blkLabel = string.Empty; // DEPRECATED: Use Block.Label via BlockId
    private string _blkStem = string.Empty;  // DEPRECATED: Use Block.Stem via BlockId
    private bool _isPreferenceBlock;
    private bool _isPreferenceBlockType;
    private int _numberOfPreferenceItems;
    private ObjectListBase<PreferenceItem> _preferenceItems = [];
    private ObjectListBase<Response> _responses = [];
    private bool? _preserveDifferentResponsesInPreferenceBlock;
    private int _totalIndex;
    private int _totalN;
    private bool _selected;
    private bool _weightOn;
    private string _questionNotes = string.Empty;
    private bool _useAlternatePosNegLabels;
    private string _alternatePosLabel = string.Empty;
    private string _alternateNegLabel = string.Empty;

    #endregion

    #region Constructors

    public Question() { }

    public Question(string qstLabel) => _qstLabel = qstLabel;

    public Question(string qstNumber, string qstLabel, short dataFileCol, string missValue1)
    {
        _qstNumber = qstNumber;
        _qstLabel = qstLabel;
        _dataFileCol = dataFileCol;
        _missValue1 = missValue1;
    }

    #endregion

    #region Public Properties

    public override bool IsDirty
    {
        get => base.IsDirty || _responses.IsDirty || _preferenceItems.IsDirty;
        set => _isDirty = value;
    }

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value, nameof(Id));
    }

    public bool ColumnFilled
    {
        get => _columnFilled;
        set => SetProperty(ref _columnFilled, value, nameof(ColumnFilled));
    }

    public short DataFileCol
    {
        get => _dataFileCol;
        set => SetProperty(ref _dataFileCol, value, nameof(DataFileCol));
    }

    public string QstNumber
    {
        get => _qstNumber;
        set => SetProperty(ref _qstNumber, value ?? string.Empty, nameof(QstNumber));
    }

    public string QstLabel
    {
        get => _qstLabel;
        set => SetProperty(ref _qstLabel, value ?? string.Empty, nameof(QstLabel));
    }

    public string QstStem
    {
        get => _qstStem;
        set => SetProperty(ref _qstStem, value ?? string.Empty, nameof(QstStem));
    }

    public string MissValue1
    {
        get => _missValue1;
        set => SetProperty(ref _missValue1, value ?? string.Empty, nameof(MissValue1));
    }

    public string MissValue2
    {
        get => _missValue2;
        set => SetProperty(ref _missValue2, value ?? string.Empty, nameof(MissValue2));
    }

    public string MissValue3
    {
        get => _missValue3;
        set => SetProperty(ref _missValue3, value ?? string.Empty, nameof(MissValue3));
    }

    public double MissingLow
    {
        get => _missingLow;
        set => SetProperty(ref _missingLow, value, nameof(MissingLow));
    }

    public double MissingHigh
    {
        get => _missingHigh;
        set => SetProperty(ref _missingHigh, value, nameof(MissingHigh));
    }

    public List<int> MissingValues
    {
        get
        {
            List<int> list = [];
            if (int.TryParse(_missValue1, out int v1)) list.Add(v1);
            if (int.TryParse(_missValue2, out int v2)) list.Add(v2);
            if (int.TryParse(_missValue3, out int v3)) list.Add(v3);
            return list;
        }
    }

    public QuestionVariableType VariableType
    {
        get => _variableType;
        set
        {
            SetProperty(ref _variableType, value, nameof(VariableType));
            if (value == QuestionVariableType.Independent)
            {
                foreach (var r in _responses.Where(r => r.IndexType == ResponseIndexType.None))
                    r.IndexType = ResponseIndexType.Neutral;
            }
        }
    }

    public QuestionDataType DataType
    {
        get => _dataType;
        set => SetProperty(ref _dataType, value, nameof(DataType));
    }

    public BlkQuestionStatusType BlkQstStatus
    {
        get => _blkQstStatus;
        set => SetProperty(ref _blkQstStatus, value, nameof(BlkQstStatus));
    }

    /// <summary>
    /// Foreign key to the QuestionBlock that this question belongs to.
    /// Null for discrete questions that are not part of a block.
    /// </summary>
    public Guid? BlockId
    {
        get => _blockId;
        set => SetProperty(ref _blockId, value, nameof(BlockId));
    }

    /// <summary>
    /// Navigation property to the block this question belongs to.
    /// </summary>
    [XmlIgnore]
    public QuestionBlock? Block { get; set; }

    /// <summary>
    /// DEPRECATED: Use Block.Label via BlockId instead.
    /// Kept for backward compatibility during migration.
    /// </summary>
    [Obsolete("Use Block.Label via BlockId instead")]
    public string BlkLabel
    {
        get => _blkLabel;
        set => SetProperty(ref _blkLabel, value ?? string.Empty, nameof(BlkLabel));
    }

    /// <summary>
    /// DEPRECATED: Use Block.Stem via BlockId instead.
    /// Kept for backward compatibility during migration.
    /// </summary>
    [Obsolete("Use Block.Stem via BlockId instead")]
    public string BlkStem
    {
        get => _blkStem;
        set => SetProperty(ref _blkStem, value ?? string.Empty, nameof(BlkStem));
    }

    public bool IsPreferenceBlock
    {
        get => _isPreferenceBlock;
        set => SetProperty(ref _isPreferenceBlock, value, nameof(IsPreferenceBlock));
    }

    public bool IsPreferenceBlockType
    {
        get => _isPreferenceBlockType;
        set => SetProperty(ref _isPreferenceBlockType, value, nameof(IsPreferenceBlockType));
    }

    public int NumberOfPreferenceItems
    {
        get => _numberOfPreferenceItems;
        set => SetProperty(ref _numberOfPreferenceItems, value, nameof(NumberOfPreferenceItems));
    }

    public bool? PreserveDifferentResponsesInPreferenceBlock
    {
        get => _preserveDifferentResponsesInPreferenceBlock;
        set => SetProperty(ref _preserveDifferentResponsesInPreferenceBlock, value, nameof(PreserveDifferentResponsesInPreferenceBlock));
    }

    [XmlArrayItem(typeof(PreferenceItem))]
    public ObjectListBase<PreferenceItem> PreferenceItems
    {
        get => _preferenceItems;
        set
        {
            if (!ReferenceEquals(_preferenceItems, value))
            {
                if (_preferenceItems is not null)
                    _preferenceItems.IsDirtyChanged -= PreferenceItems_IsDirtyChanged;

                _preferenceItems = value ?? [];

                if (_preferenceItems is not null)
                    _preferenceItems.IsDirtyChanged += PreferenceItems_IsDirtyChanged;

                MarkDirty();
            }
        }
    }

    [XmlArrayItem(typeof(Response))]
    public ObjectListBase<Response> Responses
    {
        get => _responses;
        set
        {
            if (!ReferenceEquals(_responses, value))
            {
                if (_responses is not null)
                    _responses.IsDirtyChanged -= Responses_IsDirtyChanged;

                _responses = value ?? [];

                if (_responses is not null)
                    _responses.IsDirtyChanged += Responses_IsDirtyChanged;

                MarkDirty();
            }
        }
    }

    public int TotalIndex
    {
        get => _totalIndex;
        set => SetProperty(ref _totalIndex, value, nameof(TotalIndex));
    }

    public int TotalN
    {
        get => _totalN;
        set => SetProperty(ref _totalN, value, nameof(TotalN));
    }

    public decimal SamplingError => TotalN > 0 ? (decimal)Math.Sqrt(50 * 50.0 / TotalN) * 1.96m : 0m;

    [XmlIgnore]
    public bool Selected
    {
        get => _selected;
        set => _selected = value;
    }

    public bool WeightOn
    {
        get => _weightOn;
        set => _weightOn = value;
    }

    public string QuestionNotes
    {
        get => _questionNotes;
        set => SetProperty(ref _questionNotes, value ?? string.Empty, nameof(QuestionNotes));
    }

    public bool UseAlternatePosNegLabels
    {
        get => _useAlternatePosNegLabels;
        set => SetProperty(ref _useAlternatePosNegLabels, value, nameof(UseAlternatePosNegLabels));
    }

    public string AlternatePosLabel
    {
        get => _alternatePosLabel;
        set => SetProperty(ref _alternatePosLabel, value ?? string.Empty, nameof(AlternatePosLabel));
    }

    public string AlternateNegLabel
    {
        get => _alternateNegLabel;
        set => SetProperty(ref _alternateNegLabel, value ?? string.Empty, nameof(AlternateNegLabel));
    }

    #endregion

    #region Clone

    public new Question Clone() => new()
    {
        Id = Id,
        QstNumber = QstNumber,
        QstLabel = QstLabel,
        QstStem = QstStem,
        DataFileCol = DataFileCol,
        ColumnFilled = ColumnFilled,
        VariableType = VariableType,
        DataType = DataType,
        BlkQstStatus = BlkQstStatus,
        BlkLabel = BlkLabel,
        BlkStem = BlkStem,
        IsPreferenceBlock = IsPreferenceBlock,
        IsPreferenceBlockType = IsPreferenceBlockType,
        NumberOfPreferenceItems = NumberOfPreferenceItems,
        PreserveDifferentResponsesInPreferenceBlock = PreserveDifferentResponsesInPreferenceBlock,
        MissValue1 = MissValue1,
        MissValue2 = MissValue2,
        MissValue3 = MissValue3,
        TotalIndex = TotalIndex,
        TotalN = TotalN,
        Selected = Selected,
        WeightOn = WeightOn,
        QuestionNotes = QuestionNotes,
        UseAlternatePosNegLabels = UseAlternatePosNegLabels,
        AlternatePosLabel = AlternatePosLabel,
        AlternateNegLabel = AlternateNegLabel,
        _responses = [.. _responses.Select(r => (Response)r.Clone())],
        _preferenceItems = [.. _preferenceItems.Select(p => (PreferenceItem)p.Clone())]
    };

    #endregion

    #region Dirty Tracking

    private void Responses_IsDirtyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ObjectListBase<Response>.IsDirty))
            MarkDirty();
    }

    private void PreferenceItems_IsDirtyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ObjectListBase<PreferenceItem>.IsDirty))
            MarkDirty();
    }

    public override void MarkClean()
    {
        foreach (var r in Responses) r.MarkClean();
        foreach (var p in PreferenceItems) p.MarkClean();
        base.MarkClean();
    }

    public override void MarkAsOld()
    {
        foreach (var r in Responses) r.MarkAsOld();
        foreach (var p in PreferenceItems) p.MarkAsOld();
        base.MarkAsOld();
    }

    #endregion

    public override string ToString()
        => !string.IsNullOrEmpty(QstLabel) ? QstLabel :
           !string.IsNullOrEmpty(QstNumber) ? QstNumber :
           DataFileCol.ToString();
}