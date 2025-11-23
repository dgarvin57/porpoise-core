#nullable enable

using System;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a single statistical significance result item (used by old crosstab significance tab).
/// Contains Phi coefficient, significance level, question label, and ID.
/// </summary>
public class StatSigItem
{
    private double _phi;
    private string _significance = string.Empty;
    private string _questionLabel = string.Empty;
    private Guid _id;

    public double Phi
    {
        get => _phi;
        set => _phi = value;
    }

    public string Significance
    {
        get => _significance;
        set => _significance = value;
    }

    public string QuestionLabel
    {
        get => _questionLabel;
        set => _questionLabel = value;
    }

    public Guid Id
    {
        get => _id;
        set => _id = value;
    }
}