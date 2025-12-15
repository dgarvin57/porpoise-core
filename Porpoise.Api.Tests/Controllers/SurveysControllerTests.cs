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
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<IResponseRepository> _mockResponseRepository;
    private readonly Mock<ISurveyDataRepository> _mockSurveyDataRepository;
    private readonly SurveysController _controller;

    public SurveysControllerTests()
    {
        _mockRepository = new Mock<ISurveyRepository>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockResponseRepository = new Mock<IResponseRepository>();
        _mockSurveyDataRepository = new Mock<ISurveyDataRepository>();
        _controller = new SurveysController(
            _mockRepository.Object,
            _mockQuestionRepository.Object,
            _mockResponseRepository.Object,
            _mockSurveyDataRepository.Object);
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
        var survey = new Survey { Id = surveyId, SurveyName = "Test Survey" };
        var questions = new List<Question>
        {
            new() { Id = Guid.NewGuid(), QstNumber = "Q1" },
            new() { Id = Guid.NewGuid(), QstNumber = "Q2" }
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync(survey);
        _mockQuestionRepository.Setup(r => r.GetBySurveyIdAsync(surveyId)).ReturnsAsync(questions);
        _mockSurveyDataRepository.Setup(r => r.GetCaseCountBySurveyIdAsync(surveyId)).ReturnsAsync(50);

        // Act
        var result = await _controller.GetSurveyStats(surveyId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var stats = okResult!.Value;
        stats.Should().NotBeNull();
    }

    #region PatchSurvey Tests

    [Fact]
    public async Task PatchSurvey_UpdatesSurveyNotes_Successfully()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey 
        { 
            Id = surveyId, 
            SurveyName = "Test Survey", 
            SurveyNotes = "Old notes",
            Status = SurveyStatus.Initial 
        };
        var updates = new Dictionary<string, object> { { "surveyNotes", "New notes" } };
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync(survey);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Survey>())).ReturnsAsync(survey);

        // Act
        var result = await _controller.PatchSurvey(surveyId, updates);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Survey>(s => s.SurveyNotes == "New notes")), Times.Once);
    }

    [Fact]
    public async Task PatchSurvey_UpdatesSurveyName_Successfully()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey 
        { 
            Id = surveyId, 
            SurveyName = "Old Name", 
            Status = SurveyStatus.Initial 
        };
        var updates = new Dictionary<string, object> { { "surveyName", "New Name" } };
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync(survey);
        _mockRepository.Setup(r => r.SurveyNameExistsAsync("New Name", surveyId)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Survey>())).ReturnsAsync(survey);

        // Act
        var result = await _controller.PatchSurvey(surveyId, updates);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Survey>(s => s.SurveyName == "New Name")), Times.Once);
    }

    [Fact]
    public async Task PatchSurvey_UpdatesStatus_Successfully()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey 
        { 
            Id = surveyId, 
            SurveyName = "Test Survey", 
            Status = SurveyStatus.Initial 
        };
        var updates = new Dictionary<string, object> { { "status", (int)SurveyStatus.Verified } };
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync(survey);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Survey>())).ReturnsAsync(survey);

        // Act
        var result = await _controller.PatchSurvey(surveyId, updates);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Survey>(s => s.Status == SurveyStatus.Verified)), Times.Once);
    }

    [Fact]
    public async Task PatchSurvey_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var updates = new Dictionary<string, object> { { "surveyNotes", "New notes" } };
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.PatchSurvey(surveyId, updates);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PatchSurvey_ReturnsConflict_WhenSurveyNameAlreadyExists()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey 
        { 
            Id = surveyId, 
            SurveyName = "Old Name", 
            Status = SurveyStatus.Initial 
        };
        var updates = new Dictionary<string, object> { { "surveyName", "Duplicate Name" } };
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync(survey);
        _mockRepository.Setup(r => r.SurveyNameExistsAsync("Duplicate Name", surveyId)).ReturnsAsync(true);

        // Act
        var result = await _controller.PatchSurvey(surveyId, updates);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    #endregion

    #region PatchQuestion Tests

    [Fact]
    public async Task PatchQuestion_UpdatesQuestionNotes_Successfully()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var question = new Question 
        { 
            Id = questionId, 
            QstNumber = "Q1", 
            QstLabel = "Test", 
            QuestionNotes = "Old notes" 
        };
        var updates = new Dictionary<string, object> { { "questionNotes", "New notes" } };
        
        _mockQuestionRepository.Setup(r => r.GetByIdAsync(questionId)).ReturnsAsync(question);
        _mockQuestionRepository.Setup(r => r.UpdateQuestionNotesAsync(questionId, "New notes")).ReturnsAsync(true);

        // Act
        var result = await _controller.PatchQuestion(surveyId, questionId, updates);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockQuestionRepository.Verify(r => r.UpdateQuestionNotesAsync(questionId, "New notes"), Times.Once);
    }

    [Fact]
    public async Task PatchQuestion_ReturnsNotFound_WhenQuestionDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var updates = new Dictionary<string, object> { { "questionNotes", "New notes" } };
        
        _mockQuestionRepository.Setup(r => r.GetByIdAsync(questionId)).ReturnsAsync((Question?)null);

        // Act
        var result = await _controller.PatchQuestion(surveyId, questionId, updates);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PatchQuestion_ReturnsProblem_WhenUpdateFails()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var question = new Question 
        { 
            Id = questionId, 
            QstNumber = "Q1", 
            QstLabel = "Test", 
            QuestionNotes = "Old notes" 
        };
        var updates = new Dictionary<string, object> { { "questionNotes", "New notes" } };
        
        _mockQuestionRepository.Setup(r => r.GetByIdAsync(questionId)).ReturnsAsync(question);
        _mockQuestionRepository.Setup(r => r.UpdateQuestionNotesAsync(questionId, "New notes")).ReturnsAsync(false);

        // Act
        var result = await _controller.PatchQuestion(surveyId, questionId, updates);

        // Assert
        result.Should().BeOfType<ObjectResult>();
    }

    #endregion

    #region SoftDelete/Restore/Permanent Delete Tests

    [Fact]
    public async Task SoftDeleteSurvey_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.SoftDeleteSurveyAsync(surveyId)).ReturnsAsync(true);

        // Act
        var result = await _controller.SoftDeleteSurvey(surveyId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SoftDeleteSurvey_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.SoftDeleteSurveyAsync(surveyId)).ReturnsAsync(false);

        // Act
        var result = await _controller.SoftDeleteSurvey(surveyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RestoreSurvey_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.RestoreSurveyAsync(surveyId)).ReturnsAsync(true);

        // Act
        var result = await _controller.RestoreSurvey(surveyId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RestoreSurvey_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.RestoreSurveyAsync(surveyId)).ReturnsAsync(false);

        // Act
        var result = await _controller.RestoreSurvey(surveyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PermanentlyDeleteSurvey_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.PermanentlyDeleteSurveyAsync(surveyId)).ReturnsAsync(true);

        // Act
        var result = await _controller.PermanentlyDeleteSurvey(surveyId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PermanentlyDeleteSurvey_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.PermanentlyDeleteSurveyAsync(surveyId)).ReturnsAsync(false);

        // Act
        var result = await _controller.PermanentlyDeleteSurvey(surveyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetDeletedSurveys_ReturnsOkWithSurveys()
    {
        // Arrange
        var surveys = new List<Survey>
        {
            new() { Id = Guid.NewGuid(), SurveyName = "Deleted Survey 1", IsDeleted = true },
            new() { Id = Guid.NewGuid(), SurveyName = "Deleted Survey 2", IsDeleted = true }
        };
        _mockRepository.Setup(r => r.GetDeletedSurveysAsync()).ReturnsAsync(surveys);

        // Act
        var result = await _controller.GetDeletedSurveys();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedSurveys = okResult!.Value as List<Survey>;
        returnedSurveys.Should().HaveCount(2);
    }

    #endregion

    #region GetSurveyData Tests

    [Fact]
    public async Task GetSurveyData_ReturnsOkWithData()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveyData = new SurveyData
        {
            DataList = new List<List<string>>
            {
                new() { "Col1", "Col2", "Col3" },
                new() { "1", "2", "3" },
                new() { "4", "5", "6" }
            },
            DataFilePath = "test.csv"
        };
        
        _mockSurveyDataRepository.Setup(r => r.GetBySurveyIdAsync(surveyId)).ReturnsAsync(surveyData);

        // Act
        var result = await _controller.GetSurveyData(surveyId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSurveyData_ReturnsNotFound_WhenNoData()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockSurveyDataRepository.Setup(r => r.GetBySurveyIdAsync(surveyId)).ReturnsAsync((SurveyData?)null);

        // Act
        var result = await _controller.GetSurveyData(surveyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSurveyData_ReturnsNotFound_WhenDataListIsEmpty()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveyData = new SurveyData
        {
            DataList = new List<List<string>>()
        };
        
        _mockSurveyDataRepository.Setup(r => r.GetBySurveyIdAsync(surveyId)).ReturnsAsync(surveyData);

        // Act
        var result = await _controller.GetSurveyData(surveyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetSurveyQuestionsList Tests

    [Fact]
    public async Task GetSurveyQuestionsList_ReturnsOkWithQuestions()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey { Id = surveyId, SurveyName = "Test Survey", Status = SurveyStatus.Initial };
        var questions = new List<Question>
        {
            new() { Id = Guid.NewGuid(), QstNumber = "Q1", QstLabel = "Question 1", DataFileCol = 1 },
            new() { Id = Guid.NewGuid(), QstNumber = "Q2", QstLabel = "Question 2", DataFileCol = 2 }
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync(survey);
        _mockQuestionRepository.Setup(r => r.GetBySurveyIdAsync(surveyId)).ReturnsAsync(questions);

        // Act
        var result = await _controller.GetSurveyQuestionsList(surveyId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSurveyQuestionsList_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.GetSurveyQuestionsList(surveyId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetQuestionResults Tests

    [Fact]
    public async Task GetQuestionResults_ReturnsOkWithResults()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var survey = new Survey { Id = surveyId, SurveyName = "Test Survey", Status = SurveyStatus.Initial };
        var question = new Question 
        { 
            Id = questionId, 
            QstNumber = "Q1", 
            QstLabel = "Test Question",
            DataFileCol = 1
        };
        var responses = new List<Response>
        {
            new() { Id = Guid.NewGuid(), RespValue = 1, Label = "Yes", Weight = 1.0 },
            new() { Id = Guid.NewGuid(), RespValue = 2, Label = "No", Weight = 1.0 }
        };
        var surveyData = new SurveyData
        {
            DataList = new List<List<string>>
            {
                new() { "CaseNum", "Q1" },
                new() { "1", "1" },
                new() { "2", "2" },
                new() { "3", "1" }
            }
        };
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync(survey);
        _mockQuestionRepository.Setup(r => r.GetByIdAsync(questionId)).ReturnsAsync(question);
        _mockResponseRepository.Setup(r => r.GetByQuestionIdAsync(questionId)).ReturnsAsync(responses);
        _mockSurveyDataRepository.Setup(r => r.GetBySurveyIdAsync(surveyId)).ReturnsAsync(surveyData);

        // Act
        var result = await _controller.GetQuestionResults(surveyId, questionId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetQuestionResults_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.GetQuestionResults(surveyId, questionId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetQuestionResults_ReturnsNotFound_WhenQuestionDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var survey = new Survey { Id = surveyId, SurveyName = "Test Survey", Status = SurveyStatus.Initial };
        
        _mockRepository.Setup(r => r.GetByIdAsync(surveyId)).ReturnsAsync(survey);
        _mockQuestionRepository.Setup(r => r.GetByIdAsync(questionId)).ReturnsAsync((Question?)null);

        // Act
        var result = await _controller.GetQuestionResults(surveyId, questionId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
