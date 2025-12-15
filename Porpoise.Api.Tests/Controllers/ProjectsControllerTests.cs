using Microsoft.AspNetCore.Mvc;
using Moq;
using Porpoise.Api.Controllers;
using Porpoise.Api.Models;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace Porpoise.Api.Tests.Controllers
{
    public class ProjectsControllerTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly ProjectsController _controller;

        public ProjectsControllerTests()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _controller = new ProjectsController(_mockProjectRepository.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllProjects_ReturnsOkResult_WithListOfProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new Project { Id = Guid.NewGuid(), ProjectName = "Project 1" },
                new Project { Id = Guid.NewGuid(), ProjectName = "Project 2" }
            };
            _mockProjectRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(projects);

            // Act
            var result = await _controller.GetAllProjects();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<Project>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetProjectById_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Project);

            // Act
            var result = await _controller.GetProjectById(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetProjectById_ReturnsOkResult_WithProject()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, ProjectName = "Project 1" };
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ProjectResponse>(okResult.Value);
            Assert.Equal(projectId, returnValue.Id);
        }

        [Fact]
        public async Task CreateProject_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("ProjectName", "Required");

            // Act
            var result = await _controller.CreateProject(new Project());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateProject_ReturnsCreatedAtAction_WithProject()
        {
            // Arrange
            var project = new Project { Id = Guid.NewGuid(), ProjectName = "Project 1" };
            _mockProjectRepository.Setup(repo => repo.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.CreateProject(project);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Project>(createdAtActionResult.Value);
            Assert.Equal(project.Id, returnValue.Id);
        }

        [Fact]
        public async Task UpdateProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Project);

            // Act
            var result = await _controller.UpdateProject(Guid.NewGuid(), new UpdateProjectRequest { ProjectName = "Test" });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateProject_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var existingProject = new Project { Id = projectId, ProjectName = "Project 1" };
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId)).ReturnsAsync(existingProject);
            _mockProjectRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Project>())).ReturnsAsync(existingProject);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateProject(projectId, new UpdateProjectRequest { ProjectName = "Updated Project" });

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Project);

            // Act
            var result = await _controller.DeleteProject(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProject_ReturnsNoContent_WhenDeleteIsSuccessful()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, ProjectName = "Project 1" };
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockProjectRepository.Setup(repo => repo.DeleteAsync(projectId)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetProjectsWithSurveyCounts_ReturnsOkResult_WithProjectCounts()
        {
            // Arrange
            var projectsWithCounts = new List<dynamic>
            {
                new { Id = Guid.NewGuid(), ProjectName = "Project 1", SurveyCount = 3 },
                new { Id = Guid.NewGuid(), ProjectName = "Project 2", SurveyCount = 1 }
            };
            _mockProjectRepository.Setup(repo => repo.GetProjectsWithSurveyCountAsync()).ReturnsAsync(projectsWithCounts);

            // Act
            var result = await _controller.GetProjectsWithCounts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetMultiSurveyProjects_ReturnsOkResult_WithMultiSurveyProjects()
        {
            // Arrange
            var multiSurveyProjects = new List<dynamic>
            {
                new { Id = Guid.NewGuid(), ProjectName = "Multi Survey Project", SurveyCount = 4 }
            };
            _mockProjectRepository.Setup(repo => repo.GetMultiSurveyProjectsAsync()).ReturnsAsync(multiSurveyProjects);

            // Act
            var result = await _controller.GetMultiSurveyProjects();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetProjectSurveys_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Project);

            // Act
            var result = await _controller.GetProjectSurveys(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MoveSurveyToProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(null as Project);

            // Act
            var result = await _controller.MoveSurvey(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MoveSurveyToProject_ReturnsNoContent_WhenMoveIsSuccessful()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var surveyId = Guid.NewGuid();
            var project = new Project { Id = projectId, ProjectName = "Project 1" };
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockProjectRepository.Setup(repo => repo.MoveSurveyToProjectAsync(surveyId, projectId)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.MoveSurvey(projectId, surveyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetProjectById_HandlesNullLogo()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, ProjectName = "Test", ClientLogo = null };
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ProjectResponse>(okResult.Value);
            response.ClientLogoBase64.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectById_ConvertsLogoToBase64()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project 
            { 
                Id = projectId, 
                ProjectName = "Test",
                ClientLogo = new byte[] { 1, 2, 3, 4 }
            };
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ProjectResponse>(okResult.Value);
            response.ClientLogoBase64.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UpdateProject_HandlesBase64LogoWithDataUrlPrefix()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var existingProject = new Project { Id = projectId, ProjectName = "Test" };
            var base64Logo = "data:image/png;base64," + Convert.ToBase64String(new byte[] { 1, 2, 3 });
            
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId)).ReturnsAsync(existingProject);
            _mockProjectRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Project>())).ReturnsAsync(existingProject);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateProject(projectId, new UpdateProjectRequest 
            { 
                ProjectName = "Test",
                ClientLogoBase64 = base64Logo 
            });

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockProjectRepository.Verify(repo => repo.UpdateAsync(It.Is<Project>(p => p.ClientLogo != null)), Times.Once);
        }

        [Fact]
        public async Task UpdateProject_ReturnsBadRequest_WhenLogoFormatIsInvalid()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var existingProject = new Project { Id = projectId, ProjectName = "Test" };
            
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId)).ReturnsAsync(existingProject);

            // Act
            var result = await _controller.UpdateProject(projectId, new UpdateProjectRequest 
            { 
                ProjectName = "Test",
                ClientLogoBase64 = "invalid-base64!!!" 
            });

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be("Invalid logo image format");
        }

        [Fact]
        public async Task GetProjectLogos_ReturnsOnlyProjectsWithLogos()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { Id = Guid.NewGuid(), ProjectName = "P1", ClientLogo = new byte[] { 1, 2 } },
                new() { Id = Guid.NewGuid(), ProjectName = "P2", ClientLogo = null },
                new() { Id = Guid.NewGuid(), ProjectName = "P3", ClientLogo = new byte[] { 3, 4 } }
            };
            _mockProjectRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(projects);

            // Act
            var result = await _controller.GetProjectLogos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetProjectSurveys_ReturnsOkWithSurveys()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, ProjectName = "Test" };
            var surveys = new List<Survey>
            {
                new() { Id = Guid.NewGuid(), SurveyName = "Survey 1", ProjectId = projectId }
            };
            
            _mockProjectRepository.Setup(repo => repo.GetByIdAsync(projectId)).ReturnsAsync(project);
            _mockProjectRepository.Setup(repo => repo.GetSurveysWithCountsAsync(projectId)).ReturnsAsync(surveys);

            // Act
            var result = await _controller.GetProjectSurveys(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(surveys);
        }

        [Fact]
        public async Task MoveSurvey_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var surveyId = Guid.NewGuid();
            
            _mockProjectRepository.Setup(repo => repo.MoveSurveyToProjectAsync(surveyId, projectId)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.MoveSurvey(projectId, surveyId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteProject_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(repo => repo.SoftDeleteProjectAsync(projectId)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.SoftDeleteProject(projectId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(repo => repo.SoftDeleteProjectAsync(projectId)).ReturnsAsync(false);

            // Act
            var result = await _controller.SoftDeleteProject(projectId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task RestoreProject_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(repo => repo.RestoreProjectAsync(projectId)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.RestoreProject(projectId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task RestoreProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(repo => repo.RestoreProjectAsync(projectId)).ReturnsAsync(false);

            // Act
            var result = await _controller.RestoreProject(projectId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task PermanentlyDeleteProject_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(repo => repo.PermanentlyDeleteProjectAsync(projectId)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(uow => uow.CommitAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.PermanentlyDeleteProject(projectId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task PermanentlyDeleteProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(repo => repo.PermanentlyDeleteProjectAsync(projectId)).ReturnsAsync(false);

            // Act
            var result = await _controller.PermanentlyDeleteProject(projectId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task PermanentlyDeleteProject_ReturnsProblem_WhenExceptionThrown()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectRepository.Setup(repo => repo.PermanentlyDeleteProjectAsync(projectId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.PermanentlyDeleteProject(projectId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetDeletedProjects_ReturnsOkWithProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { Id = Guid.NewGuid(), ProjectName = "Deleted 1", IsDeleted = true },
                new() { Id = Guid.NewGuid(), ProjectName = "Deleted 2", IsDeleted = true }
            };
            _mockProjectRepository.Setup(repo => repo.GetDeletedProjectsAsync()).ReturnsAsync(projects);

            // Act
            var result = await _controller.GetDeletedProjects();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProjects = okResult.Value as List<Project>;
            returnedProjects.Should().HaveCount(2);
        }
    }
}