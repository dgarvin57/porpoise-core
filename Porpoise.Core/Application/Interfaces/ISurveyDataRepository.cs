#nullable enable

using System;
using System.Threading.Tasks;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

/// <summary>
/// Repository interface for SurveyData entities.
/// Manages raw survey response data storage.
/// </summary>
public interface ISurveyDataRepository : IRepository<SurveyData>
{
    /// <summary>
    /// Get survey data by survey ID.
    /// </summary>
    Task<SurveyData?> GetBySurveyIdAsync(Guid surveyId);
    
    /// <summary>
    /// Get the number of cases (rows) in the survey data.
    /// </summary>
    Task<int> GetCaseCountBySurveyIdAsync(Guid surveyId);
    
    /// <summary>
    /// Delete survey data by survey ID.
    /// </summary>
    Task DeleteBySurveyIdAsync(Guid surveyId);

    /// <summary>
    /// Add survey data with explicit survey ID.
    /// </summary>
    Task<SurveyData> AddAsync(SurveyData surveyData, Guid surveyId);
}
