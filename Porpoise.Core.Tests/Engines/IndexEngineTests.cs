using FluentAssertions;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Engines;

public class IndexEngineTests
{
    [Fact]
    public void IndexEngine_Constructor_ThrowsWhenSurveyIsNull()
    {
        // Arrange
        var dvQuestion = new Question();

        // Act & Assert
        var act = () => new IndexEngine(null!, dvQuestion);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IndexEngine_Constructor_ThrowsWhenDVQuestionIsNull()
    {
        // Arrange
        var survey = new Survey();

        // Act & Assert
        var act = () => new IndexEngine(survey, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IndexEngine_Constructor_ThrowsWhenSurveyDataIsNull()
    {
        // Arrange
        var survey = new Survey();
        var dvQuestion = new Question();

        // Act & Assert
        var act = () => new IndexEngine(survey, dvQuestion);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Survey data is required*");
    }

    [Fact]
    public void IndexEngine_BuildsIndexListFromIndependentVariables()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Gender", "Age", "Satisfaction" },
            new() { "1", "1", "1", "5" },
            new() { "2", "1", "2", "4" },
            new() { "3", "2", "1", "5" },
            new() { "4", "2", "2", "3" }
        };

        var survey = new Survey
        {
            Data = new SurveyData(dataList),
            QuestionList = new ObjectListBase<Question>
            {
                new Question
                {
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

        var dvQuestion = new Question
        {
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

        // Act
        var engine = new IndexEngine(survey, dvQuestion);

        // Assert
        engine.Indexes.Should().NotBeEmpty();
        engine.Indexes.Should().HaveCount(4); // 2 genders + 2 ages = 4 index items
        engine.Indexes.Should().AllSatisfy(idx =>
        {
            idx.Index.Should().BeGreaterThan(0);
            idx.QuestionLabel.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public void IndexEngine_SortsIndexesDescending()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Gender", "Satisfaction" },
            new() { "1", "1", "5" },
            new() { "2", "1", "5" },
            new() { "3", "1", "4" },
            new() { "4", "2", "3" },
            new() { "5", "2", "2" }
        };

        var survey = new Survey
        {
            Data = new SurveyData(dataList),
            QuestionList = new ObjectListBase<Question>
            {
                new Question
                {
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

        var dvQuestion = new Question
        {
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5, Label = "Very Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 4, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 3, Label = "Neutral", IndexType = ResponseIndexType.Neutral },
                new Response { RespValue = 2, Label = "Dissatisfied", IndexType = ResponseIndexType.Negative }
            }
        };

        // Act
        var engine = new IndexEngine(survey, dvQuestion);

        // Assert
        engine.Indexes.Should().HaveCount(2);
        // First index should be higher than second (sorted descending)
        engine.Indexes[0].Index.Should().BeGreaterThan(engine.Indexes[1].Index);
    }

    [Fact]
    public void IndexEngine_SkipsNonIndependentVariables()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Gender", "OtherQ", "Satisfaction" },
            new() { "1", "1", "1", "5" },
            new() { "2", "2", "2", "4" }
        };

        var survey = new Survey
        {
            Data = new SurveyData(dataList),
            QuestionList = new ObjectListBase<Question>
            {
                new Question
                {
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
                    DataFileCol = 2,
                    QstLabel = "OtherQ",
                    VariableType = QuestionVariableType.Dependent, // Not Independent
                    Responses = new ObjectListBase<Response>
                    {
                        new Response { RespValue = 1, Label = "Yes" },
                        new Response { RespValue = 2, Label = "No" }
                    }
                }
            }
        };

        var dvQuestion = new Question
        {
            DataFileCol = 3,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5, Label = "Very Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 4, Label = "Satisfied", IndexType = ResponseIndexType.Positive }
            }
        };

        // Act
        var engine = new IndexEngine(survey, dvQuestion);

        // Assert
        engine.Indexes.Should().HaveCount(2); // Only Gender's 2 responses, not OtherQ
        engine.Indexes.Should().AllSatisfy(idx =>
            idx.QuestionLabel.Should().Be("Gender"));
    }
}
