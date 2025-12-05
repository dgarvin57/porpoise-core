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

    // ResearcherLabel removed - branding moved to tenant level

    // ResearcherSubLabel removed - branding moved to tenant level

    [Fact]
    public void ClientLogo_CanBeSetAndRetrieved()
    {
        // Arrange
        var project = new Project();
        var logoBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header

        // Act
        project.ClientLogo = logoBytes;

        // Assert
        project.ClientLogo.Should().BeEquivalentTo(logoBytes);
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
        project.Description = "Test Description";

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

    [Fact]
    public void SummarizeSurveyList_CreatesCorrectSummaries()
    {
        // Arrange
        var project = new Project();
        var survey1 = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Survey 1",
            SurveyFileName = "survey1.porps"
        };
        var survey2 = new Survey
        {
            Id = Guid.NewGuid(),
            SurveyName = "Survey 2",
            SurveyFileName = "survey2.porps"
        };
        project.SurveyList = new ObjectListBase<Survey> { survey1, survey2 };

        // Act
        project.SummarizeSurveyList();

        // Assert
        project.SurveyListSummary.Should().NotBeNull();
        project.SurveyListSummary.Should().HaveCount(2);
        project.SurveyListSummary![0].Id.Should().Be(survey1.Id);
        project.SurveyListSummary[0].SurveyName.Should().Be("Survey 1");
        project.SurveyListSummary[1].Id.Should().Be(survey2.Id);
        project.SurveyListSummary[1].SurveyName.Should().Be("Survey 2");
    }

    [Fact]
    public void SummarizeSurveyList_HandlesNullSurveyList()
    {
        // Arrange
        var project = new Project();
        project.SurveyList = null;

        // Act
        project.SummarizeSurveyList();

        // Assert - should not throw
        project.SurveyListSummary.Should().BeNull();
    }

    [Fact]
    public void MarkClean_ClearsSurveyListDirtyFlags()
    {
        // Arrange
        var project = new Project();
        var survey = new Survey { SurveyName = "Test" };
        project.SurveyList = new ObjectListBase<Survey> { survey };
        project.MarkClean(); // Mark everything clean first
        
        // Modify a survey
        survey.SurveyName = "Modified";

        // Act
        project.MarkClean();

        // Assert
        project.IsDirty.Should().BeFalse();
        survey.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void GetSurveyFromSurveyList_ReturnsMatchingSurvey()
    {
        // Arrange
        var project = new Project();
        var surveyId = Guid.NewGuid();
        var survey = new Survey { Id = surveyId, SurveyName = "Target Survey" };
        project.SurveyList = new ObjectListBase<Survey>
        {
            new Survey { Id = Guid.NewGuid(), SurveyName = "Other Survey" },
            survey
        };

        // Act
        var result = project.GetSurveyFromSurveyList(surveyId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(surveyId);
        result.SurveyName.Should().Be("Target Survey");
    }

    [Fact]
    public void GetSurveyFromSurveyList_ReturnsNullWhenNotFound()
    {
        // Arrange
        var project = new Project();
        project.SurveyList = new ObjectListBase<Survey>
        {
            new Survey { Id = Guid.NewGuid(), SurveyName = "Survey 1" }
        };

        // Act
        var result = project.GetSurveyFromSurveyList(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetSurveyFromSurveyList_ReturnsNullWhenSurveyListNull()
    {
        // Arrange
        var project = new Project();
        project.SurveyList = null;

        // Act
        var result = project.GetSurveyFromSurveyList(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SaveSurveyInSurveyList_UpdatesExistingSurvey()
    {
        // Arrange
        var project = new Project();
        var surveyId = Guid.NewGuid();
        var originalSurvey = new Survey { Id = surveyId, SurveyName = "Original" };
        project.SurveyList = new ObjectListBase<Survey> { originalSurvey };

        var updatedSurvey = new Survey { Id = surveyId, SurveyName = "Updated" };

        // Act
        var result = project.SaveSurveyInSurveyList(updatedSurvey);

        // Assert
        result.Should().BeTrue();
        project.SurveyList[0].SurveyName.Should().Be("Updated");
    }

    [Fact]
    public void SaveSurveyInSurveyList_ReturnsFalseWhenSurveyNotFound()
    {
        // Arrange
        var project = new Project();
        project.SurveyList = new ObjectListBase<Survey>
        {
            new Survey { Id = Guid.NewGuid(), SurveyName = "Existing" }
        };
        var newSurvey = new Survey { Id = Guid.NewGuid(), SurveyName = "New" };

        // Act
        var result = project.SaveSurveyInSurveyList(newSurvey);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SaveSurveyInSurveyList_ReturnsFalseWhenSurveyListNull()
    {
        // Arrange
        var project = new Project();
        project.SurveyList = null;
        var survey = new Survey { Id = Guid.NewGuid() };

        // Act
        var result = project.SaveSurveyInSurveyList(survey);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SaveSurveyInSurveyList_ReturnsFalseWhenSurveyNull()
    {
        // Arrange
        var project = new Project();
        project.SurveyList = new ObjectListBase<Survey>();

        // Act
        var result = project.SaveSurveyInSurveyList(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMoreThanOneUnlockedSurvey_ReturnsTrueWhenMultipleUnlocked()
    {
        // Arrange
        var project = new Project();
        project.SurveyList = new ObjectListBase<Survey>
        {
            new Survey { Status = SurveyStatus.Initial },
            new Survey { Status = SurveyStatus.Initial }
        };

        // Act
        var result = project.IsMoreThanOneUnlockedSurvey();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMoreThanOneUnlockedSurvey_ReturnsTrueWhenMoreThanOneSurvey()
    {
        // Arrange - Licensing removed, method now just checks count > 1
        var project = new Project();
        project.SurveyList = new ObjectListBase<Survey>
        {
            new Survey { Status = SurveyStatus.Initial },
            new Survey { Status = SurveyStatus.Verified }
        };

        // Act
        var result = project.IsMoreThanOneUnlockedSurvey();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMoreThanOneUnlockedSurvey_ReturnsFalseWhenLessThanTwoSurveys()
    {
        // Arrange
        var project = new Project();
        project.SurveyList = new ObjectListBase<Survey>
        {
            new Survey { Status = SurveyStatus.Initial }
        };

        // Act
        var result = project.IsMoreThanOneUnlockedSurvey();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsMoreThanOneUnlockedSurvey_ReturnsFalseWhenSurveyListNull()
    {
        // Arrange
        var project = new Project();
        project.SurveyList = null;

        // Act
        var result = project.IsMoreThanOneUnlockedSurvey();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateProjectName_ThrowsWhenNull()
    {
        // Arrange
        var project = new Project();

        // Act
        var act = () => project.ValidateProjectName();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Project name is required*");
    }

    [Fact]
    public void ValidateProjectName_ThrowsWhenEmpty()
    {
        // Arrange
        var project = new Project { ProjectName = "" };

        // Act
        var act = () => project.ValidateProjectName();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ValidateProjectName_SucceedsWithValidName()
    {
        // Arrange
        var project = new Project { ProjectName = "Valid Project Name" };

        // Act
        var act = () => project.ValidateProjectName();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateClientName_ThrowsWhenNull()
    {
        // Arrange
        var project = new Project();

        // Act
        var act = () => project.ValidateClientName();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Client name is required*");
    }

    [Fact]
    public void ValidateClientName_SucceedsWithValidName()
    {
        // Arrange
        var project = new Project { ClientName = "Acme Corp" };

        // Act
        var act = () => project.ValidateClientName();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateFullPath_ThrowsWhenNull()
    {
        // Arrange
        var project = new Project();

        // Act
        var act = () => project.ValidateFullPath();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Full project path is required*");
    }

    [Fact]
    public void ValidateFullPath_SucceedsWithValidPath()
    {
        // Arrange
        var project = new Project { FullPath = "/valid/path/project.porp" };

        // Act
        var act = () => project.ValidateFullPath();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateProjectFolder_ThrowsWhenNull()
    {
        // Arrange
        var project = new Project();

        // Act
        var act = () => project.ValidateProjectFolder();

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Project folder is required*");
    }

    [Fact]
    public void ValidateProjectFolder_SucceedsWithValidFolder()
    {
        // Arrange
        var project = new Project { ProjectFolder = "project-folder" };

        // Act
        var act = () => project.ValidateProjectFolder();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MarkAsOld_MarksSurveysAsOld()
    {
        // Arrange
        var project = new Project();
        var survey1 = new Survey { SurveyName = "Survey 1" };
        var survey2 = new Survey { SurveyName = "Survey 2" };
        project.SurveyList = new ObjectListBase<Survey> { survey1, survey2 };

        // Act
        project.MarkAsOld();

        // Assert
        survey1.IsNew.Should().BeFalse();
        survey2.IsNew.Should().BeFalse();
    }
}
