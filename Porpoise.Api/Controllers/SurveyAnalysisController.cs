using Microsoft.AspNetCore.Mvc;
using Porpoise.Api.Configuration;
using Porpoise.Api.Services;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;

namespace Porpoise.Api.Controllers;

[ApiController]
[Route("api/survey-analysis")]
public class SurveyAnalysisController : ControllerBase
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly ISurveyDataRepository _surveyDataRepository;
    private readonly IResponseRepository _responseRepository;
    private readonly AIInsightsService _aiService;
    private readonly AiPromptConfiguration _promptConfig;
    private readonly PromptTemplateEngine _templateEngine;

    public SurveyAnalysisController(
        ISurveyRepository surveyRepository,
        IQuestionRepository questionRepository,
        ISurveyDataRepository surveyDataRepository,
        IResponseRepository responseRepository,
        AIInsightsService aiService,
        AiPromptConfiguration promptConfig,
        PromptTemplateEngine templateEngine)
    {
        _surveyRepository = surveyRepository;
        _questionRepository = questionRepository;
        _surveyDataRepository = surveyDataRepository;
        _responseRepository = responseRepository;
        _aiService = aiService;
        _promptConfig = promptConfig;
        _templateEngine = templateEngine;
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

    /// <summary>
    /// Generate crosstab analysis between two questions
    /// </summary>
    [HttpPost("{surveyId:guid}/crosstab")]
    public async Task<IActionResult> GetCrosstab(
        Guid surveyId,
        [FromBody] CrosstabRequest request)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
                return NotFound($"Survey with ID {surveyId} not found");

            // Load questions from database
            var questions = await _questionRepository.GetBySurveyIdAsync(surveyId);
            var questionsList = questions.ToList();
            
            if (questionsList.Count == 0)
                return BadRequest("Survey has no questions");

            // Load survey data
            var surveyData = await _surveyDataRepository.GetBySurveyIdAsync(surveyId);
            
            if (surveyData == null || surveyData.DataList == null || surveyData.DataList.Count == 0)
                return BadRequest("Survey has no response data");

            // Find the two questions
            var firstQuestion = questionsList.FirstOrDefault(q => q.Id == request.FirstQuestionId);
            var secondQuestion = questionsList.FirstOrDefault(q => q.Id == request.SecondQuestionId);

            if (firstQuestion == null)
                return NotFound($"First question with ID {request.FirstQuestionId} not found");

            if (secondQuestion == null)
                return NotFound($"Second question with ID {request.SecondQuestionId} not found");

            // Set DataFileCol for both questions by finding the column index in survey data
            if (surveyData.DataList != null && surveyData.DataList.Count > 0)
            {
                var headerRow = surveyData.DataList[0];
                
                // DEFENSIVE: Remove duplicate header rows if they exist (rows 1+ that match row 0)
                // This prevents parsing errors when header row is accidentally duplicated in data
                for (int i = surveyData.DataList.Count - 1; i >= 1; i--)
                {
                    var row = surveyData.DataList[i];
                    // Check if this row matches the header (all cells are question numbers/column names)
                    bool isDuplicateHeader = true;
                    if (row.Count == headerRow.Count)
                    {
                        for (int j = 0; j < row.Count && j < headerRow.Count; j++)
                        {
                            if (!string.Equals(row[j], headerRow[j], StringComparison.OrdinalIgnoreCase))
                            {
                                isDuplicateHeader = false;
                                break;
                            }
                        }
                        if (isDuplicateHeader)
                        {
                            Console.WriteLine($"WARNING: Removing duplicate header row at index {i}");
                            surveyData.DataList.RemoveAt(i);
                        }
                    }
                }
                
                int firstQuestionIndex = headerRow.FindIndex(col => 
                    string.Equals(col, firstQuestion.QstNumber, StringComparison.OrdinalIgnoreCase));
                int secondQuestionIndex = headerRow.FindIndex(col => 
                    string.Equals(col, secondQuestion.QstNumber, StringComparison.OrdinalIgnoreCase));
                
                if (firstQuestionIndex < 0)
                {
                    var availableCols = string.Join(", ", headerRow);
                    return BadRequest($"First question number '{firstQuestion.QstNumber}' not found in survey data header. Available columns: {availableCols}");
                }
                    
                if (secondQuestionIndex < 0)
                {
                    var availableCols = string.Join(", ", headerRow);
                    return BadRequest($"Second question number '{secondQuestion.QstNumber}' not found in survey data header. Available columns: {availableCols}");
                }
                
                firstQuestion.DataFileCol = (short)firstQuestionIndex;
                secondQuestion.DataFileCol = (short)secondQuestionIndex;
            }

            // Load responses for both questions
            var firstResponses = await _responseRepository.GetByQuestionIdAsync(firstQuestion.Id);
            var firstResponseList = new ObjectListBase<Response>();
            foreach (var resp in firstResponses)
            {
                firstResponseList.Add(resp);
            }
            firstQuestion.Responses = firstResponseList;

            var secondResponses = await _responseRepository.GetByQuestionIdAsync(secondQuestion.Id);
            var secondResponseList = new ObjectListBase<Response>();
            foreach (var resp in secondResponses)
            {
                secondResponseList.Add(resp);
            }
            secondQuestion.Responses = secondResponseList;

            // Validate that questions are suitable for crosstab
            // Crosstab requires categorical questions with numeric response values
            if (firstQuestion.Responses.Count == 0)
                return BadRequest($"First question '{firstQuestion.QstLabel}' has no response categories defined. Crosstab analysis requires categorical questions.");
            
            if (secondQuestion.Responses.Count == 0)
                return BadRequest($"Second question '{secondQuestion.QstLabel}' has no response categories defined. Crosstab analysis requires categorical questions.");

            // Create crosstab with error handling
            Crosstab crosstab;
            try
            {
                crosstab = new Crosstab(surveyData, firstQuestion, secondQuestion, false, false);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating crosstab: {ex.Message}. This may occur if the selected questions contain non-numeric or incompatible data.");
            }

            // Helper function to sanitize double values (replace infinity/NaN with null)
            double? SanitizeDouble(double value)
            {
                if (double.IsInfinity(value) || double.IsNaN(value))
                    return null;
                return value;
            }

            // Build response
            var response = new CrosstabResponse
            {
                FirstQuestion = new QuestionInfo
                {
                    Id = firstQuestion.Id,
                    Label = firstQuestion.QstLabel,
                    VariableType = (int)firstQuestion.VariableType
                },
                SecondQuestion = new QuestionInfo
                {
                    Id = secondQuestion.Id,
                    Label = secondQuestion.QstLabel,
                    VariableType = (int)secondQuestion.VariableType
                },
                TotalN = crosstab.TotalN,
                ChiSquare = SanitizeDouble(crosstab.ChiSquare),
                PValue = SanitizeDouble(crosstab.PValue),
                Significant = crosstab.Significant,
                ChiSquareSignificant = crosstab.Significant.Contains("Significant", StringComparison.OrdinalIgnoreCase) && !crosstab.Significant.Contains("Not", StringComparison.OrdinalIgnoreCase),
                Phi = SanitizeDouble(crosstab.Phi),
                ContingencyCoefficient = SanitizeDouble(crosstab.ContingencyCoefficient),
                CramersV = SanitizeDouble(crosstab.CramersV),
                Table = ConvertDataTableToList(crosstab.CxTable),
                IVIndexes = crosstab.CxIVIndexes.Select(idx => new IVIndexInfo
                {
                    Label = idx.IVLabel,
                    Index = SanitizeDouble(idx.Index),
                    PosIndex = SanitizeDouble(idx.PosIndex),
                    NegIndex = SanitizeDouble(idx.NegIndex)
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating crosstab: {ex.Message}");
        }
    }

    /// <summary>
    /// Get statistical significance data for a question compared to all other questions
    /// </summary>
    [HttpGet("{surveyId:guid}/statistical-significance/{questionId:guid}")]
    public async Task<IActionResult> GetStatisticalSignificance(Guid surveyId, Guid questionId)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
                return NotFound($"Survey with ID {surveyId} not found");

            var selectedQuestion = await _questionRepository.GetByIdAsync(questionId);
            if (selectedQuestion == null)
                return NotFound($"Question with ID {questionId} not found");

            // Get survey data
            var surveyData = await _surveyDataRepository.GetBySurveyIdAsync(surveyId);
            if (surveyData == null)
                return NotFound($"No data found for survey {surveyId}");

            // Get all questions from the repository (not from survey.QuestionList which may be null)
            var allQuestionsEnumerable = await _questionRepository.GetBySurveyIdAsync(surveyId);
            var allQuestions = allQuestionsEnumerable.ToList();
            
            // Set DataFileCol for all questions by finding column indices in survey data
            if (surveyData.DataList != null && surveyData.DataList.Count > 0)
            {
                var headerRow = surveyData.DataList[0];
                
                // Remove duplicate header rows if they exist
                for (int i = surveyData.DataList.Count - 1; i >= 1; i--)
                {
                    var row = surveyData.DataList[i];
                    bool isDuplicateHeader = true;
                    if (row.Count == headerRow.Count)
                    {
                        for (int j = 0; j < row.Count && j < headerRow.Count; j++)
                        {
                            if (!string.Equals(row[j], headerRow[j], StringComparison.OrdinalIgnoreCase))
                            {
                                isDuplicateHeader = false;
                                break;
                            }
                        }
                        if (isDuplicateHeader)
                        {
                            surveyData.DataList.RemoveAt(i);
                        }
                    }
                }

                // Map DataFileCol for selected question
                int selectedQuestionIndex = headerRow.FindIndex(col => 
                    string.Equals(col, selectedQuestion.QstNumber, StringComparison.OrdinalIgnoreCase));
                if (selectedQuestionIndex >= 0)
                {
                    selectedQuestion.DataFileCol = (short)selectedQuestionIndex;
                }

                // Map DataFileCol for all other questions
                foreach (var q in allQuestions)
                {
                    int qIndex = headerRow.FindIndex(col => 
                        string.Equals(col, q.QstNumber, StringComparison.OrdinalIgnoreCase));
                    if (qIndex >= 0)
                    {
                        q.DataFileCol = (short)qIndex;
                    }
                }
            }
            
            // Load responses for the selected question
            var selectedQuestionResponses = await _responseRepository.GetByQuestionIdAsync(questionId);
            selectedQuestion.Responses = new ObjectListBase<Response>(selectedQuestionResponses);
            
            // Build list of statistical significance items
            var statSigItems = new List<StatSigItemDto>();

            foreach (var compareQuestion in allQuestions)
            {
                // Skip comparing question to itself
                if (compareQuestion.Id == questionId)
                    continue;

                // Only compare against Independent variables (demographics/classifiers)
                if (compareQuestion.VariableType != QuestionVariableType.Independent)
                    continue;
                
                // Load responses for this compare question
                var compareQuestionResponses = await _responseRepository.GetByQuestionIdAsync(compareQuestion.Id);
                compareQuestion.Responses = new ObjectListBase<Response>(compareQuestionResponses);

                // Skip if either question doesn't have responses
                if (selectedQuestion.Responses == null || selectedQuestion.Responses.Count == 0 ||
                    compareQuestion.Responses == null || compareQuestion.Responses.Count == 0)
                {
                    continue;
                }

                try
                {
                    // Create crosstab to calculate statistical significance
                    // Note: compareQuestion is the DV, selectedQuestion is the IV
                    var crosstab = new Crosstab(surveyData, compareQuestion, selectedQuestion, false, false);
                    
                    // Get stat sig item from crosstab
                    var statSigItem = crosstab.GetStatSigItems();
                    
                    // Include ALL IVs (not just significant ones), but only if we have valid data
                    if (crosstab.TotalN > 0 && !double.IsNaN(statSigItem.Phi))
                    {
                        statSigItems.Add(new StatSigItemDto
                        {
                            Id = compareQuestion.Id,
                            QuestionLabel = compareQuestion.QstLabel,
                            Phi = statSigItem.Phi,
                            Significance = statSigItem.Significance
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Log but continue processing other questions
                    continue;
                }
            }

            // Sort by Phi value descending (strongest relationships first)
            statSigItems = statSigItems.OrderByDescending(x => x.Phi).ToList();

            return Ok(statSigItems);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error calculating statistical significance: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate AI analysis for a single question's results
    /// </summary>
    [HttpPost("{surveyId:guid}/analyze-question")]
    public async Task<IActionResult> AnalyzeQuestion(Guid surveyId, [FromBody] QuestionAnalysisRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.QuestionLabel))
            {
                return BadRequest("Invalid request: missing question data");
            }
            
            // Build context for AI analysis from provided data
            var prompt = BuildQuestionAnalysisPrompt(request);
            var analysis = await _aiService.CallAIAPI(prompt);
            
            return Ok(new { analysis });
        }
        catch (Exception ex)
        {
            // Return a user-friendly fallback message instead of exposing the error
            var fallbackMessage = "Unable to generate AI analysis at this time. The statistical results show the key findings from your data.";
            return Ok(new { analysis = fallbackMessage });
        }
    }

    private string BuildQuestionAnalysisPrompt(QuestionAnalysisRequest request)
    {
        // Build context dictionary for template
        var context = new Dictionary<string, object>
        {
            { "QuestionLabel", request.QuestionLabel },
            { "TotalN", request.TotalN },
            { "Responses", request.Responses.OrderByDescending(r => r.Frequency).Select(r => new Dictionary<string, object>
                {
                    { "label", r.Label },
                    { "frequency", r.Frequency },
                    { "percent", $"{r.Percent:F1}" }
                }).ToList()
            }
        };

        // Use template from configuration
        return _templateEngine.Render(_promptConfig.QuestionAnalysis.Template, context);
    }

    /// <summary>
    /// Generate AI analysis for crosstab results
    /// </summary>
    [HttpPost("{surveyId:guid}/analyze-crosstab")]
    public async Task<IActionResult> AnalyzeCrosstab(Guid surveyId, [FromBody] CrosstabAnalysisRequest request)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
                return NotFound($"Survey with ID {surveyId} not found");

            // Build context for AI analysis
            var prompt = BuildCrosstabAnalysisPrompt(request);
            
            var analysis = await _aiService.CallAIAPI(prompt);
            
            return Ok(new { analysis });
        }
        catch (Exception ex)
        {
            // Return a user-friendly fallback message instead of exposing the error
            var fallbackMessage = "Unable to generate AI analysis at this time. The statistical results above show the key findings from your crosstab.";
            return Ok(new { analysis = fallbackMessage });
        }
    }

    [HttpPost("{surveyId:guid}/analyze-statsig")]
    public async Task<IActionResult> AnalyzeStatisticalSignificance(Guid surveyId, [FromBody] StatSigAnalysisRequest request)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(surveyId);
            if (survey == null)
                return NotFound($"Survey with ID {surveyId} not found");

            // Build context for AI analysis
            var prompt = BuildStatSigAnalysisPrompt(request);
            
            var analysis = await _aiService.CallAIAPI(prompt);
            
            return Ok(new { analysis });
        }
        catch (Exception ex)
        {
            // Return a user-friendly fallback message instead of exposing the error
            var fallbackMessage = "Unable to generate AI analysis at this time. The statistical significance results show the key relationships in your data.";
            return Ok(new { analysis = fallbackMessage });
        }
    }

    private string BuildStatSigAnalysisPrompt(StatSigAnalysisRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a data analyst reviewing statistical significance results for a survey question.");
        sb.AppendLine();
        sb.AppendLine($"The survey question being analyzed is: {request.QuestionLabel}");
        sb.AppendLine();
        sb.AppendLine("The following independent variables (demographics/classifiers) were tested for their relationship with this question:");
        sb.AppendLine();
        
        foreach (var item in request.StatSigData.OrderByDescending(x => x.Phi))
        {
            sb.AppendLine($"- {item.Variable}: Phi = {item.Phi:F3}, {item.Significance}");
        }
        
        sb.AppendLine();
        sb.AppendLine("Please provide a concise analysis with these sections:");
        sb.AppendLine("## Key Findings");
        sb.AppendLine("Identify the top 2-3 variables with the strongest relationships and what this means.");
        sb.AppendLine();
        sb.AppendLine("## Statistical Interpretation");
        sb.AppendLine("Explain the significance levels and what they tell us about the reliability of these relationships.");
        sb.AppendLine();
        sb.AppendLine("## Recommendations");
        sb.AppendLine("Suggest which variables would be most valuable to explore further through crosstab analysis.");
        sb.AppendLine();
        sb.AppendLine("Keep the analysis practical and actionable for survey researchers.");
        
        return sb.ToString();
    }

    private string BuildCrosstabAnalysisPrompt(CrosstabAnalysisRequest request)
    {
        // Build context dictionary for template
        var context = new Dictionary<string, object>
        {
            { "DependentVariable", request.DependentVariable },
            { "IndependentVariable", request.IndependentVariable },
            { "TotalN", request.TotalN },
            { "ChiSquare", $"{request.ChiSquare:F3}" },
            { "PValue", request.ChiSquareSignificant ? "<0.05" : "≥0.05" },
            { "CramersV", $"{request.CramersV:F3}" }
        };

        // Add rows data if indexes are provided
        if (request.Indexes != null && request.Indexes.Count > 0)
        {
            var rows = request.Indexes.Select(idx => new Dictionary<string, object>
            {
                { "rowLabel", idx.Label },
                { "index", $"{idx.Index:F0}" },
                { "posIndex", $"{idx.PosIndex:F1}" },
                { "negIndex", $"{idx.NegIndex:F1}" },
                { "sentiment", idx.Index > 100 ? "positive" : idx.Index < 100 ? "negative" : "neutral" }
            }).ToList();
            
            context["Rows"] = rows;
        }
        else
        {
            context["Rows"] = new List<Dictionary<string, object>>();
        }

        // Use template from configuration
        return _templateEngine.Render(_promptConfig.CrosstabAnalysis.Template, context);
    }

    private List<Dictionary<string, object>> ConvertDataTableToList(DataTable? table)
    {
        if (table == null) return new List<Dictionary<string, object>>();

        var result = new List<Dictionary<string, object>>();
        
        foreach (DataRow row in table.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in table.Columns)
            {
                // Replace auto-generated column names like "Column1" with a space to keep data but hide header
                string columnName = col.ColumnName.StartsWith("Column", StringComparison.OrdinalIgnoreCase) 
                    ? " " 
                    : col.ColumnName;
                dict[columnName] = row[col] ?? DBNull.Value;
            }
            result.Add(dict);
        }

        return result;
    }
}

