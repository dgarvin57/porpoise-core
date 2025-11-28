using FluentAssertions;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Engines;

public class ProfileEngineTests
{
    [Fact]
    public void ProfileEngine_Constructor_ThrowsWhenSurveyIsNull()
    {
        // Arrange
        var dvQuestion = new Question();
        var response = new Response();

        // Act & Assert
        var act = () => new ProfileEngine(null!, dvQuestion, response);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProfileEngine_Constructor_ThrowsWhenDVQuestionIsNull()
    {
        // Arrange
        var survey = new Survey();
        var response = new Response();

        // Act & Assert
        var act = () => new ProfileEngine(survey, null!, response);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProfileEngine_Constructor_ThrowsWhenResponseIsNull()
    {
        // Arrange
        var survey = new Survey();
        var dvQuestion = new Question();

        // Act & Assert
        var act = () => new ProfileEngine(survey, dvQuestion, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProfileEngine_Constructor_ThrowsWhenResponseNotInQuestion()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Satisfaction" },
            new() { "1", "5" }
        };

        var survey = new Survey
        {
            Data = new SurveyData(dataList),
            QuestionList = new ObjectListBase<Question>()
        };

        var dvQuestion = new Question
        {
            DataFileCol = 1,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5, Label = "Very Satisfied" }
            }
        };

        var invalidResponse = new Response { RespValue = 99, Label = "Invalid" };

        // Act & Assert
        var act = () => new ProfileEngine(survey, dvQuestion, invalidResponse);
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*not found in Question*");
    }

