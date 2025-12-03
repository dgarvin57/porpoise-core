using Dapper;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.DataAccess.Context;

namespace Porpoise.DataAccess.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    protected override string TableName => "Projects";
    private readonly TenantContext _tenantContext;

    public ProjectRepository(DapperContext context, TenantContext tenantContext) : base(context)
    {
        _tenantContext = tenantContext;
    }

    public override async Task<Project?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT * FROM Projects 
            WHERE Id = @Id AND TenantId = @TenantId";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Project>(sql, 
            new { Id = id.ToString(), TenantId = _tenantContext.TenantId });
    }

    public override async Task<IEnumerable<Project>> GetAllAsync()
    {
        const string sql = @"
            SELECT * FROM Projects 
            WHERE TenantId = @TenantId AND (IsDeleted = 0 OR IsDeleted IS NULL)
            ORDER BY ProjectName";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Project>(sql, new { TenantId = _tenantContext.TenantId });
    }

    public async Task<Project?> GetByNameAsync(string projectName)
    {
        const string sql = @"
            SELECT * FROM Projects 
            WHERE ProjectName = @ProjectName 
            AND TenantId = @TenantId";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Project>(sql, 
            new { ProjectName = projectName, TenantId = _tenantContext.TenantId });
    }

    public async Task<IEnumerable<Project>> GetByClientAsync(string clientName)
    {
        const string sql = @"
            SELECT * FROM Projects 
            WHERE ClientName = @ClientName 
            AND TenantId = @TenantId
            ORDER BY ProjectName";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Project>(sql, 
            new { ClientName = clientName, TenantId = _tenantContext.TenantId });
    }

    public override async Task<Project> AddAsync(Project project)
    {
        project.TenantId = _tenantContext.TenantId;
        
        const string sql = @"
            INSERT INTO Projects (
                Id, TenantId, ProjectName, ClientName, Description, StartDate, EndDate,
                DefaultWeightingScheme, BrandingSettings, ResearcherLabel, ResearcherSubLabel,
                ResearcherLogo, ResearcherLogoFilename, ResearcherLogoPath, ProjectFile,
                IsExported, IsDeleted, CreatedBy
            ) VALUES (
                @Id, @TenantId, @ProjectName, @ClientName, @Description, @StartDate, @EndDate,
                @DefaultWeightingScheme, @BrandingSettings, @ResearcherLabel, @ResearcherSubLabel,
                @ResearcherLogo, @ResearcherLogoFilename, @ResearcherLogoPath, @ProjectFile,
                @IsExported, @IsDeleted, @CreatedBy
            )";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = project.Id.ToString(),
            project.TenantId,
            project.ProjectName,
            project.ClientName,
            project.Description,
            project.StartDate,
            project.EndDate,
            project.DefaultWeightingScheme,
            project.BrandingSettings,
            project.ResearcherLabel,
            project.ResearcherSubLabel,
            project.ResearcherLogo,
            project.ResearcherLogoFilename,
            project.ResearcherLogoPath,
            project.ProjectFile,
            project.IsExported,
            project.IsDeleted,
            project.CreatedBy
        });

        return project;
    }

    public override async Task<Project> UpdateAsync(Project project)
    {
        const string sql = @"
            UPDATE Projects SET
                ProjectName = @ProjectName,
                ClientName = @ClientName,
                Description = @Description,
                StartDate = @StartDate,
                EndDate = @EndDate,
                DefaultWeightingScheme = @DefaultWeightingScheme,
                BrandingSettings = @BrandingSettings,
                ResearcherLabel = @ResearcherLabel,
                ResearcherSubLabel = @ResearcherSubLabel,
                ResearcherLogo = @ResearcherLogo,
                ResearcherLogoFilename = @ResearcherLogoFilename,
                ResearcherLogoPath = @ResearcherLogoPath,
                ProjectFile = @ProjectFile,
                IsExported = @IsExported,
                ModifiedBy = @ModifiedBy
            WHERE Id = @Id AND TenantId = @TenantId";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = project.Id.ToString(),
            project.ProjectName,
            project.ClientName,
            project.Description,
            project.StartDate,
            project.EndDate,
            project.DefaultWeightingScheme,
            project.BrandingSettings,
            project.ResearcherLabel,
            project.ResearcherSubLabel,
            project.ResearcherLogo,
            project.ResearcherLogoFilename,
            project.ResearcherLogoPath,
            project.ProjectFile,
            project.IsExported,
            project.ModifiedBy,
            TenantId = _tenantContext.TenantId
        });

        return project;
    }

    public override async Task<bool> DeleteAsync(Guid id)
    {
        const string sql = @"
            DELETE FROM Projects 
            WHERE Id = @Id AND TenantId = @TenantId";

        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(sql, 
            new { Id = id.ToString(), TenantId = _tenantContext.TenantId });
        return affected > 0;
    }

    /// <summary>
    /// Get project with all its surveys (for pooling/trending)
    /// </summary>
    public async Task<Project?> GetProjectWithSurveysAsync(Guid projectId)
    {
        const string sql = @"
            SELECT * FROM Projects WHERE Id = @ProjectId AND TenantId = @TenantId;
            SELECT * FROM Surveys WHERE ProjectId = @ProjectId AND TenantId = @TenantId ORDER BY SurveyName;";

        using var connection = _context.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(sql, 
            new { ProjectId = projectId.ToString(), TenantId = _tenantContext.TenantId });

        var project = await multi.ReadSingleOrDefaultAsync<Project>();
        if (project != null)
        {
            var surveys = await multi.ReadAsync<Survey>();
            project.SurveyList = new ObjectListBase<Survey>(surveys.ToList());
        }

        return project;
    }

    /// <summary>
    /// Get all surveys for a specific project
    /// </summary>
    public async Task<IEnumerable<Survey>> GetSurveysByProjectIdAsync(Guid projectId)
    {
        const string sql = @"
            SELECT * FROM Surveys 
            WHERE ProjectId = @ProjectId 
            AND TenantId = @TenantId
            ORDER BY SurveyName";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Survey>(sql, 
            new { ProjectId = projectId.ToString(), TenantId = _tenantContext.TenantId });
    }

    /// <summary>
    /// Get surveys for a project with question and data counts
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetSurveysWithCountsAsync(Guid projectId)
    {
        const string sql = @"
            SELECT 
                CAST(s.Id AS CHAR) as id,
                s.SurveyName as name,
                s.Status as status,
                s.CreatedDate as createdDate,
                s.ModifiedDate as modifiedDate,
                COUNT(DISTINCT q.Id) as questionCount,
                GREATEST(COALESCE(MAX(JSON_LENGTH(sd.DataList)), 0) - 1, 0) as caseCount
            FROM Surveys s
            LEFT JOIN Questions q ON s.Id = q.SurveyId
            LEFT JOIN SurveyData sd ON s.Id = sd.SurveyId
            WHERE s.ProjectId = @ProjectId 
            AND s.TenantId = @TenantId
            GROUP BY s.Id, s.SurveyName, s.Status, s.CreatedDate, s.ModifiedDate
            ORDER BY s.SurveyName";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync(sql, 
            new { ProjectId = projectId.ToString(), TenantId = _tenantContext.TenantId });
    }

    /// <summary>
    /// Get project with survey count and metadata (for UI display)
    /// For standalone surveys (1 survey), includes question and case counts
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetProjectsWithSurveyCountAsync()
    {
        const string sql = @"
            SELECT 
                CAST(p.Id AS CHAR) as id,
                p.ProjectName as name,
                p.ClientName as clientName,
                p.Description as description,
                p.StartDate as startDate,
                p.EndDate as endDate,
                p.CreatedDate as createdAt,
                MAX(s.ModifiedDate) as lastModified,
                COUNT(DISTINCT s.Id) as surveyCount,
                CAST(MAX(s.Id) AS CHAR) as firstSurveyId,
                MAX(s.SurveyName) as firstSurveyName,
                MAX(s.Status) as status,
                MAX(s.CreatedDate) as firstSurveyCreatedDate,
                COUNT(DISTINCT q.Id) as questionCount,
                GREATEST(COALESCE(MAX(JSON_LENGTH(sd.DataList)), 0) - 1, 0) as caseCount
            FROM Projects p
            LEFT JOIN Surveys s ON p.Id = s.ProjectId AND s.TenantId = @TenantId AND (s.IsDeleted = 0 OR s.IsDeleted IS NULL)
            LEFT JOIN Questions q ON s.Id = q.SurveyId
            LEFT JOIN SurveyData sd ON s.Id = sd.SurveyId
            WHERE p.TenantId = @TenantId AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
            GROUP BY p.Id, p.ProjectName, p.ClientName, p.Description, p.StartDate, p.EndDate, p.CreatedDate
            ORDER BY p.ProjectName";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync(sql, new { TenantId = _tenantContext.TenantId });
    }

    /// <summary>
    /// Get only projects that have multiple surveys (for pooling/trending)
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetMultiSurveyProjectsAsync()
    {
        const string sql = @"
            SELECT 
                p.Id,
                p.ProjectName,
                p.ClientName,
                p.Description,
                p.StartDate,
                p.EndDate,
                COUNT(s.Id) as SurveyCount
            FROM Projects p
            INNER JOIN Surveys s ON p.Id = s.ProjectId AND s.TenantId = @TenantId AND (s.IsDeleted = 0 OR s.IsDeleted IS NULL)
            WHERE p.TenantId = @TenantId AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
            GROUP BY p.Id, p.ProjectName, p.ClientName, p.Description, p.StartDate, p.EndDate
            HAVING COUNT(s.Id) > 1
            ORDER BY p.ProjectName";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync(sql, new { TenantId = _tenantContext.TenantId });
    }

    /// <summary>
    /// Move a survey to a different project
    /// </summary>
    public async Task<bool> MoveSurveyToProjectAsync(Guid surveyId, Guid newProjectId)
    {
        const string sql = @"
            UPDATE Surveys 
            SET ProjectId = @NewProjectId 
            WHERE Id = @SurveyId 
            AND TenantId = @TenantId";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            SurveyId = surveyId.ToString(),
            NewProjectId = newProjectId.ToString(),
            TenantId = _tenantContext.TenantId
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Soft delete a project and all its surveys
    /// </summary>
    public async Task<bool> SoftDeleteProjectAsync(Guid projectId)
    {
        const string sql = @"
            UPDATE Projects 
            SET IsDeleted = 1, DeletedDate = @DeletedDate 
            WHERE Id = @ProjectId AND TenantId = @TenantId;
            
            UPDATE Surveys 
            SET IsDeleted = 1, DeletedDate = @DeletedDate 
            WHERE ProjectId = @ProjectId AND TenantId = @TenantId;";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ProjectId = projectId.ToString(),
            DeletedDate = DateTime.UtcNow,
            TenantId = _tenantContext.TenantId
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Restore a soft-deleted project and all its surveys
    /// </summary>
    public async Task<bool> RestoreProjectAsync(Guid projectId)
    {
        const string sql = @"
            UPDATE Projects 
            SET IsDeleted = 0, DeletedDate = NULL 
            WHERE Id = @ProjectId AND TenantId = @TenantId;
            
            UPDATE Surveys 
            SET IsDeleted = 0, DeletedDate = NULL 
            WHERE ProjectId = @ProjectId AND TenantId = @TenantId;";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ProjectId = projectId.ToString(),
            TenantId = _tenantContext.TenantId
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Permanently delete a project and all related data
    /// </summary>
    public async Task<bool> PermanentlyDeleteProjectAsync(Guid projectId)
    {
        const string sql = @"
            DELETE FROM Responses WHERE SurveyId IN (
                SELECT Id FROM Surveys WHERE ProjectId = @ProjectId AND TenantId = @TenantId
            );
            DELETE FROM QuestionBlocks WHERE SurveyId IN (
                SELECT Id FROM Surveys WHERE ProjectId = @ProjectId AND TenantId = @TenantId
            );
            DELETE FROM Questions WHERE SurveyId IN (
                SELECT Id FROM Surveys WHERE ProjectId = @ProjectId AND TenantId = @TenantId
            );
            DELETE FROM SurveyData WHERE SurveyId IN (
                SELECT Id FROM Surveys WHERE ProjectId = @ProjectId AND TenantId = @TenantId
            );
            DELETE FROM Surveys WHERE ProjectId = @ProjectId AND TenantId = @TenantId;
            DELETE FROM Projects WHERE Id = @ProjectId AND TenantId = @TenantId;";

        using var connection = _context.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            ProjectId = projectId.ToString(),
            TenantId = _tenantContext.TenantId
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Get all deleted projects (trash)
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetDeletedProjectsAsync()
    {
        const string sql = @"
            SELECT 
                p.Id,
                p.ProjectName,
                p.ClientName,
                p.Description,
                p.DeletedDate,
                COUNT(DISTINCT s.Id) as SurveyCount
            FROM Projects p
            LEFT JOIN Surveys s ON p.Id = s.ProjectId AND s.TenantId = @TenantId
            WHERE p.TenantId = @TenantId AND p.IsDeleted = 1
            GROUP BY p.Id, p.ProjectName, p.ClientName, p.Description, p.DeletedDate
            ORDER BY p.DeletedDate DESC";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync(sql, new { TenantId = _tenantContext.TenantId });
    }
}

