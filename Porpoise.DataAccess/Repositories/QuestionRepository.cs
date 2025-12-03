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
                q.Id, q.SurveyId, q.QstNumber, q.QstLabel, q.QstStem,
                q.DataFileColumn as DataFileCol, q.ColumnFilled,
                q.VariableType, q.DataType,
                q.MissValue1, q.MissValue2, q.MissValue3,
                q.MissingLow, q.MissingHigh,
                q.BlkQstStatus, q.BlockId,
                qb.Label as BlkLabel, qb.Stem as BlkStem,
                q.IsPreferenceBlock, q.IsPreferenceBlockType, q.NumberOfPreferenceItems, q.PreserveDifferentResponsesInPreferenceBlock,
                q.QuestionNotes,
                q.UseAlternatePosNegLabels, q.AlternatePosLabel, q.AlternateNegLabel,
                q.CreatedDate, q.ModifiedDate
            FROM Questions q
            LEFT JOIN QuestionBlocks qb ON q.BlockId = qb.Id
            WHERE q.SurveyId = @SurveyId 
            ORDER BY q.QstNumber";
        
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
                Id, SurveyId, QstNumber, QstLabel, QstStem, DataFileColumn, ColumnFilled,
                VariableType, DataType,
                MissValue1, MissValue2, MissValue3, MissingLow, MissingHigh,
                BlkQstStatus, BlockId,
                IsPreferenceBlock, IsPreferenceBlockType, NumberOfPreferenceItems, PreserveDifferentResponsesInPreferenceBlock,
                QuestionNotes,
                UseAlternatePosNegLabels, AlternatePosLabel, AlternateNegLabel,
                CreatedDate, ModifiedDate
            ) VALUES (
                @Id, @SurveyId, @QstNumber, @QstLabel, @QstStem, @DataFileColumn, @ColumnFilled,
                @VariableType, @DataType,
                @MissValue1, @MissValue2, @MissValue3, @MissingLow, @MissingHigh,
                @BlkQstStatus, @BlockId,
                @IsPreferenceBlock, @IsPreferenceBlockType, @NumberOfPreferenceItems, @PreserveDifferentResponsesInPreferenceBlock,
                @QuestionNotes,
                @UseAlternatePosNegLabels, @AlternatePosLabel, @AlternateNegLabel,
                @CreatedDate, @ModifiedDate
            )";
        
        question.Id = Guid.NewGuid();
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            question.Id,
            SurveyId = surveyId,
            QstNumber = question.QstNumber ?? string.Empty,
            QstLabel = question.QstLabel ?? string.Empty,
            QstStem = question.QstStem ?? string.Empty,
            DataFileColumn = question.DataFileCol,
            ColumnFilled = question.ColumnFilled,
            VariableType = (int)question.VariableType,
            DataType = (int?)question.DataType,
            MissValue1 = question.MissValue1 ?? string.Empty,
            MissValue2 = question.MissValue2 ?? string.Empty,
            MissValue3 = question.MissValue3 ?? string.Empty,
            MissingLow = 0.0,
            MissingHigh = 0.0,
            BlkQstStatus = (int?)question.BlkQstStatus,
            BlockId = question.BlockId,
            IsPreferenceBlock = question.IsPreferenceBlock,
            IsPreferenceBlockType = question.IsPreferenceBlockType,
            NumberOfPreferenceItems = question.NumberOfPreferenceItems,
            PreserveDifferentResponsesInPreferenceBlock = question.PreserveDifferentResponsesInPreferenceBlock,
            QuestionNotes = question.QuestionNotes ?? string.Empty,
            UseAlternatePosNegLabels = question.UseAlternatePosNegLabels,
            AlternatePosLabel = question.AlternatePosLabel ?? string.Empty,
            AlternateNegLabel = question.AlternateNegLabel ?? string.Empty,
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
                QstStem = @QstStem,
                DataFileColumn = @DataFileColumn,
                ColumnFilled = @ColumnFilled,
                VariableType = @VariableType,
                DataType = @DataType,
                MissValue1 = @MissValue1,
                MissValue2 = @MissValue2,
                MissValue3 = @MissValue3,
                MissingLow = @MissingLow,
                MissingHigh = @MissingHigh,
                BlkQstStatus = @BlkQstStatus,
                BlkLabel = @BlkLabel,
                BlkStem = @BlkStem,
                IsPreferenceBlock = @IsPreferenceBlock,
                IsPreferenceBlockType = @IsPreferenceBlockType,
                NumberOfPreferenceItems = @NumberOfPreferenceItems,
                PreserveDifferentResponsesInPreferenceBlock = @PreserveDifferentResponsesInPreferenceBlock,
                QuestionNotes = @QuestionNotes,
                UseAlternatePosNegLabels = @UseAlternatePosNegLabels,
                AlternatePosLabel = @AlternatePosLabel,
                AlternateNegLabel = @AlternateNegLabel,
                ModifiedDate = @ModifiedDate
            WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            question.Id,
            QstNumber = question.QstNumber ?? string.Empty,
            QstLabel = question.QstLabel ?? string.Empty,
            QstStem = question.QstStem ?? string.Empty,
            DataFileColumn = question.DataFileCol,
            ColumnFilled = question.ColumnFilled,
            VariableType = (int)question.VariableType,
            DataType = (int?)question.DataType,
            MissValue1 = question.MissValue1 ?? string.Empty,
            MissValue2 = question.MissValue2 ?? string.Empty,
            MissValue3 = question.MissValue3 ?? string.Empty,
            MissingLow = 0.0,
            MissingHigh = 0.0,
            BlkQstStatus = (int?)question.BlkQstStatus,
            BlkLabel = question.BlkLabel ?? string.Empty,
            BlkStem = question.BlkStem ?? string.Empty,
            IsPreferenceBlock = question.IsPreferenceBlock,
            IsPreferenceBlockType = question.IsPreferenceBlockType,
            NumberOfPreferenceItems = question.NumberOfPreferenceItems,
            PreserveDifferentResponsesInPreferenceBlock = question.PreserveDifferentResponsesInPreferenceBlock,
            QuestionNotes = question.QuestionNotes ?? string.Empty,
            UseAlternatePosNegLabels = question.UseAlternatePosNegLabels,
            AlternatePosLabel = question.AlternatePosLabel ?? string.Empty,
            AlternateNegLabel = question.AlternateNegLabel ?? string.Empty,
            ModifiedDate = DateTime.UtcNow
        });
        
        return question;
    }
}
