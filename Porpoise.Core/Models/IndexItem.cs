#nullable enable

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a single response option in an index-style question.
/// </summary>
public sealed record IndexItem
{
    /// <summary>
    /// Sequential index of the response within the question.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Text shown to the respondent (e.g., "Strongly Agree").
    /// </summary>
    public string? ResponseLabel { get; init; }

    /// <summary>
    /// The question text or label this item belongs to.
    /// </summary>
    public string? QuestionLabel { get; init; }

    /// <summary>
    /// Unique identifier for the item.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    // Kept exactly as it was in the original — looks like a magic support key
    public static string GetSupport1() => "S9kqcpJQjxmbkdo8/";
}