using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Porpoise.Core.Models;

namespace Porpoise.Core.Services;

/// <summary>
/// Generates AI-powered insights from survey statistics using OpenAI/Claude API
/// </summary>
public class AIInsightsService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _provider; // "openai" or "anthropic"

    public AIInsightsService(HttpClient httpClient, string? apiKey = null, string provider = "openai")
    {
        _httpClient = httpClient;
        _apiKey = apiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        _provider = provider;
    }

    /// <summary>
    /// Generate executive summary for survey statistics
    /// </summary>
    public async Task<string> GenerateSurveySummary(
        int totalSurveys, 
        int totalQuestions, 
        int totalCases, 
        string? projectName = null,
        List<string>? sampleQuestions = null,
        List<string>? questionTypes = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return GenerateFallbackSummary(totalSurveys, totalQuestions, totalCases, projectName);
        }

        var prompt = BuildSummaryPrompt(totalSurveys, totalQuestions, totalCases, projectName, sampleQuestions, questionTypes);
        
        try
        {
            return await CallAIAPI(prompt);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI API call failed: {ex.Message}");
            return GenerateFallbackSummary(totalSurveys, totalQuestions, totalCases, projectName);
        }
    }

    /// <summary>
    /// Generate insights about specific survey results
    /// </summary>
    public async Task<string> GenerateSurveyInsights(
        string surveyName, 
        int questionCount, 
        int caseCount,
        List<string>? topQuestionLabels = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return GenerateFallbackInsights(surveyName, questionCount, caseCount);
        }

        var prompt = BuildInsightsPrompt(surveyName, questionCount, caseCount, topQuestionLabels);
        
        try
        {
            return await CallAIAPI(prompt);
        }
        catch
        {
            return GenerateFallbackInsights(surveyName, questionCount, caseCount);
        }
    }

    private string BuildSummaryPrompt(int totalSurveys, int totalQuestions, int totalCases, string? projectName, List<string>? sampleQuestions = null, List<string>? questionTypes = null)
    {
        var questionsContext = "";
        if (sampleQuestions != null && sampleQuestions.Any())
        {
            questionsContext = $"\n\nSample Questions:\n{string.Join("\n", sampleQuestions.Take(8).Select((q, i) => $"{i + 1}. {q}"))}";
        }

        var typesContext = "";
        if (questionTypes != null && questionTypes.Any())
        {
            var typeGroups = questionTypes.GroupBy(t => t).Select(g => $"{g.Key} ({g.Count()})");
            typesContext = $"\n\nQuestion Types: {string.Join(", ", typeGroups)}";
        }

        return $@"You are a survey research analyst. Generate a brief, professional executive summary (2-3 sentences) for this project:

Project: {projectName ?? "Survey Research Project"}
Total Surveys: {totalSurveys}
Total Questions: {totalQuestions}
Total Respondents: {totalCases}{questionsContext}{typesContext}

Analyze the actual survey content and provide specific insights about what this research is studying (e.g., political polling, customer satisfaction, market research). Comment on data quality, sample adequacy, and research objectives based on the questions shown. Be specific and actionable.";
    }

    private string BuildInsightsPrompt(string surveyName, int questionCount, int caseCount, List<string>? topQuestionLabels)
    {
        var questionsText = topQuestionLabels != null && topQuestionLabels.Any()
            ? $"\n\nSample questions:\n{string.Join("\n", topQuestionLabels.Take(5).Select((q, i) => $"{i + 1}. {q}"))}"
            : "";

        return $@"You are a survey research analyst. Provide brief insights (2-3 sentences) about this survey:

Survey: {surveyName}
Questions: {questionCount}
Respondents: {caseCount}{questionsText}

Comment on sample size, survey complexity, and any initial observations about the research scope.";
    }

    public async Task<string> CallAIAPI(string prompt)
    {
        if (_provider == "openai")
        {
            return await CallOpenAI(prompt);
        }
        else
        {
            return await CallAnthropic(prompt);
        }
    }

    private async Task<string> CallOpenAI(string prompt)
    {
        var request = new
        {
            model = "gpt-4o-mini", // Fast and cost-effective
            messages = new[]
            {
                new { role = "system", content = "You are a professional survey research analyst." },
                new { role = "user", content = prompt }
            },
            max_tokens = 200,
            temperature = 0.7
        };

        var requestJson = JsonSerializer.Serialize(request);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Headers = { { "Authorization", $"Bearer {_apiKey}" } },
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseJson);
        
        return jsonDoc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "Unable to generate insights.";
    }

    private async Task<string> CallAnthropic(string prompt)
    {
        var request = new
        {
            model = "claude-3-haiku-20240307", // Fast and cost-effective
            max_tokens = 200,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var requestJson = JsonSerializer.Serialize(request);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
        {
            Headers = 
            { 
                { "x-api-key", _apiKey },
                { "anthropic-version", "2023-06-01" }
            },
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseJson);
        
        return jsonDoc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "Unable to generate insights.";
    }

    private string GenerateFallbackSummary(int totalSurveys, int totalQuestions, int totalCases, string? projectName)
    {
        var projectText = !string.IsNullOrEmpty(projectName) ? $"The '{projectName}' project" : "This project";
        var sampleQuality = totalCases switch
        {
            < 30 => "a small sample size that may limit statistical power",
            < 100 => "a modest sample size suitable for exploratory analysis",
            < 500 => "a solid sample size providing good statistical reliability",
            _ => "a large sample size enabling robust statistical analysis"
        };

        return $"{projectText} contains {totalSurveys} survey{(totalSurveys != 1 ? "s" : "")} with {totalQuestions} questions and {totalCases} respondents. " +
               $"The data represents {sampleQuality}. " +
               $"Ready for detailed analysis including toplines, crosstabs, and significance testing.";
    }

    private string GenerateFallbackInsights(string surveyName, int questionCount, int caseCount)
    {
        var complexity = questionCount switch
        {
            < 10 => "short",
            < 30 => "moderate-length",
            < 50 => "comprehensive",
            _ => "extensive"
        };

        var power = caseCount switch
        {
            < 30 => "limited",
            < 100 => "adequate",
            _ => "strong"
        };

        return $"The '{surveyName}' survey is a {complexity} instrument with {questionCount} questions. " +
               $"With {caseCount} respondents, the data provides {power} statistical power for most analyses.";
    }
}
