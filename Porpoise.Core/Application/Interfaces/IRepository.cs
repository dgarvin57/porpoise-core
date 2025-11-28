#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Porpoise.Core.Application.Interfaces;

/// <summary>
/// Generic repository interface for common CRUD operations.
/// Provides async data access patterns for all entities.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get an entity by its unique identifier.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all entities of this type.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Add a new entity.
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Update an existing entity.
    /// </summary>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Delete an entity by its unique identifier.
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Check if an entity exists by its unique identifier.
    /// </summary>
    Task<bool> ExistsAsync(Guid id);
}
