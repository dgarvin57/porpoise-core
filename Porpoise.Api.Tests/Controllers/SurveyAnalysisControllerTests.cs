using Microsoft.AspNetCore.Mvc;
using Moq;
using Porpoise.Api.Configuration;
using Porpoise.Api.Controllers;
using Porpoise.Api.Services;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Xunit;

namespace Porpoise.Api.Tests.Controllers;

public class SurveyAnalysisControllerTests
{
    private readonly Mock<ISurveyRepository> _mockSurveyRepository;
    private readonly Mock<IQuestionRepository> _mockQuestionRepository;
    private readonly Mock<ISurveyDataRepository> _mockSurveyDataRepository;
    private readonly Mock<IResponseRepository> _mockResponseRepository;
    private readonly Mock<AIInsightsService> _mockAIService;
    private readonly AiPromptConfiguration _promptConfig;
    private readonly PromptTemplateEngine _templateEngine;
    private readonly SurveyAnalysisController _controller;

    public SurveyAnalysisControllerTests()
    {
        _mockSurveyRepository = new Mock<ISurveyRepository>();
        _mockQuestionRepository = new Mock<IQuestionRepository>();
        _mockSurveyDataRepository = new Mock<ISurveyDataRepository>();
        _mockResponseRepository = new Mock<IResponseRepository>();
        
        // Mock AIInsightsService with null parameters since we're mocking it
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type
        _mockAIService = new Mock<AIInsightsService>(MockBehavior.Default, null!, null, "anthropic");
#pragma warning restore CS8625
        _promptConfig = new AiPromptConfiguration();
        _templateEngine = new PromptTemplateEngine();

        _controller = new SurveyAnalysisController(
            _mockSurveyRepository.Object,
            _mockQuestionRepository.Object,
            _mockSurveyDataRepository.Object,
            _mockResponseRepository.Object,
            _mockAIService.Object,
            _promptConfig,
            _templateEngine
        );
    }

    #region GetSurveySummary Tests

    [Fact]
    public async Task GetSurveySummary_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        Survey? nullSurvey = null;
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync(nullSurvey);

        // Act
        var result = await _controller.GetSurveySummary(surveyId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(surveyId.ToString(), notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetSurveySummary_ReturnsBadRequest_WhenSurveyHasNoQuestions()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = new ObjectListBase<Question>()  // Empty list
        };
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        // Act
        var result = await _controller.GetSurveySummary(surveyId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Survey has no questions", badRequestResult.Value);
    }

    [Fact]
    public async Task GetSurveySummary_ReturnsBadRequest_WhenSurveyHasNoData()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionList = new ObjectListBase<Question>();
        questionList.Add(new Question { Id = Guid.NewGuid(), QstLabel = "Q1" });
        
        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = questionList,
            Data = null // No data
        };
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        // Act
        var result = await _controller.GetSurveySummary(surveyId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Survey has no response data", badRequestResult.Value);
    }

