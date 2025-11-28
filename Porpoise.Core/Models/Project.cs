using Porpoise.Core.Models;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Porpoise.Core.Models
{
    public class Project : ObjectBase
    {
        #region Fields

        private Guid _id = Guid.NewGuid();
        private string? _projectName;
        private string? _clientName;
        private string? _researcherLabel;
        private string? _researcherSubLabel;
        // Legacy WinForms: public Image? ResearcherLogo { get; set; }
        // Web version: use URL or base64
        private string? _researcherLogo = null;
        //private Image? _researcherLogo;
        private string? _researcherLogoFilename;
        private string? _researcherLogoPath;
        private ObjectListBase<Survey>? _surveyList;
        private ObjectListBase<SurveySummary>? _surveyListSummary;

        // These are runtime-only (not serialized)
        private string? _fullPath;
        private string? _fullFolder;
        private string? _projectFolder;
        private string? _fileName;
        private string? _baseProjectFolder;

        private bool _isExported;

        #endregion

        #region Constructors

        public Project()
        {
            // Parameterless ctor required for XML serialization
        }

        public Project(string baseProjectFolder)
        {
            _baseProjectFolder = baseProjectFolder;
            _surveyList = [new Survey()];
        }

        #endregion

        #region Public Properties

        public override bool IsDirty
        {
            get => base.IsDirty || _surveyList?.IsDirty == true;
            set => base.IsDirty = value;
        }

        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value, nameof(Id));
        }

        public string? ProjectName
        {
            get => _projectName;
            set => SetProperty(ref _projectName, value, nameof(ProjectName));
        }

        public string? ClientName
        {
            get => _clientName;
            set => SetProperty(ref _clientName, value, nameof(ClientName));
        }

        public string? ResearcherLabel
        {
            get => _researcherLabel;
            set => SetProperty(ref _researcherLabel, value, nameof(ResearcherLabel));
        }

        public string? ResearcherSubLabel
        {
            get => _researcherSubLabel;
            set => SetProperty(ref _researcherSubLabel, value, nameof(ResearcherSubLabel));
        }

        [XmlIgnore]
        // Legacy WinForms: public Image? ResearcherLogo { get; set; }
        // Web version: use URL or base64
        public string? ResearcherLogo { 
            get => _researcherLogo; 
            set => SetProperty(ref _researcherLogo, value, nameof(ResearcherLogo));
        }
        // public Image? ResearcherLogo
        // {
        //     get => _researcherLogo;
        //     set => SetProperty(ref _researcherLogo, value, nameof(ResearcherLogo));
        // }

        public string? ResearcherLogoFilename
        {
            get => _researcherLogoFilename;
            set => SetProperty(ref _researcherLogoFilename, value, nameof(ResearcherLogoFilename));
        }

        public string? ResearcherLogoPath
        {
            get => _researcherLogoPath;
            set => SetProperty(ref _researcherLogoPath, value, nameof(ResearcherLogoPath));
        }

        [XmlArrayItem(typeof(SurveySummary))]
        public ObjectListBase<SurveySummary>? SurveyListSummary
        {
            get => _surveyListSummary;
            set => _surveyListSummary = value;
        }

        [XmlIgnore]
        public ObjectListBase<Survey>? SurveyList
        {
            get => _surveyList;
            set
            {
                if (_surveyList != value)
                {
                    if (_surveyList != null)
                        _surveyList.IsDirtyChanged -= Surveys_IsDirtyChanged;

                    _surveyList = value;
                    MarkDirty();

                    if (_surveyList != null)
                        _surveyList.IsDirtyChanged += Surveys_IsDirtyChanged;
                }
            }
        }

        [XmlIgnore]
        public string? FullPath
        {
            get => _fullPath;
            set => SetProperty(ref _fullPath, value, nameof(FullPath));
        }

        [XmlIgnore]
        public string? FullFolder
        {
            get => _fullFolder;
            set => SetProperty(ref _fullFolder, value, nameof(FullFolder));
        }

        public string? ProjectFolder
        {
            get => _projectFolder;
            set => SetProperty(ref _projectFolder, value, nameof(ProjectFolder));
        }

        public string? BaseProjectFolder
        {
            get => _baseProjectFolder;
            set => SetProperty(ref _baseProjectFolder, value, nameof(BaseProjectFolder));
        }

        public string? FileName
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

        #region Private Event Handlers

        private void Surveys_IsDirtyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsDirty))
            {
                MarkDirty();
            }
        }

        #endregion

        #region Public Methods

        public void SummarizeSurveyList()
        {
            if (SurveyList == null) return;

            var list = new ObjectListBase<SurveySummary>();

            foreach (var s in SurveyList)
            {
                var summary = new SurveySummary
                {
                    Id = s.Id,
                    SurveyName = s.SurveyName,
                    SurveyFileName = s.SurveyFileName,
                    SurveyFolder = s.SurveyFolder
                };
                list.Add(summary);
            }

            _surveyListSummary = list;
        }

        public override void MarkClean()
        {
            SurveyList?.ForEach(s => s.MarkClean());
            SurveyList?.MarkClean();
            base.MarkClean();
        }

        public override void MarkAsOld()
        {
            SurveyList?.ForEach(s => s.MarkAsOld());
            base.MarkAsOld();
        }

        public Survey? GetSurveyFromSurveyList(Guid id)
        {
            return SurveyList?.FirstOrDefault(s => s.Id == id);
        }

        public bool SaveSurveyInSurveyList(Survey s)
        {
            if (SurveyList == null || s == null) return false;

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

        public bool IsMoreThanOneUnlockedSurvey()
        {
            if (SurveyList == null || SurveyList.Count < 2) return false;

            int unlockedCount = SurveyList.Count(s => s.LockStatus == LockStatusType.Unlocked);
            return unlockedCount > 1;
        }

        #endregion

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
                throw new ArgumentNullException(nameof(FullPath), "Full project path is required");
        }

        public void ValidateProjectFolder()
        {
            if (string.IsNullOrWhiteSpace(ProjectFolder))
                throw new ArgumentNullException(nameof(ProjectFolder), "Project folder is required");
        }

        #endregion
    }
}