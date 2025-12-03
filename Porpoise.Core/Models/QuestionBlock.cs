#nullable enable

using System;
using System.ComponentModel.DataAnnotations;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a block of related questions in a survey.
/// Normalizes block information that was previously duplicated across multiple questions.
/// </summary>
public class QuestionBlock
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to the survey this block belongs to
    /// </summary>
    [Required]
    public Guid SurveyId { get; set; }

    /// <summary>
    /// The block label/identifier (e.g., "Candidate Vital Signs", "Ballot Tests")
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The block stem text - the introductory text that applies to all questions in the block
    /// </summary>
    public string Stem { get; set; } = string.Empty;

    /// <summary>
    /// Display order for this block within the survey
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Navigation property to the survey
    /// </summary>
    public virtual Survey? Survey { get; set; }

    /// <summary>
    /// Navigation property to questions in this block
    /// </summary>
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
