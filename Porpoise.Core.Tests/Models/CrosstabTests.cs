using FluentAssertions;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Models;

public class CrosstabTests
{
    private SurveyData CreateTestSurveyData()
    {
        // Create 2x2 crosstab data: Gender (M/F) x Satisfaction (Satisfied/Not Satisfied)
        var dataList = new List<List<string>>
        {
            new() { "ID", "Gender", "Satisfaction" },
            new() { "1", "1", "1" },  // Male, Satisfied
            new() { "2", "1", "1" },  // Male, Satisfied
            new() { "3", "1", "2" },  // Male, Not Satisfied
            new() { "4", "2", "1" },  // Female, Satisfied
            new() { "5", "2", "2" },  // Female, Not Satisfied
            new() { "6", "2", "2" }   // Female, Not Satisfied
        };

        return new SurveyData(dataList);
    }

    [Fact]
    public void Crosstab_Constructor_ThrowsWhenDepVarIsNull()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var indVar = new Question { DataFileCol = 2 };

        // Act & Assert
        var act = () => new Crosstab(surveyData, null!, indVar, false, false);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Dependent variable*");
    }

    [Fact]
    public void Crosstab_Constructor_ThrowsWhenIndVarIsNull()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question { DataFileCol = 1 };

        // Act & Assert
        var act = () => new Crosstab(surveyData, depVar, null!, false, false);
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Independent variable*");
    }

    [Fact]
    public void Crosstab_CreatesMatrix()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question
        {
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, Label = "Not Satisfied", IndexType = ResponseIndexType.Negative }
            }
        };
        var indVar = new Question
        {
            DataFileCol = 1,
            QstLabel = "Gender",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Male" },
                new Response { RespValue = 2, Label = "Female" }
            }
        };

        // Act
        var crosstab = new Crosstab(surveyData, depVar, indVar, false, false);

        // Assert
        crosstab.TotalN.Should().Be(6);
        crosstab.CxTable.Should().NotBeNull();
        crosstab.CxTable!.Rows.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Crosstab_CalculatesChiSquare()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question
        {
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, Label = "Not Satisfied", IndexType = ResponseIndexType.Negative }
            }
        };
        var indVar = new Question
        {
            DataFileCol = 1,
            QstLabel = "Gender",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Male" },
                new Response { RespValue = 2, Label = "Female" }
            }
        };

        // Act
        var crosstab = new Crosstab(surveyData, depVar, indVar, false, false);

        // Assert
        crosstab.ChiSquare.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Crosstab_CalculatesPhi()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question
        {
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, Label = "Not Satisfied", IndexType = ResponseIndexType.Negative }
            }
        };
        var indVar = new Question
        {
            DataFileCol = 1,
            QstLabel = "Gender",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Male" },
                new Response { RespValue = 2, Label = "Female" }
            }
        };

        // Act
        var crosstab = new Crosstab(surveyData, depVar, indVar, false, false);

        // Assert
        crosstab.Phi.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Crosstab_CalculatesCramersV()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question
        {
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, Label = "Not Satisfied", IndexType = ResponseIndexType.Negative }
            }
        };
        var indVar = new Question
        {
            DataFileCol = 1,
            QstLabel = "Gender",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Male" },
                new Response { RespValue = 2, Label = "Female" }
            }
        };

        // Act
        var crosstab = new Crosstab(surveyData, depVar, indVar, false, false);

        // Assert
        crosstab.CramersV.Should().BeGreaterThanOrEqualTo(0);
        crosstab.CramersV.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public void Crosstab_GeneratesIndexList()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question
        {
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, Label = "Not Satisfied", IndexType = ResponseIndexType.Negative }
            }
        };
        var indVar = new Question
        {
            DataFileCol = 1,
            QstLabel = "Gender",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Male" },
                new Response { RespValue = 2, Label = "Female" }
            }
        };

        // Act
        var crosstab = new Crosstab(surveyData, depVar, indVar, false, false);
        var indexes = crosstab.GetIndexesList();

        // Assert
        indexes.Should().NotBeEmpty();
        indexes.Should().HaveCount(2); // One for each gender
        indexes.Should().AllSatisfy(i => i.Index.Should().BeGreaterThan(0));
    }

    [Fact]
    public void Crosstab_GetProfilePercentages_ReturnsCorrectCount()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question
        {
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, Label = "Not Satisfied", IndexType = ResponseIndexType.Negative }
            }
        };
        var indVar = new Question
        {
            DataFileCol = 1,
            QstLabel = "Gender",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Male" },
                new Response { RespValue = 2, Label = "Female" }
            }
        };

        // Act
        var crosstab = new Crosstab(surveyData, depVar, indVar, false, false);
        var profiles = crosstab.GetProfilePercentages(0);

        // Assert
        profiles.Should().NotBeNull();
        profiles!.Count.Should().Be(2); // One for each gender
        profiles.Should().AllSatisfy(p =>
        {
            p.PercNum.Should().BeGreaterThanOrEqualTo(0);
            p.MarginalPercent.Should().BeGreaterThanOrEqualTo(0);
        });
    }

    [Fact]
    public void Crosstab_GetProfilePercentages_ReturnsNullForInvalidIndex()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question
        {
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Satisfied", IndexType = ResponseIndexType.Positive }
            }
        };
        var indVar = new Question
        {
            DataFileCol = 1,
            QstLabel = "Gender",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Male" }
            }
        };

        // Act
        var crosstab = new Crosstab(surveyData, depVar, indVar, false, false);
        var profiles = crosstab.GetProfilePercentages(999);

        // Assert
        profiles.Should().BeNull();
    }

    [Fact]
    public void Crosstab_GetStatSigItems_ReturnsValidItem()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question
        {
            Id = Guid.NewGuid(),
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, Label = "Not Satisfied", IndexType = ResponseIndexType.Negative }
            }
        };
        var indVar = new Question
        {
            Id = Guid.NewGuid(),
            DataFileCol = 1,
            QstLabel = "Gender",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Male" },
                new Response { RespValue = 2, Label = "Female" }
            }
        };

        // Act
        var crosstab = new Crosstab(surveyData, depVar, indVar, false, false);
        var statSig = crosstab.GetStatSigItems();

        // Assert
        statSig.Should().NotBeNull();
        statSig.Id.Should().Be(indVar.Id);
        statSig.QuestionLabel.Should().Be("Gender");
        statSig.Phi.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Crosstab_CxIVIndexes_CalculatesPositiveAndNegative()
    {
        // Arrange
        var surveyData = CreateTestSurveyData();
        var depVar = new Question
        {
            DataFileCol = 2,
            QstLabel = "Satisfaction",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Satisfied", IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, Label = "Not Satisfied", IndexType = ResponseIndexType.Negative }
            }
        };
        var indVar = new Question
        {
            DataFileCol = 1,
            QstLabel = "Gender",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, Label = "Male" },
                new Response { RespValue = 2, Label = "Female" }
            }
        };

        // Act
        var crosstab = new Crosstab(surveyData, depVar, indVar, false, false);
        _ = crosstab.CxTable; // Accessing CxTable triggers CxIVIndexes calculation

        // Assert
        crosstab.CxIVIndexes.Should().NotBeEmpty();
        crosstab.CxIVIndexes.Should().AllSatisfy(idx =>
        {
            idx.Index.Should().BeGreaterThan(0);
            idx.IVLabel.Should().NotBeNullOrEmpty();
        });
    }
}
