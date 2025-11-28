using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class ProjectModelTests
{
    [Fact]
    public void Constructor_Default_InitializesProperties()
    {
        // Act
        var project = new Project();

        // Assert
        project.Id.Should().NotBe(Guid.Empty);
        project.ProjectName.Should().BeNull();
        project.ClientName.Should().BeNull();
        project.SurveyList.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithBaseFolder_InitializesSurveyList()
    {
        // Arrange
        var baseFolder = "/projects";

        // Act
        var project = new Project(baseFolder);

        // Assert
        project.BaseProjectFolder.Should().Be(baseFolder);
        project.SurveyList.Should().NotBeNull();
        project.SurveyList.Should().HaveCount(1);
    }

    [Fact]
    public void Id_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();
        var newId = Guid.NewGuid();

        // Act
        project.Id = newId;

        // Assert
        project.Id.Should().Be(newId);
    }

    [Fact]
    public void ProjectName_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();

        // Act
        project.ProjectName = "Customer Satisfaction 2025";

        // Assert
        project.ProjectName.Should().Be("Customer Satisfaction 2025");
    }

    [Fact]
    public void ClientName_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();

        // Act
        project.ClientName = "Acme Corporation";

        // Assert
        project.ClientName.Should().Be("Acme Corporation");
    }

    [Fact]
    public void ResearcherLabel_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();

        // Act
        project.ResearcherLabel = "Market Research Inc.";

        // Assert
        project.ResearcherLabel.Should().Be("Market Research Inc.");
    }

    [Fact]
    public void ResearcherSubLabel_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();

        // Act
        project.ResearcherSubLabel = "Consumer Insights Division";

        // Assert
        project.ResearcherSubLabel.Should().Be("Consumer Insights Division");
    }

    [Fact]
    public void ResearcherLogo_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();
        var logoBase64 = "data:image/png;base64,iVBORw0KGgoAAAANS...";

        // Act
        project.ResearcherLogo = logoBase64;

        // Assert
        project.ResearcherLogo.Should().Be(logoBase64);
    }

    [Fact]
    public void SurveyList_CanBeAddedTo()
    {
        // Arrange
        var project = new Project();
        var survey1 = new Survey { SurveyName = "Q1 2025" };
        var survey2 = new Survey { SurveyName = "Q2 2025" };

        // Act
        project.SurveyList = new ObjectListBase<Survey> { survey1, survey2 };

        // Assert
        project.SurveyList.Should().HaveCount(2);
        project.SurveyList[0].SurveyName.Should().Be("Q1 2025");
        project.SurveyList[1].SurveyName.Should().Be("Q2 2025");
    }

    [Fact]
    public void IsDirty_ReturnsTrueWhenPropertyChanged()
    {
        // Arrange
        var project = new Project();
        project.MarkClean();

        // Act
        project.ProjectName = "Modified Project";

        // Assert
        project.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void IsDirty_ReturnsTrueWhenSurveyListDirty()
    {
        // Arrange
        var project = new Project();
        var survey = new Survey { SurveyName = "Test Survey" };
        project.SurveyList = new ObjectListBase<Survey> { survey };
        project.MarkClean();

        // Act
        survey.SurveyName = "Modified Survey";

        // Assert
        project.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void MarkClean_ClearsDirtyFlag()
    {
        // Arrange
        var project = new Project();
        project.ProjectName = "Modified";

        // Act
        project.MarkClean();

        // Assert
        project.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void IsExported_DefaultsToFalse()
    {
        // Arrange & Act
        var project = new Project();

        // Assert
        project.IsExported.Should().BeFalse();
    }

    [Fact]
    public void IsExported_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();

        // Act
        project.IsExported = true;

        // Assert
        project.IsExported.Should().BeTrue();
    }

    [Fact]
    public void BaseProjectFolder_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();

        // Act
        project.BaseProjectFolder = "/data/projects";

        // Assert
        project.BaseProjectFolder.Should().Be("/data/projects");
    }

    [Fact]
    public void FullPath_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();

        // Act
        project.FullPath = "/data/projects/2025/customer-survey.porp";

        // Assert
        project.FullPath.Should().Be("/data/projects/2025/customer-survey.porp");
    }

    [Fact]
    public void FileName_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();

        // Act
        project.FileName = "customer-survey.porp";

        // Assert
        project.FileName.Should().Be("customer-survey.porp");
    }

    [Fact]
    public void MultiplePropertiesChanged_MaintainsDirtyState()
    {
        // Arrange
        var project = new Project();
        project.MarkClean();

        // Act
        project.ProjectName = "Project 1";
        project.ClientName = "Client A";
        project.ResearcherLabel = "Researcher B";

        // Assert
        project.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void SurveyListSummary_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();
        var summary = new ObjectListBase<SurveySummary>
        {
            new SurveySummary { SurveyName = "Summary 1" }
        };

        // Act
        project.SurveyListSummary = summary;

        // Assert
        project.SurveyListSummary.Should().HaveCount(1);
        project.SurveyListSummary![0].SurveyName.Should().Be("Summary 1");
    }
}
