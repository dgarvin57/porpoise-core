#nullable enable

using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

/// <summary>
/// Unit tests for Question model - the foundation of survey analysis.
/// </summary>
public class QuestionTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Act
        var question = new Question("Q1", "Question 1", 5, "99");

        // Assert
        question.QstNumber.Should().Be("Q1");
        question.QstLabel.Should().Be("Question 1");
        question.DataFileCol.Should().Be(5);
        question.MissValue1.Should().Be("99");
    }

    [Fact]
    public void MissingValues_ShouldReturnParsedValues()
    {
        // Arrange
        var question = new Question
        {
            MissValue1 = "99",
            MissValue2 = "98",
            MissValue3 = "97"
        };

        // Act
        var missingValues = question.MissingValues;

        // Assert
        missingValues.Should().HaveCount(3);
        missingValues.Should().Contain(new[] { 99, 98, 97 });
    }

    [Fact]
    public void MissingValues_ShouldIgnoreInvalidValues()
    {
        // Arrange
        var question = new Question
        {
            MissValue1 = "99",
            MissValue2 = "invalid",
            MissValue3 = ""
        };

        // Act
        var missingValues = question.MissingValues;

        // Assert
        missingValues.Should().HaveCount(1);
        missingValues.Should().Contain(99);
    }

    [Fact]
    public void Responses_ShouldTrackDirtyState()
    {
        // Arrange
        var question = new Question
        {
            Responses = new ObjectListBase<Response>
            {
                new Response(1, "Yes", ResponseIndexType.Positive),
                new Response(2, "No", ResponseIndexType.Negative)
            }
        };
        question.MarkClean();

        // Act
        question.Responses[0].Label = "Definitely Yes";

        // Assert
        question.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Responses_CanFilterByIndexType()
    {
        // Arrange
        var question = new Question
        {
            Responses = new ObjectListBase<Response>
            {
                new Response(1, "Strongly Agree", ResponseIndexType.Positive),
                new Response(2, "Agree", ResponseIndexType.Positive),
                new Response(3, "Neutral", ResponseIndexType.Neutral),
                new Response(4, "Disagree", ResponseIndexType.Negative)
            }
        };

        // Act
        var positiveResponses = question.Responses.Where(r => r.IndexType == ResponseIndexType.Positive);
        var negativeResponses = question.Responses.Where(r => r.IndexType == ResponseIndexType.Negative);

        // Assert
        positiveResponses.Should().HaveCount(2);
        negativeResponses.Should().HaveCount(1);
    }

    [Fact]
    public void MarkClean_ShouldMarkQuestionAndResponsesAsClean()
    {
        // Arrange
        var question = new Question
        {
            QstLabel = "Test",
            Responses = new ObjectListBase<Response>
            {
                new Response(1, "Yes", ResponseIndexType.Positive)
            }
        };

        // Act
        question.MarkClean();

        // Assert
        question.Responses[0].IsDirty.Should().BeFalse();
    }

    [Fact]
    public void DataType_ShouldDefaultToNominal()
    {
        // Arrange & Act
        var question = new Question();

        // Assert
        question.DataType.Should().Be(QuestionDataType.Nominal);
    }

    [Fact]
    public void VariableType_ShouldDefaultToDependent()
    {
        // Arrange & Act
        var question = new Question();

        // Assert
        question.VariableType.Should().Be(QuestionVariableType.Dependent);
    }

    [Fact]
    public void Clone_ShouldCreateDeepCopy()
    {
        // Arrange
        var original = new Question
        {
            Id = Guid.NewGuid(),
            QstNumber = "Q1",
            QstLabel = "Age",
            QstStem = "What is your age?",
            DataFileCol = 5,
            ColumnFilled = true,
            VariableType = QuestionVariableType.Independent,
            DataType = QuestionDataType.Interval,
            BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock,
            BlkLabel = "Demographics",
            BlkStem = "Demographics Block",
            MissValue1 = "99",
            MissValue2 = "98",
            TotalIndex = 120,
            TotalN = 500,
            Selected = true,
            WeightOn = true,
            QuestionNotes = "Test notes",
            UseAlternatePosNegLabels = true,
            AlternatePosLabel = "Positive",
            AlternateNegLabel = "Negative"
        };
        original.Responses.Add(new Response { RespValue = 1, Label = "18-24" });
        original.Responses.Add(new Response { RespValue = 2, Label = "25-34" });

        // Act
        var clone = original.Clone();

        // Assert
        clone.Should().NotBeSameAs(original);
        clone.Id.Should().Be(original.Id);
        clone.QstNumber.Should().Be(original.QstNumber);
        clone.QstLabel.Should().Be(original.QstLabel);
        clone.QstStem.Should().Be(original.QstStem);
        clone.DataFileCol.Should().Be(original.DataFileCol);
        clone.ColumnFilled.Should().Be(original.ColumnFilled);
        clone.VariableType.Should().Be(original.VariableType);
        clone.DataType.Should().Be(original.DataType);
        clone.BlkQstStatus.Should().Be(original.BlkQstStatus);
        clone.BlkLabel.Should().Be(original.BlkLabel);
        clone.BlkStem.Should().Be(original.BlkStem);
        clone.MissValue1.Should().Be(original.MissValue1);
        clone.MissValue2.Should().Be(original.MissValue2);
        clone.TotalIndex.Should().Be(original.TotalIndex);
        clone.TotalN.Should().Be(original.TotalN);
        clone.Selected.Should().Be(original.Selected);
        clone.WeightOn.Should().Be(original.WeightOn);
        clone.QuestionNotes.Should().Be(original.QuestionNotes);
        clone.UseAlternatePosNegLabels.Should().Be(original.UseAlternatePosNegLabels);
        clone.AlternatePosLabel.Should().Be(original.AlternatePosLabel);
        clone.AlternateNegLabel.Should().Be(original.AlternateNegLabel);
        clone.Responses.Should().HaveCount(2);
        clone.Responses[0].Should().NotBeSameAs(original.Responses[0]);
        clone.Responses[0].Label.Should().Be("18-24");
    }

    [Fact]
    public void Clone_ShouldCopyResponses()
    {
        // Arrange
        var original = new Question { QstNumber = "Q1" };
        original.Responses.Add(new Response { RespValue = 1, Label = "Yes", ResultFrequency = 10 });
        original.Responses.Add(new Response { RespValue = 2, Label = "No", ResultFrequency = 20 });

        // Act
        var clone = original.Clone();

        // Assert
        clone.Responses.Should().HaveCount(2);
        clone.Responses.Should().NotBeSameAs(original.Responses);
        clone.Responses[0].Should().NotBeSameAs(original.Responses[0]);
        clone.Responses[1].Should().NotBeSameAs(original.Responses[1]);
        clone.Responses[0].Label.Should().Be("Yes");
        clone.Responses[1].Label.Should().Be("No");
    }

    [Fact]
    public void Clone_ShouldAllowIndependentModification()
    {
        // Arrange
        var original = new Question { QstNumber = "Q1", QstLabel = "Original" };
        original.Responses.Add(new Response { RespValue = 1, Label = "Yes" });

        // Act
        var clone = original.Clone();
        clone.QstLabel = "Modified";
        clone.Responses[0].Label = "Modified Yes";

        // Assert
        original.QstLabel.Should().Be("Original");
        original.Responses[0].Label.Should().Be("Yes");
        clone.QstLabel.Should().Be("Modified");
        clone.Responses[0].Label.Should().Be("Modified Yes");
    }

    [Fact]
    public void Clone_ShouldHandleEmptyResponses()
    {
        // Arrange
        var original = new Question { QstNumber = "Q1", QstLabel = "Test" };

        // Act
        var clone = original.Clone();

        // Assert
        clone.Responses.Should().BeEmpty();
        clone.QstNumber.Should().Be("Q1");
    }

    [Fact]
    public void Clone_ShouldCopyPreferenceItems()
    {
        // Arrange
        var original = new Question 
        { 
            QstNumber = "Q1",
            IsPreferenceBlock = true,
            NumberOfPreferenceItems = 2
        };
        original.PreferenceItems.Add(new PreferenceItem { ItemName = "Item 1" });
        original.PreferenceItems.Add(new PreferenceItem { ItemName = "Item 2" });

        // Act
        var clone = original.Clone();

        // Assert
        clone.PreferenceItems.Should().HaveCount(2);
        clone.PreferenceItems.Should().NotBeSameAs(original.PreferenceItems);
        clone.PreferenceItems[0].Should().NotBeSameAs(original.PreferenceItems[0]);
        clone.PreferenceItems[0].ItemName.Should().Be("Item 1");
        clone.IsPreferenceBlock.Should().BeTrue();
        clone.NumberOfPreferenceItems.Should().Be(2);
    }

    [Fact]
    public void ToString_ShouldReturnQstLabel_WhenAvailable()
    {
        // Arrange
        var question = new Question { QstLabel = "Age", QstNumber = "Q1", DataFileCol = 5 };

        // Act
        var result = question.ToString();

        // Assert
        result.Should().Be("Age");
    }

    [Fact]
    public void ToString_ShouldReturnQstNumber_WhenQstLabelIsEmpty()
    {
        // Arrange
        var question = new Question { QstLabel = "", QstNumber = "Q1", DataFileCol = 5 };

        // Act
        var result = question.ToString();

        // Assert
        result.Should().Be("Q1");
    }

    [Fact]
    public void ToString_ShouldReturnDataFileCol_WhenBothLabelsAreEmpty()
    {
        // Arrange
        var question = new Question { QstLabel = "", QstNumber = "", DataFileCol = 5 };

        // Act
        var result = question.ToString();

        // Assert
        result.Should().Be("5");
    }
}
