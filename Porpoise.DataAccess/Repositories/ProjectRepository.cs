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
            WHERE TenantId = @TenantId 
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
                Id, TenantId, ProjectName, ClientName, ResearcherLabel, ResearcherSubLabel,
                ResearcherLogo, ResearcherLogoFilename, ResearcherLogoPath,
                BaseProjectFolder, ProjectFolder, FullFolder, FullPath, FileName,
                IsExported, CreatedBy, CreatedOn
            ) VALUES (
                @Id, @TenantId, @ProjectName, @ClientName, @ResearcherLabel, @ResearcherSubLabel,
                @ResearcherLogo, @ResearcherLogoFilename, @ResearcherLogoPath,
                @BaseProjectFolder, @ProjectFolder, @FullFolder, @FullPath, @FileName,
                @IsExported, @CreatedBy, @CreatedOn
            )";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = project.Id.ToString(),
            project.TenantId,
            project.ProjectName,
            project.ClientName,
            project.ResearcherLabel,
            project.ResearcherSubLabel,
            project.ResearcherLogo,
            project.ResearcherLogoFilename,
            project.ResearcherLogoPath,
            project.BaseProjectFolder,
            project.ProjectFolder,
            project.FullFolder,
            project.FullPath,
            project.FileName,
            project.IsExported,
            project.CreatedBy,
            project.CreatedOn
        });

        return project;
    }

    public override async Task<Project> UpdateAsync(Project project)
    {
        const string sql = @"
            UPDATE Projects SET
                ProjectName = @ProjectName,
                ClientName = @ClientName,
                ResearcherLabel = @ResearcherLabel,
                ResearcherSubLabel = @ResearcherSubLabel,
                ResearcherLogo = @ResearcherLogo,
                ResearcherLogoFilename = @ResearcherLogoFilename,
                ResearcherLogoPath = @ResearcherLogoPath,
                BaseProjectFolder = @BaseProjectFolder,
                ProjectFolder = @ProjectFolder,
                FullFolder = @FullFolder,
                FullPath = @FullPath,
                FileName = @FileName,
                IsExported = @IsExported,
                ModifiedBy = @ModifiedBy,
                ModifiedOn = @ModifiedOn
            WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            Id = project.Id.ToString(),
            project.ProjectName,
            project.ClientName,
            project.ResearcherLabel,
            project.ResearcherSubLabel,
            project.ResearcherLogo,
            project.ResearcherLogoFilename,
            project.ResearcherLogoPath,
            project.BaseProjectFolder,
            project.ProjectFolder,
            project.FullFolder,
            project.FullPath,
            project.FileName,
            project.IsExported,
            project.ModifiedBy,
            project.ModifiedOn
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
}
