#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.DataAccess.Context;

namespace Porpoise.DataAccess.Repositories;

/// <summary>
/// Repository for SurveyData - stores raw respondent data.
/// DataList is serialized as JSON for storage.
/// </summary>
public class SurveyDataRepository : Repository<SurveyData>, ISurveyDataRepository
{
    protected override string TableName => "SurveyData";

    public SurveyDataRepository(DapperContext context) : base(context) { }

    public async Task<SurveyData?> GetBySurveyIdAsync(Guid surveyId)
    {
        const string sql = @"
            SELECT * FROM SurveyData 
            WHERE SurveyId = @SurveyId";
        
        using var connection = _context.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { SurveyId = surveyId });
        
        if (row == null) return null;

        var surveyData = new SurveyData
        {
            DataFilePath = row.DataFilePath ?? string.Empty
        };

        // Deserialize DataList from JSON
        if (!string.IsNullOrEmpty((string)row.DataList))
        {
            surveyData.DataList = JsonSerializer.Deserialize<List<List<string>>>((string)row.DataList) ?? [];
        }

        // Deserialize MissingResponseValues from JSON
        if (!string.IsNullOrEmpty((string)row.MissingResponseValues))
        {
            surveyData.MissingResponseValues = JsonSerializer.Deserialize<List<int>>((string)row.MissingResponseValues) ?? [];
        }

        return surveyData;
    }

    public async Task DeleteBySurveyIdAsync(Guid surveyId)
    {
        const string sql = "DELETE FROM SurveyData WHERE SurveyId = @SurveyId";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { SurveyId = surveyId });
    }

    public async Task<SurveyData> AddAsync(SurveyData surveyData, Guid surveyId)
    {
        const string sql = @"
            INSERT INTO SurveyData (
                Id, SurveyId, DataFilePath, DataList, MissingResponseValues, 
                CreatedDate, ModifiedDate
            ) VALUES (
                @Id, @SurveyId, @DataFilePath, @DataList, @MissingResponseValues, 
                @CreatedDate, @ModifiedDate
            )";
        
        var id = Guid.NewGuid();
        
        // Serialize DataList to JSON
        var dataListJson = surveyData.DataList != null && surveyData.DataList.Count > 0
            ? JsonSerializer.Serialize(surveyData.DataList)
            : null;

        // Serialize MissingResponseValues to JSON
        var missingValuesJson = surveyData.MissingResponseValues != null && surveyData.MissingResponseValues.Count > 0
            ? JsonSerializer.Serialize(surveyData.MissingResponseValues)
            : null;

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = id,
            SurveyId = surveyId,
            DataFilePath = surveyData.DataFilePath ?? string.Empty,
            DataList = dataListJson,
            MissingResponseValues = missingValuesJson,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        });
        
        return surveyData;
    }

    public override async Task<SurveyData> AddAsync(SurveyData surveyData)
    {
        // Fallback for base interface
        return await AddAsync(surveyData, Guid.Empty);
    }

    public override async Task<SurveyData> UpdateAsync(SurveyData surveyData)
    {
        // SurveyData doesn't have an Id property, so we can't update by Id
        // This would need to update by SurveyId instead
        throw new NotImplementedException("Use GetBySurveyIdAsync to retrieve, then re-add with new data");
    }
}
