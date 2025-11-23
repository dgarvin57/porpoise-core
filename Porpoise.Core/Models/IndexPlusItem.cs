#nullable enable

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a paired index item (typically used for "before/after" or "first/last" style questions).
/// Provides a calculated IndexPlus difference between Index2 and Index1.
/// </summary>
public sealed record IndexPlusItem
{
    /// <summary>
    /// First index value (e.g., starting position or "before" value).
    /// </summary>
    public int Index1 { get; init; }

    /// <summary>
    /// Second index value (e.g., ending position or "after" value).
    /// </summary>
    public int Index2 { get; init; }

    /// <summary>
    /// Calculated difference: Index2 - Index1.
    /// </summary>
    public int IndexPlus => Index2 - Index1;

    /// <summary>
    /// Text shown to the respondent for this response option.
    /// </summary>
    public string? ResponseLabel { get; init; }

    /// <summary>
    /// Label or text of the question this item belongs to.
    /// </summary>
    public string? QuestionLabel { get; init; }

    /// <summary>
    /// Unique identifier for the item.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
}