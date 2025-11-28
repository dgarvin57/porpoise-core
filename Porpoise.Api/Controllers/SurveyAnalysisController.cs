using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using System.Text;

namespace Porpoise.Api.Controllers;

[ApiController]
[Route("api/survey-analysis")]
public class SurveyAnalysisController : ControllerBase
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly AIInsightsService _aiService;

    public SurveyAnalysisController(
        ISurveyRepository surveyRepository,
        AIInsightsService aiService)
    {
        _surveyRepository = surveyRepository;
        _aiService = aiService;
    }

    /// <summary>
    /// Get AI-powered summary for a survey
    /// </summary>
    [HttpGet("{surveyId:guid}/summary")]
    public async Task<IActionResult> GetSurveySummary(Guid surveyId)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
                return NotFound($"Survey with ID {surveyId} not found");

            // Check if survey has questions and data
            if (survey.QuestionList == null || survey.QuestionList.Count == 0)
                return BadRequest("Survey has no questions");

            if (survey.Data == null || survey.Data.DataList == null || survey.Data.DataList.Count == 0)
                return BadRequest("Survey has no response data");

            var totalQuestions = survey.QuestionList.Count;
            var totalCases = survey.Data.DataList.Count - 1; // Exclude header row

            // Gather sample questions and types for AI context
            var sampleQuestionLabels = survey.QuestionList.Take(10).Select(q => q.QstLabel).ToList();
            var questionTypes = survey.QuestionList.Select(q => ((int)q.VariableType) switch
            {
                1 => "Single Choice",
                2 => "Multiple Choice",
                3 => "Numeric",
                4 => "Open-ended",
                _ => "Other"
            }).ToList();

            // Generate AI summary
            var aiSummary = await _aiService.GenerateSurveySummary(
                1,
                totalQuestions,
                totalCases,
                survey.SurveyName,
                sampleQuestionLabels,
                questionTypes
            );

            return Ok(new
            {
                SurveyId = surveyId,
                SurveyName = survey.SurveyName,
                TotalQuestions = totalQuestions,
                TotalCases = totalCases,
                AISummary = aiSummary
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error generating survey summary: {ex.Message}");
        }
    }

    /// <summary>
    /// Get topline results with AI insights for a survey
    /// </summary>
    [HttpGet("{surveyId:guid}/topline")]
    public async Task<IActionResult> GetToplineResults(Guid surveyId, [FromQuery] int maxQuestions = 5)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
                return NotFound($"Survey with ID {surveyId} not found");

            if (survey.QuestionList == null || survey.QuestionList.Count == 0)
                return BadRequest("Survey has no questions");

            if (survey.Data == null)
                return BadRequest("Survey has no response data");

            // Calculate statistics for first N questions
            var questionsToAnalyze = survey.QuestionList.Take(maxQuestions).ToList();
            var toplineResults = new List<object>();

            foreach (var question in questionsToAnalyze)
            {
                if (question.Responses == null || question.Responses.Count == 0)
                    continue;

                QuestionEngine.CalculateQuestionAndResponseStatistics(survey, question);

                toplineResults.Add(new
                {
                    question.QstLabel,
                    TotalN = question.TotalN,
                    Responses = question.Responses.Select(r => new
                    {
                        Label = r.Label,
                        Frequency = r.ResultFrequency,
                        Percentage = Math.Round((decimal)r.ResultPercent * 100, 1),
                        CumulativePercent = Math.Round((decimal)r.CumPercent * 100, 1),
                        SamplingError = Math.Round((decimal)r.SamplingError, 1)
                    }).ToList()
                });
            }

            // Generate AI insights about the statistics
            var statsContext = string.Join("\n\n", toplineResults.Select(r =>
            {
                var q = r.GetType().GetProperty("QstLabel")?.GetValue(r);
                var responses = r.GetType().GetProperty("Responses")?.GetValue(r);
                return $"Q: {q}\n{string.Join("\n", ((IEnumerable<object>)responses!).Take(3).Select(resp =>
                {
                    var label = resp.GetType().GetProperty("Label")?.GetValue(resp);
                    var pct = resp.GetType().GetProperty("Percentage")?.GetValue(resp);
                    return $"  - {label}: {pct}%";
                }))}";
            }));

            var aiInsights = await _aiService.CallAIAPI($@"You are a survey research analyst. Analyze these topline results and provide 2-3 key insights:

{statsContext}

REQUIREMENTS:
- Always cite specific percentages when discussing findings (e.g., ""45% of respondents..."")
- Identify notable patterns, surprising findings, or actionable insights
- Keep each insight focused and specific to the data shown
- Format as numbered points for clarity",
                temperature: 0.3);

            return Ok(new
            {
                SurveyId = surveyId,
                SurveyName = survey.SurveyName,
                QuestionsAnalyzed = questionsToAnalyze.Count,
                ToplineResults = toplineResults,
                AIInsights = aiInsights
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error generating topline results: {ex.Message}");
        }
    }

    /// <summary>
    /// Get formatted topline report with AI insights
    /// </summary>
    [HttpGet("{surveyId:guid}/topline/formatted")]
    public async Task<IActionResult> GetFormattedToplineReport(Guid surveyId, [FromQuery] int maxQuestions = 10)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
                return NotFound($"Survey with ID {surveyId} not found");

            if (survey.QuestionList == null || survey.QuestionList.Count == 0)
                return BadRequest("Survey has no questions");

            if (survey.Data == null || survey.Data.DataList == null)
                return BadRequest("Survey has no response data");

            var questionsToAnalyze = survey.QuestionList.Take(maxQuestions).ToList();
            var report = new StringBuilder();

            // Header
            report.AppendLine("═══════════════════════════════════════════════════════════════════════");
            report.AppendLine($"                    TOPLINE RESULTS");
            report.AppendLine($"                    {survey.SurveyName}");
            report.AppendLine($"                    {DateTime.Now:MMMM dd, yyyy}");
            report.AppendLine($"                    Base: {survey.Data.DataList.Count - 1} Respondents");
            report.AppendLine("═══════════════════════════════════════════════════════════════════════");
            report.AppendLine();

            var statsForAI = new StringBuilder();

            foreach (var question in questionsToAnalyze)
            {
                if (question.Responses == null || question.Responses.Count == 0)
                    continue;

                QuestionEngine.CalculateQuestionAndResponseStatistics(survey, question);

                // Question label
                report.AppendLine($"{question.QstLabel}");
                report.AppendLine();

                // Column headers
                report.AppendLine($"{"Response",-45} {"Freq",6} {"%-",6} {"Cum%",6} {"±",5}");
                report.AppendLine("─────────────────────────────────────────────────────────────────────");

                // Response rows
                foreach (var response in question.Responses)
                {
                    var label = response.Label.Length > 42 ? response.Label.Substring(0, 42) + "..." : response.Label;
                    var freq = (int)response.ResultFrequency;
                    var pct = response.ResultPercent * 100;
                    var cumPct = response.CumPercent * 100;
                    var sampErr = response.SamplingError;

                    report.AppendLine($"{label,-45} {freq,6} {pct,5:F1}% {cumPct,5:F1}% {sampErr,4:F1}");
                }

                // Total line
                report.AppendLine("─────────────────────────────────────────────────────────────────────");
                report.AppendLine($"{"TOTAL",-45} {question.TotalN,6} {100.0,5:F1}%");
                report.AppendLine("═══════════════════════════════════════════════════════════════════════");
                report.AppendLine();

                // Build statistics context for AI
                statsForAI.AppendLine($"Q: {question.QstLabel}");
                foreach (var response in question.Responses.Take(3))
                {
                    var pct = response.ResultPercent * 100;
                    statsForAI.AppendLine($"  - {response.Label}: {pct:F1}%");
                }
                statsForAI.AppendLine();
            }

            // Generate AI insights
            var aiInsights = await _aiService.CallAIAPI($@"You are a survey research analyst. Analyze these topline results and provide 3-5 key insights.

CRITICAL INSTRUCTIONS:
- Always cite specific percentages from the data
- When comparing numbers, verify which is actually higher
- Double-check all numerical comparisons before making claims
- Format as numbered points
- Be precise and accurate with all statistics

DATA:
{statsForAI}

Provide data-driven insights about patterns, notable findings, and implications:", 0.1);

            // Validate AI insights
            var validatedInsights = ValidateAIInsights(aiInsights, statsForAI.ToString());

            // Add AI insights section
            report.AppendLine();
            report.AppendLine("═══════════════════════════════════════════════════════════════════════");
            report.AppendLine("                         KEY INSIGHTS");
            report.AppendLine("                      (AI-Powered Analysis)");
            report.AppendLine("═══════════════════════════════════════════════════════════════════════");
            report.AppendLine();
            report.AppendLine("NOTE: AI-generated insights. Verify accuracy of interpretations.");
            report.AppendLine();

            // Format AI insights with proper wrapping
            var insightLines = validatedInsights.Split('\n');
            foreach (var line in insightLines)
            {
                if (line.Length <= 71)
                {
                    report.AppendLine(line);
                }
                else
                {
                    var words = line.Split(' ');
                    var currentLine = "";
                    foreach (var word in words)
                    {
                        if ((currentLine + " " + word).Length <= 71)
                        {
                            currentLine = currentLine == "" ? word : currentLine + " " + word;
                        }
                        else
                        {
                            report.AppendLine(currentLine);
                            currentLine = word;
                        }
                    }
                    if (currentLine != "") report.AppendLine(currentLine);
                }
            }

            report.AppendLine();
            report.AppendLine("═══════════════════════════════════════════════════════════════════════");

            return Content(report.ToString(), "text/plain; charset=utf-8");
        }
        catch (Exception ex)
        {
            return Problem($"Error generating formatted report: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method to validate AI insights for numerical errors
    /// </summary>
    private static string ValidateAIInsights(string insights, string rawData)
    {
        var warnings = new List<string>();

        var comparisonPatterns = new[]
        {
            @"(\d+\.?\d*)\%?\s*(is\s+higher|higher|greater|more|exceeds)\s+than\s+(\d+\.?\d*)\%?",
            @"(\d+\.?\d*)\%?\s+(vs\.?|versus)\s+(\d+\.?\d*)\%?"
        };

        foreach (var pattern in comparisonPatterns)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(insights, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (decimal.TryParse(match.Groups[1].Value, out decimal val1) &&
                    decimal.TryParse(match.Groups[3].Value, out decimal val2))
                {
                    if (match.Value.ToLower().Contains("higher") ||
                        match.Value.ToLower().Contains("greater") ||
                        match.Value.ToLower().Contains("more") ||
                        match.Value.ToLower().Contains("exceeds"))
                    {
                        if (val1 < val2)
                        {
                            warnings.Add($"[⚠ VALIDATION: {val1}% is NOT higher than {val2}%]");
                        }
                    }
                }
            }
        }

        if (warnings.Count > 0)
        {
            return string.Join("\n", warnings) + "\n\n" + insights;
        }

        return insights;
    }
}
