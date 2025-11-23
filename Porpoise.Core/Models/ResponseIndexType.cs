#nullable enable

using System.ComponentModel;

namespace Porpoise.Core.Models;

public enum ResponseIndexType
{
    [Description("None")] None = 0,
    [Description("Neutral")] Neutral = 1,    // /
    [Description("Positive")] Positive = 2,  // +
    [Description("Negative")] Negative = 3   // -
}