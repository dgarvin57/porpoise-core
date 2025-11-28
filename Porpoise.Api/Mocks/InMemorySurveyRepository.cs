#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

namespace Porpoise.Api.Mocks;

/// <summary>
/// In-memory implementation of ISurveyRepository for testing without a database.
/// Stores surveys in a List instead of MySQL.
/// </summary>
public class InMemorySurveyRepository : ISurveyRepository
{
    private readonly List<Survey> _surveys = new();

    public Task<Survey?> GetByIdAsync(Guid id)
    {
        var survey = _surveys.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(survey);
    }

    public Task<IEnumerable<Survey>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Survey>>(_surveys.ToList());
    }

    public Task<Survey> AddAsync(Survey survey)
    {
        if (survey.Id == Guid.Empty)
            survey.Id = Guid.NewGuid();
        
        _surveys.Add(survey);
        return Task.FromResult(survey);
    }

    public Task<Survey> UpdateAsync(Survey survey)
    {
        var existing = _surveys.FirstOrDefault(s => s.Id == survey.Id);
        if (existing != null)
        {
            _surveys.Remove(existing);
            _surveys.Add(survey);
        }
        return Task.FromResult(survey);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var survey = _surveys.FirstOrDefault(s => s.Id == id);
        if (survey != null)
        {
            _surveys.Remove(survey);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        var exists = _surveys.Any(s => s.Id == id);
        return Task.FromResult(exists);
    }

    public Task<Survey?> GetByNameAsync(string surveyName)
    {
        var survey = _surveys.FirstOrDefault(s => 
            s.SurveyName.Equals(surveyName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(survey);
    }

    public Task<IEnumerable<Survey>> GetByStatusAsync(SurveyStatus status)
    {
        var surveys = _surveys.Where(s => s.Status == status).ToList();
        return Task.FromResult<IEnumerable<Survey>>(surveys);
    }

    public Task<IEnumerable<Survey>> SearchByNameAsync(string searchTerm)
    {
        var surveys = _surveys.Where(s => 
            s.SurveyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult<IEnumerable<Survey>>(surveys);
    }

    public Task<IEnumerable<Survey>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        // For simplicity, return all surveys (could enhance with CreatedDate property)
        return Task.FromResult<IEnumerable<Survey>>(_surveys.ToList());
    }

    public Task<int> GetQuestionCountAsync(Guid surveyId)
    {
        var survey = _surveys.FirstOrDefault(s => s.Id == surveyId);
        var count = survey?.QuestionList?.Count ?? 0;
        return Task.FromResult(count);
    }

    public Task<int> GetResponseCountAsync(Guid surveyId)
    {
        var survey = _surveys.FirstOrDefault(s => s.Id == surveyId);
        var count = (int)(survey?.ResponsesNumber ?? 0);
        return Task.FromResult(count);
    }

    public Task<bool> SurveyNameExistsAsync(string surveyName, Guid? excludeSurveyId = null)
    {
        var exists = _surveys.Any(s => 
            s.SurveyName.Equals(surveyName, StringComparison.OrdinalIgnoreCase) &&
            (!excludeSurveyId.HasValue || s.Id != excludeSurveyId.Value));
        return Task.FromResult(exists);
    }

    // Helper methods for testing
    public void Clear() => _surveys.Clear();
    public int Count => _surveys.Count;
}
