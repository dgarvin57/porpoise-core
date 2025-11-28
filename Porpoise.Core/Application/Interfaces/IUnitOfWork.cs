#nullable enable

using System;
using System.Threading.Tasks;

namespace Porpoise.Core.Application.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing database transactions.
/// Ensures multiple repository operations can be committed or rolled back together.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Repository for Survey operations.
    /// </summary>
    ISurveyRepository Surveys { get; }

    /// <summary>
    /// Commit all changes made in this unit of work.
    /// </summary>
    Task<int> CommitAsync();

    /// <summary>
    /// Roll back all changes made in this unit of work.
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// Begin a new transaction.
    /// </summary>
    Task BeginTransactionAsync();
}
