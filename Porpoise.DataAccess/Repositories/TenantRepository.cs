using System.Data;
using Dapper;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.DataAccess.Context;

namespace Porpoise.DataAccess.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly DapperContext _context;

    public TenantRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(int tenantId)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT TenantId, TenantKey, Name, IsActive, CreatedAt 
            FROM Tenants 
            WHERE TenantId = @TenantId";
        
        return await connection.QueryFirstOrDefaultAsync<Tenant>(sql, new { TenantId = tenantId });
    }

    public async Task<Tenant?> GetByKeyAsync(string tenantKey)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT TenantId, TenantKey, Name, IsActive, CreatedAt 
            FROM Tenants 
            WHERE TenantKey = @TenantKey";
        
        return await connection.QueryFirstOrDefaultAsync<Tenant>(sql, new { TenantKey = tenantKey });
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            SELECT TenantId, TenantKey, Name, IsActive, CreatedAt 
            FROM Tenants 
            ORDER BY Name";
        
        return await connection.QueryAsync<Tenant>(sql);
    }

    public async Task<int> AddAsync(Tenant tenant)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            INSERT INTO Tenants (TenantKey, Name, IsActive) 
            VALUES (@TenantKey, @Name, @IsActive);
            SELECT LAST_INSERT_ID();";
        
        return await connection.ExecuteScalarAsync<int>(sql, tenant);
    }

    public async Task<bool> UpdateAsync(Tenant tenant)
    {
        using var connection = _context.CreateConnection();
        const string sql = @"
            UPDATE Tenants 
            SET TenantKey = @TenantKey, 
                Name = @Name, 
                IsActive = @IsActive 
            WHERE TenantId = @TenantId";
        
        var rowsAffected = await connection.ExecuteAsync(sql, tenant);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int tenantId)
    {
        using var connection = _context.CreateConnection();
        const string sql = "DELETE FROM Tenants WHERE TenantId = @TenantId";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { TenantId = tenantId });
        return rowsAffected > 0;
    }
}
