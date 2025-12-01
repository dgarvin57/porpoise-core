using Microsoft.AspNetCore.Mvc;
using Moq;
using Porpoise.Api.Controllers;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

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
            var returnValue = Assert.IsType<Project>(okResult.Value);
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
            var result = await _controller.UpdateProject(Guid.NewGuid(), new Project());

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
            var result = await _controller.UpdateProject(projectId, new Project { ProjectName = "Updated Project" });

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
    }
}