public class CrosstabRequest
{
    public Guid FirstQuestionId { get; set; }
    public Guid SecondQuestionId { get; set; }
}

public class CrosstabResponse
{
    public QuestionInfo FirstQuestion { get; set; } = null!;
    public QuestionInfo SecondQuestion { get; set; } = null!;
    public int TotalN { get; set; }
    public double? ChiSquare { get; set; }
    public double? PValue { get; set; }
    public string Significant { get; set; } = string.Empty;
    public bool ChiSquareSignificant { get; set; } // Boolean version for AI analysis
    public double? Phi { get; set; }
    public double? ContingencyCoefficient { get; set; }
    public double? CramersV { get; set; }
    public List<Dictionary<string, object>> Table { get; set; } = new();
    public List<IVIndexInfo> IVIndexes { get; set; } = new();
}

public class QuestionInfo
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public int VariableType { get; set; }
}

public class IVIndexInfo
{
    public string Label { get; set; } = string.Empty;
    public double? Index { get; set; }
    public double? PosIndex { get; set; }
    public double? NegIndex { get; set; }
}

public class QuestionAnalysisRequest
{
    [JsonPropertyName("questionLabel")]
    public string QuestionLabel { get; set; } = string.Empty;
    
    [JsonPropertyName("totalN")]
    public int TotalN { get; set; }
    
