#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Porpoise.Core.Models;

/// <summary>
/// Core class representing a single survey in the Porpoise system.
/// The **central object** that contains questions, data, status, licensing, and paths.
/// This + Question + Response = the beating heart of the entire engine.
/// </summary>
[Serializable]
public class Survey : ObjectBase
{
    #region Members

    private Guid _id = Guid.NewGuid();
    private Guid? _projectId;
    private int _tenantId;
    private string _surveyName = string.Empty;
    private SurveyStatus _status = SurveyStatus.Initial;
    private string _surveyFileName = string.Empty;
    private string _dataFileName = string.Empty;
    private ObjectListBase<Question> _questionList = [];
    private SurveyData? _data;
    private bool _errorsExist = true;
    private string _surveyNotes = string.Empty;
    private bool _isDeleted;
    private DateTime? _deletedDate;

    #endregion

    #region Constructor

    public Survey() { }

    #endregion

    #region Public Properties

    public override bool IsDirty
    {
        get => base.IsDirty || (_questionList?.IsDirty == true);
        set => _isDirty = value;
    }

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value, nameof(Id));
    }

    public Guid? ProjectId
    {
        get => _projectId;
        set => SetProperty(ref _projectId, value, nameof(ProjectId));
    }

    public int TenantId
    {
        get => _tenantId;
        set => SetProperty(ref _tenantId, value, nameof(TenantId));
    }

    public string SurveyName
    {
        get => _surveyName;
        set => SetProperty(ref _surveyName, value, nameof(SurveyName));
    }

    public SurveyStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value, nameof(Status));
    }

    public string SurveyFileName
    {
        get => _surveyFileName;
        set => SetProperty(ref _surveyFileName, value, "PathToQuestionFile");
    }

    public string DataFileName
    {
        get => _dataFileName;
        set => SetProperty(ref _dataFileName, value, "PathToDataFile");
    }

    [XmlArrayItem(typeof(Question))]
    public ObjectListBase<Question> QuestionList
    {
        get => _questionList;
        set
        {
            if (!ReferenceEquals(value, _questionList))
            {
                if (_questionList is not null)
                    _questionList.IsDirtyChanged -= Questions_IsDirtyChanged;

                _questionList = value;
                _isDirty = true;

                if (value is not null)
                    value.IsDirtyChanged += Questions_IsDirtyChanged;
            }
        }
    }

    public short QuestionsNumber => QuestionList is null ? (short)0 : (short)QuestionList.Count;

    public short ResponsesNumber => Data?.DataList?.Count > 0 ? (short)(Data.DataList.Count - 1) : (short)0;

    public bool ErrorsExist
    {
        get => _errorsExist;
        set => SetProperty(ref _errorsExist, value, nameof(ErrorsExist));
    }

    public SurveyData? Data
    {
        get => _data;
        set
        {
            if (!ReferenceEquals(value, _data))
            {
                if (_data is not null)
                    _data.PropertyChanged -= Questions_IsDirtyChanged;

                _data = value;
                _isDirty = true;

                if (value is not null)
                    value.PropertyChanged += Questions_IsDirtyChanged;
            }
        }
    }

    public string SurveyNotes
    {
        get => _surveyNotes;
        set => SetProperty(ref _surveyNotes, value, nameof(SurveyNotes));
    }

    public bool IsDeleted
    {
        get => _isDeleted;
        set => SetProperty(ref _isDeleted, value, nameof(IsDeleted));
    }

    public DateTime? DeletedDate
    {
        get => _deletedDate;
        set => SetProperty(ref _deletedDate, value, nameof(DeletedDate));
    }

    private void Questions_IsDirtyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsDirty")
        {
            _isDirty = true;
            MarkDirty();
        }
    }

    #endregion

    #region Public Methods

    public override void MarkClean()
    {
        foreach (Question q in QuestionList)
            q.MarkClean();
        base.MarkClean();
    }

    public override void MarkAsOld()
    {
        foreach (Question q in QuestionList)
            q.MarkAsOld();
        base.MarkAsOld();
    }

    public List<int> GetAllResponsesForQuestion(Question q, bool omitMissingValues)
    {
        int questionPos = QuestionList.FindIndex(quest => quest.Id == q.Id);
        return Data!.GetAllResponsesInColumn(questionPos + 1, omitMissingValues, q.MissingValues);
    }

    public void ResequenceColumnNumbers()
    {
        bool changed = false;

        foreach (Question q in QuestionList)
        {
            int indexOfQ = Data!.DataList[0].IndexOf(q.QstNumber);
            if (indexOfQ > -1 && indexOfQ != q.DataFileCol)
            {
                q.DataFileCol = (short)indexOfQ;
                changed = true;
            }
        }

        if (changed)
            QuestionList.Sort((x, y) => x.DataFileCol.CompareTo(y.DataFileCol));
    }

    public static byte[] GetViolet() => [0xAF, 0x82, 0x9D, 0xFB];

    public bool IsAllResponsesInQuestionMissingValuesOK()
    {
        if (QuestionList is null || Data?.DataList is null || Data.DataList.Count == 0) return true;

        foreach (Question q in QuestionList)
        {
            List<int> combinedMissingValues = [..q.MissingValues, ..Data.MissingResponseValues];

            if (combinedMissingValues.Count == 0) continue;

            int rnum = q.DataFileCol;
            if (rnum >= Data.DataList[0].Count || Data.DataList[0][rnum] != q.QstNumber) continue;

            bool allMissing = true;
            for (int rowNum = 1; rowNum < Data.DataList.Count; rowNum++)
            {
                if (int.TryParse(Data.DataList[rowNum][rnum], out int responseValue))
                {
                    if (!combinedMissingValues.Contains(responseValue))
                    {
                        allMissing = false;
                        break;
                    }
                }
                else
                {
                    // If parsing fails, treat as not missing (or handle as needed)
                    allMissing = false;
                    break;
                }
            }

            if (allMissing)
            {
                string msg = $"Question '{Data.DataList[0][rnum]}' in data column '{rnum}' has all missing values as responses, which is invalid.{Environment.NewLine}{Environment.NewLine}" +
                             $"A question must have at least one non-missing value for a response. Please edit the data file '{Data.DataFilePath}' and remove the invalid column and try again.";
                throw new SurveyColumnAllMissingValuesException(msg, Data.DataFilePath, Data.DataList[0][rnum], rnum);
            }
        }
        return true;
    }

    #endregion

    #region Validation Methods

    public void ValidateSurveyName()
    {
        if (string.IsNullOrWhiteSpace(SurveyName))
            throw new ArgumentNullException(nameof(SurveyName), "Survey name is required");

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (SurveyName.Contains(c))
                throw new ArgumentOutOfRangeException(nameof(SurveyName), $"Invalid character '{c}' in survey name.");
        }
    }

    #endregion
}