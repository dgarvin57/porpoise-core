using FluentAssertions;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Engines;

public class QuestionEngineCalculationTests
{
    #region CalculateStatisticsHelper Tests

    [Fact]
    public void CalculateStatisticsHelper_CalculatesBasicPercentages()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 100,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 40 },
                new Response { RespValue = 2, ResultFrequency = 35 },
                new Response { RespValue = 3, ResultFrequency = 25 }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 100);

        // Assert
        question.Responses[0].ResultPercent.Should().Be(0.40m);
        question.Responses[1].ResultPercent.Should().Be(0.35m);
        question.Responses[2].ResultPercent.Should().Be(0.25m);
    }

    [Fact]
    public void CalculateStatisticsHelper_CalculatesCumulativePercentages()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 100,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 40 },
                new Response { RespValue = 2, ResultFrequency = 35 },
                new Response { RespValue = 3, ResultFrequency = 25 }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 100);

        // Assert
        question.Responses[0].CumPercent.Should().Be(0.40m);
        question.Responses[1].CumPercent.Should().Be(0.75m);
        question.Responses[2].CumPercent.Should().Be(1.00m);
    }

    [Fact]
    public void CalculateStatisticsHelper_CalculatesInverseCumulativePercentages()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 100,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 40 },
                new Response { RespValue = 2, ResultFrequency = 35 },
                new Response { RespValue = 3, ResultFrequency = 25 }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 100);

        // Assert
        question.Responses[0].InverseCumPercent.Should().Be(1.00m);
        question.Responses[1].InverseCumPercent.Should().Be(0.60m);
        question.Responses[2].InverseCumPercent.Should().Be(0.25m);
    }

    [Fact]
    public void CalculateStatisticsHelper_CalculatesSamplingError()
    {
        // Arrange - 40% response with n=100
        var question = new Question
        {
            TotalN = 100,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 40 }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 100);

        // Assert
        // Sampling Error = sqrt((p * q) / n) * 1.96
        // p = 40, q = 60, n = 100
        // sqrt((40 * 60) / 100) * 1.96 = sqrt(24) * 1.96 ≈ 9.6
        question.Responses[0].SamplingError.Should().BeApproximately(9.6, 0.1);
    }

    [Fact]
    public void CalculateStatisticsHelper_CalculatesTotalIndex()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 100,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 40, IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, ResultFrequency = 35, IndexType = ResponseIndexType.Neutral },
                new Response { RespValue = 3, ResultFrequency = 25, IndexType = ResponseIndexType.Negative }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 100);

        // Assert
        // TotalIndex = (Positive - Negative + 100)
        // = (40% - 25% + 100) = 115
        question.TotalIndex.Should().Be(115);
    }

    [Fact]
    public void CalculateStatisticsHelper_SkipsMissingValues()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 100,
            MissValue1 = "99",
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 40, IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 99, ResultFrequency = 10, IndexType = ResponseIndexType.Positive }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 100);

        // Assert
        // Only response 1 should be counted (99 is missing value)
        question.TotalIndex.Should().Be(140); // 40% positive, 0% negative + 100
        // Missing value response is skipped entirely in the loop, so percent stays 0
        question.Responses[1].ResultPercent.Should().Be(0m);
    }

    [Fact]
    public void CalculateStatisticsHelper_HandlesZeroUnweightedN()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 100,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 0 }
            }
        };

        // Act - Method now handles zero gracefully, returns 0 instead of throwing
        var act = () => QuestionEngine.CalculateStatisticsHelper(question, 0);
        act.Should().NotThrow();
        
        // Assert - Verify calculations are safe with zero
        question.Responses[0].ResultPercent.Should().Be(0m);
        question.Responses[0].SamplingError.Should().Be(0);
    }

    [Fact]
    public void CalculateStatisticsHelper_CalculatesComplexIndex()
    {
        // Arrange - realistic 5-point scale
        var question = new Question
        {
            TotalN = 200,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5, ResultFrequency = 60, IndexType = ResponseIndexType.Positive },   // 30%
                new Response { RespValue = 4, ResultFrequency = 80, IndexType = ResponseIndexType.Positive },   // 40%
                new Response { RespValue = 3, ResultFrequency = 30, IndexType = ResponseIndexType.Neutral },    // 15%
                new Response { RespValue = 2, ResultFrequency = 20, IndexType = ResponseIndexType.Negative },   // 10%
                new Response { RespValue = 1, ResultFrequency = 10, IndexType = ResponseIndexType.Negative }    // 5%
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 200);

        // Assert
        // Positive = 30% + 40% = 70%
        // Negative = 10% + 5% = 15%
        // Index = 70 - 15 + 100 = 155
        question.TotalIndex.Should().Be(155);
        question.Responses[0].ResultPercent.Should().Be(0.30m);
        question.Responses[4].ResultPercent.Should().Be(0.05m);
    }

    #endregion

    #region Integration Tests with Real Data

    [Fact]
    public void CalculateQuestionAndResponseStatistics_IntegrationTest()
    {
        // Arrange - Create survey with actual data
        var dataList = new List<List<string>>
        {
            new() { "ID", "Q1", "Q2" },
            new() { "1", "5", "1" },
            new() { "2", "5", "2" },
            new() { "3", "4", "1" },
            new() { "4", "4", "2" },
            new() { "5", "3", "1" },
            new() { "6", "2", "1" },
            new() { "7", "1", "2" }
        };

        var survey = new Survey
        {
            Data = new SurveyData(dataList)
        };

        var q1 = new Question
        {
            QstNumber = "Q1",
            DataFileCol = 1,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 5, IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 4, IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 3, IndexType = ResponseIndexType.Neutral },
                new Response { RespValue = 2, IndexType = ResponseIndexType.Negative },
                new Response { RespValue = 1, IndexType = ResponseIndexType.Negative }
            }
        };

        survey.QuestionList = new ObjectListBase<Question> { q1 };

        // Act
        QuestionEngine.CalculateQuestionAndResponseStatistics(survey, q1);

        // Assert
        q1.Responses[0].ResultFrequency.Should().BeGreaterThan(0); // 2 responses with value 5
        q1.Responses[1].ResultFrequency.Should().BeGreaterThan(0); // 2 responses with value 4
        q1.TotalN.Should().Be(7); // 7 total responses
        q1.TotalIndex.Should().BeGreaterThan(100); // More positive than negative
    }

    [Fact]
    public void CalculateStatisticsHelper_WeightedResponsesScenario()
    {
        // Arrange - Weighted frequencies
        var question = new Question
        {
            TotalN = 150, // Weighted N
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 75.5, IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, ResultFrequency = 74.5, IndexType = ResponseIndexType.Negative }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 100); // Unweighted N is 100

        // Assert
        question.Responses[0].ResultPercent.Should().BeApproximately(0.503m, 0.01m);
        question.Responses[1].ResultPercent.Should().BeApproximately(0.497m, 0.01m);
        // Index = 50.3% - 49.7% + 100 ≈ 101
        question.TotalIndex.Should().Be(101);
    }

    [Fact]
    public void CalculateStatisticsHelper_AllPositiveResponses()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 100,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 100, IndexType = ResponseIndexType.Positive }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 100);

        // Assert
        question.TotalIndex.Should().Be(200); // 100% positive - 0% negative + 100
    }

    [Fact]
    public void CalculateStatisticsHelper_AllNegativeResponses()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 100,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 100, IndexType = ResponseIndexType.Negative }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 100);

        // Assert
        question.TotalIndex.Should().Be(0); // 0% positive - 100% negative + 100
    }

    [Fact]
    public void CalculateStatisticsHelper_RoundingBehavior()
    {
        // Arrange
        var question = new Question
        {
            TotalN = 3,
            Responses = new ObjectListBase<Response>
            {
                new Response { RespValue = 1, ResultFrequency = 1, IndexType = ResponseIndexType.Positive },
                new Response { RespValue = 2, ResultFrequency = 1, IndexType = ResponseIndexType.Neutral },
                new Response { RespValue = 3, ResultFrequency = 1, IndexType = ResponseIndexType.Negative }
            }
        };

        // Act
        QuestionEngine.CalculateStatisticsHelper(question, 3);

        // Assert
        // Each response is 33.333...%
        question.Responses[0].ResultPercent.Should().BeApproximately(0.333m, 0.001m);
        question.Responses[1].ResultPercent.Should().BeApproximately(0.333m, 0.001m);
        question.Responses[2].ResultPercent.Should().BeApproximately(0.333m, 0.001m);
        // Index = 33.33% - 33.33% + 100 = 100
        question.TotalIndex.Should().Be(100);
    }

    #endregion
}