    [JsonPropertyName("responses")]
    public List<ResponseData> Responses { get; set; } = new();
}

public class ResponseData
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
    
    [JsonPropertyName("frequency")]
    public int Frequency { get; set; }
    
    [JsonPropertyName("percent")]
    public double Percent { get; set; }
}

public class CrosstabAnalysisRequest
{
    public string IndependentVariable { get; set; } = string.Empty;
    public string DependentVariable { get; set; } = string.Empty;
    public int TotalN { get; set; }
    public double ChiSquare { get; set; }
    public bool ChiSquareSignificant { get; set; }
    public double Phi { get; set; }
    public double CramersV { get; set; }
    public List<IVIndexInfo>? Indexes { get; set; }
}

public class StatSigItemDto
{
    public Guid Id { get; set; }
    public string QuestionLabel { get; set; } = string.Empty;
    public double Phi { get; set; }
    public string Significance { get; set; } = string.Empty;
}

public class StatSigAnalysisRequest
{
    public string QuestionLabel { get; set; } = string.Empty;
    public List<StatSigDataItem> StatSigData { get; set; } = new();
}

public class StatSigDataItem
{
    public string Variable { get; set; } = string.Empty;
    public double Phi { get; set; }
    public string Significance { get; set; } = string.Empty;
}
