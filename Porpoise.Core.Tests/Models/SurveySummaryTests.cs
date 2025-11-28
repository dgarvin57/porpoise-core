using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class SurveySummaryTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Act
        var summary = new SurveySummary();

        // Assert
        summary.Id.Should().NotBe(Guid.Empty);
        summary.SurveyName.Should().BeEmpty();
        summary.SurveyFileName.Should().BeEmpty();
        summary.SurveyFolder.Should().BeEmpty();
    }

    [Fact]
    public void Id_CanBeSetAndRetrieved()
    {
        // Arrange
        var summary = new SurveySummary();
        var newId = Guid.NewGuid();

        // Act
        summary.Id = newId;

        // Assert
        summary.Id.Should().Be(newId);
    }

    [Fact]
    public void SurveyName_CanBeSetAndRetrieved()
    {
        // Arrange
        var summary = new SurveySummary();

        // Act
        summary.SurveyName = "Customer Satisfaction 2025";

        // Assert
        summary.SurveyName.Should().Be("Customer Satisfaction 2025");
    }

    [Fact]
    public void SurveyFileName_CanBeSetAndRetrieved()
    {
        // Arrange
        var summary = new SurveySummary();

        // Act
        summary.SurveyFileName = "survey2025.porps";

        // Assert
        summary.SurveyFileName.Should().Be("survey2025.porps");
    }

    [Fact]
    public void SurveyFolder_CanBeSetAndRetrieved()
    {
        // Arrange
        var summary = new SurveySummary();

        // Act
        summary.SurveyFolder = "/projects/2025";

        // Assert
        summary.SurveyFolder.Should().Be("/projects/2025");
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        // Arrange
        var summary = new SurveySummary();
        var testId = Guid.NewGuid();

        // Act
        summary.Id = testId;
        summary.SurveyName = "Test Survey";
        summary.SurveyFileName = "test.porps";
        summary.SurveyFolder = "/test";

        // Assert
        summary.Id.Should().Be(testId);
        summary.SurveyName.Should().Be("Test Survey");
        summary.SurveyFileName.Should().Be("test.porps");
        summary.SurveyFolder.Should().Be("/test");
    }
}
