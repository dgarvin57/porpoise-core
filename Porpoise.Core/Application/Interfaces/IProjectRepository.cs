using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

/// <summary>
/// Repository interface for Project-specific operations.
/// Extends base repository with project-related queries.
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// Get a project by its name.
    /// </summary>
    Task<Project?> GetByNameAsync(string projectName);

    /// <summary>
    /// Get all projects for a specific client.
    /// </summary>
    Task<IEnumerable<Project>> GetByClientAsync(string clientName);

    /// <summary>
    /// Get a project with all its associated surveys loaded.
    /// </summary>
    Task<Project?> GetProjectWithSurveysAsync(Guid projectId);

    /// <summary>
    /// Get all surveys associated with a specific project.
    /// </summary>
    Task<IEnumerable<Survey>> GetSurveysByProjectIdAsync(Guid projectId);

    /// <summary>
    /// Get surveys for a project with question and data counts.
    /// </summary>
    Task<IEnumerable<dynamic>> GetSurveysWithCountsAsync(Guid projectId);

    /// <summary>
    /// Get all projects with survey counts (for UI display).
    /// </summary>
    Task<IEnumerable<dynamic>> GetProjectsWithSurveyCountAsync();

    /// <summary>
    /// Get only projects that have multiple surveys (for pooling/trending).
    /// </summary>
    Task<IEnumerable<dynamic>> GetMultiSurveyProjectsAsync();

    /// <summary>
    /// Move a survey to a different project.
    /// </summary>
    Task<bool> MoveSurveyToProjectAsync(Guid surveyId, Guid newProjectId);

    /// <summary>
    /// Soft delete a project and all its surveys.
    /// </summary>
    Task<bool> SoftDeleteProjectAsync(Guid projectId);

    /// <summary>
    /// Restore a soft-deleted project and all its surveys.
    /// </summary>
    Task<bool> RestoreProjectAsync(Guid projectId);

    /// <summary>
    /// Permanently delete a project and all related data.
    /// </summary>
    Task<bool> PermanentlyDeleteProjectAsync(Guid projectId);

    /// <summary>
    /// Get all deleted projects (trash).
    /// </summary>
    Task<IEnumerable<dynamic>> GetDeletedProjectsAsync();
}
