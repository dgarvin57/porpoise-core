using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Porpoise.Core.Services;
using Xunit;

namespace Porpoise.Core.Tests.Services;

/// <summary>
/// Tests for AIInsightsService covering API calls, fallback behavior, and error handling
/// </summary>
public class AIInsightsServiceTests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

        public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
        {
            _sendAsync = sendAsync;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _sendAsync(request, cancellationToken);
        }
    }

    #region OpenAI Tests

    [Fact]
    public async Task CallAIAPI_OpenAI_ReturnsResponseContent()
    {
        // Arrange
        var expectedContent = "This is a test response from OpenAI.";
        var mockResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new { content = expectedContent }
                }
            }
        };

        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            Assert.Equal("https://api.openai.com/v1/chat/completions", request.RequestUri?.ToString());
            Assert.True(request.Headers.Authorization?.ToString().StartsWith("Bearer "));
            
            var responseJson = JsonSerializer.Serialize(mockResponse);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "openai");

        // Act
        var result = await service.CallAIAPI("Test prompt");

        // Assert
        Assert.Equal(expectedContent, result);
    }

    [Fact]
    public async Task GenerateSurveySummary_OpenAI_WithValidData_ReturnsAIResponse()
    {
        // Arrange
        var expectedContent = "Professional survey analysis with 100 respondents across 20 questions.";
        var mockResponse = new
        {
            choices = new[]
            {
                new { message = new { content = expectedContent } }
            }
        };

        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            var responseJson = JsonSerializer.Serialize(mockResponse);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "openai");

        // Act
        var result = await service.GenerateSurveySummary(
            totalSurveys: 2,
            totalQuestions: 20,
            totalCases: 100,
            projectName: "Customer Satisfaction"
        );

        // Assert
        Assert.Equal(expectedContent, result);
    }

    #endregion

    #region Anthropic Tests

    [Fact]
    public async Task CallAIAPI_Anthropic_ReturnsResponseContent()
    {
        // Arrange
        var expectedContent = "This is a test response from Claude.";
        var mockResponse = new
        {
            content = new[]
            {
                new { text = expectedContent }
            }
        };

        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            Assert.Equal("https://api.anthropic.com/v1/messages", request.RequestUri?.ToString());
            Assert.True(request.Headers.Contains("x-api-key"));
            Assert.True(request.Headers.Contains("anthropic-version"));
            
            var responseJson = JsonSerializer.Serialize(mockResponse);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "anthropic");

        // Act
        var result = await service.CallAIAPI("Test prompt");

        // Assert
        Assert.Equal(expectedContent, result);
    }

    [Fact]
    public async Task CallAIAPI_Anthropic_NoApiKey_ThrowsException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, null, "anthropic");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.CallAIAPI("Test prompt")
        );
    }

    [Fact]
    public async Task GenerateSurveySummary_Anthropic_WithValidData_ReturnsAIResponse()
    {
        // Arrange
        var expectedContent = "Market research survey with strong statistical power.";
        var mockResponse = new
        {
            content = new[] { new { text = expectedContent } }
        };

        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            var responseJson = JsonSerializer.Serialize(mockResponse);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "anthropic");

        // Act
        var result = await service.GenerateSurveySummary(
            totalSurveys: 1,
            totalQuestions: 50,
            totalCases: 500,
            projectName: "Market Research"
        );

        // Assert
        Assert.Equal(expectedContent, result);
    }

    #endregion

    #region Fallback Behavior Tests

    [Fact]
    public async Task GenerateSurveySummary_NoApiKey_ReturnsFallbackMessage()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new AIInsightsService(httpClient, null, "openai");

        // Act
        var result = await service.GenerateSurveySummary(
            totalSurveys: 1,
            totalQuestions: 25,
            totalCases: 150,
            projectName: "Test Project"
        );

        // Assert
        Assert.Contains("Test Project", result);
        Assert.Contains("1 survey", result);
        Assert.Contains("25 questions", result);
        Assert.Contains("150 respondents", result);
        Assert.DoesNotContain("surveys", result); // Should be singular
    }

    [Fact]
    public async Task GenerateSurveySummary_MultipleSurveys_ReturnsPluralFallback()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new AIInsightsService(httpClient, null, "openai");

        // Act
        var result = await service.GenerateSurveySummary(
            totalSurveys: 3,
            totalQuestions: 50,
            totalCases: 300
        );

        // Assert
        Assert.Contains("3 surveys", result);
    }

    [Theory]
    [InlineData(20, "small sample size")]
    [InlineData(50, "modest sample size")]
    [InlineData(200, "solid sample size")]
    [InlineData(1000, "large sample size")]
    public async Task GenerateSurveySummary_FallbackMessage_DescribesSampleSize(int cases, string expectedPhrase)
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new AIInsightsService(httpClient, null, "openai");

        // Act
        var result = await service.GenerateSurveySummary(
            totalSurveys: 1,
            totalQuestions: 10,
            totalCases: cases
        );

        // Assert
        Assert.Contains(expectedPhrase, result);
    }

    [Fact]
    public async Task GenerateSurveyInsights_NoApiKey_ReturnsFallbackMessage()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new AIInsightsService(httpClient, null, "openai");

        // Act
        var result = await service.GenerateSurveyInsights(
            surveyName: "Employee Survey",
            questionCount: 15,
            caseCount: 75
        );

        // Assert
        Assert.Contains("Employee Survey", result);
        Assert.Contains("15 questions", result);
        Assert.Contains("75 respondents", result);
    }

    [Theory]
    [InlineData(5, "short")]
    [InlineData(20, "moderate-length")]
    [InlineData(40, "comprehensive")]
    [InlineData(100, "extensive")]
    public async Task GenerateSurveyInsights_FallbackMessage_DescribesComplexity(int questionCount, string expectedComplexity)
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new AIInsightsService(httpClient, null, "openai");

        // Act
        var result = await service.GenerateSurveyInsights(
            surveyName: "Test Survey",
            questionCount: questionCount,
            caseCount: 100
        );

        // Assert
        Assert.Contains(expectedComplexity, result);
    }

    [Theory]
    [InlineData(20, "limited")]
    [InlineData(50, "adequate")]
    [InlineData(200, "strong")]
    public async Task GenerateSurveyInsights_FallbackMessage_DescribesStatisticalPower(int caseCount, string expectedPower)
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new AIInsightsService(httpClient, null, "openai");

        // Act
        var result = await service.GenerateSurveyInsights(
            surveyName: "Test Survey",
            questionCount: 10,
            caseCount: caseCount
        );

        // Assert
        Assert.Contains(expectedPower, result);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GenerateSurveySummary_APIError_ReturnsFallbackMessage()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("API Error")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "openai");

        // Act
        var result = await service.GenerateSurveySummary(
            totalSurveys: 1,
            totalQuestions: 10,
            totalCases: 100,
            projectName: "Error Test"
        );

        // Assert
        Assert.Contains("Error Test", result);
        Assert.Contains("1 survey", result);
        Assert.Contains("10 questions", result);
        Assert.Contains("100 respondents", result);
    }

    [Fact]
    public async Task GenerateSurveyInsights_APIError_ReturnsFallbackMessage()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            throw new HttpRequestException("Network error");
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "anthropic");

        // Act
        var result = await service.GenerateSurveyInsights(
            surveyName: "Network Test",
            questionCount: 20,
            caseCount: 50
        );

        // Assert
        Assert.Contains("Network Test", result);
        Assert.Contains("20 questions", result);
        Assert.Contains("50 respondents", result);
    }

    [Fact]
    public async Task CallAIAPI_InvalidJSON_ThrowsException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Invalid JSON", System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "openai");

        // Act & Assert - Invalid JSON will throw when parsing response
        await Assert.ThrowsAnyAsync<Exception>(
            async () => await service.CallAIAPI("Test prompt")
        );
    }

    #endregion

    #region Advanced Features Tests

    [Fact]
    public async Task GenerateSurveySummary_WithSampleQuestions_IncludesInPrompt()
    {
        // Arrange
        string? capturedPrompt = null;
        var mockResponse = new
        {
            choices = new[] { new { message = new { content = "Analysis with questions" } } }
        };

        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            var requestBody = await request.Content!.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(requestBody);
            capturedPrompt = jsonDoc.RootElement
                .GetProperty("messages")[1]
                .GetProperty("content")
                .GetString();
            
            var responseJson = JsonSerializer.Serialize(mockResponse);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "openai");

        var sampleQuestions = new List<string>
        {
            "How satisfied are you?",
            "Would you recommend us?"
        };

        // Act
        await service.GenerateSurveySummary(
            totalSurveys: 1,
            totalQuestions: 10,
            totalCases: 100,
            sampleQuestions: sampleQuestions
        );

        // Assert
        Assert.NotNull(capturedPrompt);
        Assert.Contains("How satisfied are you?", capturedPrompt);
        Assert.Contains("Would you recommend us?", capturedPrompt);
    }

    [Fact]
    public async Task GenerateSurveySummary_WithQuestionTypes_IncludesInPrompt()
    {
        // Arrange
        string? capturedPrompt = null;
        var mockResponse = new
        {
            choices = new[] { new { message = new { content = "Analysis with types" } } }
        };

        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            var requestBody = await request.Content!.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(requestBody);
            capturedPrompt = jsonDoc.RootElement
                .GetProperty("messages")[1]
                .GetProperty("content")
                .GetString();
            
            var responseJson = JsonSerializer.Serialize(mockResponse);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "openai");

        var questionTypes = new List<string> { "SingleChoice", "SingleChoice", "MultipleChoice", "Text" };

        // Act
        await service.GenerateSurveySummary(
            totalSurveys: 1,
            totalQuestions: 4,
            totalCases: 100,
            questionTypes: questionTypes
        );

        // Assert
        Assert.NotNull(capturedPrompt);
        Assert.Contains("SingleChoice (2)", capturedPrompt);
        Assert.Contains("MultipleChoice (1)", capturedPrompt);
        Assert.Contains("Text (1)", capturedPrompt);
    }

    [Fact]
    public async Task GenerateSurveyInsights_WithTopQuestions_IncludesInPrompt()
    {
        // Arrange
        string? capturedPrompt = null;
        var mockResponse = new
        {
            content = new[] { new { text = "Insights with questions" } }
        };

        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            var requestBody = await request.Content!.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(requestBody);
            capturedPrompt = jsonDoc.RootElement
                .GetProperty("messages")[0]
                .GetProperty("content")
                .GetString();
            
            var responseJson = JsonSerializer.Serialize(mockResponse);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "anthropic");

        var topQuestions = new List<string>
        {
            "Overall satisfaction",
            "Likelihood to recommend"
        };

        // Act
        await service.GenerateSurveyInsights(
            surveyName: "NPS Survey",
            questionCount: 10,
            caseCount: 200,
            topQuestionLabels: topQuestions
        );

        // Assert
        Assert.NotNull(capturedPrompt);
        Assert.Contains("Overall satisfaction", capturedPrompt);
        Assert.Contains("Likelihood to recommend", capturedPrompt);
    }

    [Fact]
    public async Task CallAIAPI_CustomTemperature_PassedToAPI()
    {
        // Arrange
        double? capturedTemperature = null;
        var mockResponse = new
        {
            choices = new[] { new { message = new { content = "Response" } } }
        };

        var handler = new MockHttpMessageHandler(async (request, ct) =>
        {
            var requestBody = await request.Content!.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(requestBody);
            capturedTemperature = jsonDoc.RootElement.GetProperty("temperature").GetDouble();
            
            var responseJson = JsonSerializer.Serialize(mockResponse);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler);
        var service = new AIInsightsService(httpClient, "test-api-key", "openai");

        // Act
        await service.CallAIAPI("Test prompt", temperature: 0.3);

        // Assert
        Assert.Equal(0.3, capturedTemperature);
    }

    #endregion
}
