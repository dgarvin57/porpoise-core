#nullable enable

using System.Collections.Generic;

namespace Porpoise.Core.Models;

public class ResponseComparer : Comparer<Response>
{
    public override int Compare(Response? x, Response? y)
    {
        if (x is null || y is null) return 0;
        return string.Compare(x.Label, y.Label, StringComparison.Ordinal);
    }
}