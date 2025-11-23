#nullable enable

using System.ComponentModel;

namespace Porpoise.Core.Models;

public enum SelectPlusConditionType
{
    [Description("None")] None = 0,
    [Description("Goes positive")] GoesPositive = 1,
    [Description("Goes negative")] GoesNegative = 2,
    [Description("Stays positive")] StaysPositive = 3,
    [Description("Stays negative")] StaysNegative = 4,
    [Description("Stays neutral")] StaysNeutral = 5
}