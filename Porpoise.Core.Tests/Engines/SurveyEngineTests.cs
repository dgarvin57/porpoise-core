using FluentAssertions;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using System.Text;

namespace Porpoise.Core.Tests.Engines;

public class SurveyEngineTests
{
    #region LoadDataIntoQuestions Tests

    [Fact]
    public void LoadDataIntoQuestions_ThrowsWhenSurveyIsNull()
    {
        // Act & Assert
        var act = () => SurveyEngine.LoadDataIntoQuestions(null!, null);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Survey data is required*");
    }

    [Fact]
    public void LoadDataIntoQuestions_ThrowsWhenDataIsNull()
    {
        // Arrange
        var survey = new Survey();

        // Act & Assert
        var act = () => SurveyEngine.LoadDataIntoQuestions(survey, null);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Survey data is required*");
    }

    [Fact]
    public void LoadDataIntoQuestions_CreatesQuestionsFromDataHeaders()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2", "Q3" },
            new() { "1", "1", "2", "3" },
            new() { "2", "2", "3", "4" },
            new() { "3", "1", "1", "5" }
        };
        var survey = new Survey
        {
            Data = new SurveyData(dataList)
        };

        // Act
        var result = SurveyEngine.LoadDataIntoQuestions(survey, null);

        // Assert
        result.Should().BeTrue();
        // Note: #?SIM_WEIGHT/?# column added by SurveyData creates a question too
        survey.QuestionList.Should().HaveCountGreaterThanOrEqualTo(3);
        survey.QuestionList.Should().Contain(q => q.QstNumber == "Q1");
        survey.QuestionList.Should().Contain(q => q.QstNumber == "Q2");
        survey.QuestionList.Should().Contain(q => q.QstNumber == "Q3");
    }

    [Fact]
    public void LoadDataIntoQuestions_SkipsWeightColumn()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "WEIGHT", "Q2" },
            new() { "1", "1", "1.0", "2" },
            new() { "2", "2", "1.5", "3" }
        };
        var survey = new Survey
        {
            Data = new SurveyData(dataList)
        };

        // Act
        var result = SurveyEngine.LoadDataIntoQuestions(survey, null);

        // Assert
        result.Should().BeTrue();
        survey.QuestionList.Should().Contain(q => q.QstNumber == "Q1");
        survey.QuestionList.Should().Contain(q => q.QstNumber == "Q2");
        survey.QuestionList.Should().NotContain(q => q.QstNumber == "WEIGHT"); // WEIGHT column skipped
    }

    [Fact]
    public void LoadDataIntoQuestions_SetsDataFileColCorrectly()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1", "1", "2" },
            new() { "2", "2", "3" }
        };
        var survey = new Survey
        {
            Data = new SurveyData(dataList)
        };

        // Act
        SurveyEngine.LoadDataIntoQuestions(survey, null);

        // Assert
        survey.QuestionList[0].DataFileCol.Should().Be(1); // Q1 is at index 1
        survey.QuestionList[1].DataFileCol.Should().Be(2); // Q2 is at index 2
    }

    [Fact]
    public void LoadDataIntoQuestions_SetsDataTypeBasedOnResponseCount()
    {
        // Arrange - Q1 has 2 responses (Nominal), Q2 has 20 responses (Interval)
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" }
        };
        // Add rows with few unique values for Q1, many for Q2
        for (int i = 1; i <= 20; i++)
        {
            dataList.Add(new List<string> { i.ToString(), (i % 2 + 1).ToString(), i.ToString() });
        }

        var survey = new Survey
        {
            Data = new SurveyData(dataList)
        };

        // Act
        SurveyEngine.LoadDataIntoQuestions(survey, null);

        // Assert
        survey.QuestionList[0].DataType.Should().Be(QuestionDataType.Nominal); // 2 responses <= 12
        survey.QuestionList[1].DataType.Should().Be(QuestionDataType.Interval); // 20 responses > 12
    }

    [Fact]
    public void LoadDataIntoQuestions_IdentifiesBlockQuestions()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1a", "Q1b", "Q2" },
            new() { "1", "1", "2", "3" }
        };
        var survey = new Survey
        {
            Data = new SurveyData(dataList)
        };

        // Act
        SurveyEngine.LoadDataIntoQuestions(survey, null);

        // Assert
        survey.QuestionList[0].BlkQstStatus.Should().Be(BlkQuestionStatusType.FirstQuestionInBlock); // Q1a contains 'a'
        survey.QuestionList[1].BlkQstStatus.Should().Be(BlkQuestionStatusType.ContinuationQuestion); // Q1b matches pattern
        survey.QuestionList[2].BlkQstStatus.Should().Be(BlkQuestionStatusType.DiscreetQuestion); // Q2 is standalone
    }

    [Fact]
    public void LoadDataIntoQuestions_CreatesResponsesForEachQuestion()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "1" },
            new() { "2", "2" },
            new() { "3", "1" }
        };
        var survey = new Survey
        {
            Data = new SurveyData(dataList)
        };

        // Act
        SurveyEngine.LoadDataIntoQuestions(survey, null);

        // Assert
        var q1 = survey.QuestionList.First(q => q.QstNumber == "Q1");
        q1.Responses.Should().HaveCount(2); // Responses: 1 and 2
        q1.Responses.Should().Contain(r => r.RespValue == 1);
        q1.Responses.Should().Contain(r => r.RespValue == 2);
    }

    [Fact]
    public void LoadDataIntoQuestions_ClearsExistingQuestions()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "1" }
        };
        var survey = new Survey
        {
            Data = new SurveyData(dataList),
            QuestionList = new ObjectListBase<Question>
            {
                new Question { QstNumber = "OLD_Q1" },
                new Question { QstNumber = "OLD_Q2" }
            }
        };

        // Act
        SurveyEngine.LoadDataIntoQuestions(survey, null);

        // Assert
        survey.QuestionList.Should().Contain(q => q.QstNumber == "Q1");
        survey.QuestionList.Should().NotContain(q => q.QstNumber == "OLD_Q1");
        survey.QuestionList.Should().NotContain(q => q.QstNumber == "OLD_Q2");
    }

    #endregion

    #region LoadTemplateIntoQuestion Tests

    [Fact]
    public void LoadTemplateIntoQuestion_HandlesNullTemplate()
    {
        // Arrange
        var survey = new Survey { QuestionList = new ObjectListBase<Question>() };
        string message = string.Empty;

        // Act
        SurveyEngine.LoadTemplateIntoQuestion(null!, "test.porps", survey, ref message);

        // Assert
        message.Should().Contain("was not applied");
        message.Should().Contain("no matching questions found");
    }

    [Fact]
    public void LoadTemplateIntoQuestion_HandlesEmptyTemplate()
    {
        // Arrange
        var template = new List<Question>();
        var survey = new Survey { QuestionList = new ObjectListBase<Question>() };
        string message = string.Empty;

        // Act
        SurveyEngine.LoadTemplateIntoQuestion(template, "test.porps", survey, ref message);

        // Assert
        message.Should().Contain("was not applied");
    }

    [Fact]
    public void LoadTemplateIntoQuestion_AppliesMatchingQuestionProperties()
    {
        // Arrange
        var template = new List<Question>
        {
            new Question
            {
                QstNumber = "Q1",
                QstLabel = "Template Label",
                BlkLabel = "Block Label",
                BlkStem = "Block Stem",
                QstStem = "Question Stem",
                DataType = QuestionDataType.Interval
            }
        };
        var survey = new Survey
        {
            QuestionList = new ObjectListBase<Question>
            {
                new Question { QstNumber = "Q1", QstLabel = "Original Label" }
            },
            Data = new SurveyData(new List<List<string>>
            {
                new() { "ID", "Q1" },
                new() { "1", "1" }
            })
        };
        string message = string.Empty;

        // Act
        SurveyEngine.LoadTemplateIntoQuestion(template, "test.porps", survey, ref message);

        // Assert
        var q1 = survey.QuestionList[0];
        q1.QstLabel.Should().Be("Template Label");
        q1.BlkLabel.Should().Be("Block Label");
        q1.BlkStem.Should().Be("Block Stem");
        q1.QstStem.Should().Be("Question Stem");
        q1.DataType.Should().Be(QuestionDataType.Interval);
        message.Should().Contain("successfully applied");
        message.Should().Contain("Matched 1");
    }

    [Fact]
    public void LoadTemplateIntoQuestion_AppliesResponseLabels()
    {
        // Arrange
        var template = new List<Question>
        {
            new Question
            {
                QstNumber = "Q1",
                Responses = new ObjectListBase<Response>
                {
                    new Response { RespValue = 1, Label = "Template Response 1" },
                    new Response { RespValue = 2, Label = "Template Response 2" }
                }
            }
        };
        var survey = new Survey
        {
            QuestionList = new ObjectListBase<Question>
            {
                new Question
                {
                    QstNumber = "Q1",
                    Responses = new ObjectListBase<Response>
                    {
                        new Response { RespValue = 1, Label = "Original" },
                        new Response { RespValue = 2, Label = "Original" }
                    }
                }
            },
            Data = new SurveyData(new List<List<string>>
            {
                new() { "ID", "Q1" },
                new() { "1", "1" }
            })
        };
        string message = string.Empty;

        // Act
        SurveyEngine.LoadTemplateIntoQuestion(template, "test.porps", survey, ref message);

        // Assert
        var q1 = survey.QuestionList[0];
        q1.Responses[0].Label.Should().Be("Template Response 1");
        q1.Responses[1].Label.Should().Be("Template Response 2");
    }

    [Fact]
    public void LoadTemplateIntoQuestion_SkipsNonMatchingQuestions()
    {
        // Arrange
        var template = new List<Question>
        {
            new Question { QstNumber = "Q99", QstLabel = "Non-existent" }
        };
        var survey = new Survey
        {
            QuestionList = new ObjectListBase<Question>
            {
                new Question { QstNumber = "Q1", QstLabel = "Original" }
            },
            Data = new SurveyData(new List<List<string>>
            {
                new() { "ID", "Q1" },
                new() { "1", "1" }
            })
        };
        string message = string.Empty;

        // Act
        SurveyEngine.LoadTemplateIntoQuestion(template, "test.porps", survey, ref message);

        // Assert
        survey.QuestionList[0].QstLabel.Should().Be("Original"); // Unchanged
        message.Should().Contain("was not applied");
        message.Should().Contain("no matching questions found");
    }

    #endregion

    #region Formatting Tests

    [Fact]
    public void FormatSelectedResponsesToString_HandlesNullQuestion()
    {
        // Act
        var result = SurveyEngine.FormatSelectedResponsesToString(null!);

        // Assert
        result.Should().Be("No selected responses");
    }

    [Fact]
    public void FormatSelectedResponsesToString_HandlesEmptyResponses()
    {
        // Arrange
        var question = new Question { Responses = new ObjectListBase<Response>() };

        // Act
        var result = SurveyEngine.FormatSelectedResponsesToString(question);

        // Assert
        result.Should().Be("No selected responses");
    }

    [Fact]
    public void FormatSelectedResponsesToString_FormatsResponses()
    {
        // Arrange
        var question = new Question
        {
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Strongly Agree" },
                new Response { RespValue = 2, Label = "Agree" }
            }
        };

        // Act
        var result = SurveyEngine.FormatSelectedResponsesToString(question);

        // Assert
        result.Should().Contain("Selected responses:");
        result.Should().Contain("Strongly Agree");
        result.Should().Contain("Agree");
    }

    [Fact]
    public void FormatWeightedResponsesToString_HandlesNoWeights()
    {
        // Arrange
        var question = new Question
        {
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Response 1", Weight = 1.0 },
                new Response { RespValue = 2, Label = "Response 2", Weight = 1.0 }
            }
        };

        // Act
        var result = SurveyEngine.FormatWeightedResponsesToString(question);

        // Assert
        result.Should().Be("No weighted responses");
    }

    [Fact]
    public void FormatWeightedResponsesToString_FormatsWeightedResponses()
    {
        // Arrange
        var question = new Question
        {
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Response 1", Weight = 2.5 },
                new Response { RespValue = 2, Label = "Response 2", Weight = 1.0 },
                new Response { RespValue = 3, Label = "Response 3", Weight = 0.5 }
            }
        };

        // Act
        var result = SurveyEngine.FormatWeightedResponsesToString(question);

        // Assert
        result.Should().Contain("Weighted responses:");
        result.Should().Contain("Response 1: 2.500"); // Format is #.##0
        result.Should().Contain("Response 3: .500"); // Format is #.##0
        result.Should().NotContain("Response 2"); // Weight is 1.0, so not included
    }

    [Fact]
    public void FormatSelectPlusSelectionToString_ShortFormat()
    {
        // Arrange
        var data = new SurveyData
        {
            SelectPlusQ1 = new Question { QstLabel = "Question 1 Label" },
            SelectPlusQ2 = new Question { QstLabel = "Question 2 Label" },
            SelectPlusCondition = SelectPlusConditionType.GoesPositive
        };

        // Act
        var result = SurveyEngine.FormatSelectPlusSelectionToString(data, true);

        // Assert
        result.Should().Contain("Select+:");
        result.Should().Contain("Quest"); // Truncated to 5 chars
        result.Should().Contain("Go+");
    }

    [Fact]
    public void FormatSelectPlusSelectionToString_LongFormat()
    {
        // Arrange
        var data = new SurveyData
        {
            SelectPlusQ1 = new Question { QstLabel = "Question 1" },
            SelectPlusQ2 = new Question { QstLabel = "Question 2" },
            SelectPlusCondition = SelectPlusConditionType.StaysNegative
        };

        // Act
        var result = SurveyEngine.FormatSelectPlusSelectionToString(data, false);

        // Assert
        result.Should().Contain("Select Plus:");
        result.Should().Contain("From Q1: Question 1");
        result.Should().Contain("To Q2: Question 2");
        result.Should().Contain("Movement:");
    }

    [Fact]
    public void FormatSelectPlusSelectionToString_HandlesNullQuestions()
    {
        // Arrange
        var data = new SurveyData
        {
            SelectPlusCondition = SelectPlusConditionType.StaysNeutral
        };

        // Act
        var result = SurveyEngine.FormatSelectPlusSelectionToString(data, false);

        // Assert
        result.Should().Contain("N/A");
    }

    #endregion

    #region SaveSurvey Validation Tests

    // Test disabled - FullProjectFolder property removed from Survey model
    // [Fact]
    // public void SaveSurvey_ThrowsWhenProjectFolderIsEmpty()
    // {
    //     // Arrange
    //     var survey = new Survey
    //     {
    //         FullProjectFolder = "",
    //         SurveyName = "Test"
    //     };
    //
    //     // Act & Assert
    //     var act = () => SurveyEngine.SaveSurvey(survey, false);
    //     act.Should().Throw<ArgumentException>()
    //         .WithMessage("*Project folder required*");
    // }

    // Test disabled - FullProjectFolder property removed from Survey model
    // [Fact]
    // public void SaveSurvey_ThrowsWhenSurveyNameIsEmpty()
    // {
    //     // Arrange
    //     var survey = new Survey
    //     {
    //         FullProjectFolder = "/path/to/project",
    //         SurveyName = ""
    //     };
    //
    //     // Act & Assert
    //     var act = () => SurveyEngine.SaveSurvey(survey, false);
    //     act.Should().Throw<ArgumentException>()
    //         .WithMessage("*Survey name required*");
    // }

    // Test disabled - FullProjectFolder property removed from Survey model
    // [Fact]
    // public void SaveSurvey_ThrowsWhenDataFileNameIsEmpty()
    // {
    //     // Arrange
    //     var survey = new Survey
    //     {
    //         FullProjectFolder = "/path/to/project",
    //         SurveyName = "TestSurvey",
    //         DataFileName = ""
    //     };
    //
    //     // Act & Assert
    //     var act = () => SurveyEngine.SaveSurvey(survey, false);
    //     act.Should().Throw<ArgumentException>()
    //         .WithMessage("*Data file name required*");
    // }

    // Test disabled - FullProjectFolder property removed from Survey model
    // [Fact]
    // public void SaveSurvey_ThrowsWhenDataIsNull()
    // {
    //     // Arrange
    //     var survey = new Survey
    //     {
    //         FullProjectFolder = "/path/to/project",
    //         SurveyName = "TestSurvey",
    //         DataFileName = "data.csv"
    //     };
    //
    //     // Act & Assert
    //     var act = () => SurveyEngine.SaveSurvey(survey, false);
    //     act.Should().Throw<ArgumentException>()
    //         .WithMessage("*Survey data required*");
    // }

    #endregion

    #region SurveyIdAlter Tests

    [Fact]
    public void SurveyIdAlter_ReturnsNonEmptyString()
    {
        // Arrange
        var surveyId = Guid.NewGuid();

        // Act
        var result = SurveyEngine.SurveyIdAlter(surveyId);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SurveyIdAlter_ReturnsSameValueForSameGuid()
    {
        // Arrange
        var surveyId = Guid.NewGuid();

        // Act
        var result1 = SurveyEngine.SurveyIdAlter(surveyId);
        var result2 = SurveyEngine.SurveyIdAlter(surveyId);

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void SurveyIdAlter_ReturnsDifferentValuesForDifferentGuids()
    {
        // Arrange
        var surveyId1 = Guid.NewGuid();
        var surveyId2 = Guid.NewGuid();

        // Act
        var result1 = SurveyEngine.SurveyIdAlter(surveyId1);
        var result2 = SurveyEngine.SurveyIdAlter(surveyId2);

        // Assert
        result1.Should().NotBe(result2);
    }

    #endregion
}
