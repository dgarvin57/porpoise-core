#nullable enable

using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

/// <summary>
/// Unit tests for SurveyData - the raw response data engine.
/// </summary>
public class SurveyDataTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDataList()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1", "1", "2" },
            new() { "2", "2", "1" }
        };

        // Act
        var surveyData = new SurveyData(dataList);

        // Assert
        surveyData.DataList.Should().HaveCount(3);
        surveyData.DataList[0][1].Should().Be("Q1");
    }

    [Fact]
    public void GetAllResponsesInColumn_ShouldReturnCorrectValues()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1", "1", "2" },
            new() { "2", "2", "1" },
            new() { "3", "1", "2" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var responses = surveyData.GetAllResponsesInColumn(1, false, new List<int>());

        // Assert
        responses.Should().HaveCount(3);
        responses.Should().Contain(new[] { 1, 2, 1 });
    }

    [Fact]
    public void GetAllResponsesInColumn_ShouldOmitMissingValues()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "1" },
            new() { "2", "99" }, // Missing value
            new() { "3", "2" }
        };
        var surveyData = new SurveyData(dataList);
        var missingValues = new List<int> { 99 };

        // Act
        var responses = surveyData.GetAllResponsesInColumn(1, true, missingValues);

        // Assert
        responses.Should().HaveCount(2);
        responses.Should().Contain(new[] { 1, 2 });
        responses.Should().NotContain(99);
    }

    [Fact]
    public void GetQuestionResponses_ShouldReturnUniqueValues()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "1" },
            new() { "2", "2" },
            new() { "3", "1" },
            new() { "4", "3" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var responses = surveyData.GetQuestionResponses(1, new List<int>());

        // Assert
        responses.Should().HaveCount(3); // Unique values: 1, 2, 3
    }

    [Fact]
    public void DataList_ShouldBeModifiable()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2", "Q3" },
            new() { "1", "A", "B", "C" },
            new() { "2", "D", "E", "F" }
        };
        var surveyData = new SurveyData(dataList);

        // Act - manually reorder by swapping columns
        foreach (var row in surveyData.DataList)
        {
            var temp = row[1];
            row[1] = row[2];
            row[2] = temp;
        }

        // Assert
        surveyData.DataList[0][1].Should().Be("Q2");
        surveyData.DataList[0][2].Should().Be("Q1");
    }

    [Fact]
    public void DataList_CanRemoveColumn()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2", "Q3" },
            new() { "1", "A", "B", "C" },
            new() { "2", "D", "E", "F" }
        };
        var surveyData = new SurveyData(dataList);
        // Note: Constructor adds #?SIM_WEIGHT/?# column automatically
        int originalCount = surveyData.DataList[0].Count;

        // Act - manually remove Q2 column at index 2
        foreach (var row in surveyData.DataList)
        {
            row.RemoveAt(2);
        }

        // Assert
        surveyData.DataList[0].Should().HaveCount(originalCount - 1);
        surveyData.DataList[0][2].Should().Be("Q3");
    }

    [Fact]
    public void DataList_CanAddColumn()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1", "A", "B" },
            new() { "2", "C", "D" }
        };
        var surveyData = new SurveyData(dataList);
        // Note: Constructor adds #?SIM_WEIGHT/?# column automatically
        int originalCount = surveyData.DataList[0].Count;

        // Act - manually insert column at index 2
        surveyData.DataList[0].Insert(2, "Q_NEW");
        surveyData.DataList[1].Insert(2, "0");
        surveyData.DataList[2].Insert(2, "0");

        // Assert
        surveyData.DataList[0].Should().HaveCount(originalCount + 1);
        surveyData.DataList[0][2].Should().Be("Q_NEW");
    }

    [Fact]
    public void PropertyChanged_ShouldMarkAsDirty()
    {
        // Arrange
        var surveyData = new SurveyData();
        surveyData.MarkClean();

        // Act
        surveyData.DataFilePath = "/path/to/data.csv";

        // Assert
        surveyData.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Clone_ShouldCreateDeepCopy()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1" },
            new() { "1", "A" }
        };
        var surveyData = new SurveyData(dataList);

        // Act
        var cloned = (SurveyData)surveyData.Clone();
        cloned.DataList[1][1] = "B"; // Modify clone

        // Assert
        surveyData.DataList[1][1].Should().Be("A"); // Original unchanged
        cloned.DataList[1][1].Should().Be("B");
    }

    [Fact]
    public void SelectOn_InitiallyFalse()
    {
        // Arrange
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1", "1", "A" },
            new() { "2", "2", "B" },
            new() { "3", "1", "C" }
        };
        var surveyData = new SurveyData(dataList);

        // Assert - SelectOn defaults to false without special setup
        surveyData.SelectOn.Should().BeFalse();
    }
}
