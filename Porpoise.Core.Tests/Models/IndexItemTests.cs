using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class IndexItemTests
{
    [Fact]
    public void Constructor_WithInitProperties_SetsValues()
    {
        // Arrange & Act
        var item = new IndexItem
        {
            Index = 1,
            ResponseLabel = "Strongly Agree",
            QuestionLabel = "Satisfaction Question"
        };

        // Assert
        item.Index.Should().Be(1);
        item.ResponseLabel.Should().Be("Strongly Agree");
        item.QuestionLabel.Should().Be("Satisfaction Question");
        item.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Id_GeneratesUniqueValues()
    {
        // Arrange & Act
        var item1 = new IndexItem { Index = 1 };
        var item2 = new IndexItem { Index = 2 };

        // Assert
        item1.Id.Should().NotBe(item2.Id);
    }

    [Fact]
    public void Id_CanBeExplicitlySet()
    {
        // Arrange
        var customId = Guid.NewGuid();

        // Act
        var item = new IndexItem { Id = customId };

        // Assert
        item.Id.Should().Be(customId);
    }

    [Fact]
    public void RecordEquality_ComparesAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var item1 = new IndexItem
        {
            Id = id,
            Index = 1,
            ResponseLabel = "Agree",
            QuestionLabel = "Q1"
        };
        var item2 = new IndexItem
        {
            Id = id,
            Index = 1,
            ResponseLabel = "Agree",
            QuestionLabel = "Q1"
        };

        // Assert
        item1.Should().Be(item2);
    }

    [Fact]
    public void RecordEquality_DiffersByValue()
    {
        // Arrange
        var item1 = new IndexItem { Index = 1, ResponseLabel = "Agree" };
        var item2 = new IndexItem { Index = 2, ResponseLabel = "Agree" };

        // Assert
        item1.Should().NotBe(item2);
    }

    [Fact]
    public void GetSupport1_ReturnsExpectedValue()
    {
        // Act
        var result = IndexItem.GetSupport1();

        // Assert
        result.Should().Be("S9kqcpJQjxmbkdo8/");
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        // Arrange
        var original = new IndexItem
        {
            Index = 1,
            ResponseLabel = "Original",
            QuestionLabel = "Q1"
        };

        // Act
        var modified = original with { ResponseLabel = "Modified" };

        // Assert
        modified.ResponseLabel.Should().Be("Modified");
        modified.Index.Should().Be(1);
        modified.QuestionLabel.Should().Be("Q1");
        original.ResponseLabel.Should().Be("Original");
    }
}
