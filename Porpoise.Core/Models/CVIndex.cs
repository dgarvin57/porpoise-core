#nullable enable

namespace Porpoise.Core.Models;

/// <summary>
/// Represents the calculated index values (Positive, Negative, Neutral, Overall) 
/// for a single column (IV response) in a crosstab.
/// </summary>
public class CxIVIndex
{
    private string _ivLabel = string.Empty;
    private double _posIndex;
    private double _negIndex;
    private double _neutralIndex;
    private int _index;

    public string IVLabel
    {
        get => _ivLabel;
        set => _ivLabel = value;
    }

    public double PosIndex
    {
        get => _posIndex;
        set => _posIndex = value;
    }

    public double NegIndex
    {
        get => _negIndex;
        set => _negIndex = value;
    }

    public double NeutralIndex
    {
        get => _neutralIndex;
        set => _neutralIndex = value;
    }

    public int Index
    {
        get => _index;
        set => _index = value;
    }

    #region Methods

    // Color Yellow
    public static byte[] GetYellow()
    {
        byte[] rYellow = { 0xEA, 0xFB, 0x7A, 0x50 };
        return rYellow;
    }

    #endregion
}