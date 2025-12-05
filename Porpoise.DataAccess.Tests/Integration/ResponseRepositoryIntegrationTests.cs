#nullable enable

using Dapper;
using FluentAssertions;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.DataAccess.Repositories;

namespace Porpoise.DataAccess.Tests.Integration;

/// <summary>
/// Integration tests for ResponseRepository using real MySQL database.
/// Tests all CRUD operations and query methods with actual database persistence.
/// Creates and cleans up its own test tenant automatically.
/// </summary>
[Collection("Database")]
public class ResponseRepositoryIntegrationTests : IntegrationTestBase
{
    private readonly SurveyRepository _surveyRepository;
    private readonly ResponseRepository _responseRepository;
    private readonly TenantContext _tenantContext;
    private readonly List<Guid> _createdSurveyIds = new();
    private readonly List<Guid> _createdQuestionIds = new();
    private readonly List<Guid> _createdResponseIds = new();

    public ResponseRepositoryIntegrationTests(DatabaseFixture fixture) : base(fixture)
    {
        _tenantContext = new TenantContext { TenantId = TestTenantId, TenantKey = TestTenantKey };
        _surveyRepository = new SurveyRepository(Context, _tenantContext);
        _responseRepository = new ResponseRepository(Context);
    }

    public override Task InitializeAsync() => Task.CompletedTask;

    public override async Task DisposeAsync()
    {
        // Clean up in reverse order due to foreign keys
        foreach (var responseId in _createdResponseIds)
        {
            try { await _responseRepository.DeleteAsync(responseId); }
            catch { /* Ignore cleanup errors */ }
        }
        
        foreach (var questionId in _createdQuestionIds)
        {
            try { await DeleteQuestionAsync(questionId); }
            catch { /* Ignore cleanup errors */ }
        }
        
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

    private async Task<Question> CreateQuestionForSurvey(Guid surveyId, string qstNumber, string qstLabel)
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            QstNumber = qstNumber,
            QstLabel = qstLabel,
            DataFileCol = 1,
            VariableType = QuestionVariableType.Dependent
        };

        using var connection = Context.CreateConnection();
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
                VariableType = (int)question.VariableType,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            });

        _createdQuestionIds.Add(question.Id);
        return question;
    }

    private async Task<Response> CreateResponseForQuestion(Guid questionId, int respValue, string label, 
        ResponseIndexType indexType = ResponseIndexType.None)
    {
        var response = new Response
        {
            Id = Guid.NewGuid(),
            RespValue = respValue,
            Label = label,
            IndexType = indexType
        };

        using var connection = Context.CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO Responses (Id, QuestionId, RespValue, Label, IndexType, CreatedDate, ModifiedDate)
            VALUES (@Id, @QuestionId, @RespValue, @Label, @IndexType, @CreatedDate, @ModifiedDate)",
            new
            {
                response.Id,
                QuestionId = questionId,
                response.RespValue,
                response.Label,
                IndexType = response.IndexType.ToString(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            });

        _createdResponseIds.Add(response.Id);
        return response;
    }

    private async Task DeleteQuestionAsync(Guid questionId)
    {
        using var connection = Context.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Questions WHERE Id = @Id", new { Id = questionId });
    }

    [Fact]
    public async Task AddAsync_ShouldPersistResponseToDatabase()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Response {Guid.NewGuid()}");
        var question = await CreateQuestionForSurvey(survey.Id, "Q1", "Test Question");
        var response = await CreateResponseForQuestion(question.Id, 1, "Yes");

        // Assert
        var retrieved = await _responseRepository.GetByIdAsync(response.Id);
        retrieved.Should().NotBeNull();
        retrieved!.RespValue.Should().Be(1);
        retrieved.Label.Should().Be("Yes");
    }

    [Fact]
    public async Task GetByQuestionIdAsync_ShouldReturnAllResponsesForQuestion()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Multiple Responses {Guid.NewGuid()}");
        var question = await CreateQuestionForSurvey(survey.Id, "Q1", "Rating Question");
        
        await CreateResponseForQuestion(question.Id, 1, "Poor");
        await CreateResponseForQuestion(question.Id, 2, "Fair");
        await CreateResponseForQuestion(question.Id, 3, "Good");
        await CreateResponseForQuestion(question.Id, 4, "Excellent");

        // Act
        var results = await _responseRepository.GetByQuestionIdAsync(question.Id);

        // Assert
        results.Should().HaveCount(4);
        results.Select(r => r.RespValue).Should().ContainInOrder(1, 2, 3, 4);
    }

    [Fact]
    public async Task GetByResponseValueAsync_ShouldFindSpecificResponse()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Response Value {Guid.NewGuid()}");
        var question = await CreateQuestionForSurvey(survey.Id, "Q1", "Yes/No Question");
        
        await CreateResponseForQuestion(question.Id, 1, "Yes");
        await CreateResponseForQuestion(question.Id, 2, "No");

        // Act
        var result = await _responseRepository.GetByResponseValueAsync(question.Id, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Label.Should().Be("Yes");
    }

    [Fact]
    public async Task GetByIndexTypeAsync_ShouldFilterByIndexType()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Index Type {Guid.NewGuid()}");
        var question = await CreateQuestionForSurvey(survey.Id, "Q1", "Mixed Index Question");
        
        await CreateResponseForQuestion(question.Id, 1, "Positive", ResponseIndexType.Positive);
        await CreateResponseForQuestion(question.Id, 2, "Negative", ResponseIndexType.Negative);
        await CreateResponseForQuestion(question.Id, 3, "Another Positive", ResponseIndexType.Positive);

        // Act
        var results = await _responseRepository.GetByIndexTypeAsync(question.Id, ResponseIndexType.Positive);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r => r.IndexType.Should().Be(ResponseIndexType.Positive));
    }

    [Fact]
    public async Task DeleteByQuestionIdAsync_ShouldRemoveAllResponses()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Delete {Guid.NewGuid()}");
        var question = await CreateQuestionForSurvey(survey.Id, "Q1", "Question for Deletion");
        
        await CreateResponseForQuestion(question.Id, 1, "Response 1");
        await CreateResponseForQuestion(question.Id, 2, "Response 2");

        // Act
        await _responseRepository.DeleteByQuestionIdAsync(question.Id);

        // Assert
        var results = await _responseRepository.GetByQuestionIdAsync(question.Id);
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyResponseInDatabase()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Update {Guid.NewGuid()}");
        var question = await CreateQuestionForSurvey(survey.Id, "Q1", "Update Question");
        var response = await CreateResponseForQuestion(question.Id, 1, "Original Label");

        // Act
        response.Label = "Updated Label";
        response.RespValue = 2;
        await _responseRepository.UpdateAsync(response);

        // Assert
        var retrieved = await _responseRepository.GetByIdAsync(response.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Label.Should().Be("Updated Label");
        retrieved.RespValue.Should().Be(2);
    }

    [Fact]
    public async Task CascadeDelete_ShouldRemoveResponsesWhenQuestionDeleted()
    {
        // Arrange
        var survey = await CreateTestSurveyAsync($"Survey for Cascade {Guid.NewGuid()}");
        var question = await CreateQuestionForSurvey(survey.Id, "Q1", "Cascade Question");
        
        var response = await CreateResponseForQuestion(question.Id, 1, "Will be deleted");

        // Act - Delete the question (should cascade to responses)
        await DeleteQuestionAsync(question.Id);

        // Assert
        var retrieved = await _responseRepository.GetByIdAsync(response.Id);
        retrieved.Should().BeNull();
    }
}
