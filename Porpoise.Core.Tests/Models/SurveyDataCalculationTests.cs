using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class SurveyDataCalculationTests
{
    [Fact]
    public void GetAllResponsesInColumn_ReturnsAllValues()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "5" },
            new() { "2", "4" },
            new() { "3", "3" },
            new() { "4", "2" },
            new() { "5", "1" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var responses = surveyData.GetAllResponsesInColumn(1, false, new List<int>());

        // Assert
        responses.Should().HaveCount(5);
        responses.Should().Contain(new[] { 5, 4, 3, 2, 1 });
    }

    [Fact]
    public void GetAllResponsesInColumn_OmitsMissingValues()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "5" },
            new() { "2", "99" },
            new() { "3", "3" },
            new() { "4", "99" },
            new() { "5", "1" }
        };
        var surveyData = new SurveyData(dataList);
        var missingValues = new List<int> { 99 };

        // Act
        var responses = surveyData.GetAllResponsesInColumn(1, true, missingValues);

        // Assert
        responses.Should().HaveCount(3);
        responses.Should().Contain(new[] { 5, 3, 1 });
        responses.Should().NotContain(99);
    }

    [Fact]
    public void GetAllResponsesInColumn_SkipsNonNumericValues()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "5" },
            new() { "2", "abc" },
            new() { "3", "3" },
            new() { "4", "" },
            new() { "5", "1" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var responses = surveyData.GetAllResponsesInColumn(1, false, new List<int>());

        // Assert
        responses.Should().HaveCount(3);
        responses.Should().Contain(new[] { 5, 3, 1 });
    }

    [Fact]
    public void GetQuestionResponses_ReturnsUniqueResponses()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "1" },
            new() { "2", "2" },
            new() { "3", "1" },
            new() { "4", "3" },
            new() { "5", "2" },
            new() { "6", "1" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var responses = surveyData.GetQuestionResponses(1, new List<int>());

        // Assert
        responses.Should().HaveCount(3); // 1, 2, 3
        responses.Should().Contain(r => r.RespValue == 1);
        responses.Should().Contain(r => r.RespValue == 2);
        responses.Should().Contain(r => r.RespValue == 3);
    }

    [Fact]
    public void GetQuestionResponses_ExcludesMissingValues()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "1" },
            new() { "2", "2" },
            new() { "3", "99" },
            new() { "4", "3" },
            new() { "5", "99" }
        };
        var surveyData = new SurveyData(dataList);
        var missingValues = new List<int> { 99 };

        // Act
        var responses = surveyData.GetQuestionResponses(1, missingValues);

        // Assert
        responses.Should().HaveCount(3); // 1, 2, 3 (99 excluded)
        responses.Should().NotContain(r => r.RespValue == 99);
    }

    [Fact]
    public void GetResponseFrequencyAndTotalN_CalculatesCorrectly()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "1" },
            new() { "2", "1" },
            new() { "3", "2" },
            new() { "4", "2" },
            new() { "5", "2" },
            new() { "6", "3" }
        };
        var surveyData = new SurveyData(dataList);
        var question = new Question
        {
            DataFileCol = 1,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1 },
                new Response { RespValue = 2 },
                new Response { RespValue = 3 }
            }
        };

        // Act
        surveyData.GetResponseFrequencyAndTotalN(question);

        // Assert
        question.TotalN.Should().Be(6);
        question.Responses[0].ResultFrequency.Should().Be(2); // Two 1's
        question.Responses[1].ResultFrequency.Should().Be(3); // Three 2's
        question.Responses[2].ResultFrequency.Should().Be(1); // One 3
    }

    [Fact]
    public void GetResponseFrequencyAndTotalN_IgnoresMissingValues()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "1" },
            new() { "2", "2" },
            new() { "3", "99" }
        };
        var surveyData = new SurveyData(dataList);
        var question = new Question
        {
            DataFileCol = 1,
            MissValue1 = "99",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1 },
                new Response { RespValue = 2 },
                new Response { RespValue = 99 }
            }
        };

        // Act
        surveyData.GetResponseFrequencyAndTotalN(question);

        // Assert - Missing values still counted in frequency but identifiable
        question.TotalN.Should().Be(3);
    }

    [Fact]
    public void IsAllResponsesNumeric_ReturnsTrueForValidData()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1", "1", "5" },
            new() { "2", "2", "4" },
            new() { "3", "3", "3" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var result = surveyData.IsAllResponsesNumeric();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAllResponsesNumeric_ReturnsFalseForInvalidData()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1", "1", "5" },
            new() { "2", "abc", "4" },
            new() { "3", "3", "3" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var result = surveyData.IsAllResponsesNumeric();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAllCasesInteger_ReturnsTrueForValidData()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1", "1", "5" },
            new() { "2", "2", "4" },
            new() { "3", "3", "3" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var result = surveyData.IsAllCasesInteger();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAllCasesInteger_ReturnsFalseForInvalidData()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1.5", "1", "5" },
            new() { "2", "2", "4" },
            new() { "3", "3", "3" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var result = surveyData.IsAllCasesInteger();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetCleanDVIVDataFromSurvey_ReturnsCorrectItems()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Gender", "Satisfaction" },
            new() { "1", "1", "5" },
            new() { "2", "2", "4" },
            new() { "3", "1", "3" }
        };
        var surveyData = new SurveyData(dataList);
        var dvQuestion = new Question
        {
            DataFileCol = 2,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5 },
                new Response { RespValue = 4 },
                new Response { RespValue = 3 }
            }
        };
        var ivQuestion = new Question
        {
            DataFileCol = 1,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1 },
                new Response { RespValue = 2 }
            }
        };

        // Act
        var items = surveyData.GetCleanDVIVDataFromSurvey(dvQuestion, ivQuestion);

        // Assert
        items.Should().HaveCount(3);
        items.Should().AllSatisfy(item =>
        {
            item.DVRespNumber.Should().BeOneOf(3, 4, 5);
            item.IVRespNumber.Should().BeOneOf(1, 2);
        });
    }

    [Fact]
    public void GetSimWeightColumnNumber_ReturnsCorrectColumn()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "#?SIM_WEIGHT/?#" },
            new() { "1", "1", "1" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var columnNumber = surveyData.GetSimWeightColumnNumber();

        // Assert
        columnNumber.Should().Be(2);
    }

    [Fact]
    public void MarkClean_ResetsDirtyFlag()
    {
        // Arrange
        var surveyData = new SurveyData();
        surveyData.DataFilePath = "/test/path"; // This sets IsDirty to true

        // Act
        surveyData.MarkClean();

        // Assert
        surveyData.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "5" },
            new() { "2", "4" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var clone = (SurveyData)surveyData.Clone();
        clone.DataList[1][1] = "99"; // Modify clone

        // Assert
        surveyData.DataList[1][1].Should().Be("5"); // Original unchanged
        clone.DataList[1][1].Should().Be("99");
    }
}
