using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class ResponseModelTests
{
    [Fact]
    public void Constructor_Default_SetsDefaults()
    {
        // Act
        var response = new Response();

        // Assert
        response.Id.Should().NotBe(Guid.Empty);
        response.RespValue.Should().Be(0);
        response.Label.Should().BeEmpty();
        response.IndexType.Should().Be(ResponseIndexType.None);
        response.Weight.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_WithParameters_SetsProperties()
    {
        // Arrange
        int respValue = 1;
        var label = "Yes";
        var indexType = ResponseIndexType.Positive;

        // Act
        var response = new Response(respValue, label, indexType);

        // Assert
        response.RespValue.Should().Be(respValue);
        response.Label.Should().Be(label);
        response.IndexType.Should().Be(indexType);
    }

    [Fact]
    public void Constructor_WithWeight_SetsWeightProperty()
    {
        // Arrange
        int respValue = 1;
        var label = "Yes";
        var indexType = ResponseIndexType.Positive;
        double weight = 2.5;

        // Act
        var response = new Response(respValue, label, indexType, weight);

        // Assert
        response.RespValue.Should().Be(respValue);
        response.Label.Should().Be(label);
        response.IndexType.Should().Be(indexType);
        response.Weight.Should().Be(weight);
    }

    [Fact]
    public void ResultPercent100_ConvertsDecimalToDouble()
    {
        // Arrange
        var response = new Response
        {
            ResultPercent = 0.45m // 45%
        };

        // Act
        var percent100 = response.ResultPercent100;

        // Assert
        percent100.Should().BeApproximately(45.0, 0.001);
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        // Arrange
        var original = new Response(1, "Original Label", ResponseIndexType.Positive)
        {
            ResultPercent = 0.25m,
            ResultFrequency = 100,
            CumPercent = 0.75m,
            Weight = 1.5
        };

        // Act
        var clone = (Response)original.Clone();

        // Assert
        clone.Should().NotBeSameAs(original);
        clone.RespValue.Should().Be(original.RespValue);
        clone.Label.Should().Be(original.Label);
        clone.IndexType.Should().Be(original.IndexType);
        clone.ResultPercent.Should().Be(original.ResultPercent);
        clone.ResultFrequency.Should().Be(original.ResultFrequency);
        clone.CumPercent.Should().Be(original.CumPercent);
        clone.Weight.Should().Be(original.Weight);
    }

    [Fact]
    public void Clone_ModificationDoesNotAffectOriginal()
    {
        // Arrange
        var original = new Response(1, "Original", ResponseIndexType.Positive);

        // Act
        var clone = (Response)original.Clone();
        clone.Label = "Modified";
        clone.IndexType = ResponseIndexType.Negative;

        // Assert
        original.Label.Should().Be("Original");
        original.IndexType.Should().Be(ResponseIndexType.Positive);
    }

    [Fact]
    public void PropertyChanges_MarkAsDirty()
    {
        // Arrange
        var response = new Response();
        response.MarkClean();

        // Act
        response.Label = "New Label";

        // Assert
        response.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void MarkClean_ClearsIsDirtyFlag()
    {
        // Arrange
        var response = new Response();
        response.Label = "Modified";

        // Act
        response.MarkClean();

        // Assert
        response.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void Id_PropertyAccess()
    {
        // Arrange
        var response = new Response();
        var newId = Guid.NewGuid();

        // Act
        response.Id = newId;

        // Assert
        response.Id.Should().Be(newId);
    }

    [Fact]
    public void IndexType_CanBeSetAndRetrieved()
    {
        // Arrange
        var response = new Response
        {
            IndexType = ResponseIndexType.Positive
        };

        // Act
        var indexType = response.IndexType;

        // Assert
        indexType.Should().Be(ResponseIndexType.Positive);
    }

    [Theory]
    [InlineData(ResponseIndexType.None)]
    [InlineData(ResponseIndexType.Neutral)]
    [InlineData(ResponseIndexType.Positive)]
    [InlineData(ResponseIndexType.Negative)]
    public void IndexType_SupportsAllTypes(ResponseIndexType indexType)
    {
        // Arrange
        var response = new Response();

        // Act
        response.IndexType = indexType;

        // Assert
        response.IndexType.Should().Be(indexType);
    }

    [Fact]
    public void ResultPercent_StoredAsDecimal()
    {
        // Arrange
        var response = new Response();

        // Act
        response.ResultPercent = 0.12345m;

        // Assert
        response.ResultPercent.Should().Be(0.12345m);
    }

    [Fact]
    public void CumPercent_CanBeSetAndRetrieved()
    {
        // Arrange
        var response = new Response();

        // Act
        response.CumPercent = 0.85m;

        // Assert
        response.CumPercent.Should().Be(0.85m);
    }

    [Fact]
    public void Weight_DefaultsToOne()
    {
        // Arrange & Act
        var response = new Response();

        // Assert
        response.Weight.Should().Be(1.0);
    }

    [Fact]
    public void Weight_CanBeModified()
    {
        // Arrange
        var response = new Response();

        // Act
        response.Weight = 2.5;

        // Assert
        response.Weight.Should().Be(2.5);
    }
}
