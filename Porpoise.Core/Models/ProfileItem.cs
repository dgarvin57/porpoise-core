#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a single profile percentage item used in the old Profile tab.
/// Contains percentage difference, marginal percent, and display labels.
/// </summary>
public class ProfileItem
{
    private double _percDiff;
    private double _percNum;
    private double _marginalPercent;
    private string _responseLabel = string.Empty;
    private string _questionLabel = string.Empty;
    private Guid _id;

    public double PercDiff
    {
        get => _percDiff;
        set => _percDiff = value;
    }

    public double PercNum
    {
        get => _percNum;
        set => _percNum = value;
    }

    public double MarginalPercent
    {
        get => _marginalPercent;
        set => _marginalPercent = value;
    }

    public string ResponseLabel
    {
        get => _responseLabel;
        set => _responseLabel = value;
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

    #region Public Methods

    // Gather all colors and return in a byte array
    public static byte[] GetColors()
    {
        var colorArray = new List<byte>();

        // Get colors (preserving exact original order and method calls)
        colorArray.AddRange(ErrorLog.GetOrange());
        colorArray.AddRange(Anova.GetGreen());
        colorArray.AddRange(Survey.GetViolet());
        colorArray.AddRange(TwoBlockIndex.GetBrown());

        return colorArray.ToArray();
    }

    #endregion
}