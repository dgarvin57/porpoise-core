#nullable enable

namespace Porpoise.Core.Models;

/// <summary>
/// Represents one survey in the revolutionary Pool & Trend engine.
/// 
/// This class tracks whether a survey is included in a pooled analysis (combining multiple surveys)
/// or a trend analysis (comparing the same survey over time), along with the exact DV and IV
/// selected for each mode. It powers one of Porpoise’s most powerful and unique features:
/// real-time multi-survey aggregation and longitudinal tracking — something very few tools
/// have ever offered with this level of precision and flexibility.
/// </summary>
public class PoolTrendItem
{
    #region Private Members

    private bool _isDirty;
    private Survey _survey = null!;
    private readonly string _surveyName;
    private bool _poolSurveySelected;
    private Question? _poolDVQuestionSelected;
    private Question? _poolIVQuestionSelected;
    private bool _trendSurveySelected;
    private Question? _trendDVQuestionSelected;
    private Question? _trendIVQuestionCreated;

    #endregion

    #region Public Properties

    public bool IsDirty
    {
        get => _isDirty && _survey.IsDirty;
        set => _isDirty = value;
    }

    public Survey Survey
    {
        get => _survey;
        set => _survey = value;
    }

    public string SurveyName
    {
        get => _survey.SurveyName;
    }

    public bool PoolSurveySelected
    {
        get => _poolSurveySelected;
        set
        {
            if (_poolSurveySelected != value)
            {
                _isDirty = true;
                _poolSurveySelected = value;
            }
        }
    }

    public Question? PoolDVQuestionSelected
    {
        get => _poolDVQuestionSelected;
        set
        {
            _isDirty = true;
            _poolDVQuestionSelected = value;
        }
    }

    public Question? PoolIVQuestionSelected
    {
        get => _poolIVQuestionSelected;
        set
        {
            _isDirty = true;
            _poolIVQuestionSelected = value;
        }
    }

    public Question? TrendDVQuestionSelected
    {
        get => _trendDVQuestionSelected;
        set
        {
            _isDirty = true;
            _trendDVQuestionSelected = value;
        }
    }

    public Question? TrendIVQuestionCreated
    {
        get => _trendIVQuestionCreated;
        set => _trendIVQuestionCreated = value;
    }

    public bool TrendSurveySelected
    {
        get => _trendSurveySelected;
        set
        {
            if (_trendSurveySelected != value)
            {
                _isDirty = true;
                _trendSurveySelected = value;
            }
        }
    }

    #endregion

    #region Constructors

    public PoolTrendItem(Survey survey)
    {
        _survey = survey.Clone();
        _surveyName = survey.SurveyName;
    }

    #endregion

    #region Methods
    // (empty in original)
    #endregion
}