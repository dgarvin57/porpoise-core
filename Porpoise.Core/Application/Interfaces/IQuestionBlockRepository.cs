#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

/// <summary>
/// Repository interface for managing question blocks (normalized block information).
/// </summary>
public interface IQuestionBlockRepository
{
    Task<QuestionBlock?> GetByIdAsync(Guid id);
    Task<IEnumerable<QuestionBlock>> GetBySurveyIdAsync(Guid surveyId);
    Task<QuestionBlock?> GetByLabelAsync(Guid surveyId, string label);
    Task<QuestionBlock> AddAsync(QuestionBlock block);
    Task<QuestionBlock> UpdateAsync(QuestionBlock block);
    Task DeleteAsync(Guid id);
    Task DeleteBySurveyIdAsync(Guid surveyId);
}
