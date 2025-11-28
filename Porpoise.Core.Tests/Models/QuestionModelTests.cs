using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class QuestionModelTests
{
    [Fact]
    public void Constructor_Default_SetsDefaults()
    {
        // Act
        var question = new Question();

        // Assert
        question.Id.Should().NotBe(Guid.Empty);
        question.QstNumber.Should().BeEmpty();
        question.QstLabel.Should().BeEmpty();
        question.VariableType.Should().Be(QuestionVariableType.Dependent);
        question.DataType.Should().Be(QuestionDataType.Nominal);
    }

    [Fact]
    public void Constructor_WithLabel_SetsLabel()
    {
        // Arrange
        var label = "Test Question";

        // Act
        var question = new Question(label);

        // Assert
        question.QstLabel.Should().Be(label);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var qstNumber = "Q1";
        var qstLabel = "Question Label";
        short dataFileCol = 5;
        var missValue = "99";

        // Act
        var question = new Question(qstNumber, qstLabel, dataFileCol, missValue);

        // Assert
        question.QstNumber.Should().Be(qstNumber);
        question.QstLabel.Should().Be(qstLabel);
        question.DataFileCol.Should().Be(dataFileCol);
        question.MissValue1.Should().Be(missValue);
    }

    [Fact]
    public void MissingValues_ParsesAllThreeValues()
    {
        // Arrange
        var question = new Question
        {
            MissValue1 = "98",
            MissValue2 = "99",
            MissValue3 = "100"
        };

        // Act
        var missing = question.MissingValues;

        // Assert
        missing.Should().HaveCount(3);
        missing.Should().Contain(new[] { 98, 99, 100 });
    }

    [Fact]
    public void MissingValues_IgnoresNonNumericValues()
    {
        // Arrange
        var question = new Question
        {
            MissValue1 = "98",
            MissValue2 = "invalid",
            MissValue3 = "99"
        };

        // Act
        var missing = question.MissingValues;

        // Assert
        missing.Should().HaveCount(2);
        missing.Should().Contain(new[] { 98, 99 });
    }

    [Fact]
    public void SamplingError_CalculatesCorrectly_WhenTotalNIsPositive()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 100
        };

        // Act
        var error = question.SamplingError;

        // Assert
        error.Should().BeGreaterThan(0);
        error.Should().BeApproximately(9.8m, 0.1m);
    }

    [Fact]
    public void SamplingError_ReturnsZero_WhenTotalNIsZero()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 0
        };

        // Act
        var error = question.SamplingError;

        // Assert
        error.Should().Be(0m);
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        // Arrange
        var original = new Question
        {
            QstNumber = "Q1",
            QstLabel = "Original",
            DataFileCol = 5,
            VariableType = QuestionVariableType.Independent,
            Responses = new ObjectListBase<Response>
            {
                new(1, "Yes", ResponseIndexType.Positive),
                new(2, "No", ResponseIndexType.Negative)
            }
        };

        // Act
        var clone = original.Clone();

        // Assert
        clone.Should().NotBeSameAs(original);
        clone.QstNumber.Should().Be(original.QstNumber);
        clone.QstLabel.Should().Be(original.QstLabel);
        clone.DataFileCol.Should().Be(original.DataFileCol);
        clone.VariableType.Should().Be(original.VariableType);
        clone.Responses.Should().HaveCount(2);
        clone.Responses[0].Label.Should().Be("Yes");
    }

    [Fact]
    public void Clone_ClonesResponses()
    {
        // Arrange
        var original = new Question
        {
            Responses = new ObjectListBase<Response>
            {
                new(1, "Response 1", ResponseIndexType.Positive)
            }
        };

        // Act
        var clone = original.Clone();
        clone.Responses[0].Label = "Modified";

        // Assert
        original.Responses[0].Label.Should().Be("Response 1");
        clone.Responses[0].Label.Should().Be("Modified");
    }

    [Fact]
    public void IsDirty_True_WhenResponsesAreDirty()
    {
        // Arrange
        var question = new Question
        {
            Responses = new ObjectListBase<Response>
            {
                new(1, "Test", ResponseIndexType.None)
            }
        };
        question.MarkClean();

        // Act
        question.Responses[0].Label = "Modified";

        // Assert
        question.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void IsDirty_True_WhenPreferenceItemsAreDirty()
    {
        // Arrange
        var question = new Question
        {
            PreferenceItems = new ObjectListBase<PreferenceItem>
            {
                new()
            }
        };
        question.MarkClean();

        // Act
        question.PreferenceItems[0].ItemName = "Modified";

        // Assert
        question.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void MarkClean_ClearsIsDirtyFlag()
    {
        // Arrange
        var question = new Question();
        question.QstLabel = "Modified";

        // Act
        question.MarkClean();

        // Assert
        question.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void MarkClean_ClearsResponsesDirtyFlag()
    {
        // Arrange
        var question = new Question
        {
            Responses = new ObjectListBase<Response>
            {
                new(1, "Yes", ResponseIndexType.Positive)
            }
        };
        question.Responses[0].Label = "Modified";

        // Act
        question.MarkClean();

        // Assert
        question.Responses[0].IsDirty.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsQstNumber()
    {
        // Arrange
        var question = new Question
        {
            QstNumber = "Q42"
        };

        // Act
        var result = question.ToString();

        // Assert
        result.Should().Be("Q42");
    }

    [Fact]
    public void VariableType_ChangesToIndependent_ClearsResponses()
    {
        // Arrange
        var question = new Question
        {
            Responses = new ObjectListBase<Response>
            {
                new(1, "Yes", ResponseIndexType.None)
            }
        };

        // Act
        question.VariableType = QuestionVariableType.Independent;

        // Assert
        question.Responses[0].IndexType.Should().Be(ResponseIndexType.Neutral);
    }
}
