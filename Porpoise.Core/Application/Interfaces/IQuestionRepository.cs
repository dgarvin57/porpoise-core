#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

/// <summary>
/// Repository interface for Question entities.
/// Manages survey questions with their associated responses.
/// </summary>
public interface IQuestionRepository : IRepository<Question>
{
    /// <summary>
    /// Get all questions for a specific survey.
    /// </summary>
    Task<IEnumerable<Question>> GetBySurveyIdAsync(Guid surveyId);
    
    /// <summary>
    /// Get a question by its question number within a survey.
    /// </summary>
    Task<Question?> GetByQuestionNumberAsync(Guid surveyId, string questionNumber);
    
    /// <summary>
    /// Get questions by variable type.
    /// </summary>
    Task<IEnumerable<Question>> GetByVariableTypeAsync(Guid surveyId, QuestionVariableType variableType);
    
    /// <summary>
    /// Get count of responses for a specific question.
    /// </summary>
    Task<int> GetResponseCountAsync(Guid questionId);
    
    /// <summary>
    /// Delete all questions for a survey.
    /// </summary>
    Task DeleteBySurveyIdAsync(Guid surveyId);

    /// <summary>
    /// Add a question with explicit survey ID.
    /// </summary>
    Task<Question> AddAsync(Question question, Guid surveyId);
}
