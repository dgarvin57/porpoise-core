#nullable enable

using FluentAssertions;
using Porpoise.Core.Models;
using Porpoise.DataAccess.Tests.Mocks;

namespace Porpoise.DataAccess.Tests;

/// <summary>
/// Unit tests for SurveyRepository using in-memory implementation.
/// Tests all CRUD operations and query methods without needing a database.
/// </summary>
public class SurveyRepositoryTests
{
    private readonly InMemorySurveyRepository _repository;

    public SurveyRepositoryTests()
    {
        _repository = new InMemorySurveyRepository();
    }

    [Fact]
    public async Task AddAsync_ShouldAddSurvey_AndGenerateId()
    {
        // Arrange
        var survey = new Survey
        {
            SurveyName = "Test Survey",
            Status = SurveyStatus.Initial
        };

        // Act
        var result = await _repository.AddAsync(survey);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.SurveyName.Should().Be("Test Survey");
        _repository.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSurvey_WhenExists()
    {
        // Arrange
        var survey = new Survey { SurveyName = "Test Survey" };
        await _repository.AddAsync(survey);

        // Act
        var result = await _repository.GetByIdAsync(survey.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(survey.Id);
        result.SurveyName.Should().Be("Test Survey");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSurveys()
    {
        // Arrange
        await _repository.AddAsync(new Survey { SurveyName = "Survey 1" });
        await _repository.AddAsync(new Survey { SurveyName = "Survey 2" });
        await _repository.AddAsync(new Survey { SurveyName = "Survey 3" });

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(3);
        results.Select(s => s.SurveyName).Should().Contain(new[] { "Survey 1", "Survey 2", "Survey 3" });
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingSurvey()
    {
        // Arrange
        var survey = new Survey { SurveyName = "Original Name" };
        await _repository.AddAsync(survey);
        
        survey.SurveyName = "Updated Name";
        survey.Status = SurveyStatus.Verified;

        // Act
        await _repository.UpdateAsync(survey);
        var result = await _repository.GetByIdAsync(survey.Id);

        // Assert
        result.Should().NotBeNull();
        result!.SurveyName.Should().Be("Updated Name");
        result.Status.Should().Be(SurveyStatus.Verified);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSurvey_WhenExists()
    {
        // Arrange
        var survey = new Survey { SurveyName = "Test Survey" };
        await _repository.AddAsync(survey);

        // Act
        var result = await _repository.DeleteAsync(survey.Id);
        var deleted = await _repository.GetByIdAsync(survey.Id);

        // Assert
        result.Should().BeTrue();
        deleted.Should().BeNull();
        _repository.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenSurveyExists()
    {
        // Arrange
        var survey = new Survey { SurveyName = "Test Survey" };
        await _repository.AddAsync(survey);

        // Act
        var result = await _repository.ExistsAsync(survey.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenSurveyNotExists()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnSurvey_WhenNameMatches()
    {
        // Arrange
        await _repository.AddAsync(new Survey { SurveyName = "Demo Survey" });

        // Act
        var result = await _repository.GetByNameAsync("Demo Survey");

        // Assert
        result.Should().NotBeNull();
        result!.SurveyName.Should().Be("Demo Survey");
    }

    [Fact]
    public async Task GetByNameAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        await _repository.AddAsync(new Survey { SurveyName = "Demo Survey" });

        // Act
        var result = await _repository.GetByNameAsync("demo survey");

        // Assert
        result.Should().NotBeNull();
        result!.SurveyName.Should().Be("Demo Survey");
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnSurveysWithMatchingStatus()
    {
        // Arrange
        await _repository.AddAsync(new Survey { SurveyName = "Survey 1", Status = SurveyStatus.Initial });
        await _repository.AddAsync(new Survey { SurveyName = "Survey 2", Status = SurveyStatus.Verified });
        await _repository.AddAsync(new Survey { SurveyName = "Survey 3", Status = SurveyStatus.Initial });

        // Act
        var results = await _repository.GetByStatusAsync(SurveyStatus.Initial);

        // Assert
        results.Should().HaveCount(2);
        results.Select(s => s.SurveyName).Should().Contain(new[] { "Survey 1", "Survey 3" });
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnMatchingSurveys()
    {
        // Arrange
        await _repository.AddAsync(new Survey { SurveyName = "Customer Satisfaction 2024" });
        await _repository.AddAsync(new Survey { SurveyName = "Employee Satisfaction 2024" });
        await _repository.AddAsync(new Survey { SurveyName = "Product Feedback" });

        // Act
        var results = await _repository.SearchByNameAsync("Satisfaction");

        // Assert
        results.Should().HaveCount(2);
        results.All(s => s.SurveyName.Contains("Satisfaction")).Should().BeTrue();
    }

    [Fact]
    public async Task SearchByNameAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        await _repository.AddAsync(new Survey { SurveyName = "Customer Satisfaction" });

        // Act
        var results = await _repository.SearchByNameAsync("satisfaction");

        // Assert
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task SurveyNameExistsAsync_ShouldReturnTrue_WhenNameExists()
    {
        // Arrange
        await _repository.AddAsync(new Survey { SurveyName = "Test Survey" });

        // Act
        var result = await _repository.SurveyNameExistsAsync("Test Survey");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SurveyNameExistsAsync_ShouldReturnFalse_WhenNameNotExists()
    {
        // Act
        var result = await _repository.SurveyNameExistsAsync("Non-existent Survey");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SurveyNameExistsAsync_ShouldExcludeSpecificSurvey()
    {
        // Arrange
        var survey = new Survey { SurveyName = "Test Survey" };
        await _repository.AddAsync(survey);

        // Act - checking if name exists but excluding this specific survey
        var result = await _repository.SurveyNameExistsAsync("Test Survey", survey.Id);

        // Assert
        result.Should().BeFalse(); // Should not find it because we're excluding this survey
    }

    [Fact]
    public async Task GetQuestionCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var survey = new Survey 
        { 
            SurveyName = "Test Survey",
            QuestionList = new ObjectListBase<Question>
            {
                new Question { QstLabel = "Q1" },
                new Question { QstLabel = "Q2" },
                new Question { QstLabel = "Q3" }
            }
        };
        await _repository.AddAsync(survey);

        // Act
        var count = await _repository.GetQuestionCountAsync(survey.Id);

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task GetResponseCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var survey = new Survey 
        { 
            SurveyName = "Test Survey",
            Data = new SurveyData(new List<List<string>>
            {
                new() { "Header" },
                new() { "Row1" },
                new() { "Row2" }
            })
        };
        await _repository.AddAsync(survey);

        // Act
        var count = await _repository.GetResponseCountAsync(survey.Id);

        // Assert
        count.Should().Be(2); // ResponsesNumber = DataList.Count - 1 (header row)
    }
}
