using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class SurveyValidationTests
{
    [Fact]
    public void GetAllResponsesForQuestion_ShouldReturnResponses_WhenQuestionExists()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { Id = Guid.NewGuid(), QstNumber = "Q1", DataFileCol = 1 },
                new Question { Id = Guid.NewGuid(), QstNumber = "Q2", DataFileCol = 2, MissValue1 = "99" }
            ],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q1", "Q2"],
                    ["1", "1", "2"],
                    ["2", "3", "99"],
                    ["3", "5", "6"]
                ]
            }
        };

        // Act
        var responses = survey.GetAllResponsesForQuestion(survey.QuestionList[1], omitMissingValues: false);

        // Assert
        responses.Should().BeEquivalentTo([2, 99, 6]);
    }

    [Fact]
    public void GetAllResponsesForQuestion_ShouldOmitMissingValues_WhenRequested()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { Id = Guid.NewGuid(), QstNumber = "Q1", DataFileCol = 1 },
                new Question { Id = Guid.NewGuid(), QstNumber = "Q2", DataFileCol = 2, MissValue1 = "99" }
            ],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q1", "Q2"],
                    ["1", "1", "2"],
                    ["2", "3", "99"],
                    ["3", "5", "6"]
                ]
            }
        };

        // Act
        var responses = survey.GetAllResponsesForQuestion(survey.QuestionList[1], omitMissingValues: true);

        // Assert
        responses.Should().BeEquivalentTo([2, 6]);
    }

    [Fact]
    public void GetAllResponsesForQuestion_ShouldUseQuestionMissingValues()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { Id = Guid.NewGuid(), QstNumber = "Q1", DataFileCol = 1, MissValue1 = "98", MissValue2 = "99" }
            ],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q1"],
                    ["1", "1"],
                    ["2", "98"],
                    ["3", "99"],
                    ["4", "5"]
                ]
            }
        };

        // Act
        var responses = survey.GetAllResponsesForQuestion(survey.QuestionList[0], omitMissingValues: true);

        // Assert
        responses.Should().BeEquivalentTo([1, 5]);
    }

    [Fact]
    public void IsAllResponsesInQuestionMissingValuesOK_ShouldReturnTrue_WhenNoQuestions()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList = [],
            Data = new SurveyData { DataList = [] }
        };

        // Act
        var result = survey.IsAllResponsesInQuestionMissingValuesOK();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAllResponsesInQuestionMissingValuesOK_ShouldReturnTrue_WhenNoData()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList = [new Question { QstNumber = "Q1", DataFileCol = 1 }],
            Data = null
        };

        // Act
        var result = survey.IsAllResponsesInQuestionMissingValuesOK();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAllResponsesInQuestionMissingValuesOK_ShouldReturnTrue_WhenQuestionsHaveValidResponses()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1, MissValue1 = "99" }
            ],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q1"],
                    ["1", "1"],
                    ["2", "2"],
                    ["3", "99"]
                ]
            }
        };

        // Act
        var result = survey.IsAllResponsesInQuestionMissingValuesOK();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAllResponsesInQuestionMissingValuesOK_ShouldThrowException_WhenAllResponsesAreMissing()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1, MissValue1 = "98", MissValue2 = "99" }
            ],
            Data = new SurveyData
            {
                DataFilePath = "/path/to/data.txt",
                DataList =
                [
                    ["ID", "Q1"],
                    ["1", "98"],
                    ["2", "99"],
                    ["3", "98"]
                ]
            }
        };

        // Act & Assert
        var exception = Assert.Throws<SurveyColumnAllMissingValuesException>(() => 
            survey.IsAllResponsesInQuestionMissingValuesOK());

        exception.Message.Should().Contain("Question 'Q1'");
        exception.Message.Should().Contain("has all missing values");
        exception.SurveyDataFilePath.Should().Be("/path/to/data.txt");
        exception.QstNum.Should().Be("Q1");
        exception.DataColNum.Should().Be(1);
    }

    [Fact]
    public void IsAllResponsesInQuestionMissingValuesOK_ShouldCombineQuestionAndDataMissingValues()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1, MissValue1 = "98" }
            ],
            Data = new SurveyData
            {
                DataFilePath = "/path/to/data.txt",
                MissingResponseValues = [99],
                DataList =
                [
                    ["ID", "Q1"],
                    ["1", "98"],
                    ["2", "99"],
                    ["3", "98"]
                ]
            }
        };

        // Act & Assert
        var exception = Assert.Throws<SurveyColumnAllMissingValuesException>(() => 
            survey.IsAllResponsesInQuestionMissingValuesOK());

        exception.Message.Should().Contain("Question 'Q1'");
        exception.DataColNum.Should().Be(1);
    }

    [Fact]
    public void IsAllResponsesInQuestionMissingValuesOK_ShouldReturnTrue_WhenNoMissingValuesDefined()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1, MissValue1 = "", MissValue2 = "", MissValue3 = "" }
            ],
            Data = new SurveyData
            {
                MissingResponseValues = [],
                DataList =
                [
                    ["ID", "Q1"],
                    ["1", "1"],
                    ["2", "2"],
                    ["3", "3"]
                ]
            }
        };

        // Act
        var result = survey.IsAllResponsesInQuestionMissingValuesOK();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAllResponsesInQuestionMissingValuesOK_ShouldHandleNonNumericResponses()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1, MissValue1 = "99" }
            ],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q1"],
                    ["1", "text"],
                    ["2", "99"],
                    ["3", "invalid"]
                ]
            }
        };

        // Act - non-numeric responses treated as not missing
        var result = survey.IsAllResponsesInQuestionMissingValuesOK();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAllResponsesInQuestionMissingValuesOK_ShouldSkipQuestionsWithMismatchedColumnNumbers()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1, MissValue1 = "99" },
                new Question { QstNumber = "Q99", DataFileCol = 2, MissValue1 = "99" } // Mismatched
            ],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q1", "Q2"], // Q2 doesn't match Q99
                    ["1", "1", "99"],
                    ["2", "2", "99"]
                ]
            }
        };

        // Act
        var result = survey.IsAllResponsesInQuestionMissingValuesOK();

        // Assert - should succeed because mismatched question is skipped
        result.Should().BeTrue();
    }
}
