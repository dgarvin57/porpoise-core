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

    /// <summary>
    /// Soft delete a survey.
    /// </summary>
    Task<bool> SoftDeleteSurveyAsync(Guid surveyId);

    /// <summary>
    /// Restore a soft-deleted survey.
    /// </summary>
    Task<bool> RestoreSurveyAsync(Guid surveyId);

    /// <summary>
    /// Permanently delete a survey and all related data.
    /// </summary>
    Task<bool> PermanentlyDeleteSurveyAsync(Guid surveyId);

    /// <summary>
    /// Get all deleted surveys (trash).
    /// </summary>
    Task<IEnumerable<dynamic>> GetDeletedSurveysAsync();

    /// <summary>
    /// Get recently accessed surveys with metadata (project name, counts).
    /// </summary>
    Task<IEnumerable<dynamic>> GetRecentlyAccessedAsync(int limit = 4);
}
