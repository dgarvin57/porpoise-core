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
/// Question repository implementation using Dapper.
/// Manages survey questions with full CRUD support.
/// </summary>
public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    protected override string TableName => "Questions";

    public QuestionRepository(DapperContext context) : base(context) { }

    public async Task<IEnumerable<Question>> GetBySurveyIdAsync(Guid surveyId)
    {
        const string sql = @"
            SELECT 
                Id, SurveyId, QstNumber, QstLabel,
                DataFileColumn as DataFileCol,
                VariableType, MissingLow, MissingHigh,
                CreatedDate, ModifiedDate
            FROM Questions 
            WHERE SurveyId = @SurveyId 
            ORDER BY QstNumber";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Question>(sql, new { SurveyId = surveyId });
    }

    public async Task<Question?> GetByQuestionNumberAsync(Guid surveyId, string questionNumber)
    {
        const string sql = @"
            SELECT * FROM Questions 
            WHERE SurveyId = @SurveyId 
            AND QstNumber = @QuestionNumber";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Question>(sql, 
            new { SurveyId = surveyId, QuestionNumber = questionNumber });
    }

    public async Task<IEnumerable<Question>> GetByVariableTypeAsync(Guid surveyId, QuestionVariableType variableType)
    {
        const string sql = @"
            SELECT * FROM Questions 
            WHERE SurveyId = @SurveyId 
            AND VariableType = @VariableType 
            ORDER BY QstNumber";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Question>(sql, 
            new { SurveyId = surveyId, VariableType = (int)variableType });
    }

    public async Task<int> GetResponseCountAsync(Guid questionId)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Responses 
            WHERE QuestionId = @QuestionId";
        
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { QuestionId = questionId });
    }

    public async Task DeleteBySurveyIdAsync(Guid surveyId)
    {
        const string sql = "DELETE FROM Questions WHERE SurveyId = @SurveyId";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { SurveyId = surveyId });
    }

    public async Task<Question> AddAsync(Question question, Guid surveyId)
    {
        const string sql = @"
            INSERT INTO Questions (
                Id, SurveyId, QstNumber, QstLabel, DataFileColumn, 
                VariableType, MissingLow, MissingHigh, CreatedDate, ModifiedDate
            ) VALUES (
                @Id, @SurveyId, @QstNumber, @QstLabel, @DataFileColumn, 
                @VariableType, @MissingLow, @MissingHigh, @CreatedDate, @ModifiedDate
            )";
        
        question.Id = Guid.NewGuid();
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            question.Id,
            SurveyId = surveyId,
            QstNumber = question.QstNumber ?? string.Empty,
            QstLabel = question.QstLabel ?? string.Empty,
            DataFileColumn = question.DataFileCol,
            VariableType = (int)question.VariableType,
            MissingLow = 0.0,
            MissingHigh = 0.0,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        });
        
        return question;
    }

    public override async Task<Question> AddAsync(Question question)
    {
        // Fallback for base interface - requires SurveyId to be set externally
        return await AddAsync(question, Guid.Empty);
    }

    public override async Task<Question> UpdateAsync(Question question)
    {
        const string sql = @"
            UPDATE Questions SET
                QstNumber = @QstNumber,
                QstLabel = @QstLabel,
                DataFileColumn = @DataFileColumn,
                VariableType = @VariableType,
                MissingLow = @MissingLow,
                MissingHigh = @MissingHigh,
                ModifiedDate = @ModifiedDate
            WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            question.Id,
            QstNumber = question.QstNumber ?? string.Empty,
            QstLabel = question.QstLabel ?? string.Empty,
            DataFileColumn = question.DataFileCol,
            VariableType = (int)question.VariableType,
            MissingLow = 0.0,
            MissingHigh = 0.0,
            ModifiedDate = DateTime.UtcNow
        });
        
        return question;
    }
}
