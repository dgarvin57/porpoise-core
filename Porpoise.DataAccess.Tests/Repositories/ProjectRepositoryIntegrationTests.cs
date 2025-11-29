using FluentAssertions;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.DataAccess.Context;
using Porpoise.DataAccess.Repositories;

namespace Porpoise.DataAccess.Tests.Repositories;

[Collection("Database")]
public class ProjectRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DapperContext _context;
    private readonly ProjectRepository _repository;
    private readonly TenantContext _tenantContext;
    private readonly List<Guid> _testProjectIds = new();

    public ProjectRepositoryIntegrationTests()
    {
        _context = new DapperContext("Server=localhost;Port=3306;Database=porpoise_dev;User=root;Password=Dg5901%1;CharSet=utf8mb4;");
        _tenantContext = new TenantContext { TenantId = 1, TenantKey = "demo-tenant" };
        _repository = new ProjectRepository(_context, _tenantContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Clean up test data
        foreach (var id in _testProjectIds)
        {
            await _repository.DeleteAsync(id);
        }
    }

    [Fact]
    public async Task AddAsync_ValidProject_InsertsSuccessfully()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Integration Test Project",
            ClientName = "Test Client",
            ResearcherLabel = "Test Researcher",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(project);
        _testProjectIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.ProjectName.Should().Be("Integration Test Project");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProject_ReturnsProject()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Get By ID Test",
            ClientName = "Test Client",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var added = await _repository.AddAsync(project);
        _testProjectIds.Add(added.Id);

        // Act
        var result = await _repository.GetByIdAsync(added.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(added.Id);
        result.ProjectName.Should().Be("Get By ID Test");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentProject_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProjects()
    {
        // Arrange
        var project1 = new Project
        {
            ProjectName = "Project Alpha",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var project2 = new Project
        {
            ProjectName = "Project Beta",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };

        var added1 = await _repository.AddAsync(project1);
        var added2 = await _repository.AddAsync(project2);
        _testProjectIds.Add(added1.Id);
        _testProjectIds.Add(added2.Id);

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(p => p.Id == added1.Id);
        results.Should().Contain(p => p.Id == added2.Id);
    }

    [Fact]
    public async Task GetByNameAsync_ExistingProject_ReturnsProject()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Unique Project Name 123",
            ClientName = "Test Client",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var added = await _repository.AddAsync(project);
        _testProjectIds.Add(added.Id);

        // Act
        var result = await _repository.GetByNameAsync("Unique Project Name 123");

        // Assert
        result.Should().NotBeNull();
        result!.ProjectName.Should().Be("Unique Project Name 123");
    }

    [Fact]
    public async Task GetByClientAsync_ReturnsClientProjects()
    {
        // Arrange
        var project1 = new Project
        {
            ProjectName = "Client A Project 1",
            ClientName = "Client XYZ",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var project2 = new Project
        {
            ProjectName = "Client A Project 2",
            ClientName = "Client XYZ",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };

        var added1 = await _repository.AddAsync(project1);
        var added2 = await _repository.AddAsync(project2);
        _testProjectIds.Add(added1.Id);
        _testProjectIds.Add(added2.Id);

        // Act
        var results = await _repository.GetByClientAsync("Client XYZ");

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(2);
        results.Should().Contain(p => p.Id == added1.Id);
        results.Should().Contain(p => p.Id == added2.Id);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesProject()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Original Name",
            ClientName = "Original Client",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var added = await _repository.AddAsync(project);
        _testProjectIds.Add(added.Id);

        // Act
        added.ProjectName = "Updated Name";
        added.ClientName = "Updated Client";
        added.ModifiedBy = "modifier_user";
        added.ModifiedOn = DateTime.UtcNow;
        var updated = await _repository.UpdateAsync(added);

        // Assert
        var retrieved = await _repository.GetByIdAsync(added.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ProjectName.Should().Be("Updated Name");
        retrieved.ClientName.Should().Be("Updated Client");
    }

    [Fact]
    public async Task DeleteAsync_RemovesProject()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Project to Delete",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var added = await _repository.AddAsync(project);

        // Act
        var deleted = await _repository.DeleteAsync(added.Id);

        // Assert
        deleted.Should().BeTrue();
        var retrieved = await _repository.GetByIdAsync(added.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetProjectWithSurveysAsync_LoadsProjectAndSurveys()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Project With Surveys",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var addedProject = await _repository.AddAsync(project);
        _testProjectIds.Add(addedProject.Id);

        // Add surveys
        var surveyRepo = new SurveyRepository(_context, _tenantContext);
        var survey1 = new Survey
        {
            ProjectId = addedProject.Id,
            SurveyName = "Q1 Survey",
            Status = SurveyStatus.Initial
        };
        var survey2 = new Survey
        {
            ProjectId = addedProject.Id,
            SurveyName = "Q2 Survey",
            Status = SurveyStatus.Initial
        };
        await surveyRepo.AddAsync(survey1);
        await surveyRepo.AddAsync(survey2);

        // Act
        var result = await _repository.GetProjectWithSurveysAsync(addedProject.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ProjectName.Should().Be("Project With Surveys");
        result.SurveyList.Should().NotBeNull();
        result.SurveyList.Should().HaveCount(2);
        result.SurveyList.Should().Contain(s => s.SurveyName == "Q1 Survey");
        result.SurveyList.Should().Contain(s => s.SurveyName == "Q2 Survey");
    }

    [Fact]
    public async Task GetSurveysByProjectIdAsync_ReturnsProjectSurveys()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Multi-Survey Project",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var addedProject = await _repository.AddAsync(project);
        _testProjectIds.Add(addedProject.Id);

        // Add surveys
        var surveyRepo = new SurveyRepository(_context, _tenantContext);
        var survey1 = new Survey
        {
            ProjectId = addedProject.Id,
            SurveyName = "Wave 1",
            Status = SurveyStatus.Initial
        };
        var survey2 = new Survey
        {
            ProjectId = addedProject.Id,
            SurveyName = "Wave 2",
            Status = SurveyStatus.Initial
        };
        await surveyRepo.AddAsync(survey1);
        await surveyRepo.AddAsync(survey2);

        // Act
        var results = await _repository.GetSurveysByProjectIdAsync(addedProject.Id);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(s => s.SurveyName == "Wave 1");
        results.Should().Contain(s => s.SurveyName == "Wave 2");
        results.Should().OnlyContain(s => s.ProjectId == addedProject.Id);
    }

    [Fact]
    public async Task DeleteProject_CascadesDeleteToSurveys()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Cascade Delete Test",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var addedProject = await _repository.AddAsync(project);

        var surveyRepo = new SurveyRepository(_context, _tenantContext);
        var survey = new Survey
        {
            ProjectId = addedProject.Id,
            SurveyName = "Child Survey",
            Status = SurveyStatus.Initial
        };
        var addedSurvey = await surveyRepo.AddAsync(survey);

        // Act
        await _repository.DeleteAsync(addedProject.Id);

        // Assert
        var deletedProject = await _repository.GetByIdAsync(addedProject.Id);
        deletedProject.Should().BeNull();

        var deletedSurvey = await surveyRepo.GetByIdAsync(addedSurvey.Id);
        deletedSurvey.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WithAllProperties_SavesAllData()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Complete Project",
            ClientName = "Complete Client",
            ResearcherLabel = "Research Firm",
            ResearcherSubLabel = "Data Analytics Division",
            ResearcherLogo = "data:image/png;base64,abc123",
            ResearcherLogoFilename = "logo.png",
            ResearcherLogoPath = "/logos/logo.png",
            BaseProjectFolder = "/projects/base",
            ProjectFolder = "/projects/2025",
            FullFolder = "/projects/2025/complete",
            FullPath = "/projects/2025/complete/project.porp",
            FileName = "project.porp",
            IsExported = true,
            CreatedBy = "admin",
            CreatedOn = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(project);
        _testProjectIds.Add(result.Id);

        // Assert
        var retrieved = await _repository.GetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ProjectName.Should().Be("Complete Project");
        retrieved.ClientName.Should().Be("Complete Client");
        retrieved.ResearcherLabel.Should().Be("Research Firm");
        retrieved.ResearcherSubLabel.Should().Be("Data Analytics Division");
        retrieved.IsExported.Should().BeTrue();
        retrieved.FileName.Should().Be("project.porp");
    }

    [Fact]
    public async Task GetByNameAsync_NonExistentProject_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("This Project Does Not Exist 999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByClientAsync_NonExistentClient_ReturnsEmpty()
    {
        // Act
        var results = await _repository.GetByClientAsync("Non Existent Client 999");

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSurveysByProjectIdAsync_ProjectWithNoSurveys_ReturnsEmpty()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Empty Project",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var addedProject = await _repository.AddAsync(project);
        _testProjectIds.Add(addedProject.Id);

        // Act
        var results = await _repository.GetSurveysByProjectIdAsync(addedProject.Id);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProjectWithSurveysAsync_ProjectWithNoSurveys_ReturnsProjectWithEmptyList()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Solo Project",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var addedProject = await _repository.AddAsync(project);
        _testProjectIds.Add(addedProject.Id);

        // Act
        var result = await _repository.GetProjectWithSurveysAsync(addedProject.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ProjectName.Should().Be("Solo Project");
        result.SurveyList.Should().NotBeNull();
        result.SurveyList.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_MultipleUpdates_PersistsChanges()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Original",
            ClientName = "Original Client",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var added = await _repository.AddAsync(project);
        _testProjectIds.Add(added.Id);

        // Act - First update
        added.ProjectName = "First Update";
        await _repository.UpdateAsync(added);

        // Act - Second update
        added.ProjectName = "Second Update";
        added.ClientName = "Updated Client";
        await _repository.UpdateAsync(added);

        // Assert
        var retrieved = await _repository.GetByIdAsync(added.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ProjectName.Should().Be("Second Update");
        retrieved.ClientName.Should().Be("Updated Client");
    }

    [Fact]
    public async Task DeleteAsync_NonExistentProject_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WithMinimalData_Succeeds()
    {
        // Arrange - Only required fields (ProjectName is required in DB)
        var project = new Project
        {
            ProjectName = "Minimal Test Project",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(project);
        _testProjectIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);

        var retrieved = await _repository.GetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleProjects_ReturnsAllInOrder()
    {
        // Arrange
        var project1 = new Project
        {
            ProjectName = "Alpha Project",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow.AddDays(-2)
        };
        var project2 = new Project
        {
            ProjectName = "Beta Project",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow.AddDays(-1)
        };
        var project3 = new Project
        {
            ProjectName = "Gamma Project",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };

        var added1 = await _repository.AddAsync(project1);
        var added2 = await _repository.AddAsync(project2);
        var added3 = await _repository.AddAsync(project3);
        _testProjectIds.Add(added1.Id);
        _testProjectIds.Add(added2.Id);
        _testProjectIds.Add(added3.Id);

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(3);
        results.Should().Contain(p => p.Id == added1.Id);
        results.Should().Contain(p => p.Id == added2.Id);
        results.Should().Contain(p => p.Id == added3.Id);
    }

    [Fact]
    public async Task UpdateAsync_NullableFieldsToNull_UpdatesSuccessfully()
    {
        // Arrange
        var project = new Project
        {
            ProjectName = "Test Project",
            ClientName = "Test Client",
            ResearcherLabel = "Test Researcher",
            CreatedBy = "test_user",
            CreatedOn = DateTime.UtcNow
        };
        var added = await _repository.AddAsync(project);
        _testProjectIds.Add(added.Id);

        // Act - Set nullable fields to null
        added.ResearcherLabel = null;
        await _repository.UpdateAsync(added);

        // Assert
        var retrieved = await _repository.GetByIdAsync(added.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ResearcherLabel.Should().BeNull();
        retrieved.ProjectName.Should().Be("Test Project");
    }

    [Fact]
    public async Task GetProjectWithSurveysAsync_NonExistentProject_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetProjectWithSurveysAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSurveysByProjectIdAsync_NonExistentProject_ReturnsEmpty()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var results = await _repository.GetSurveysByProjectIdAsync(nonExistentId);

        // Assert
        results.Should().BeEmpty();
    }
}

