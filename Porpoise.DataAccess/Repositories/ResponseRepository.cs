#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.DataAccess.Context;

namespace Porpoise.DataAccess.Repositories;

/// <summary>
/// Response repository implementation using Dapper.
/// Manages question responses with full CRUD support.
/// </summary>
public class ResponseRepository : Repository<Response>, IResponseRepository
{
    protected override string TableName => "Responses";

    public ResponseRepository(DapperContext context) : base(context) { }

    public override async Task<Response?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT Id, QuestionId, RespValue, Label, 
                   IndexType 
            FROM Responses 
            WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Response>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Response>> GetByQuestionIdAsync(Guid questionId)
    {
        const string sql = @"
            SELECT Id, QuestionId, RespValue, Label, 
                   IndexType 
            FROM Responses 
            WHERE QuestionId = @QuestionId 
            ORDER BY RespValue";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Response>(sql, new { QuestionId = questionId });
    }

    public async Task<Response?> GetByResponseValueAsync(Guid questionId, int responseValue)
    {
        const string sql = @"
            SELECT Id, QuestionId, RespValue, Label, 
                   IndexType 
            FROM Responses 
            WHERE QuestionId = @QuestionId 
            AND RespValue = @ResponseValue";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Response>(sql, 
            new { QuestionId = questionId, ResponseValue = responseValue });
    }

    public async Task<IEnumerable<Response>> GetByIndexTypeAsync(Guid questionId, ResponseIndexType indexType)
    {
        const string sql = @"
            SELECT Id, QuestionId, RespValue, Label, 
                   IndexType 
            FROM Responses 
            WHERE QuestionId = @QuestionId 
            AND IndexType = @IndexType 
            ORDER BY RespValue";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Response>(sql, 
            new { QuestionId = questionId, IndexType = indexType.ToString() });
    }

    public async Task DeleteByQuestionIdAsync(Guid questionId)
    {
        const string sql = "DELETE FROM Responses WHERE QuestionId = @QuestionId";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { QuestionId = questionId });
    }

    public async Task<Response> AddAsync(Response response, Guid questionId)
    {
        const string sql = @"
            INSERT INTO Responses (
                Id, QuestionId, RespValue, Label, 
                IndexType, CreatedDate, ModifiedDate
            ) VALUES (
                @Id, @QuestionId, @RespValue, @Label, 
                @IndexType, @CreatedDate, @ModifiedDate
            )";
        
        response.Id = Guid.NewGuid();
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            response.Id,
            QuestionId = questionId,
            response.RespValue,
            Label = response.Label ?? string.Empty,
            IndexType = response.IndexType.ToString(),
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        });
        
        return response;
    }

    public override async Task<Response> AddAsync(Response response)
    {
        // Fallback for base interface - requires QuestionId to be set externally
        return await AddAsync(response, Guid.Empty);
    }

    public override async Task<Response> UpdateAsync(Response response)
    {
        const string sql = @"
            UPDATE Responses SET
                RespValue = @RespValue,
                Label = @Label,
                IndexType = @IndexType,
                ModifiedDate = @ModifiedDate
            WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            response.Id,
            response.RespValue,
            Label = response.Label ?? string.Empty,
            IndexType = response.IndexType.ToString(),
            ModifiedDate = DateTime.UtcNow
        });
        
        return response;
    }
}
