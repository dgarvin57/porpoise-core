using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using System.Data;
using System.Text;

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

    public SurveyAnalysisController(
        ISurveyRepository surveyRepository,
        IQuestionRepository questionRepository,
        ISurveyDataRepository surveyDataRepository,
        IResponseRepository responseRepository,
        AIInsightsService aiService)
    {
        _surveyRepository = surveyRepository;
        _questionRepository = questionRepository;
        _surveyDataRepository = surveyDataRepository;
        _responseRepository = responseRepository;
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
                ChiSquare = crosstab.ChiSquare,
                PValue = crosstab.PValue,
                Significant = crosstab.Significant,
                ChiSquareSignificant = crosstab.Significant.Contains("Significant", StringComparison.OrdinalIgnoreCase) && !crosstab.Significant.Contains("Not", StringComparison.OrdinalIgnoreCase),
                Phi = crosstab.Phi,
                ContingencyCoefficient = crosstab.ContingencyCoefficient,
                CramersV = crosstab.CramersV,
                Table = ConvertDataTableToList(crosstab.CxTable),
                IVIndexes = crosstab.CxIVIndexes.Select(idx => new IVIndexInfo
                {
                    Label = idx.IVLabel,
                    Index = idx.Index,
                    PosIndex = idx.PosIndex,
                    NegIndex = idx.NegIndex
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
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private string BuildCrosstabAnalysisPrompt(CrosstabAnalysisRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("CRITICAL: You are analyzing real survey data. Accuracy is paramount. Any error will damage user trust.");
        sb.AppendLine();
        sb.AppendLine("=== DATA (DO NOT ALTER OR MISINTERPRET) ===");
        sb.AppendLine($"Independent Variable: {request.IndependentVariable}");
        sb.AppendLine($"Dependent Variable: {request.DependentVariable}");
        sb.AppendLine($"Sample Size (N): {request.TotalN}");
        sb.AppendLine();
        
        // Make significance status crystal clear with explicit boolean check
        var significanceStatus = request.ChiSquareSignificant 
            ? "STATISTICALLY SIGNIFICANT (p<0.05)" 
            : "NOT STATISTICALLY SIGNIFICANT (p≥0.05)";
        var relationshipStatement = request.ChiSquareSignificant
            ? "There IS a statistically significant relationship between these variables."
            : "There is NO statistically significant relationship between these variables.";
            
        sb.AppendLine($"Chi-Square Test: {request.ChiSquare:F3}");
        sb.AppendLine($"Result: {significanceStatus}");
        sb.AppendLine($"INTERPRETATION: {relationshipStatement}");
        sb.AppendLine($"Cramér's V: {request.CramersV:F3} (0=no association, 0.1=weak, 0.3=moderate, 0.5=strong)");
        sb.AppendLine();
        
        if (request.Indexes != null && request.Indexes.Count > 0)
        {
            sb.AppendLine("Index Values by Category (Scale: 0-200, where 100=neutral):");
            foreach (var idx in request.Indexes)
            {
                var sentiment = idx.Index > 100 ? "positive" : idx.Index < 100 ? "negative" : "neutral";
                sb.AppendLine($"  • {idx.Label}: Index={idx.Index} ({sentiment}), Positive={idx.PosIndex:F1}%, Negative={idx.NegIndex:F1}%");
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("=== OUTPUT FORMAT (REQUIRED) ===");
        sb.AppendLine("Write exactly 4 paragraphs, separated by blank lines:");
        sb.AppendLine();
        sb.AppendLine($"Paragraph 1 (Summary): Write ONE sentence that briefly summarizes the key finding or meaning of this crosstab analysis. This should be a clear, concise statement about the relationship between {request.IndependentVariable} and {request.DependentVariable}.");
        sb.AppendLine();
        sb.AppendLine($"Paragraph 2 (Statistical Result): Start with this EXACT phrase: 'The analysis {(request.ChiSquareSignificant ? "shows a statistically significant relationship" : "found no statistically significant relationship")} between {request.IndependentVariable} and {request.DependentVariable}.' Then explain what this means in 1-2 additional sentences.");
        sb.AppendLine();
        sb.AppendLine("Paragraph 3 (Category Comparison): Compare the categories. State which has the highest index value and which has the lowest, citing the EXACT numbers from the data above. Use format: 'The [category name] category shows the highest sentiment (Index=[number], [X]% positive), while [category name] has the lowest (Index=[number], [X]% positive).'");
        sb.AppendLine();
        sb.AppendLine("Paragraph 4 (Actionable Insight): Provide one actionable insight for decision-makers based on the actual data patterns.");
        sb.AppendLine();
        sb.AppendLine("VALIDATION RULES:");
        sb.AppendLine($"- Paragraph 2 MUST state '{(request.ChiSquareSignificant ? "significant relationship" : "no significant relationship")}'");
        sb.AppendLine("- All numbers MUST exactly match the data above");
        sb.AppendLine("- Do NOT invent or estimate any values");
        sb.AppendLine("- Separate paragraphs with blank lines (\\n\\n)");
        
        return sb.ToString();
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
    public double ChiSquare { get; set; }
    public double PValue { get; set; }
    public string Significant { get; set; } = string.Empty;
    public bool ChiSquareSignificant { get; set; } // Boolean version for AI analysis
    public double Phi { get; set; }
    public double ContingencyCoefficient { get; set; }
    public double CramersV { get; set; }
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
    public int Index { get; set; }
    public double PosIndex { get; set; }
    public double NegIndex { get; set; }
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
