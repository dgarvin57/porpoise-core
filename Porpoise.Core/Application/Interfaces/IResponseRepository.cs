#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

/// <summary>
/// Repository interface for Response entities.
/// Manages question responses and their associated data.
/// </summary>
public interface IResponseRepository : IRepository<Response>
{
    /// <summary>
    /// Get all responses for a specific question.
    /// </summary>
    Task<IEnumerable<Response>> GetByQuestionIdAsync(Guid questionId);
    
    /// <summary>
    /// Get a response by its value for a specific question.
    /// </summary>
    Task<Response?> GetByResponseValueAsync(Guid questionId, int responseValue);
    
    /// <summary>
    /// Get responses by index type.
    /// </summary>
    Task<IEnumerable<Response>> GetByIndexTypeAsync(Guid questionId, ResponseIndexType indexType);
    
    /// <summary>
    /// Delete all responses for a question.
    /// </summary>
    Task DeleteByQuestionIdAsync(Guid questionId);
}
