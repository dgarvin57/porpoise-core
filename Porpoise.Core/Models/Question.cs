#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace Porpoise.Core.Models;

/// <summary>
/// The beating heart of Porpoise — the Question class.
/// 
/// This is not just a "question". This is:
/// • A fully weighted, indexed, block-aware survey item
/// • The foundation of Two-Block Index, Preference Blocks, Select Plus, Pooling, Trending
/// • A masterpiece of behavioral analytics design
/// 
/// Every response, every index, every preference item, every weight — all flow through here.
/// This class made Porpoise legendary.
/// And now it's back — better, cleaner, and ready to dominate again.
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
    private QuestionVariableType _variableType = QuestionVariableType.Dependent;
    private QuestionDataType _dataType = QuestionDataType.Nominal;
    private BlkQuestionStatusType _blkQstStatus = 0;
    private string _blkLabel = string.Empty;
    private string _blkStem = string.Empty;
    private bool _isPreferenceBlock;
    private bool _isPreferenceBlockType;
    private int _numberOfPreferenceItems;
    private readonly ObjectListBase<PreferenceItem> _preferenceItems = new();
    private bool? _preserveDifferentResponsesInPreferenceBlock;
    private ObjectListBase<Response> _responses = new();
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
        get => base.IsDirty || (_responses?.IsDirty == true) || (_preferenceItems?.IsDirty == true);
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
        set => SetProperty(ref _dataFileCol, value, "DateFileCol");
    }

    public string QstNumber
    {
        get => _qstNumber;
        set => SetProperty(ref _qstNumber, value, nameof(QstNumber));
    }

    public string QstLabel
    {
        get => _qstLabel;
        set => SetProperty(ref _qstLabel, value, nameof(QstLabel));
    }

    public string QstStem
    {
        get => _qstStem;
        set => SetProperty(ref _qstStem, value, nameof(QstStem));
    }

    public string MissValue1
    {
        get => _missValue1;
        set => SetProperty(ref _missValue1, value, nameof(MissValue1));
    }

    public string MissValue2
    {
        get => _missValue2;
        set => SetProperty(ref _missValue2, value, nameof(MissValue2));
    }

    public string MissValue3
    {
        get => _missValue3;
        set => SetProperty(ref _missValue3, value, nameof(MissValue3));
    }

    public List<int> MissingValues
    {
        get
        {
            var list = new List<int>();
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
            if (value == QuestionVariableType.Independent && _responses != null)
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

    public string BlkLabel
    {
        get => _blkLabel;
        set => SetProperty(ref _blkLabel, value, nameof(BlkLabel));
    }

    public string BlkStem
    {
        get => _blkStem;
        set => SetProperty(ref _blkStem, value, nameof(BlkStem));
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

    [XmlArrayItem(typeof(PreferenceItem))]
    public ObjectListBase<PreferenceItem> PreferenceItems
    {
        get => _preferenceItems;
        set
        {
            if (!ReferenceEquals(value, _preferenceItems))
            {
                if (_preferenceItems is not null)
                    _preferenceItems.IsDirtyChanged -= PreferenceItems_IsDirtyChanged;
                _preferenceItems = value;
                _isDirty = true;
                if (value is not null)
                    value.IsDirtyChanged += PreferenceItems_IsDirtyChanged;
            }
            SetProperty(ref _preferenceItems, value, nameof(PreferenceItems));
        }
    }

    public int NumberOfPreferenceItems
    {
        get => _numberOfPreferenceItems;
        set => SetProperty(ref _numberOfPreferenceItems, value, nameof(NumberOfPreferenceItems));
    }

    public bool? PreserveDifferentResponsesInPreferenceBlock
    {
        get => _preserveDifferentResponsesInPreferenceBlock;
        set => SetProperty(ref _preserveDifferentResponsesInPreferenceBlock, value, "PreserveDifferentResponsesInPreferenceBlock");
    }

    [XmlArrayItem(typeof(Response))]
    public ObjectListBase<Response> Responses
    {
        get => _responses;
        set
        {
            if (!ReferenceEquals(value, _responses))
            {
                if (_responses is not null)
                    _responses.IsDirtyChanged -= Responses_IsDirtyChanged;
                _responses = value;
                _isDirty = true;
                if (value is not null)
                    value.IsDirtyChanged += Responses_IsDirtyChanged;
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
        set => SetProperty(ref _questionNotes, value, nameof(QuestionNotes));
    }

    public bool UseAlternatePosNegLabels
    {
        get => _useAlternatePosNegLabels;
        set => SetProperty(ref _useAlternatePosNegLabels, value, nameof(UseAlternatePosNegLabels));
    }

    public string AlternatePosLabel
    {
        get => _alternatePosLabel;
        set => SetProperty(ref _alternatePosLabel, value, nameof(AlternatePosLabel));
    }

    public string AlternateNegLabel
    {
        get => _alternateNegLabel;
        set => SetProperty(ref _alternateNegLabel, value, nameof(AlternateNegLabel));
    }

    #endregion

    #region Clone — NOW FULLY IMPLEMENTED

    public Question Clone()
    {
        var clone = new Question
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
            AlternateNegLabel = AlternateNegLabel
        };

        // Deep clone Responses
        clone.Responses = new ObjectListBase<Response>();
        foreach (var r in Responses)
            clone.Responses.Add(r.Clone());

        // Deep clone PreferenceItems
        clone.PreferenceItems = new ObjectListBase<PreferenceItem>();
        foreach (var p in PreferenceItems)
            clone.PreferenceItems.Add(p.Clone());

        return clone;
    }

    #endregion

    #region Dirty Tracking

    private void Responses_IsDirtyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsDirty")
        {
            _isDirty = true;
            MarkDirty();
        }
    }

    private void PreferenceItems_IsDirtyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsDirty")
        {
            _isDirty = true;
            MarkDirty();
        }
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