using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Xunit;

namespace Porpoise.Core.Tests.Services;

public class SurveyPersistenceServiceTests
{
    private readonly Mock<ISurveyRepository> _surveyRepoMock;
    private readonly Mock<IQuestionRepository> _questionRepoMock;
    private readonly Mock<IResponseRepository> _responseRepoMock;
    private readonly Mock<ISurveyDataRepository> _surveyDataRepoMock;
    private readonly Mock<IQuestionBlockRepository> _questionBlockRepoMock;
    private readonly Mock<IProjectRepository> _projectRepoMock;
    private readonly SurveyPersistenceService _service;

    public SurveyPersistenceServiceTests()
    {
        _surveyRepoMock = new Mock<ISurveyRepository>();
        _questionRepoMock = new Mock<IQuestionRepository>();
        _responseRepoMock = new Mock<IResponseRepository>();
        _surveyDataRepoMock = new Mock<ISurveyDataRepository>();
        _questionBlockRepoMock = new Mock<IQuestionBlockRepository>();
        _projectRepoMock = new Mock<IProjectRepository>();

        _service = new SurveyPersistenceService(
            _surveyRepoMock.Object,
            _questionRepoMock.Object,
            _responseRepoMock.Object,
            _surveyDataRepoMock.Object,
            _questionBlockRepoMock.Object,
            _projectRepoMock.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesService_WithoutProjectRepository()
    {
        // Act
        var service = new SurveyPersistenceService(
            _surveyRepoMock.Object,
            _questionRepoMock.Object,
            _responseRepoMock.Object,
            _surveyDataRepoMock.Object,
            _questionBlockRepoMock.Object
        );

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region SaveSurveyWithDetailsAsync Tests

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_SavesSurvey_WithoutQuestions()
    {
        // Arrange
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Test Survey",
            QuestionList = null
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(survey.Id);
        _surveyRepoMock.Verify(r => r.AddAsync(survey), Times.Once);
        _questionRepoMock.Verify(r => r.AddAsync(It.IsAny<Question>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_SavesSurvey_WithEmptyQuestionList()
    {
        // Arrange
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Test Survey",
            QuestionList = new ObjectListBase<Question>()
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        result.Should().NotBeNull();
        _surveyRepoMock.Verify(r => r.AddAsync(survey), Times.Once);
        _questionRepoMock.Verify(r => r.AddAsync(It.IsAny<Question>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_SavesQuestions_WithoutResponses()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var question = new Question
        {
            Id = questionId,
            QstLabel = "Test Question",
            Responses = null
        };

        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = new ObjectListBase<Question> { question }
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);
        _questionRepoMock.Setup(r => r.AddAsync(question, surveyId))
            .ReturnsAsync(question);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        result.Should().NotBeNull();
        _questionRepoMock.Verify(r => r.AddAsync(question, surveyId), Times.Once);
        _responseRepoMock.Verify(r => r.AddAsync(It.IsAny<Response>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_SavesQuestions_WithResponses()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        
        var response = new Response
        {
            Id = responseId,
            Label = "Option 1"
        };

        var question = new Question
        {
            Id = questionId,
            QstLabel = "Test Question",
            Responses = new ObjectListBase<Response> { response }
        };

        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = new ObjectListBase<Question> { question }
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);
        _questionRepoMock.Setup(r => r.AddAsync(question, surveyId))
            .ReturnsAsync(question);
        _responseRepoMock.Setup(r => r.AddAsync(response, questionId))
            .ReturnsAsync(response);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        result.Should().NotBeNull();
        _questionRepoMock.Verify(r => r.AddAsync(question, surveyId), Times.Once);
        _responseRepoMock.Verify(r => r.AddAsync(response, questionId), Times.Once);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_SavesQuestions_WithMultipleResponses()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        
        var responses = new ObjectListBase<Response>
        {
            new Response { Id = Guid.NewGuid(), Label = "Option 1" },
            new Response { Id = Guid.NewGuid(), Label = "Option 2" },
            new Response { Id = Guid.NewGuid(), Label = "Option 3" }
        };

        var question = new Question
        {
            Id = questionId,
            QstLabel = "Test Question",
            Responses = responses
        };

        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = new ObjectListBase<Question> { question }
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);
        _questionRepoMock.Setup(r => r.AddAsync(question, surveyId))
            .ReturnsAsync(question);
        _responseRepoMock.Setup(r => r.AddAsync(It.IsAny<Response>(), questionId))
            .ReturnsAsync((Response r, Guid qId) => r);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        _responseRepoMock.Verify(r => r.AddAsync(It.IsAny<Response>(), questionId), Times.Exactly(3));
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_SavesSurveyData_WhenProvided()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var surveyData = new SurveyData
        {
            DataList = new List<List<string>>
            {
                new List<string> { "Q1", "Q2" },
                new List<string> { "A1", "A2" }
            }
        };

        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            Data = surveyData
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);
        _surveyDataRepoMock.Setup(r => r.AddAsync(surveyData, surveyId))
            .ReturnsAsync(surveyData);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        _surveyDataRepoMock.Verify(r => r.AddAsync(surveyData, surveyId), Times.Once);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_DoesNotSaveSurveyData_WhenNull()
    {
        // Arrange
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Test Survey",
            Data = null
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        _surveyDataRepoMock.Verify(r => r.AddAsync(It.IsAny<SurveyData>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_DoesNotSaveSurveyData_WhenDataListIsNull()
    {
        // Arrange
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Test Survey",
            Data = new SurveyData { DataList = null }
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        _surveyDataRepoMock.Verify(r => r.AddAsync(It.IsAny<SurveyData>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_DoesNotSaveSurveyData_WhenDataListIsEmpty()
    {
        // Arrange
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Test Survey",
            Data = new SurveyData { DataList = new List<List<string>>() }
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        _surveyDataRepoMock.Verify(r => r.AddAsync(It.IsAny<SurveyData>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_LinksToExistingProject_WhenProjectExists()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = projectId,
            ProjectName = "Existing Project"
        };

        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "Existing Project"
        };

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Test Survey"
        };

        _projectRepoMock.Setup(r => r.GetByNameAsync("Existing Project"))
            .ReturnsAsync(existingProject);
        _surveyRepoMock.Setup(r => r.AddAsync(It.IsAny<Survey>()))
            .ReturnsAsync((Survey s) => s);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey, project);

        // Assert
        result.ProjectId.Should().Be(projectId);
        _projectRepoMock.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_CreatesNewProject_WhenProjectDoesNotExist()
    {
        // Arrange
        var newProjectId = Guid.NewGuid();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = "New Project"
        };

        var savedProject = new Project
        {
            Id = newProjectId,
            ProjectName = "New Project"
        };

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Test Survey"
        };

        _projectRepoMock.Setup(r => r.GetByNameAsync("New Project"))
            .ReturnsAsync((Project?)null);
        _projectRepoMock.Setup(r => r.AddAsync(project))
            .ReturnsAsync(savedProject);
        _surveyRepoMock.Setup(r => r.AddAsync(It.IsAny<Survey>()))
            .ReturnsAsync((Survey s) => s);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey, project);

        // Assert
        result.ProjectId.Should().Be(newProjectId);
        _projectRepoMock.Verify(r => r.AddAsync(project), Times.Once);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_DoesNotProcessProject_WhenProjectIsNull()
    {
        // Arrange
        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Test Survey"
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey, null);

        // Assert
        _projectRepoMock.Verify(r => r.GetByNameAsync(It.IsAny<string>()), Times.Never);
        _projectRepoMock.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_DoesNotProcessProject_WhenProjectNameIsEmpty()
    {
        // Arrange
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = ""
        };

        var survey = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Test Survey"
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey, project);

        // Assert
        _projectRepoMock.Verify(r => r.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_CreatesQuestionBlocks_ForFirstQuestionsInBlocks()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var blockId = Guid.NewGuid();

#pragma warning disable CS0618 // Type or member is obsolete
        var question1 = new Question
        {
            Id = Guid.NewGuid(),
            QstLabel = "First Question",
            BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock,
            BlkLabel = "Block1",
            BlkStem = "Block 1 Stem"
        };

        var question2 = new Question
        {
            Id = Guid.NewGuid(),
            QstLabel = "Continuation Question",
            BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion
        };
#pragma warning restore CS0618 // Type or member is obsolete

        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = new ObjectListBase<Question> { question1, question2 }
        };

        var savedBlock = new QuestionBlock
        {
            Id = blockId,
            SurveyId = surveyId,
            Label = "Block1",
            Stem = "Block 1 Stem",
            DisplayOrder = 0
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);
        _questionBlockRepoMock.Setup(r => r.AddAsync(It.IsAny<QuestionBlock>()))
            .ReturnsAsync(savedBlock);
        _questionRepoMock.Setup(r => r.AddAsync(It.IsAny<Question>(), surveyId))
            .ReturnsAsync((Question q, Guid sId) => q);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        _questionBlockRepoMock.Verify(r => r.AddAsync(It.Is<QuestionBlock>(
            qb => qb.Label == "Block1" && qb.Stem == "Block 1 Stem")), Times.Once);
        question1.BlockId.Should().Be(blockId);
        question2.BlockId.Should().Be(blockId);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_AssignsBlockId_ToContinuationQuestions()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var blockId = Guid.NewGuid();

#pragma warning disable CS0618 // Type or member is obsolete
        var question1 = new Question
        {
            Id = Guid.NewGuid(),
            QstLabel = "First Question",
            BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock,
            BlkLabel = "Block1",
            BlkStem = "Block Stem"
        };

        var question2 = new Question
        {
            Id = Guid.NewGuid(),
            QstLabel = "Continuation Question 1",
            BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion
        };

        var question3 = new Question
        {
            Id = Guid.NewGuid(),
            QstLabel = "Continuation Question 2",
            BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion
        };
#pragma warning restore CS0618 // Type or member is obsolete

        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = new ObjectListBase<Question> { question1, question2, question3 }
        };

        var savedBlock = new QuestionBlock
        {
            Id = blockId,
            SurveyId = surveyId,
            Label = "Block1",
            Stem = "Block Stem",
            DisplayOrder = 0
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);
        _questionBlockRepoMock.Setup(r => r.AddAsync(It.IsAny<QuestionBlock>()))
            .ReturnsAsync(savedBlock);
        _questionRepoMock.Setup(r => r.AddAsync(It.IsAny<Question>(), surveyId))
            .ReturnsAsync((Question q, Guid sId) => q);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        question1.BlockId.Should().Be(blockId);
        question2.BlockId.Should().Be(blockId);
        question3.BlockId.Should().Be(blockId);
    }

    [Fact]
    public async Task SaveSurveyWithDetailsAsync_CreatesMultipleBlocks_ForMultipleFirstQuestions()
    {
        // Arrange
        var surveyId = Guid.NewGuid();

#pragma warning disable CS0618 // Type or member is obsolete
        var question1 = new Question
        {
            Id = Guid.NewGuid(),
            QstLabel = "First in Block 1",
            BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock,
            BlkLabel = "Block1",
            BlkStem = "Block 1 Stem"
        };

        var question2 = new Question
        {
            Id = Guid.NewGuid(),
            QstLabel = "First in Block 2",
            BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock,
            BlkLabel = "Block2",
            BlkStem = "Block 2 Stem"
        };
#pragma warning restore CS0618 // Type or member is obsolete

        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey",
            QuestionList = new ObjectListBase<Question> { question1, question2 }
        };

        _surveyRepoMock.Setup(r => r.AddAsync(survey))
            .ReturnsAsync(survey);
        _questionBlockRepoMock.Setup(r => r.AddAsync(It.IsAny<QuestionBlock>()))
            .ReturnsAsync((QuestionBlock qb) => new QuestionBlock
            {
                Id = Guid.NewGuid(),
                SurveyId = qb.SurveyId,
                Label = qb.Label,
                Stem = qb.Stem,
                DisplayOrder = qb.DisplayOrder
            });
        _questionRepoMock.Setup(r => r.AddAsync(It.IsAny<Question>(), surveyId))
            .ReturnsAsync((Question q, Guid sId) => q);

        // Act
        var result = await _service.SaveSurveyWithDetailsAsync(survey);

        // Assert
        _questionBlockRepoMock.Verify(r => r.AddAsync(It.Is<QuestionBlock>(
            qb => qb.Label == "Block1")), Times.Once);
        _questionBlockRepoMock.Verify(r => r.AddAsync(It.Is<QuestionBlock>(
            qb => qb.Label == "Block2")), Times.Once);
        question1.BlockId.Should().NotBeNull();
        question2.BlockId.Should().NotBeNull();
        question1.BlockId.Should().NotBe(question2.BlockId.Value);
    }

    #endregion

    #region SaveMultipleSurveysAsync Tests

    [Fact]
    public async Task SaveMultipleSurveysAsync_SavesAllSurveys()
    {
        // Arrange
        var surveys = new List<Survey>
        {
            new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 1" },
            new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 2" },
            new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 3" }
        };

        _surveyRepoMock.Setup(r => r.AddAsync(It.IsAny<Survey>()))
            .ReturnsAsync((Survey s) => s);

        // Act
        var result = await _service.SaveMultipleSurveysAsync(surveys);

        // Assert
        result.Should().HaveCount(3);
        _surveyRepoMock.Verify(r => r.AddAsync(It.IsAny<Survey>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SaveMultipleSurveysAsync_ContinuesOnError()
    {
        // Arrange
        var survey1 = new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 1" };
        var survey2 = new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 2" };
        var survey3 = new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 3" };
        var surveys = new List<Survey> { survey1, survey2, survey3 };

        _surveyRepoMock.SetupSequence(r => r.AddAsync(It.IsAny<Survey>()))
            .ReturnsAsync(survey1)
            .ThrowsAsync(new InvalidOperationException("Database error"))
            .ReturnsAsync(survey3);

        // Act
        var result = await _service.SaveMultipleSurveysAsync(surveys);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.SurveyName == "Survey 1");
        result.Should().Contain(s => s.SurveyName == "Survey 3");
        result.Should().NotContain(s => s.SurveyName == "Survey 2");
    }

    [Fact]
    public async Task SaveMultipleSurveysAsync_ReturnsEmptyList_WhenAllFail()
    {
        // Arrange
        var surveys = new List<Survey>
        {
            new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 1" },
            new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 2" }
        };

        _surveyRepoMock.Setup(r => r.AddAsync(It.IsAny<Survey>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _service.SaveMultipleSurveysAsync(surveys);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveMultipleSurveysAsync_PassesProjectToEachSurvey()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new Project
        {
            Id = projectId,
            ProjectName = "Test Project"
        };

        var surveys = new List<Survey>
        {
            new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 1" },
            new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 2" }
        };

        _projectRepoMock.Setup(r => r.GetByNameAsync("Test Project"))
            .ReturnsAsync(project);
        _surveyRepoMock.Setup(r => r.AddAsync(It.IsAny<Survey>()))
            .ReturnsAsync((Survey s) => s);

        // Act
        var result = await _service.SaveMultipleSurveysAsync(surveys, project);

        // Assert
        result.Should().HaveCount(2);
        result.All(s => s.ProjectId == projectId).Should().BeTrue();
    }

    #endregion

    #region SurveyExistsAsync Tests

    [Fact]
    public async Task SurveyExistsAsync_ReturnsTrue_WhenSurveyExists()
    {
        // Arrange
        _surveyRepoMock.Setup(r => r.SurveyNameExistsAsync("Existing Survey"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.SurveyExistsAsync("Existing Survey");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SurveyExistsAsync_ReturnsFalse_WhenSurveyDoesNotExist()
    {
        // Arrange
        _surveyRepoMock.Setup(r => r.SurveyNameExistsAsync("Nonexistent Survey"))
            .ReturnsAsync(false);

        // Act
        var result = await _service.SurveyExistsAsync("Nonexistent Survey");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetImportStatsAsync Tests

    [Fact]
    public async Task GetImportStatsAsync_ReturnsStats_WhenSurveyExists()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var survey = new Survey
        {
            Id = surveyId,
            SurveyName = "Test Survey"
        };

        _surveyRepoMock.Setup(r => r.GetByIdAsync(surveyId))
            .ReturnsAsync(survey);
        _surveyRepoMock.Setup(r => r.GetQuestionCountAsync(surveyId))
            .ReturnsAsync(10);
        _surveyRepoMock.Setup(r => r.GetResponseCountAsync(surveyId))
            .ReturnsAsync(100);

        // Act
        var result = await _service.GetImportStatsAsync(surveyId);

        // Assert
        result.Should().NotBeNull();
        result.SurveyId.Should().Be(surveyId);
        result.SurveyName.Should().Be("Test Survey");
        result.QuestionCount.Should().Be(10);
        result.ResponseCount.Should().Be(100);
        result.ImportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetImportStatsAsync_ThrowsArgumentException_WhenSurveyNotFound()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        _surveyRepoMock.Setup(r => r.GetByIdAsync(surveyId))
            .ReturnsAsync((Survey?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.GetImportStatsAsync(surveyId)
        );
    }

    #endregion
}
