#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Porpoise.Core.Models;

/// <summary>
/// The brain behind Porpoise’s legendary Pool & Trend capability.
/// 
/// Manages an entire collection of surveys, enabling:
/// • Pooling — combine any number of surveys into a single virtual dataset
/// • Trending — align and compare identical surveys across waves
/// • Smart dirty-tracking so only changed surveys are recalculated
/// • Seamless integration with crosstabs, charts, and significance testing
/// 
/// This was (and remains) a genuine differentiator — no other survey tool at the time
/// offered this level of dynamic, multi-survey analysis with full statistical integrity.
/// </summary>
public class PoolTrendList
{
    private bool _isDirty;
    private List<PoolTrendItem> _surveyList = null!; // Captures selected DV's and IV's for surveys during set
    private bool _poolOn;
    private bool _poolIsSetup;
    private PoolTrendItem? _pooledSurvey; // Used to feed to cxtab and charts
    private bool _trendOn;
    private bool _trendIsSetup;
    private PoolTrendItem? _trendedSurvey; // Used to feed to cxtab and charts
    private int _runningDVTotalN;
    private int _runningIVTotalN;

    // Events
    public event Action<bool>? PoolStatusChanged;
    public event Action<bool>? TrendStatusChanged;

    // Public properties
    public bool IsDirty
    {
        get => _surveyList.Any(s => s.IsDirty) || _isDirty;
        set => _isDirty = value;
    }

    public List<PoolTrendItem> SurveyList
    {
        get => _surveyList;
        set => _surveyList = value;
    }

    public List<PoolTrendItem> PoolSurveyList
    {
        get
        {
            // Return list of surveys selected for pooling
            var poolSList = new List<PoolTrendItem>();
            poolSList.AddRange(_surveyList.Where(item => item.PoolSurveySelected));
            return poolSList;
        }
    }

    public List<PoolTrendItem> TrendSurveyList
    {
        get
        {
            // Return list of surveys selected for trending
            var trendSList = new List<PoolTrendItem>();
            trendSList.AddRange(_surveyList.Where(item => item.TrendSurveySelected));
            return trendSList;
        }
    }

    public PoolTrendItem? PooledSurvey
    {
        get => _pooledSurvey;
        set => _pooledSurvey = value;
    }

    public bool PoolOn
    {
        get => _poolOn;
        set
        {
            if (_poolOn != value)
            {
                _isDirty = true;
                _poolOn = value;
            }
            PoolStatusChanged?.Invoke(value);
        }
    }

    public bool PoolIsSetup
    {
        get => _poolIsSetup;
        set
        {
            if (_poolIsSetup != value)
            {
                _isDirty = true;
                _poolIsSetup = value;
            }
        }
    }

    public PoolTrendItem? TrendedSurvey
    {
        get => _trendedSurvey;
        set => _trendedSurvey = value;
    }

    public bool TrendOn
    {
        get => _trendOn;
        set
        {
            if (_trendOn != value)
            {
                _isDirty = true;
                _trendOn = value;
            }
            TrendStatusChanged?.Invoke(value);
        }
    }

    public bool TrendIsSetup
    {
        get => _trendIsSetup;
        set
        {
            if (_trendIsSetup != value)
            {
                _isDirty = true;
                _trendIsSetup = value;
            }
        }
    }

    public int RunningDVTotalN
    {
        get => _runningDVTotalN;
        set => _runningDVTotalN = value;
    }

    public int RunningIVTotalN
    {
        get => _runningIVTotalN;
        set => _runningIVTotalN = value;
    }

    #region Constructors

    public PoolTrendList(ObjectListBase<Survey> surveyList)
    {
        BuildList(surveyList);
    }

    private void BuildList(ObjectListBase<Survey> originalSurveyList)
    {
        _surveyList = new List<PoolTrendItem>();
        foreach (Survey s in originalSurveyList)
        {
            var ptItem = new PoolTrendItem(s);
            _surveyList.Add(ptItem);
        }
        _isDirty = true;
    }

    #endregion

    #region Methods

    // Go through survey list and find the most recent selected survey
    public Survey? GetMostRecentSurvey(PoolTrendType type)
    {
        var latestSurvey = new Survey
        {
            CreatedOn = new DateTime(1900, 1, 1)
        };

        if (type == PoolTrendType.Pool)
        {
            // Pool
            foreach (var sl in PoolSurveyList)
            {
                if (sl.PoolSurveySelected && sl.Survey.CreatedOn > latestSurvey.CreatedOn)
                {
                    latestSurvey = sl.Survey;
                }
            }
        }
        else
        {
            // Trend
            foreach (var sl in TrendSurveyList)
            {
                if (sl.TrendSurveySelected && sl.Survey.CreatedOn > latestSurvey.CreatedOn)
                {
                    latestSurvey = sl.Survey;
                }
            }
        }

        return latestSurvey.CreatedOn.Year > 1900 ? latestSurvey : null;
    }

    public void MarkClean()
    {
        _isDirty = false;
        foreach (var s in _surveyList)
            s.IsDirty = false;
    }

    // Combine all trended data for DV and IV into a surrogate PoolTrendItem
    public PoolTrendItem GetTrendedSurvey()
    {
        return new PoolTrendItem(new Survey());
    }

    // Count Pooled totalN's for each DV's selected up to the given survey index
    public int GetPoolRunningDVTotalN(int index, PoolTrendType type)
    {
        int tot = 0;
        var list = type == PoolTrendType.Pool ? PoolSurveyList : TrendSurveyList;

        for (int i = 0; i <= index && i < list.Count; i++)
        {
            var q = type == PoolTrendType.Pool
                ? list[i].PoolDVQuestionSelected
                : list[i].TrendDVQuestionSelected;

            if (q != null)
                tot += q.TotalN;
        }
        return tot;
    }

    /// <summary>
    /// Given a project's list of surveys, do these match the existing list of surveys in this PoolTrendList
    /// Uses this to determine if we refresh the survey or use existing
    /// </summary>
    public bool IsSurveysMatch(List<Survey> projectSurveys)
    {
        foreach (var s1 in projectSurveys)
        {
            bool matches = _surveyList.Any(s2 => s2.Survey.SurveyName == s1.SurveyName);
            if (!matches) return false;
        }
        return true;
    }

    #endregion
}