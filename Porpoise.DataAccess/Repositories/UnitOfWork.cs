#nullable enable

using System;
using System.Data;
using System.Threading.Tasks;
using Porpoise.Core.Application.Interfaces;
using Porpoise.DataAccess.Context;

namespace Porpoise.DataAccess.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions across repositories.
/// Keeps things simple—one transaction, multiple operations, commit or rollback.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DapperContext _context;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public ISurveyRepository Surveys { get; }
    public ProjectRepository Projects { get; }

    public UnitOfWork(DapperContext context, ISurveyRepository surveyRepository, ProjectRepository projectRepository)
    {
        _context = context;
        Surveys = surveyRepository;
        Projects = projectRepository;
    }

    public async Task BeginTransactionAsync()
    {
        _connection = _context.CreateConnection();
        _connection.Open();
        _transaction = _connection.BeginTransaction();
        await Task.CompletedTask;
    }

    public async Task<int> CommitAsync()
    {
        try
        {
            _transaction?.Commit();
            return 1;
        }
        catch
        {
            _transaction?.Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
            await Task.CompletedTask;
        }
    }

    public async Task RollbackAsync()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
        _disposed = true;
    }
}
