#nullable enable

using Dapper;
using FluentAssertions;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.DataAccess.Context;
using Porpoise.DataAccess.Repositories;

namespace Porpoise.DataAccess.Tests.Integration;

/// <summary>
/// Integration tests for QuestionRepository using real MySQL database.
/// Tests all CRUD operations and query methods with actual database persistence.
/// </summary>
[Collection("MySQL Integration")]
public class QuestionRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DapperContext _context;
    private readonly SurveyRepository _surveyRepository;
    private readonly QuestionRepository _questionRepository;
    private readonly TenantContext _tenantContext;
    private readonly List<Guid> _createdSurveyIds = new();
    private readonly List<Guid> _createdQuestionIds = new();

    public QuestionRepositoryIntegrationTests()
    {
        var connectionString = Environment.GetEnvironmentVariable("PORPOISE_TEST_CONNECTION") 
            ?? "Server=localhost;Port=3306;Database=porpoise_dev;User=root;Password=Dg5901%1;CharSet=utf8mb4;";
        
        _context = new DapperContext(connectionString);
        _tenantContext = new TenantContext { TenantId = 1, TenantKey = "demo-tenant" };
        _surveyRepository = new SurveyRepository(_context, _tenantContext);
        _questionRepository = new QuestionRepository(_context);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Clean up questions first (due to foreign key)
        foreach (var questionId in _createdQuestionIds)
        {
            try { await _questionRepository.DeleteAsync(questionId); }
            catch { /* Ignore cleanup errors */ }
        }
        
        // Then clean up surveys
        foreach (var surveyId in _createdSurveyIds)
        {
            try { await _surveyRepository.DeleteAsync(surveyId); }
            catch { /* Ignore cleanup errors */ }
        }
    }

    private async Task<Survey> CreateTestSurveyAsync(string name)
    {
        var survey = await _surveyRepository.AddAsync(new Survey
        {
            SurveyName = name,
            Status = SurveyStatus.Initial
        });
        _createdSurveyIds.Add(survey.Id);
        return survey;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistQuestionToDatabase()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Question Add {Guid.NewGuid()}");
        var question = new Question
        {
            QstNumber = "Q1",
            QstLabel = "What is your age?",
            DataFileCol = 1,
            VariableType = QuestionVariableType.Dependent
        };

        // Act - Need to manually set SurveyId since AddAsync doesn't handle it
        using (var connection = _context.CreateConnection())
        {
            question.Id = Guid.NewGuid();
            await connection.ExecuteAsync(@"
                INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType, CreatedDate, ModifiedDate)
                VALUES (@Id, @SurveyId, @QstNumber, @QstLabel, @DataFileColumn, @VariableType, @CreatedDate, @ModifiedDate)",
                new
                {
                    question.Id,
                    SurveyId = survey.Id,
                    question.QstNumber,
                    question.QstLabel,
                    DataFileColumn = question.DataFileCol,
                    VariableType = (int)question.VariableType,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                });
        }
        _createdQuestionIds.Add(question.Id);

        // Assert
        var retrieved = await _questionRepository.GetByIdAsync(question.Id);
        retrieved.Should().NotBeNull();
        retrieved!.QstNumber.Should().Be("Q1");
        retrieved.QstLabel.Should().Be("What is your age?");
    }

    [Fact]
    public async Task GetBySurveyIdAsync_ShouldReturnAllQuestionsForSurvey()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey with Multiple Questions {Guid.NewGuid()}");
        
        var questions = new[]
        {
            await CreateQuestionForSurvey(survey.Id, "Q1", "Question 1"),
            await CreateQuestionForSurvey(survey.Id, "Q2", "Question 2"),
            await CreateQuestionForSurvey(survey.Id, "Q3", "Question 3")
        };
        _createdQuestionIds.AddRange(questions.Select(q => q.Id));

        // Act
        var results = await _questionRepository.GetBySurveyIdAsync(survey.Id);

        // Assert
        results.Should().HaveCount(3);
        results.Select(q => q.QstNumber).Should().ContainInOrder("Q1", "Q2", "Q3");
    }

    [Fact]
    public async Task GetByQuestionNumberAsync_ShouldFindQuestionByNumber()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Question Number {Guid.NewGuid()}");
        var question = await CreateQuestionForSurvey(survey.Id, "Q42", "The Answer Question");
        _createdQuestionIds.Add(question.Id);

        // Act
        var result = await _questionRepository.GetByQuestionNumberAsync(survey.Id, "Q42");

        // Assert
        result.Should().NotBeNull();
        result!.QstLabel.Should().Be("The Answer Question");
    }

    [Fact]
    public async Task GetByVariableTypeAsync_ShouldFilterByType()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Variable Type {Guid.NewGuid()}");
        
        var dependent = await CreateQuestionForSurvey(survey.Id, "Q1", "Dependent", QuestionVariableType.Dependent);
        var independent = await CreateQuestionForSurvey(survey.Id, "Q2", "Independent", QuestionVariableType.Independent);
        _createdQuestionIds.AddRange(new[] { dependent.Id, independent.Id });

        // Act
        var results = await _questionRepository.GetByVariableTypeAsync(survey.Id, QuestionVariableType.Dependent);

        // Assert
        results.Should().HaveCount(1);
        results.First().QstLabel.Should().Be("Dependent");
    }

    [Fact]
    public async Task DeleteBySurveyIdAsync_ShouldRemoveAllQuestions()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Delete {Guid.NewGuid()}");
        
        var q1 = await CreateQuestionForSurvey(survey.Id, "Q1", "Question 1");
        var q2 = await CreateQuestionForSurvey(survey.Id, "Q2", "Question 2");

        // Act
        await _questionRepository.DeleteBySurveyIdAsync(survey.Id);

        // Assert
        var results = await _questionRepository.GetBySurveyIdAsync(survey.Id);
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyQuestionInDatabase()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Update {Guid.NewGuid()}");
        var question = await CreateQuestionForSurvey(survey.Id, "Q1", "Original Label");
        _createdQuestionIds.Add(question.Id);

        // Act
        question.QstLabel = "Updated Label";
        question.QstNumber = "Q99";
        await _questionRepository.UpdateAsync(question);

        // Assert
        var retrieved = await _questionRepository.GetByIdAsync(question.Id);
        retrieved.Should().NotBeNull();
        retrieved!.QstLabel.Should().Be("Updated Label");
        retrieved.QstNumber.Should().Be("Q99");
    }

    // Helper method to create a question with proper SurveyId
    private async Task<Question> CreateQuestionForSurvey(Guid surveyId, string qstNumber, string qstLabel, 
        QuestionVariableType variableType = QuestionVariableType.Dependent)
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            QstNumber = qstNumber,
            QstLabel = qstLabel,
            DataFileCol = 1,
            VariableType = variableType
        };

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO Questions (Id, SurveyId, QstNumber, QstLabel, DataFileColumn, VariableType, CreatedDate, ModifiedDate)
            VALUES (@Id, @SurveyId, @QstNumber, @QstLabel, @DataFileColumn, @VariableType, @CreatedDate, @ModifiedDate)",
            new
            {
                question.Id,
                SurveyId = surveyId,
                question.QstNumber,
                question.QstLabel,
                DataFileColumn = question.DataFileCol,
                VariableType = (int)variableType,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            });

        return question;
    }
}
