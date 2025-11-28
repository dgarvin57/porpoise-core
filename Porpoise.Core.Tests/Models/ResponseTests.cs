#nullable enable

using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

/// <summary>
/// Unit tests for Response model - individual survey response options.
/// </summary>
public class ResponseTests
{
    [Fact]
    public void Constructor_ShouldSetBasicProperties()
    {
        // Act
        var response = new Response(1, "Strongly Agree", ResponseIndexType.Positive);

        // Assert
        response.RespValue.Should().Be(1);
        response.Label.Should().Be("Strongly Agree");
        response.IndexType.Should().Be(ResponseIndexType.Positive);
        response.Weight.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_WithWeight_ShouldSetWeight()
    {
        // Act
        var response = new Response(1, "Yes", ResponseIndexType.Positive, 1.5);

        // Assert
        response.Weight.Should().Be(1.5);
    }

    [Fact]
    public void IndexTypeDesc_ShouldReturnCorrectSymbol_ForPositive()
    {
        // Arrange
        var response = new Response(1, "Yes", ResponseIndexType.Positive);

        // Act & Assert
        response.IndexTypeDesc.Should().Be("+");
    }

    [Fact]
    public void IndexTypeDesc_ShouldReturnCorrectSymbol_ForNegative()
    {
        // Arrange
        var response = new Response(1, "No", ResponseIndexType.Negative);

        // Act & Assert
        response.IndexTypeDesc.Should().Be("-");
    }

    [Fact]
    public void IndexTypeDesc_ShouldReturnCorrectSymbol_ForNeutral()
    {
        // Arrange
        var response = new Response(1, "Neither", ResponseIndexType.Neutral);

        // Act & Assert
        response.IndexTypeDesc.Should().Be("/");
    }

    [Fact]
    public void IndexTypeDesc_ShouldReturnEmpty_ForNone()
    {
        // Arrange
        var response = new Response(1, "Other", ResponseIndexType.None);

        // Act & Assert
        response.IndexTypeDesc.Should().Be("");
    }

    [Fact]
    public void ResultFrequency_ShouldBeSettable()
    {
        // Arrange
        var response = new Response(1, "Yes", ResponseIndexType.Positive);

        // Act
        response.ResultFrequency = 150;

        // Assert
        response.ResultFrequency.Should().Be(150);
    }

    [Fact]
    public void ResultPercent_ShouldBeSettable()
    {
        // Arrange
        var response = new Response(1, "Yes", ResponseIndexType.Positive);

        // Act
        response.ResultPercent = 75.5m;

        // Assert
        response.ResultPercent.Should().Be(75.5m);
    }

    [Fact]
    public void CumPercent_ShouldBeSettable()
    {
        // Arrange
        var response = new Response(1, "Yes", ResponseIndexType.Positive);

        // Act
        response.CumPercent = 85.3m;

        // Assert
        response.CumPercent.Should().Be(85.3m);
    }

    [Fact]
    public void InverseCumPercent_ShouldBeSettable()
    {
        // Arrange
        var response = new Response(1, "Yes", ResponseIndexType.Positive);

        // Act
        response.InverseCumPercent = 14.7m;

        // Assert
        response.InverseCumPercent.Should().Be(14.7m);
    }

    [Fact]
    public void SamplingError_ShouldBeSettable()
    {
        // Arrange
        var response = new Response(1, "Yes", ResponseIndexType.Positive);

        // Act
        response.SamplingError = 3.2;

        // Assert
        response.SamplingError.Should().Be(3.2);
    }

    [Fact]
    public void PropertyChanges_ShouldMarkAsDirty()
    {
        // Arrange
        var response = new Response(1, "Yes", ResponseIndexType.Positive);
        response.MarkClean();

        // Act
        response.Label = "Definitely Yes";

        // Assert
        response.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void MarkClean_ShouldClearDirtyFlag()
    {
        // Arrange
        var response = new Response(1, "Yes", ResponseIndexType.Positive);
        response.Label = "Changed";

        // Act
        response.MarkClean();

        // Assert
        response.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void DefaultWeight_ShouldBeOne()
    {
        // Arrange & Act
        var response = new Response();

        // Assert
        response.Weight.Should().Be(1.0);
    }
}
