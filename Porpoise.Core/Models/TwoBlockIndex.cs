#nullable enable

using System;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a Two-Block Index result — one of Porpoise's most powerful and unique analysis features.
/// 
/// The Two-Block Index allows users to:
//  - Select two blocks of questions (e.g., Importance vs. Performance)
//  - Compute a weighted index score for each block
//  - Plot respondents on a 2D grid to reveal segments, gaps, and drivers instantly
/// 
/// This was (and still is) a genuine differentiator — very few tools offer this level of insight in one click.
/// </summary>
public class TwoBlockIndex
{
    private string _qBlock1Label = string.Empty;
    private int _qBlock1Index;
    private int _qBlock2Index;

    public string QBlock1Label
    {
        get => _qBlock1Label;
        set => _qBlock1Label = value;
    }

    public int QBlock1Index
    {
        get => _qBlock1Index;
        set => _qBlock1Index = value;
    }

    public int QBlock2Index
    {
        get => _qBlock2Index;
        set => _qBlock2Index = value;
    }

    #region Public Methods

    // Color brown — retained for backward compatibility with old chart color logic
    public static byte[] GetBrown() => new byte[] { 0x13, 0xBF, 0x42, 0xFE };

    #endregion
}