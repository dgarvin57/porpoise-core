#nullable enable

using System;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a single response item in "One Response" analysis mode.
/// Contains percentage, index value, question label, and ID.
/// </summary>
public class OneResponseItem
{
    private double _percent;
    private int _index;
    private string _questionLabel = string.Empty;
    private Guid _id;

    public double Percent
    {
        get => _percent;
        set => _percent = value;
    }

    public int Index
    {
        get => _index;
        set => _index = value;
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