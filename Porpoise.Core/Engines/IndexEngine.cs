#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Builds a sorted list of IndexItems for all Independent Variables (IVs) against a single Dependent Variable (DV).
/// Used heavily in Topline Index reports and driver analysis.
/// </summary>
public class IndexEngine
{
    public List<IndexItem> Indexes { get; } = [];

    public IndexEngine(Survey survey, Question dvQuestion)
    {
        ArgumentNullException.ThrowIfNull(survey);
        ArgumentNullException.ThrowIfNull(dvQuestion);
        if (survey.Data == null) throw new InvalidOperationException("Survey data is required for index analysis.");

        BuildIndexList(survey, dvQuestion);
    }

    private void BuildIndexList(Survey survey, Question dvQuestion)
    {
        List<IndexItem> indexList = [];

        foreach (var ivQuestion in survey.QuestionList.Where(q => q.VariableType == QuestionVariableType.Independent))
        {
            var crosstab = new Crosstab(survey.Data!, dvQuestion, ivQuestion, showCount: false, onProfileTab: false);
            indexList.AddRange(crosstab.GetIndexesList());
        }

        // Sort descending by Index value (highest drivers first)
        Indexes.AddRange(indexList.OrderByDescending(item => item.Index));
    }
}