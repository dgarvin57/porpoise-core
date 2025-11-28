using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Porpoise.Api.Controllers;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

namespace Porpoise.Api.Tests.Controllers;

public class SurveysControllerTests
{
    private readonly Mock<ISurveyRepository> _mockRepository;
    private readonly SurveysController _controller;

    public SurveysControllerTests()
    {
        _mockRepository = new Mock<ISurveyRepository>();
        _controller = new SurveysController(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAllSurveys_ReturnsOkResult_WithListOfSurveys()
    {
        // Arrange
        var surveys = new List<Survey>
        {
            new() { Id = Guid.NewGuid(), SurveyName = "Survey 1" },
            new() { Id = Guid.NewGuid(), SurveyName = "Survey 2" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(surveys);

        // Act
        var result = await _controller.GetAllSurveys();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(surveys);
    }

    [Fact]
    public async Task GetSurveyById_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.GetSurveyById(surveyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSurveyById_ReturnsOkResult_WithSurvey()
    {
        // Arrange
        var survey = new Survey { Id = Guid.NewGuid(), SurveyName = "Test Survey" };
        _mockRepository.Setup(r => r.GetByIdAsync(survey.Id)).ReturnsAsync(survey);

        // Act
        var result = await _controller.GetSurveyById(survey.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(survey);
    }

    [Fact]
    public async Task GetSurveyByName_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyName = "NonExistent";
        _mockRepository.Setup(r => r.GetByNameAsync(surveyName)).ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.GetSurveyByName(surveyName);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSurveyByName_ReturnsOkResult_WithSurvey()
    {
        // Arrange
        var survey = new Survey { Id = Guid.NewGuid(), SurveyName = "Test Survey" };
        _mockRepository.Setup(r => r.GetByNameAsync(survey.SurveyName)).ReturnsAsync(survey);

        // Act
        var result = await _controller.GetSurveyByName(survey.SurveyName);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(survey);
    }

    [Fact]
    public async Task SearchSurveys_ReturnsOkResult_WithMatchingSurveys()
    {
        // Arrange
        var searchTerm = "test";
        var surveys = new List<Survey>
        {
            new() { Id = Guid.NewGuid(), SurveyName = "Test Survey 1" },
            new() { Id = Guid.NewGuid(), SurveyName = "Test Survey 2" }
        };
        _mockRepository.Setup(r => r.SearchByNameAsync(searchTerm)).ReturnsAsync(surveys);

        // Act
        var result = await _controller.SearchSurveys(searchTerm);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(surveys);
    }

    [Fact]
    public async Task GetSurveysByStatus_ReturnsOkResult_WithSurveys()
    {
        // Arrange
        var status = SurveyStatus.Verified;
        var surveys = new List<Survey>
        {
            new() { Id = Guid.NewGuid(), SurveyName = "Verified Survey 1", Status = status },
            new() { Id = Guid.NewGuid(), SurveyName = "Verified Survey 2", Status = status }
        };
        _mockRepository.Setup(r => r.GetByStatusAsync(status)).ReturnsAsync(surveys);

        // Act
        var result = await _controller.GetSurveysByStatus((int)status);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(surveys);
    }

    [Fact]
    public async Task CreateSurvey_ReturnsConflict_WhenSurveyNameExists()
    {
        // Arrange
        var survey = new Survey { Id = Guid.NewGuid(), SurveyName = "Existing Survey" };
        _mockRepository.Setup(r => r.SurveyNameExistsAsync(survey.SurveyName, null))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateSurvey(survey);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task CreateSurvey_ReturnsCreatedAtAction_WithSurvey()
    {
        // Arrange
        var survey = new Survey { Id = Guid.NewGuid(), SurveyName = "New Survey" };
        _mockRepository.Setup(r => r.SurveyNameExistsAsync(survey.SurveyName, null))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Survey>())).ReturnsAsync(survey);

        // Act
        var result = await _controller.CreateSurvey(survey);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(survey);
        createdResult.ActionName.Should().Be(nameof(SurveysController.GetSurveyById));
    }

    [Fact]
    public async Task UpdateSurvey_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey { Id = Guid.NewGuid(), SurveyName = "Test Survey" };

        // Act
        var result = await _controller.UpdateSurvey(surveyId, survey);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateSurvey_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var survey = new Survey { Id = Guid.NewGuid(), SurveyName = "Test Survey" };
        _mockRepository.Setup(r => r.ExistsAsync(survey.Id)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateSurvey(survey.Id, survey);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateSurvey_ReturnsConflict_WhenSurveyNameExists()
    {
        // Arrange
        var survey = new Survey { Id = Guid.NewGuid(), SurveyName = "Existing Survey" };
        _mockRepository.Setup(r => r.ExistsAsync(survey.Id)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.SurveyNameExistsAsync(survey.SurveyName, survey.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateSurvey(survey.Id, survey);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task UpdateSurvey_ReturnsOkResult_WhenUpdateIsSuccessful()
    {
        // Arrange
        var survey = new Survey { Id = Guid.NewGuid(), SurveyName = "Updated Survey" };
        _mockRepository.Setup(r => r.ExistsAsync(survey.Id)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.SurveyNameExistsAsync(survey.SurveyName, survey.Id))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Survey>())).ReturnsAsync(survey);

        // Act
        var result = await _controller.UpdateSurvey(survey.Id, survey);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(survey);
    }

    [Fact]
    public async Task DeleteSurvey_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.ExistsAsync(surveyId)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteSurvey(surveyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteSurvey_ReturnsNoContent_WhenDeleteIsSuccessful()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.ExistsAsync(surveyId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(surveyId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteSurvey(surveyId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetSurveyStats_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.ExistsAsync(surveyId)).ReturnsAsync(false);

        // Act
        var result = await _controller.GetSurveyStats(surveyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSurveyStats_ReturnsOkResult_WithStats()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.ExistsAsync(surveyId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.GetQuestionCountAsync(surveyId)).ReturnsAsync(10);
        _mockRepository.Setup(r => r.GetResponseCountAsync(surveyId)).ReturnsAsync(50);

        // Act
        var result = await _controller.GetSurveyStats(surveyId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var stats = okResult!.Value;
        stats.Should().NotBeNull();
    }
}
