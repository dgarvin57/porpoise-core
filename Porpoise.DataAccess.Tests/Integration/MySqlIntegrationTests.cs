#nullable enable

using FluentAssertions;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.DataAccess.Repositories;

namespace Porpoise.DataAccess.Tests.Integration;

/// <summary>
/// Integration tests using real MySQL database.
/// These tests require MySQL to be running with porpoise_dev database.
/// Creates and cleans up its own test tenant automatically.
/// </summary>
[Collection("Database")]
public class MySqlIntegrationTests : IntegrationTestBase
{
    private readonly SurveyRepository _repository;
    private readonly TenantContext _tenantContext;
    private readonly List<Guid> _createdSurveyIds = new();

    public MySqlIntegrationTests(DatabaseFixture fixture) : base(fixture)
    {
        _tenantContext = new TenantContext { TenantId = TestTenantId, TenantKey = TestTenantKey };
        _repository = new SurveyRepository(Context, _tenantContext);
    }

    public override Task InitializeAsync() => Task.CompletedTask;

    public override async Task DisposeAsync()
    {
        // Clean up any surveys created during tests
        foreach (var surveyId in _createdSurveyIds)
        {
            try
            {
                await _repository.DeleteAsync(surveyId);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task AddAsync_ShouldPersistSurveyToDatabase()
    {
        // Arrange
        var survey = new Survey
        {
            SurveyName = $"Integration Test Survey {Guid.NewGuid()}",
            Status = SurveyStatus.Initial,
            SurveyNotes = "Created by integration test"
        };

        // Act
        var result = await _repository.AddAsync(survey);
        _createdSurveyIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        
        // Verify it was persisted
        var retrieved = await _repository.GetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
        retrieved!.SurveyName.Should().Be(survey.SurveyName);
        retrieved.SurveyNotes.Should().Be("Created by integration test");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldRetrieveSurveyFromDatabase()
    {
        // Arrange
        var survey = new Survey
        {
            SurveyName = $"GetById Test {Guid.NewGuid()}",
            Status = SurveyStatus.Verified,
            SurveyNotes = "Test retrieval"
        };
        var added = await _repository.AddAsync(survey);
        _createdSurveyIds.Add(added.Id);

        // Act
        var result = await _repository.GetByIdAsync(added.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(added.Id);
        result.SurveyName.Should().Be(survey.SurveyName);
        result.Status.Should().Be(SurveyStatus.Verified);
        result.SurveyNotes.Should().Be("Test retrieval");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSurveyInDatabase()
    {
        // Arrange
        var survey = new Survey
        {
            SurveyName = $"Update Test {Guid.NewGuid()}",
            Status = SurveyStatus.Initial,
            SurveyNotes = "Original notes"
        };
        var added = await _repository.AddAsync(survey);
        _createdSurveyIds.Add(added.Id);

        // Act
        added.SurveyName = "Updated Survey Name";
        added.Status = SurveyStatus.Verified;
        added.SurveyNotes = "Updated notes";
        await _repository.UpdateAsync(added);

        // Assert
        var retrieved = await _repository.GetByIdAsync(added.Id);
        retrieved.Should().NotBeNull();
        retrieved!.SurveyName.Should().Be("Updated Survey Name");
        retrieved.Status.Should().Be(SurveyStatus.Verified);
        retrieved.SurveyNotes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSurveyFromDatabase()
    {
        // Arrange
        var survey = new Survey
        {
            SurveyName = $"Delete Test {Guid.NewGuid()}",
            Status = SurveyStatus.Initial
        };
        var added = await _repository.AddAsync(survey);

        // Act
        await _repository.DeleteAsync(added.Id);

        // Assert
        var retrieved = await _repository.GetByIdAsync(added.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldFindSurveyByExactName()
    {
        // Arrange
        var uniqueName = $"Exact Name Test {Guid.NewGuid()}";
        var survey = new Survey
        {
            SurveyName = uniqueName,
            Status = SurveyStatus.Initial
        };
        var added = await _repository.AddAsync(survey);
        _createdSurveyIds.Add(added.Id);

        // Act
        var result = await _repository.GetByNameAsync(uniqueName);

        // Assert
        result.Should().NotBeNull();
        result!.SurveyName.Should().Be(uniqueName);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnSurveysWithMatchingStatus()
    {
        // Arrange
        var prefix = Guid.NewGuid().ToString().Substring(0, 8);
        var survey1 = await _repository.AddAsync(new Survey 
        { 
            SurveyName = $"{prefix} Verified 1", 
            Status = SurveyStatus.Verified 
        });
        var survey2 = await _repository.AddAsync(new Survey 
        { 
            SurveyName = $"{prefix} Verified 2", 
            Status = SurveyStatus.Verified 
        });
        var survey3 = await _repository.AddAsync(new Survey 
        { 
            SurveyName = $"{prefix} Initial", 
            Status = SurveyStatus.Initial 
        });
        
        _createdSurveyIds.AddRange(new[] { survey1.Id, survey2.Id, survey3.Id });

        // Act
        var results = await _repository.GetByStatusAsync(SurveyStatus.Verified);

        // Assert
        var verifiedSurveys = results.Where(s => s.SurveyName.StartsWith(prefix)).ToList();
        verifiedSurveys.Should().HaveCount(2);
        verifiedSurveys.Should().AllSatisfy(s => s.Status.Should().Be(SurveyStatus.Verified));
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldFindSurveysContainingSearchTerm()
    {
        // Arrange
        var searchTerm = $"SearchTest{Guid.NewGuid().ToString().Substring(0, 8)}";
        var survey1 = await _repository.AddAsync(new Survey 
        { 
            SurveyName = $"Before {searchTerm} After",
            Status = SurveyStatus.Initial 
        });
        var survey2 = await _repository.AddAsync(new Survey 
        { 
            SurveyName = $"{searchTerm} at start",
            Status = SurveyStatus.Initial 
        });
        var survey3 = await _repository.AddAsync(new Survey 
        { 
            SurveyName = "Should not match",
            Status = SurveyStatus.Initial 
        });
        
        _createdSurveyIds.AddRange(new[] { survey1.Id, survey2.Id, survey3.Id });

        // Act
        var results = await _repository.SearchByNameAsync(searchTerm);

        // Assert
        var matchedSurveys = results.ToList();
        matchedSurveys.Should().HaveCountGreaterThanOrEqualTo(2);
        matchedSurveys.Should().Contain(s => s.Id == survey1.Id);
        matchedSurveys.Should().Contain(s => s.Id == survey2.Id);
    }

    [Fact]
    public async Task SurveyNameExistsAsync_ShouldReturnTrue_WhenNameExists()
    {
        // Arrange
        var uniqueName = $"Unique Name {Guid.NewGuid()}";
        var survey = await _repository.AddAsync(new Survey 
        { 
            SurveyName = uniqueName,
            Status = SurveyStatus.Initial 
        });
        _createdSurveyIds.Add(survey.Id);

        // Act
        var exists = await _repository.SurveyNameExistsAsync(uniqueName);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task SurveyNameExistsAsync_ShouldReturnFalse_WhenNameDoesNotExist()
    {
        // Act
        var exists = await _repository.SurveyNameExistsAsync($"NonExistent{Guid.NewGuid()}");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SurveyNameExistsAsync_ShouldExcludeSurveyId_WhenProvided()
    {
        // Arrange
        var uniqueName = $"Exclude Test {Guid.NewGuid()}";
        var survey = await _repository.AddAsync(new Survey 
        { 
            SurveyName = uniqueName,
            Status = SurveyStatus.Initial 
        });
        _createdSurveyIds.Add(survey.Id);

        // Act - Check if name exists excluding this survey's ID
        var exists = await _repository.SurveyNameExistsAsync(uniqueName, survey.Id);

        // Assert
        exists.Should().BeFalse(); // Should return false because we excluded the only matching survey
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSurveys()
    {
        // Arrange
        var prefix = $"GetAll{Guid.NewGuid().ToString().Substring(0, 8)}";
        var survey1 = await _repository.AddAsync(new Survey { SurveyName = $"{prefix}_1", Status = SurveyStatus.Initial });
        var survey2 = await _repository.AddAsync(new Survey { SurveyName = $"{prefix}_2", Status = SurveyStatus.Initial });
        var survey3 = await _repository.AddAsync(new Survey { SurveyName = $"{prefix}_3", Status = SurveyStatus.Initial });
        _createdSurveyIds.AddRange(new[] { survey1.Id, survey2.Id, survey3.Id });

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        var ourSurveys = results.Where(s => s.SurveyName.StartsWith(prefix)).ToList();
        ourSurveys.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnSurveysInRange()
    {
        // Arrange
        var prefix = $"DateRange{Guid.NewGuid().ToString().Substring(0, 8)}";
        var now = DateTime.UtcNow;
        
        // Create surveys (they'll have CreatedDate set to current time)
        var survey1 = await _repository.AddAsync(new Survey { SurveyName = $"{prefix}_1", Status = SurveyStatus.Initial });
        _createdSurveyIds.Add(survey1.Id);
        
        await Task.Delay(100); // Small delay to ensure different timestamps
        
        var survey2 = await _repository.AddAsync(new Survey { SurveyName = $"{prefix}_2", Status = SurveyStatus.Initial });
        _createdSurveyIds.Add(survey2.Id);

        // Act - Query for surveys created in the last minute
        var startDate = now.AddMinutes(-1);
        var endDate = now.AddMinutes(1);
        var results = await _repository.GetByDateRangeAsync(startDate, endDate);

        // Assert
        var ourSurveys = results.Where(s => s.SurveyName.StartsWith(prefix)).ToList();
        ourSurveys.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetQuestionCountAsync_ShouldReturnZero_WhenNoQuestions()
    {
        // Arrange
        var survey = await _repository.AddAsync(new Survey 
        { 
            SurveyName = $"No Questions {Guid.NewGuid()}",
            Status = SurveyStatus.Initial 
        });
        _createdSurveyIds.Add(survey.Id);

        // Act
        var count = await _repository.GetQuestionCountAsync(survey.Id);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetResponseCountAsync_ShouldReturnZero_WhenNoResponses()
    {
        // Arrange
        var survey = await _repository.AddAsync(new Survey 
        { 
            SurveyName = $"No Responses {Guid.NewGuid()}",
            Status = SurveyStatus.Initial 
        });
        _createdSurveyIds.Add(survey.Id);

        // Act
        var count = await _repository.GetResponseCountAsync(survey.Id);

        // Assert
        count.Should().Be(0);
    }
}
