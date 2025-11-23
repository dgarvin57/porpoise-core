#nullable enable

using System.Collections.Generic;

namespace Porpoise.Core.Models;

public class QuestionComparer : Comparer<Question>
{
    public override int Compare(Question? x, Question? y)
    {
        if (x is null || y is null) return 0;
        return string.Compare(x.QstNumber, y.QstNumber, StringComparison.Ordinal);
    }
}