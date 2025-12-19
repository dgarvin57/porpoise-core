#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.DataAccess.Context;

namespace Porpoise.DataAccess.Repositories;

/// <summary>
/// Survey repository implementation using Dapper.
/// Pure SQL, full control, no surprises.
/// </summary>
public class SurveyRepository : Repository<Survey>, ISurveyRepository
{
    protected override string TableName => "Surveys";
    private readonly TenantContext _tenantContext;

    public SurveyRepository(DapperContext context, TenantContext tenantContext) : base(context) 
    {
        _tenantContext = tenantContext;
    }

    public override async Task<IEnumerable<Survey>> GetAllAsync()
    {
        const string sql = @"
            SELECT * FROM Surveys 
            WHERE TenantId = @TenantId AND (IsDeleted = 0 OR IsDeleted IS NULL)";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Survey>(sql, new { TenantId = _tenantContext.TenantId });
    }

    public override async Task<Survey?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT * FROM Surveys 
            WHERE Id = @Id 
            AND TenantId = @TenantId";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Survey>(sql, 
            new { Id = id, TenantId = _tenantContext.TenantId });
    }

    public async Task<Survey?> GetByNameAsync(string surveyName)
    {
        const string sql = @"
            SELECT * FROM Surveys 
            WHERE SurveyName = @SurveyName 
            AND TenantId = @TenantId";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Survey>(sql, 
            new { SurveyName = surveyName, TenantId = _tenantContext.TenantId });
    }

    public async Task<IEnumerable<Survey>> GetByStatusAsync(SurveyStatus status)
    {
        const string sql = @"
            SELECT * FROM Surveys 
            WHERE Status = @Status 
            AND TenantId = @TenantId
            AND (IsDeleted = 0 OR IsDeleted IS NULL)";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Survey>(sql, 
            new { Status = (int)status, TenantId = _tenantContext.TenantId });
    }

    public async Task<IEnumerable<Survey>> SearchByNameAsync(string searchTerm)
    {
        const string sql = @"
            SELECT * FROM Surveys 
            WHERE SurveyName LIKE @SearchTerm 
            AND TenantId = @TenantId";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Survey>(sql, 
            new { SearchTerm = $"%{searchTerm}%", TenantId = _tenantContext.TenantId });
    }

    public async Task<IEnumerable<Survey>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT * FROM Surveys 
            WHERE CreatedDate >= @StartDate 
            AND CreatedDate <= @EndDate 
            AND TenantId = @TenantId";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Survey>(sql, 
            new { StartDate = startDate, EndDate = endDate, TenantId = _tenantContext.TenantId });
    }

    public async Task<int> GetQuestionCountAsync(Guid surveyId)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Questions 
            WHERE SurveyId = @SurveyId";
        
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { SurveyId = surveyId });
    }

    public async Task<int> GetResponseCountAsync(Guid surveyId)
    {
        // Get the SurveyData and count rows in DataList (minus header row)
        const string sql = @"
            SELECT DataList 
            FROM SurveyData 
            WHERE SurveyId = @SurveyId";
        
        using var connection = _context.CreateConnection();
        var dataListJson = await connection.ExecuteScalarAsync<string>(sql, new { SurveyId = surveyId });
        
        if (string.IsNullOrEmpty(dataListJson))
            return 0;
        
        // Deserialize and count rows (subtract 1 for header row)
        var dataList = System.Text.Json.JsonSerializer.Deserialize<List<List<string>>>(dataListJson);
        return dataList != null && dataList.Count > 1 ? dataList.Count - 1 : 0;
    }

    public async Task<bool> SurveyNameExistsAsync(string surveyName, Guid? excludeSurveyId = null)
    {
        var sql = excludeSurveyId.HasValue
            ? "SELECT COUNT(1) FROM Surveys WHERE SurveyName = @SurveyName AND Id != @ExcludeSurveyId"
            : "SELECT COUNT(1) FROM Surveys WHERE SurveyName = @SurveyName";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { SurveyName = surveyName, ExcludeSurveyId = excludeSurveyId });
        return count > 0;
    }

    public override async Task<Survey> AddAsync(Survey survey)
    {
        survey.TenantId = _tenantContext.TenantId;
        
        const string sql = @"
            INSERT INTO Surveys (
                Id, ProjectId, TenantId, SurveyName, Status,
                SurveyFileName, DataFileName, ErrorsExist, SurveyNotes,
                IsDeleted, CreatedDate, ModifiedDate
            ) VALUES (
                @Id, @ProjectId, @TenantId, @SurveyName, @Status,
                @SurveyFileName, @DataFileName, @ErrorsExist, @SurveyNotes,
                @IsDeleted, @CreatedDate, @ModifiedDate
            )";
        
        survey.Id = Guid.NewGuid();
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            survey.Id,
            ProjectId = survey.ProjectId?.ToString(),
            survey.TenantId,
            survey.SurveyName,
            Status = (int)survey.Status,
            survey.SurveyFileName,
            survey.DataFileName,
            survey.ErrorsExist,
            survey.SurveyNotes,
            survey.IsDeleted,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        });
        
        return survey;
    }

    public override async Task<Survey> UpdateAsync(Survey survey)
    {
        const string sql = @"
            UPDATE Surveys SET
                ProjectId = @ProjectId,
                SurveyName = @SurveyName,
                Status = @Status,
                SurveyFileName = @SurveyFileName,
                DataFileName = @DataFileName,
                ErrorsExist = @ErrorsExist,
                SurveyNotes = @SurveyNotes,
                LastAccessedDate = @LastAccessedDate,
                ModifiedDate = @ModifiedDate
            WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            survey.Id,
            ProjectId = survey.ProjectId?.ToString(),
            survey.SurveyName,
            Status = (int)survey.Status,
            survey.SurveyFileName,
            survey.DataFileName,
            survey.ErrorsExist,
            survey.SurveyNotes,
            survey.LastAccessedDate,
            ModifiedDate = DateTime.UtcNow
        });
        
        return survey;
    }

    /// <summary>
    /// Soft delete a survey
    /// </summary>
    public async Task<bool> SoftDeleteSurveyAsync(Guid surveyId)
    {
        const string sql = @"
            UPDATE Surveys 
            SET IsDeleted = 1, DeletedDate = @DeletedDate 
            WHERE Id = @SurveyId AND TenantId = @TenantId";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            SurveyId = surveyId.ToString(),
            DeletedDate = DateTime.UtcNow,
            TenantId = _tenantContext.TenantId
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Restore a soft-deleted survey
    /// </summary>
    public async Task<bool> RestoreSurveyAsync(Guid surveyId)
    {
        const string sql = @"
            UPDATE Surveys 
            SET IsDeleted = 0, DeletedDate = NULL 
            WHERE Id = @SurveyId AND TenantId = @TenantId";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            SurveyId = surveyId.ToString(),
            TenantId = _tenantContext.TenantId
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Permanently delete a survey and all related data (CASCADE DELETE handles child records)
    /// </summary>
    public async Task<bool> PermanentlyDeleteSurveyAsync(Guid surveyId)
    {
        const string sql = "DELETE FROM Surveys WHERE Id = @SurveyId AND TenantId = @TenantId";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            SurveyId = surveyId.ToString(),
            TenantId = _tenantContext.TenantId
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Get all deleted surveys (trash)
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetDeletedSurveysAsync()
    {
        const string sql = @"
            SELECT 
                s.Id,
                s.SurveyName,
                s.DeletedDate,
                s.ProjectId,
                p.ProjectName
            FROM Surveys s
            LEFT JOIN Projects p ON s.ProjectId = p.Id
            WHERE s.TenantId = @TenantId AND s.IsDeleted = 1
            ORDER BY s.DeletedDate DESC";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync(sql, new { TenantId = _tenantContext.TenantId });
    }

    /// <summary>
    /// Get recently accessed surveys with metadata
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetRecentlyAccessedAsync(int limit = 4)
    {
        const string sql = @"
            SELECT 
                CAST(s.Id AS CHAR) as id,
                s.SurveyName as name,
                s.Status as status,
                s.LastAccessedDate as lastAccessedDate,
                CAST(s.ProjectId AS CHAR) as projectId,
                p.ProjectName as projectName,
                p.ClientName as clientName,
                COUNT(DISTINCT q.Id) as questionCount,
                GREATEST(COALESCE(MAX(JSON_LENGTH(sd.DataList)), 0) - 1, 0) as caseCount
            FROM Surveys s
            LEFT JOIN Projects p ON s.ProjectId = p.Id
            LEFT JOIN Questions q ON s.Id = q.SurveyId
            LEFT JOIN SurveyData sd ON s.Id = sd.SurveyId
            WHERE s.TenantId = @TenantId 
            AND (s.IsDeleted = 0 OR s.IsDeleted IS NULL)
            AND s.LastAccessedDate IS NOT NULL
            GROUP BY s.Id, s.SurveyName, s.Status, s.LastAccessedDate, s.ProjectId, p.ProjectName, p.ClientName
            ORDER BY s.LastAccessedDate DESC
            LIMIT @Limit";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync(sql, new 
        { 
            TenantId = _tenantContext.TenantId,
            Limit = limit
        });
    }
}
