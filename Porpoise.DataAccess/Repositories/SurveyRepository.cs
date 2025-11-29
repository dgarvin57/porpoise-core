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
            AND TenantId = @TenantId";
        
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
                Id, ProjectId, TenantId, SurveyName, Status, LockStatus, UnlockKeyName, UnlockKeyType,
                SaveAlteredString, SurveyFileName, DataFileName, OrigDataFilePath,
                SurveyPath, SurveyFolder, FullProjectFolder, ErrorsExist, SurveyNotes,
                CreatedDate, ModifiedDate
            ) VALUES (
                @Id, @ProjectId, @TenantId, @SurveyName, @Status, @LockStatus, @UnlockKeyName, @UnlockKeyType,
                @SaveAlteredString, @SurveyFileName, @DataFileName, @OrigDataFilePath,
                @SurveyPath, @SurveyFolder, @FullProjectFolder, @ErrorsExist, @SurveyNotes,
                @CreatedDate, @ModifiedDate
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
            LockStatus = (int)survey.LockStatus,
            survey.UnlockKeyName,
            UnlockKeyType = (int)survey.UnlockKeyType,
            survey.SaveAlteredString,
            survey.SurveyFileName,
            survey.DataFileName,
            survey.OrigDataFilePath,
            survey.SurveyPath,
            survey.SurveyFolder,
            survey.FullProjectFolder,
            survey.ErrorsExist,
            survey.SurveyNotes,
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
                LockStatus = @LockStatus,
                UnlockKeyName = @UnlockKeyName,
                UnlockKeyType = @UnlockKeyType,
                SaveAlteredString = @SaveAlteredString,
                SurveyFileName = @SurveyFileName,
                DataFileName = @DataFileName,
                OrigDataFilePath = @OrigDataFilePath,
                SurveyPath = @SurveyPath,
                SurveyFolder = @SurveyFolder,
                FullProjectFolder = @FullProjectFolder,
                ErrorsExist = @ErrorsExist,
                SurveyNotes = @SurveyNotes,
                ModifiedDate = @ModifiedDate
            WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            survey.Id,
            ProjectId = survey.ProjectId?.ToString(),
            survey.SurveyName,
            Status = (int)survey.Status,
            LockStatus = (int)survey.LockStatus,
            survey.UnlockKeyName,
            UnlockKeyType = (int)survey.UnlockKeyType,
            survey.SaveAlteredString,
            survey.SurveyFileName,
            survey.DataFileName,
            survey.OrigDataFilePath,
            survey.SurveyPath,
            survey.SurveyFolder,
            survey.FullProjectFolder,
            survey.ErrorsExist,
            survey.SurveyNotes,
            ModifiedDate = DateTime.UtcNow
        });
        
        return survey;
    }
}
