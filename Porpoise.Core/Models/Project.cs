#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace Porpoise.Core.Models;

/// <summary>
/// Core legacy class representing a Porpoise project.
/// Contains project metadata, survey list, paths, and validation.
/// This was the root object in the old desktop app.
/// </summary>
[Serializable]
public class Project : ObjectBase
{
    #region Members

    private Guid _id = Guid.NewGuid();
    private string _projectName = string.Empty;
    private string _clientName = string.Empty;
    private string _researcherLabel = string.Empty;
    private string _researcherSubLabel = string.Empty;
    private Image? _researcherLogo;
    private string _researcherLogoFilename = string.Empty;
    private string _researcherLogoPath = string.Empty;
    private ObjectListBase<Survey> _surveyList = new();
    private string _fullPath = string.Empty;      // c:\...\Porpoise Default Folder\Project Folder\Filename.porp (not stored)
    private string _fullFolder = string.Empty;    // c:\...\Porpoise Default Folder\Project Folder\ (not stored)
    private string _projectFolder = string.Empty; // \Project Folder\
    private string _fileName = string.Empty;      // Filename.porp
    private string _baseProjectFolder = string.Empty; // \Porpoise Default Folder\
    private ObjectListBase<SurveySummary> _surveyListSummary = new();
    private bool _isExported;

    #endregion

    #region Constructor

    // Default constructor (required for serialization)
    public Project()
    {
    }

    public Project(string baseProjectFolder)
    {
        _baseProjectFolder = baseProjectFolder;
        var s = new Survey();
        _surveyList = new ObjectListBase<Survey>();
        _surveyList.Add(s);
    }

    #endregion

    #region Public Properties

    // Overrides IsDirty to consider isdirty status of responses
    public override bool IsDirty
    {
        get => SurveyList is not null ? base.IsDirty || SurveyList.IsDirty : base.IsDirty;
        set => _isDirty = value;
    }

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value, nameof(Id));
    }

    public string ProjectName
    {
        get => _projectName;
        set => SetProperty(ref _projectName, value, nameof(ProjectName));
    }

    public string ClientName
    {
        get => _clientName;
        set => SetProperty(ref _clientName, value, nameof(ClientName));
    }

    public string ResearcherLabel
    {
        get => _researcherLabel;
        set => SetProperty(ref _researcherLabel, value, "ResearcherName");
    }

    public string ResearcherSubLabel
    {
        get => _researcherSubLabel;
        set => SetProperty(ref _researcherSubLabel, value, nameof(ResearcherSubLabel));
    }

    [XmlIgnore]
    public Image? ResearcherLogo
    {
        get => _researcherLogo;
        set => SetProperty(ref _researcherLogo, value, "ClientLogo");
    }

    public string ResearcherLogoFilename
    {
        get => _researcherLogoFilename;
        set => SetProperty(ref _researcherLogoFilename, value, "ClientLogoFilename");
    }

    public string ResearcherLogoPath
    {
        get => _researcherLogoPath;
        set => SetProperty(ref _researcherLogoPath, value, nameof(ResearcherLogoPath));
    }

    [XmlArrayItem(typeof(SurveySummary))]
    public ObjectListBase<SurveySummary> SurveyListSummary
 Valdez get => _surveyListSummary;
    set => _surveyListSummary = value;
    }

    [XmlIgnore]
    public ObjectListBase<Survey> SurveyList
    {
        get => _surveyList;
        set
        {
            // Subscribe to list isdirty changed event
            if (!ReferenceEquals(value, _surveyList))
            {
                if (_surveyList is not null)
                    _surveyList.IsDirtyChanged -= Surveys_IsDirtyChanged;

                _surveyList = value;
                _isDirty = true;

                if (value is not null)
                    value.IsDirtyChanged += Surveys_IsDirtyChanged;
            }
        }
    }

    // Catches questionslist isdirty changed event
    private void Surveys_IsDirtyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsDirty")
        {
            _isDirty = true;
            MarkDirty();
        }
    }

    [XmlIgnore]
    public string FullPath
    {
        get => _fullPath;
        set => SetProperty(ref _fullPath, value, "ProjectPathFull");
    }

    [XmlIgnore]
    public string FullFolder
    {
        get => _fullFolder;
        set => SetProperty(ref _fullFolder, value, nameof(FullFolder));
    }

    public string ProjectFolder
    {
        get => _projectFolder;
        set => SetProperty(ref _projectFolder, value, nameof(ProjectFolder));
    }

    public string BaseProjectFolder
    {
        get => _baseProjectFolder;
        set => SetProperty(ref _baseProjectFolder, value, nameof(BaseProjectFolder));
    }

    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value, nameof(FileName));
    }

    public bool IsExported
    {
        get => _isExported;
        set => SetProperty(ref _isExported, value, nameof(IsExported));
    }

    #endregion

    public void SummarizeSurveyList()
    {
        if (SurveyList is null) return;

        var list = new ObjectListBase<SurveySummary>();
        foreach (Survey s in SurveyList)
        {
            var summary = new SurveySummary
            {
                SurveyName = s.SurveyName,
                SurveyFileName = s.SurveyFileName,
                Id = s.Id,
                SurveyFolder = s.SurveyFolder
            };
            list.Add(summary);
        }
        _surveyListSummary = list;
    }

    public override void MarkClean()
    {
        if (_surveyList is not null)
        {
            foreach (Survey s in _surveyList)
                s.MarkClean();
        }
        base.MarkClean();
    }

    public override void MarkAsOld()
    {
        if (_surveyList is not null)
        {
            foreach (Survey s in _surveyList)
                s.MarkAsOld();
        }
        base.MarkAsOld();
    }

    #region Validation Methods

    public void ValidateProjectName()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
            throw new ArgumentNullException(nameof(ProjectName), "Project name is required");

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (ProjectName.Contains(c))
                throw new ArgumentOutOfRangeException(nameof(ProjectName), $"Invalid character '{c}' in project name.");
        }
    }

    public void ValidateClientName()
    {
        if (string.IsNullOrWhiteSpace(ClientName))
            throw new ArgumentNullException(nameof(ClientName), "Client name is required");
    }

    public void ValidateFullPath()
    {
        if (string.IsNullOrWhiteSpace(FullPath))
            throw new ArgumentNullException(nameof(ProjectFolder), "Full project path is required");
    }

    public void ValidateProjectFolder()
    {
        if (string.IsNullOrWhiteSpace(ProjectFolder))
            throw new ArgumentNullException(nameof(ProjectFolder), "Project folder is required");
    }

    #endregion

    #region Methods

    // Given a survey id, find the survey in SurveyList and return it
    public Survey? GetSurveyFromSurveyList(Guid id)
    {
        foreach (Survey survey in SurveyList)
        {
            if (survey.Id == id)
                return survey;
        }
        return null;
    }

    // Given a survey, update the survey list with it
    public bool SaveSurveyInSurveyList(Survey s)
    {
        for (int i = 0; i < SurveyList.Count; i++)
        {
            if (SurveyList[i].Id == s.Id)
            {
                SurveyList[i] = s;
                return true;
            }
        }
        return false;
    }

    // Answers: Are there more than one unlocked survey in this project? Used to enable/disable pool and trend setup buttons
    public bool IsMoreThanOneUnlockedSurvey()
    {
        if (_surveyList is null || _surveyList.Count < 2) return false;

        int cnt = 0;
        foreach (Survey s in _surveyList)
        {
            if (s.LockStatus == LockStatusType.Unlocked) cnt++;
        }
        return cnt > 1;
    }

    #endregion
}