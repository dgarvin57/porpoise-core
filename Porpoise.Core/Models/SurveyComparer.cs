#nullable enable

using System.Collections.Generic;

namespace Porpoise.Core.Models;

public class SurveyComparer : Comparer<Survey>
{
    public override int Compare(Survey? x, Survey? y)
    {
        if (x is null || y is null) return 0;
        return string.Compare(x.SurveyName, y.SurveyName, StringComparison.Ordinal);
    }
}