using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class KeyItemTests
{
    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var keyItem = new KeyItem();

        // Act
        keyItem.Title = "Test Key";
        keyItem.Details = "Test Details";
        keyItem.QtyRemaining = 5;
        keyItem.Description = "Test Description";
        keyItem.Type = KeyType.SingleSurvey;
        keyItem.Shorten = true;

        // Assert
        keyItem.Title.Should().Be("Test Key");
        keyItem.Details.Should().Be("Test Details");
        keyItem.QtyRemaining.Should().Be(5);
        keyItem.Description.Should().Be("Test Description");
        keyItem.Type.Should().Be(KeyType.SingleSurvey);
        keyItem.Shorten.Should().BeTrue();
    }

    [Fact]
    public void SetKeyType_Enterprise_SetsCorrectValues()
    {
        // Arrange
        var keyItem = new KeyItem();
        var emailId = "test@example.com";

        // Act
        keyItem.SetKeyType(emailId, KeyType.Enterprise, "image.png", 10, "12/31/2025", false);

        // Assert
        keyItem.Type.Should().Be(KeyType.Enterprise);
        keyItem.Title.Should().Be("Enterprise Key");
        keyItem.Description.Should().Contain("test@example.com");
        keyItem.Description.Should().Contain("unlimited");
        keyItem.Details.Should().Contain("Expires: 12/31/2025");
        keyItem.QtyRemaining.Should().Be(10);
        keyItem.Shorten.Should().BeFalse();
    }

    [Fact]
    public void SetKeyType_Enterprise_WithShortMessage_FormatsCorrectly()
    {
        // Arrange
        var keyItem = new KeyItem();

        // Act
        keyItem.SetKeyType("user@test.com", KeyType.Enterprise, "", 5, "01/15/2026", true);

        // Assert
        keyItem.Title.Should().Be("Enterprise Key");
        keyItem.Shorten.Should().BeTrue();
        keyItem.Description.Should().Contain("user@test.com");
    }

    [Fact]
    public void SetKeyType_SingleSurvey_SetsCorrectValues()
    {
        // Arrange
        var keyItem = new KeyItem();
        var emailId = "survey@example.com";

        // Act
        keyItem.SetKeyType(emailId, KeyType.SingleSurvey, "image.png", 3, "details", false);

        // Assert
        keyItem.Type.Should().Be(KeyType.SingleSurvey);
        keyItem.Title.Should().Be("Single Survey Key");
        keyItem.Description.Should().Contain("survey@example.com");
        keyItem.Description.Should().Contain("activate a single");
        keyItem.Details.Should().Contain("Qty: 3");
        keyItem.QtyRemaining.Should().Be(3);
    }

    [Fact]
    public void SetKeyType_SingleSurvey_WithShortMessage_FormatsCorrectly()
    {
        // Arrange
        var keyItem = new KeyItem();

        // Act
        keyItem.SetKeyType("short@test.com", KeyType.SingleSurvey, "", 7, "data", true);

        // Assert
        keyItem.Title.Should().Be("Single Survey Key");
        keyItem.Shorten.Should().BeTrue();
        keyItem.Description.Should().Contain("short@test.com");
    }

    [Fact]
    public void SetKeyType_None_SetsCorrectValues()
    {
        // Arrange
        var keyItem = new KeyItem();
        var emailId = "none@example.com";

        // Act
        keyItem.SetKeyType(emailId, KeyType.None, "", 0, "", false);

        // Assert
        keyItem.Type.Should().Be(KeyType.None);
        keyItem.Title.Should().Be("None");
        keyItem.Description.Should().Contain("none@example.com");
        keyItem.Description.Should().Contain("No available keys");
        keyItem.Details.Should().BeEmpty();
        keyItem.QtyRemaining.Should().Be(0);
    }

    [Fact]
    public void SetKeyType_None_WithShortMessage_FormatsCorrectly()
    {
        // Arrange
        var keyItem = new KeyItem();

        // Act
        keyItem.SetKeyType("empty@test.com", KeyType.None, "", 0, "", true);

        // Assert
        keyItem.Title.Should().Be("None");
        keyItem.Shorten.Should().BeTrue();
        keyItem.Description.Should().Contain("empty@test.com");
    }

    [Fact]
    public void SetKeyType_WithEmptyDetails_HandlesGracefully()
    {
        // Arrange
        var keyItem = new KeyItem();

        // Act
        keyItem.SetKeyType("test@test.com", KeyType.SingleSurvey, "", 5, "", false);

        // Assert
        keyItem.Details.Should().BeEmpty();
    }

    [Fact]
    public void SetKeyType_ClearsDetails_WhenCalled()
    {
        // Arrange
        var keyItem = new KeyItem
        {
            Details = "Old Details"
        };

        // Act
        keyItem.SetKeyType("test@test.com", KeyType.Enterprise, "", 1, "new", false);

        // Assert - Details gets set by the method based on type
        keyItem.Details.Should().NotBeEmpty();
    }

    [Fact]
    public void IsDirty_TracksPropertyChanges()
    {
        // Arrange
        var keyItem = new KeyItem();
        keyItem.MarkClean();

        // Act
        keyItem.Title = "Changed";

        // Assert
        keyItem.IsDirty.Should().BeTrue();
    }
}
