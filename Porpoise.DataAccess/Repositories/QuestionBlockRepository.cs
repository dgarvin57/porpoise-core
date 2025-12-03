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
/// Repository for managing question blocks (normalized block information).
/// </summary>
public class QuestionBlockRepository : IQuestionBlockRepository
{
    private readonly DapperContext _context;

    public QuestionBlockRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<QuestionBlock?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT Id, SurveyId, Label, Stem, DisplayOrder, CreatedAt, UpdatedAt
            FROM QuestionBlocks
            WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<QuestionBlock>(sql, new { Id = id });
    }

    public async Task<IEnumerable<QuestionBlock>> GetBySurveyIdAsync(Guid surveyId)
    {
        const string sql = @"
            SELECT Id, SurveyId, Label, Stem, DisplayOrder, CreatedAt, UpdatedAt
            FROM QuestionBlocks
            WHERE SurveyId = @SurveyId
            ORDER BY DisplayOrder";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<QuestionBlock>(sql, new { SurveyId = surveyId });
    }

    public async Task<QuestionBlock?> GetByLabelAsync(Guid surveyId, string label)
    {
        const string sql = @"
            SELECT Id, SurveyId, Label, Stem, DisplayOrder, CreatedAt, UpdatedAt
            FROM QuestionBlocks
            WHERE SurveyId = @SurveyId AND Label = @Label";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<QuestionBlock>(sql, 
            new { SurveyId = surveyId, Label = label });
    }

    public async Task<QuestionBlock> AddAsync(QuestionBlock block)
    {
        const string sql = @"
            INSERT INTO QuestionBlocks (Id, SurveyId, Label, Stem, DisplayOrder, CreatedAt, UpdatedAt)
            VALUES (@Id, @SurveyId, @Label, @Stem, @DisplayOrder, @CreatedAt, @UpdatedAt)";

        block.Id = Guid.NewGuid();
        block.CreatedAt = DateTime.UtcNow;
        block.UpdatedAt = DateTime.UtcNow;

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, block);
        return block;
    }

    public async Task<QuestionBlock> UpdateAsync(QuestionBlock block)
    {
        const string sql = @"
            UPDATE QuestionBlocks
            SET Label = @Label,
                Stem = @Stem,
                DisplayOrder = @DisplayOrder,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        block.UpdatedAt = DateTime.UtcNow;

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, block);
        return block;
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM QuestionBlocks WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task DeleteBySurveyIdAsync(Guid surveyId)
    {
        const string sql = "DELETE FROM QuestionBlocks WHERE SurveyId = @SurveyId";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { SurveyId = surveyId });
    }
}