    [Fact]
    public void ProfileEngine_BuildsProfileFromIndependentVariables()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Gender", "Age", "Satisfaction" },
            new() { "1", "1", "1", "5" },
            new() { "2", "1", "2", "5" },
            new() { "3", "2", "1", "4" },
            new() { "4", "2", "2", "3" }
        };

        var dvQuestion = new Question
        {
            Id = Guid.NewGuid(),
            DataFileCol = 3,
            QstLabel = "Satisfaction",
            VariableType = QuestionVariableType.Dependent,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5, Label = "Very Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 4, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 3, Label = "Neutral", IndexType = ResponseIndexType.Neutral }
            }
        };

        var survey = new Survey
        {
            Data = new SurveyData(dataList),
            QuestionList = new ObjectListBase<Question>
            {
                new Question
                {
                    Id = Guid.NewGuid(),
                    DataFileCol = 1,
                    QstLabel = "Gender",
                    VariableType = QuestionVariableType.Independent,
                    Responses = new ObjectListBase<Response>
                    {
                        new Response { RespValue = 1, Label = "Male" },
                        new Response { RespValue = 2, Label = "Female" }
                    }
                },
                new Question
                {
                    Id = Guid.NewGuid(),
                    DataFileCol = 2,
                    QstLabel = "Age",
                    VariableType = QuestionVariableType.Independent,
                    Responses = new ObjectListBase<Response>
                    {
                        new Response { RespValue = 1, Label = "18-34" },
                        new Response { RespValue = 2, Label = "35+" }
                    }
                }
            }
        };

        var targetResponse = dvQuestion.Responses[0]; // "Very Satisfied"

        // Act
        var engine = new ProfileEngine(survey, dvQuestion, targetResponse);

        // Assert
        engine.Percentages.Should().NotBeEmpty();
        engine.Percentages.Should().HaveCount(4); // 2 genders + 2 ages = 4 profile items
        engine.Percentages.Should().AllSatisfy(p =>
        {
            p.PercNum.Should().BeGreaterThanOrEqualTo(0);
            p.MarginalPercent.Should().BeGreaterThanOrEqualTo(0);
            p.QuestionLabel.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public void ProfileEngine_SortsByPercentageDifferenceDescending()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Gender", "Satisfaction" },
            new() { "1", "1", "5" },
            new() { "2", "1", "5" },
            new() { "3", "1", "5" },
            new() { "4", "2", "3" }
        };

        var dvQuestion = new Question
        {
            Id = Guid.NewGuid(),
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5, Label = "Very Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 3, Label = "Neutral", IndexType = ResponseIndexType.Neutral }
            }
        };

        var survey = new Survey
        {
            Data = new SurveyData(dataList),
            QuestionList = new ObjectListBase<Question>
            {
                new Question
                {
                    Id = Guid.NewGuid(),
                    DataFileCol = 1,
                    QstLabel = "Gender",
                    VariableType = QuestionVariableType.Independent,
                    Responses = new ObjectListBase<Response>
                    {
                        new Response { RespValue = 1, Label = "Male" },
                        new Response { RespValue = 2, Label = "Female" }
                    }
                }
            }
        };

        var targetResponse = dvQuestion.Responses[0]; // "Very Satisfied"

        // Act
        var engine = new ProfileEngine(survey, dvQuestion, targetResponse);

        // Assert
        engine.Percentages.Should().HaveCount(2);
        // Sorted by PercDiff descending
        if (engine.Percentages.Count > 1)
        {
            engine.Percentages[0].PercDiff.Should().BeGreaterThanOrEqualTo(engine.Percentages[1].PercDiff);
        }
    }

    [Fact]
    public void ProfileEngine_SkipsDVQuestionIfItAppearsAsIV()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Satisfaction1", "Satisfaction2" },
            new() { "1", "5", "5" },
            new() { "2", "4", "4" }
        };

        var dvQuestionId = Guid.NewGuid();
        var dvQuestion = new Question
        {
            Id = dvQuestionId,
            DataFileCol = 1,
            QstLabel = "Satisfaction1",
            VariableType = QuestionVariableType.Dependent,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5, Label = "Very Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 4, Label = "Satisfied", IndexType = ResponseIndexType.Positive }
            }
        };

        var survey = new Survey
        {
            Data = new SurveyData(dataList),
            QuestionList = new ObjectListBase<Question>
            {
                new Question
                {
                    Id = dvQuestionId, // Same ID as DV
                    DataFileCol = 1,
                    QstLabel = "Satisfaction1",
                    VariableType = QuestionVariableType.Independent,
                    Responses = new ObjectListBase<Response>
                    {
                        new Response { RespValue = 5, Label = "Very Satisfied" },
                        new Response { RespValue = 4, Label = "Satisfied" }
                    }
                },
                new Question
                {
                    Id = Guid.NewGuid(),
                    DataFileCol = 2,
                    QstLabel = "Satisfaction2",
                    VariableType = QuestionVariableType.Independent,
                    Responses = new ObjectListBase<Response>
                    {
                        new Response { RespValue = 5, Label = "Very Satisfied" },
                        new Response { RespValue = 4, Label = "Satisfied" }
                    }
                }
            }
        };

        var targetResponse = dvQuestion.Responses[0];

        // Act
        var engine = new ProfileEngine(survey, dvQuestion, targetResponse);

        // Assert
        // Should only have profile items from Satisfaction2, not Satisfaction1 (same as DV)
        engine.Percentages.Should().OnlyContain(p => p.QuestionLabel == "Satisfaction2");
    }

    [Fact]
    public void ProfileEngine_CalculatesPercDiff()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Gender", "Satisfaction" },
            new() { "1", "1", "5" },
            new() { "2", "1", "5" },
            new() { "3", "2", "3" }
        };

        var dvQuestion = new Question
        {
            Id = Guid.NewGuid(),
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5, Label = "Very Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 3, Label = "Neutral", IndexType = ResponseIndexType.Neutral }
            }
        };

        var survey = new Survey
        {
            Data = new SurveyData(dataList),
            QuestionList = new ObjectListBase<Question>
            {
                new Question
                {
                    Id = Guid.NewGuid(),
                    DataFileCol = 1,
                    QstLabel = "Gender",
                    VariableType = QuestionVariableType.Independent,
                    Responses = new ObjectListBase<Response>
                    {
                        new Response { RespValue = 1, Label = "Male" },
                        new Response { RespValue = 2, Label = "Female" }
                    }
                }
            }
        };

        var targetResponse = dvQuestion.Responses[0]; // "Very Satisfied"

        // Act
        var engine = new ProfileEngine(survey, dvQuestion, targetResponse);

        // Assert
        engine.Percentages.Should().AllSatisfy(p =>
        {
            // PercDiff should be PercNum - MarginalPercent
            var expectedDiff = p.PercNum - p.MarginalPercent;
            p.PercDiff.Should().BeApproximately(expectedDiff, 0.001);
        });
    }
}
