using FluentAssertions;
using Porpoise.Core.Models;
using Porpoise.DataAccess.Context;
using Porpoise.DataAccess.Repositories;
using Xunit;

namespace Porpoise.DataAccess.Tests.Integration;

/// <summary>
/// Integration tests for Project-Survey relationships.
/// Tests the foreign key relationship and repository operations.
/// </summary>
public class ProjectSurveyRelationshipTests : IDisposable
{
    private readonly DapperContext _context;
    private readonly ProjectRepository _projectRepository;
    private readonly SurveyRepository _surveyRepository;

    public ProjectSurveyRelationshipTests()
    {
        var connectionString = Environment.GetEnvironmentVariable("PORPOISE_TEST_CONNECTION") 
            ?? "Server=localhost;Port=3306;Database=porpoise;Uid=porpoise;Pwd=P0rp01se!;AllowUserVariables=True;UseAffectedRows=False";
        
        _context = new DapperContext(connectionString);
        _projectRepository = new ProjectRepository(_context);
        _surveyRepository = new SurveyRepository(_context);
    }

    [Fact(Skip = "Requires MySQL database")]
    public async Task CanCreateProjectAndAssignSurveys()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Test Project",
            ClientName = "Test Client",
            CreatedOn = DateTime.UtcNow
        };

        var survey1 = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Survey 1",
            ProjectId = project.Id,
            Status = SurveyStatus.Initial,
            CreatedOn = DateTime.UtcNow
        };

        var survey2 = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Survey 2",
            ProjectId = project.Id,
            Status = SurveyStatus.Initial,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        await _projectRepository.AddAsync(project);
        await _surveyRepository.AddAsync(survey1);
        await _surveyRepository.AddAsync(survey2);

        var retrievedProject = await _projectRepository.GetByIdAsync(project.Id);
        var surveys = await _projectRepository.GetSurveysByProjectIdAsync(project.Id);

        // Assert
        retrievedProject.Should().NotBeNull();
        retrievedProject!.ProjectName.Should().Be("Test Project");
        
        surveys.Should().HaveCount(2);
        surveys.Should().Contain(s => s.SurveyName == "Survey 1");
        surveys.Should().Contain(s => s.SurveyName == "Survey 2");

        // Cleanup
        await _surveyRepository.DeleteAsync(survey1.Id);
        await _surveyRepository.DeleteAsync(survey2.Id);
        await _projectRepository.DeleteAsync(project.Id);
    }

    [Fact(Skip = "Requires MySQL database")]
    public async Task GetProjectWithSurveysAsync_LoadsSurveyList()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Project with Surveys",
            ClientName = "Test Client",
            CreatedOn = DateTime.UtcNow
        };

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Attached Survey",
            ProjectId = project.Id,
            Status = SurveyStatus.Verified,
            CreatedOn = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        await _surveyRepository.AddAsync(survey);

        // Act
        var retrievedProject = await _projectRepository.GetProjectWithSurveysAsync(project.Id);

        // Assert
        retrievedProject.Should().NotBeNull();
        retrievedProject!.ProjectName.Should().Be("Project with Surveys");
        retrievedProject.SurveyList.Should().NotBeNull();
        retrievedProject.SurveyList.Should().HaveCount(1);
        retrievedProject.SurveyList![0].SurveyName.Should().Be("Attached Survey");

        // Cleanup
        await _surveyRepository.DeleteAsync(survey.Id);
        await _projectRepository.DeleteAsync(project.Id);
    }

    [Fact(Skip = "Requires MySQL database")]
    public async Task CanReassignSurveyToAnotherProject()
    {
        // Arrange
        var project1 = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Project 1",
            ClientName = "Client A",
            CreatedOn = DateTime.UtcNow
        };

        var project2 = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Project 2",
            ClientName = "Client B",
            CreatedOn = DateTime.UtcNow
        };

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Movable Survey",
            ProjectId = project1.Id,
            Status = SurveyStatus.Initial,
            CreatedOn = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project1);
        await _projectRepository.AddAsync(project2);
        await _surveyRepository.AddAsync(survey);

        // Act - Move survey from project1 to project2
        survey.ProjectId = project2.Id;
        await _surveyRepository.UpdateAsync(survey);

        var project1Surveys = await _projectRepository.GetSurveysByProjectIdAsync(project1.Id);
        var project2Surveys = await _projectRepository.GetSurveysByProjectIdAsync(project2.Id);

        // Assert
        project1Surveys.Should().BeEmpty();
        project2Surveys.Should().HaveCount(1);
        project2Surveys.Should().Contain(s => s.SurveyName == "Movable Survey");

        // Cleanup
        await _surveyRepository.DeleteAsync(survey.Id);
        await _projectRepository.DeleteAsync(project1.Id);
        await _projectRepository.DeleteAsync(project2.Id);
    }

    [Fact(Skip = "Requires MySQL database")]
    public async Task CanRemoveProjectIdFromSurvey()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Test Project",
            ClientName = "Test Client",
            CreatedOn = DateTime.UtcNow
        };

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Survey with Project",
            ProjectId = project.Id,
            Status = SurveyStatus.Initial,
            CreatedOn = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        await _surveyRepository.AddAsync(survey);

        // Act - Remove project assignment
        survey.ProjectId = null;
        await _surveyRepository.UpdateAsync(survey);

        var projectSurveys = await _projectRepository.GetSurveysByProjectIdAsync(project.Id);
        var retrievedSurvey = await _surveyRepository.GetByIdAsync(survey.Id);

        // Assert
        projectSurveys.Should().BeEmpty();
        retrievedSurvey.Should().NotBeNull();
        retrievedSurvey!.ProjectId.Should().BeNull();

        // Cleanup
        await _surveyRepository.DeleteAsync(survey.Id);
        await _projectRepository.DeleteAsync(project.Id);
    }

    [Fact(Skip = "Requires MySQL database")]
    public async Task GetSurveysByProjectIdAsync_ReturnsEmptyForProjectWithNoSurveys()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Empty Project",
            ClientName = "Test Client",
            CreatedOn = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);

        // Act
        var surveys = await _projectRepository.GetSurveysByProjectIdAsync(project.Id);

        // Assert
        surveys.Should().BeEmpty();

        // Cleanup
        await _projectRepository.DeleteAsync(project.Id);
    }

    [Fact(Skip = "Requires MySQL database")]
    public async Task GetProjectWithSurveysAsync_ReturnsNullForNonExistentProject()
    {
        // Act
        var project = await _projectRepository.GetProjectWithSurveysAsync(Guid.NewGuid());

        // Assert
        project.Should().BeNull();
    }

    [Fact(Skip = "Requires MySQL database")]
    public async Task CanCreateMultipleSurveysForPooling()
    {
        // Arrange - Create a project with multiple surveys for pooling/trending
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Pooled Project",
            ClientName = "Research Client",
            CreatedOn = DateTime.UtcNow
        };

        var surveys = new List<Survey>();
        for (int i = 1; i <= 5; i++)
        {
            surveys.Add(new Survey
            {
                Id = Guid.NewGuid(),
                SurveyName = $"Wave {i}",
                ProjectId = project.Id,
                Status = SurveyStatus.Verified,
                CreatedOn = DateTime.UtcNow
            });
        }

        await _projectRepository.AddAsync(project);
        foreach (var survey in surveys)
        {
            await _surveyRepository.AddAsync(survey);
        }

        // Act
        var projectWithSurveys = await _projectRepository.GetProjectWithSurveysAsync(project.Id);

        // Assert
        projectWithSurveys.Should().NotBeNull();
        projectWithSurveys!.SurveyList.Should().HaveCount(5);
        projectWithSurveys.SurveyList!.Select(s => s.SurveyName).Should().BeEquivalentTo(
            "Wave 1", "Wave 2", "Wave 3", "Wave 4", "Wave 5"
        );

        // Cleanup
        foreach (var survey in surveys)
        {
            await _surveyRepository.DeleteAsync(survey.Id);
        }
        await _projectRepository.DeleteAsync(project.Id);
    }

    [Fact(Skip = "Requires MySQL database")]
    public async Task DeleteProject_LeavesOrphanedSurveys()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Project to Delete",
            ClientName = "Test Client",
            CreatedOn = DateTime.UtcNow
        };

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Orphaned Survey",
            ProjectId = project.Id,
            Status = SurveyStatus.Initial,
            CreatedOn = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        await _surveyRepository.AddAsync(survey);

        // Act - Delete project but not surveys (no CASCADE)
        await _projectRepository.DeleteAsync(project.Id);

        var retrievedSurvey = await _surveyRepository.GetByIdAsync(survey.Id);
        var retrievedProject = await _projectRepository.GetByIdAsync(project.Id);

        // Assert
        retrievedProject.Should().BeNull();
        retrievedSurvey.Should().NotBeNull();
        retrievedSurvey!.ProjectId.Should().Be(project.Id); // Still points to deleted project

        // Cleanup
        await _surveyRepository.DeleteAsync(survey.Id);
    }

    [Fact(Skip = "Requires MySQL database")]
    public async Task GetByClientAsync_ReturnsProjectsWithSurveys()
    {
        // Arrange
        var project1 = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Client Project 1",
            ClientName = "Acme Corp",
            CreatedOn = DateTime.UtcNow
        };

        var project2 = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Client Project 2",
            ClientName = "Acme Corp",
            CreatedOn = DateTime.UtcNow
        };

        var survey1 = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Survey for P1",
            ProjectId = project1.Id,
            Status = SurveyStatus.Initial,
            CreatedOn = DateTime.UtcNow
        };

        var survey2 = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Survey for P2",
            ProjectId = project2.Id,
            Status = SurveyStatus.Verified,
            CreatedOn = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project1);
        await _projectRepository.AddAsync(project2);
        await _surveyRepository.AddAsync(survey1);
        await _surveyRepository.AddAsync(survey2);

        // Act
        var clientProjects = await _projectRepository.GetByClientAsync("Acme Corp");
        var surveys1 = await _projectRepository.GetSurveysByProjectIdAsync(project1.Id);
        var surveys2 = await _projectRepository.GetSurveysByProjectIdAsync(project2.Id);

        // Assert
        clientProjects.Should().HaveCount(2);
        surveys1.Should().HaveCount(1);
        surveys2.Should().HaveCount(1);

        // Cleanup
        await _surveyRepository.DeleteAsync(survey1.Id);
        await _surveyRepository.DeleteAsync(survey2.Id);
        await _projectRepository.DeleteAsync(project1.Id);
        await _projectRepository.DeleteAsync(project2.Id);
    }

    public void Dispose()
    {
        // Context cleanup if needed
    }
}
