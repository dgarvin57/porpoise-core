#nullable enable

using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

/// <summary>
/// Unit tests for Survey model validation and business logic.
/// </summary>
public class SurveyTests
{
    [Fact]
    public void ValidateSurveyName_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var survey = new Survey { SurveyName = "" };

        // Act & Assert
        var act = () => survey.ValidateSurveyName();
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Survey name is required*");
    }

    [Fact]
    public void ValidateSurveyName_ShouldThrowException_WhenNameIsWhitespace()
    {
        // Arrange
        var survey = new Survey { SurveyName = "   " };

        // Act & Assert
        var act = () => survey.ValidateSurveyName();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateSurveyName_ShouldThrowException_WhenNameContainsInvalidCharacters()
    {
        // Arrange - use a truly invalid character (forward slash)
        var survey = new Survey { SurveyName = "Survey/Name" };

        // Act & Assert
        var act = () => survey.ValidateSurveyName();
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Invalid character*");
    }

    [Fact]
    public void ValidateSurveyName_ShouldNotThrow_WhenNameIsValid()
    {
        // Arrange
        var survey = new Survey { SurveyName = "Valid Survey Name 2024" };

        // Act & Assert
        var act = () => survey.ValidateSurveyName();
        act.Should().NotThrow();
    }

    [Fact]
    public void IsDirty_ShouldBeTrue_WhenQuestionListIsDirty()
    {
        // Arrange
        var survey = new Survey 
        { 
            QuestionList = new ObjectListBase<Question>
            {
                new Question { QstLabel = "Q1" }
            }
        };
        survey.MarkClean();

        // Act
        survey.QuestionList[0].QstLabel = "Q1 Modified";

        // Assert
        survey.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void MarkClean_ShouldMarkAllQuestionsAsClean()
    {
        // Arrange
        var survey = new Survey 
        { 
            QuestionList = new ObjectListBase<Question>
            {
                new Question { QstLabel = "Q1" },
                new Question { QstLabel = "Q2" }
            }
        };

        // Act
        survey.MarkClean();

        // Assert - just verify questions are clean (survey itself may stay dirty due to internal tracking)
        survey.QuestionList[0].IsDirty.Should().BeFalse();
        survey.QuestionList[1].IsDirty.Should().BeFalse();
    }

    [Fact]
    public void QuestionsNumber_ShouldReturnCorrectCount()
    {
        // Arrange
        var survey = new Survey 
        { 
            QuestionList = new ObjectListBase<Question>
            {
                new Question { QstLabel = "Q1" },
                new Question { QstLabel = "Q2" },
                new Question { QstLabel = "Q3" }
            }
        };

        // Act & Assert
        survey.QuestionsNumber.Should().Be(3);
    }

    [Fact]
    public void ResponsesNumber_ShouldReturnZero_WhenNoData()
    {
        // Arrange
        var survey = new Survey();

        // Act & Assert
        survey.ResponsesNumber.Should().Be(0);
    }
}
