using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class SurveyMethodTests
{
    [Fact]
    public void ResequenceColumnNumbers_ShouldUpdateDataFileCol_WhenColumnMismatch()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1 },
                new Question { QstNumber = "Q2", DataFileCol = 2 }
            ],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q2", "Q1"], // Q2 is at index 1, Q1 is at index 2
                    ["1", "5", "10"]
                ]
            }
        };

        // Act
        var q1 = survey.QuestionList[0];
        var q2 = survey.QuestionList[1];
        survey.ResequenceColumnNumbers();

        // Assert - should update to match actual positions
        q1.DataFileCol.Should().Be(2, "Q1 is at index 2 in DataList"); // Q1 moved to column 2
        q2.DataFileCol.Should().Be(1, "Q2 is at index 1 in DataList"); // Q2 moved to column 1
        // After sorting by DataFileCol, order should be Q2 (col 1), Q1 (col 2)
        survey.QuestionList[0].Should().BeSameAs(q2);
        survey.QuestionList[1].Should().BeSameAs(q1);
    }

    [Fact]
    public void ResequenceColumnNumbers_ShouldSortQuestions_WhenColumnsChanged()
    {
        // Arrange
        var q1 = new Question { QstNumber = "Q1", DataFileCol = 1 };
        var q2 = new Question { QstNumber = "Q2", DataFileCol = 2 };
        var survey = new Survey
        {
            QuestionList = [q1, q2],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q2", "Q1"], // Reversed order
                    ["1", "5", "10"]
                ]
            }
        };

        // Act
        survey.ResequenceColumnNumbers();

        // Assert - should be sorted by DataFileCol
        survey.QuestionList[0].Should().BeSameAs(q2); // Q2 now first (col 1)
        survey.QuestionList[1].Should().BeSameAs(q1); // Q1 now second (col 2)
    }

    [Fact]
    public void ResequenceColumnNumbers_ShouldDoNothing_WhenColumnsMatch()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1 },
                new Question { QstNumber = "Q2", DataFileCol = 2 }
            ],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q1", "Q2"], // Already in correct order
                    ["1", "10", "20"]
                ]
            }
        };

        // Act
        survey.ResequenceColumnNumbers();

        // Assert - no changes
        survey.QuestionList[0].DataFileCol.Should().Be(1);
        survey.QuestionList[1].DataFileCol.Should().Be(2);
        survey.QuestionList[0].QstNumber.Should().Be("Q1");
        survey.QuestionList[1].QstNumber.Should().Be("Q2");
    }

    [Fact]
    public void ResequenceColumnNumbers_ShouldIgnoreMissingQuestions()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1 },
                new Question { QstNumber = "Q99", DataFileCol = 2 } // Not in data
            ],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q1", "Q2"], // Q99 doesn't exist
                    ["1", "10", "20"]
                ]
            }
        };

        // Act
        survey.ResequenceColumnNumbers();

        // Assert - Q99 should keep original DataFileCol
        survey.QuestionList[0].DataFileCol.Should().Be(1);
        survey.QuestionList[1].DataFileCol.Should().Be(2); // Unchanged
        survey.QuestionList[1].QstNumber.Should().Be("Q99");
    }

    [Fact]
    public void ResequenceColumnNumbers_ShouldHandleMultipleChanges()
    {
        // Arrange
        var q1 = new Question { QstNumber = "Q1", DataFileCol = 1 };
        var q2 = new Question { QstNumber = "Q2", DataFileCol = 2 };
        var q3 = new Question { QstNumber = "Q3", DataFileCol = 3 };
        var survey = new Survey
        {
            QuestionList = [q1, q2, q3],
            Data = new SurveyData
            {
                DataList =
                [
                    ["ID", "Q3", "Q1", "Q2"], // All reordered
                    ["1", "30", "10", "20"]
                ]
            }
        };

        // Act
        survey.ResequenceColumnNumbers();

        // Assert - all updated and sorted
        q1.DataFileCol.Should().Be(2);
        q2.DataFileCol.Should().Be(3);
        q3.DataFileCol.Should().Be(1);
        survey.QuestionList[0].Should().BeSameAs(q3);
        survey.QuestionList[1].Should().BeSameAs(q1);
        survey.QuestionList[2].Should().BeSameAs(q2);
    }

    [Fact]
    public void ResequenceColumnNumbers_ShouldHandleEmptyData()
    {
        // Arrange
        var survey = new Survey
        {
            QuestionList =
            [
                new Question { QstNumber = "Q1", DataFileCol = 1 }
            ],
            Data = new SurveyData
            {
                DataList = [["ID"]] // Only header, no questions
            }
        };

        // Act
        survey.ResequenceColumnNumbers();

        // Assert - no changes
        survey.QuestionList[0].DataFileCol.Should().Be(1);
    }

    [Fact]
    public void GetViolet_ShouldReturnCorrectBytes()
    {
        // Act
        var violet = Survey.GetViolet();

        // Assert
        violet.Should().Equal(new byte[] { 0xAF, 0x82, 0x9D, 0xFB });
    }
}