    [Fact]
    public async Task GetSurveySummary_ReturnsOk_WithAISummary()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionList = new ObjectListBase<Question>();
        questionList.Add(new Question { Id = Guid.NewGuid(), QstLabel = "Overall satisfaction", VariableType = QuestionVariableType.Dependent });
        questionList.Add(new Question { Id = Guid.NewGuid(), QstLabel = "Would recommend", VariableType = QuestionVariableType.Dependent });
        
        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Customer Satisfaction Survey",
            QuestionList = questionList,
            Data = new SurveyData
            {
                DataList = new List<List<string>>
                {
                    new List<string> { "ID", "Q1", "Q2" }, // Header
                    new List<string> { "1", "5", "4" },
                    new List<string> { "2", "4", "5" }
                }
            }
        };

        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        var expectedAISummary = "This customer satisfaction survey with 2 questions and 2 respondents shows strong overall satisfaction.";
        _mockAIService.Setup(service => service.GenerateSurveySummary(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync(expectedAISummary);

        // Act
        var result = await _controller.GetSurveySummary(surveyId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        
        Assert.NotNull(value);
        var summaryProp = value.GetType().GetProperty("AISummary");
        Assert.NotNull(summaryProp);
        Assert.Equal(expectedAISummary, summaryProp.GetValue(value));

        // Verify the AI service was called with correct parameters
        _mockAIService.Verify(service => service.GenerateSurveySummary(
            1, // totalSurveys
            2, // totalQuestions
            2, // totalCases (excluding header)
            "Customer Satisfaction Survey",
            It.IsAny<List<string>>(),
            It.IsAny<List<string>>()), Times.Once);
    }

    #endregion

    #region GetToplineResults Tests

    [Fact]
    public async Task GetToplineResults_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.GetToplineResults(surveyId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(surveyId.ToString(), notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetToplineResults_ReturnsBadRequest_WhenSurveyHasNoQuestions()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = null!
        };
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        // Act
        var result = await _controller.GetToplineResults(surveyId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Survey has no questions", badRequestResult.Value);
    }

    [Fact]
    public async Task GetToplineResults_ReturnsBadRequest_WhenSurveyHasNoData()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionList = new ObjectListBase<Question>();
        questionList.Add(new Question { Id = Guid.NewGuid(), QstLabel = "Q1" });
        
        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = questionList,
            Data = null
        };
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        // Act
        var result = await _controller.GetToplineResults(surveyId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Survey has no response data", badRequestResult.Value);
    }

    #endregion

    #region AnalyzeQuestion Tests

    [Fact]
    public async Task AnalyzeQuestion_ReturnsBadRequest_WhenRequestIsNull()
    {
        // Arrange
        var surveyId = Guid.NewGuid();

        // Act
        var result = await _controller.AnalyzeQuestion(surveyId, null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AnalyzeQuestion_ReturnsOk_WithAnalysis()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var request = new QuestionAnalysisRequest
        {
            QuestionLabel = "Overall satisfaction",
            TotalN = 100,
            Responses = new List<ResponseData>
            {
                new ResponseData { Label = "Very satisfied", Frequency = 45, Percent = 45.0 },
                new ResponseData { Label = "Satisfied", Frequency = 35, Percent = 35.0 },
                new ResponseData { Label = "Neutral", Frequency = 20, Percent = 20.0 }
            }
        };

        var expectedAnalysis = "The satisfaction data shows 80% positive sentiment, indicating strong overall satisfaction.";
        _mockAIService.Setup(service => service.CallAIAPI(It.IsAny<string>(), It.IsAny<double>()))
            .ReturnsAsync(expectedAnalysis);

        // Act
        var result = await _controller.AnalyzeQuestion(surveyId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify AI service was called (note: template engine may produce empty string, so we just check it was called)
        _mockAIService.Verify(service => service.CallAIAPI(
            It.IsAny<string>(),
            It.IsAny<double>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeQuestion_ReturnsOk_WhenAIServiceFails()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var request = new QuestionAnalysisRequest
        {
            QuestionLabel = "Test Question",
            TotalN = 100,
            Responses = new List<ResponseData>()
        };

        _mockAIService.Setup(service => service.CallAIAPI(It.IsAny<string>(), It.IsAny<double>()))
            .ThrowsAsync(new HttpRequestException("API rate limit exceeded"));

        // Act
        var result = await _controller.AnalyzeQuestion(surveyId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        // Should return fallback message instead of throwing
        var value = okResult.Value;
        var analysisProp = value.GetType().GetProperty("analysis");
        Assert.NotNull(analysisProp);
        var analysis = analysisProp.GetValue(value)?.ToString();
        Assert.Contains("Unable to generate AI analysis", analysis);
    }

    #endregion

    #region AnalyzeCrosstab Tests

    [Fact]
    public async Task AnalyzeCrosstab_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var request = new CrosstabAnalysisRequest
        {
            IndependentVariable = "Gender",
            DependentVariable = "Age",
            TotalN = 100,
            ChiSquare = 15.5,
            ChiSquareSignificant = true,
            Phi = 0.35,
            CramersV = 0.32
        };

        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.AnalyzeCrosstab(surveyId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Survey", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task AnalyzeCrosstab_ReturnsNotFound_WhenRequestIsNull()
    {
        // Arrange
        var surveyId = Guid.NewGuid();

        // Act
        var result = await _controller.AnalyzeCrosstab(surveyId, null!);

        // Assert - Controller checks survey first, so returns NotFound when survey doesn't exist
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region AnalyzeStatisticalSignificance Tests

    [Fact]
    public async Task AnalyzeStatSig_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var request = new StatSigAnalysisRequest
        {
            QuestionLabel = "Test Question",
            StatSigData = new List<StatSigDataItem>()
        };

        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.AnalyzeStatisticalSignificance(surveyId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Survey", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task AnalyzeStatSig_ReturnsOk_WithFallback_WhenAIFails()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey { Id = surveyId, SurveyName = "Test" };
        var request = new StatSigAnalysisRequest
        {
            QuestionLabel = "Test Question",
            StatSigData = new List<StatSigDataItem>()
        };

        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _mockAIService.Setup(service => service.CallAIAPI(It.IsAny<string>(), It.IsAny<double>()))
            .ThrowsAsync(new Exception("AI service error"));

        // Act
        var result = await _controller.AnalyzeStatisticalSignificance(surveyId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetSurveySummary_ReturnsProblem_WhenExceptionOccurs()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetSurveySummary(surveyId);

        // Assert
        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    #endregion

    #region GetFormattedToplineReport Tests

    [Fact]
    public async Task GetFormattedToplineReport_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.GetFormattedToplineReport(surveyId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(surveyId.ToString(), notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetFormattedToplineReport_ReturnsBadRequest_WhenSurveyHasNoQuestions()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey { Id = surveyId, QuestionList = new ObjectListBase<Question>() };
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        // Act
        var result = await _controller.GetFormattedToplineReport(surveyId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Survey has no questions", badRequestResult.Value);
    }

    #endregion

    #region GetCrosstab Tests

    [Fact]
    public async Task GetCrosstab_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync((Survey?)null);
        
        var request = new CrosstabRequest
        {
            FirstQuestionId = Guid.NewGuid(),
            SecondQuestionId = Guid.NewGuid()
        };

        // Act
        var result = await _controller.GetCrosstab(surveyId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Survey", notFoundResult.Value?.ToString());
    }

    #endregion

    #region GetStatisticalSignificance Tests

    [Fact]
    public async Task GetStatisticalSignificance_ReturnsNotFound_WhenSurveyDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync((Survey?)null);

        // Act
        var result = await _controller.GetStatisticalSignificance(surveyId, questionId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Survey", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetStatisticalSignificance_ReturnsNotFound_WhenQuestionDoesNotExist()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var survey = new Survey 
        { 
            Id = surveyId, 
            QuestionList = new ObjectListBase<Question>()
        };
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);

        // Act
        var result = await _controller.GetStatisticalSignificance(surveyId, questionId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Question", notFoundResult.Value?.ToString());
    }

    #endregion

    #region Additional Error Handling Tests

    [Fact]
    public async Task GetToplineResults_ReturnsProblem_WhenExceptionOccurs()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetToplineResults(surveyId);

        // Assert
        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task GetFormattedToplineReport_ReturnsProblem_WhenExceptionOccurs()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetFormattedToplineReport(surveyId);

        // Assert
        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task GetCrosstab_ReturnsProblem_WhenExceptionOccurs()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ThrowsAsync(new Exception("Unexpected error"));
        
        var request = new CrosstabRequest
        {
            FirstQuestionId = Guid.NewGuid(),
            SecondQuestionId = Guid.NewGuid()
        };

        // Act
        var result = await _controller.GetCrosstab(surveyId, request);

        // Assert
        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    [Fact]
    public async Task GetStatisticalSignificance_ReturnsProblem_WhenExceptionOccurs()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        _mockSurveyRepository.Setup(repo => repo.GetByIdAsync(surveyId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetStatisticalSignificance(surveyId, questionId);

        // Assert
        var problemResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
    }

    #endregion
}
