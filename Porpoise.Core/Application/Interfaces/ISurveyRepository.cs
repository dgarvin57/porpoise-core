#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

/// <summary>
/// Repository interface for Survey entities with specialized query methods.
/// Extends the base repository with survey-specific operations.
/// </summary>
public interface ISurveyRepository : IRepository<Survey>
{
    /// <summary>
    /// Get a survey by its name.
    /// </summary>
    Task<Survey?> GetByNameAsync(string surveyName);

    /// <summary>
    /// Get surveys by status.
    /// </summary>
    Task<IEnumerable<Survey>> GetByStatusAsync(SurveyStatus status);

    /// <summary>
    /// Search surveys by name (partial match).
    /// </summary>
    Task<IEnumerable<Survey>> SearchByNameAsync(string searchTerm);

    /// <summary>
    /// Get surveys created within a date range.
    /// </summary>
    Task<IEnumerable<Survey>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get the count of questions for a survey.
    /// </summary>
    Task<int> GetQuestionCountAsync(Guid surveyId);

    /// <summary>
    /// Get the count of responses for a survey.
    /// </summary>
    Task<int> GetResponseCountAsync(Guid surveyId);

    /// <summary>
    /// Check if a survey name already exists (for validation).
    /// </summary>
    Task<bool> SurveyNameExistsAsync(string surveyName, Guid? excludeSurveyId = null);
}
