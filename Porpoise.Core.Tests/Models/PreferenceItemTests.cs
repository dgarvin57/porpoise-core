using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class PreferenceItemTests
{
    [Fact]
    public void Constructor_Default_InitializesProperties()
    {
        // Act
        var item = new PreferenceItem();

        // Assert
        item.ItemId.Should().BeEmpty();
        item.ItemName.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithParameters_SetsProperties()
    {
        // Arrange
        var id = "P1";
        var name = "Preference Item 1";

        // Act
        var item = new PreferenceItem(id, name);

        // Assert
        item.ItemId.Should().Be(id);
        item.ItemName.Should().Be(name);
    }

    [Fact]
    public void ItemId_CanBeSetAndRetrieved()
    {
        // Arrange
        var item = new PreferenceItem();

        // Act
        item.ItemId = "P123";

        // Assert
        item.ItemId.Should().Be("P123");
    }

    [Fact]
    public void ItemName_CanBeSetAndRetrieved()
    {
        // Arrange
        var item = new PreferenceItem();

        // Act
        item.ItemName = "My Preference";

        // Assert
        item.ItemName.Should().Be("My Preference");
    }

    [Fact]
    public void PropertyChange_MarksDirty()
    {
        // Arrange
        var item = new PreferenceItem("P1", "Original");
        item.MarkClean();

        // Act
        item.ItemName = "Modified";

        // Assert
        item.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void MarkClean_ClearsDirtyFlag()
    {
        // Arrange
        var item = new PreferenceItem();
        item.ItemName = "Modified";

        // Act
        item.MarkClean();

        // Assert
        item.IsDirty.Should().BeFalse();
    }
}
