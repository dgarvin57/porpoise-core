#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Porpoise.Core.Application.Interfaces;
using Porpoise.DataAccess.Context;

namespace Porpoise.DataAccess.Repositories;

/// <summary>
/// Base repository providing common CRUD operations with Dapper.
/// This is your foundation—simple, straightforward, no magic.
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly DapperContext _context;
    protected abstract string TableName { get; }

    protected Repository(DapperContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        var sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var sql = $"SELECT * FROM {TableName}";
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<T>(sql);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        // Override this in derived classes with specific insert logic
        throw new NotImplementedException("AddAsync must be implemented in derived class");
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        // Override this in derived classes with specific update logic
        throw new NotImplementedException("UpdateAsync must be implemented in derived class");
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var sql = $"DELETE FROM {TableName} WHERE Id = @Id";
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        var sql = $"SELECT COUNT(1) FROM {TableName} WHERE Id = @Id";
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}
