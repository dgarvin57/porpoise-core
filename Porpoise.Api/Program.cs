// Porpoise.Api/Program.cs — NO SWAGGER, JUST PURE API
using Porpoise.Api.Mocks;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Data;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.DataAccess.Context;
using Porpoise.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Register Data Access Layer (with option to use in-memory for testing)
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

if (useInMemory)
{
    // Use in-memory repository for testing without MySQL
    var inMemoryRepo = new InMemorySurveyRepository();
    
    // Seed with sample data
    await inMemoryRepo.AddAsync(new Survey 
    { 
        SurveyName = "Demo Survey 2024", 
        Status = SurveyStatus.Verified,
        SurveyNotes = "Sample survey for testing"
    });
    await inMemoryRepo.AddAsync(new Survey 
    { 
        SurveyName = "Customer Satisfaction Q4", 
        Status = SurveyStatus.Initial,
        SurveyNotes = "Q4 customer feedback"
    });
    
    builder.Services.AddSingleton<ISurveyRepository>(inMemoryRepo);
    Console.WriteLine("✅ Using IN-MEMORY database (no MySQL required)");
}
else
{
    // Use real Dapper + MySQL
    var connectionString = builder.Configuration.GetConnectionString("PorpoiseDb") 
        ?? throw new InvalidOperationException("Connection string 'PorpoiseDb' not found");
    
    builder.Services.AddSingleton(new DapperContext(connectionString));
    builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
    builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    Console.WriteLine("✅ Using MYSQL database");
}

// Register AI service
builder.Services.AddHttpClient<AIInsightsService>();
builder.Services.AddSingleton<AIInsightsService>(sp => 
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    var provider = builder.Configuration["AI:Provider"] ?? "openai";
    var apiKey = builder.Configuration["AI:ApiKey"] ?? 
                 Environment.GetEnvironmentVariable(provider == "anthropic" ? "ANTHROPIC_API_KEY" : "OPENAI_API_KEY");
    return new AIInsightsService(httpClient, apiKey, provider);
});

var app = builder.Build();

app.UseCors();

// Your real project data - support multiple surveys
Project? currentProject = null;
List<(Survey Survey, SurveyData Data)> currentSurveys = new();

// Define paths project
var basePath = Path.Combine(AppContext.BaseDirectory, "SampleData");
var projectPath = Path.Combine(basePath, "Demo 2015.porp");

// Define survey file sets (can add more as needed)
var surveyFileSets = new List<(string SurveyPath, string DataPath)>
{
    (Path.Combine(basePath, "Demo 2015.porps"), Path.Combine(basePath, "Demo 2015.porpd"))
    // Add more survey/data file pairs here as needed:
    // (Path.Combine(basePath, "Wave2.porps"), Path.Combine(basePath, "Wave2.porpd")),
};

// Auto-load project on startup
try
{
    foreach (var (surveyPath, dataPath) in surveyFileSets)
    {
        if (File.Exists(surveyPath) && File.Exists(dataPath))
        {
            var (survey, project, data) = ProjectLoader.LoadProject(surveyPath, projectPath, dataPath);
            if (currentProject == null)
                currentProject = project;
            currentSurveys.Add((survey, data));
        }
    }
    if (currentSurveys.Count > 0)
        Console.WriteLine($"✓ Auto-loaded {currentSurveys.Count} survey(s) on startup");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠ Warning: Could not auto-load surveys: {ex.Message}");
}

// DEBUG ENDPOINT — shows raw file content
app.MapGet("/debug/file", () =>
{
    var path = Path.Combine(AppContext.BaseDirectory, "SampleData", "MySurvey.porps");
    if (!File.Exists(path)) return Results.NotFound("File not found");

    var raw = File.ReadAllText(path);
    return Results.Json(new
    {
        Length = raw.Length,
        First100 = raw.Length > 100 ? raw[..100] : raw,
        StartsWithPorpoiseBinary = raw.StartsWith("PORPOISEBINARY")
    });
});

app.MapGet("/api/survey", async (AIInsightsService aiService) =>
{
    try
    {
        if (currentSurveys.Count == 0)
        {
            // Load all survey/data file pairs
            foreach (var (surveyPath, dataPath) in surveyFileSets)
            {
                if (File.Exists(surveyPath) && File.Exists(dataPath))
                {
                    var (survey, project, data) = ProjectLoader.LoadProject(surveyPath, projectPath, dataPath);
                    if (currentProject == null)
                        currentProject = project;
                    currentSurveys.Add((survey, data));
                }
            }
        }

        // Calculate statistics (your existing statistical engines do this)
        var totalSurveys = currentSurveys.Count;
        var totalQuestions = currentSurveys.Sum(s => s.Survey.QuestionList.Count);
        var totalCases = currentSurveys.Sum(s => s.Data.DataList.Count - 1);

        // Gather sample questions and types for AI context
        var allQuestions = currentSurveys.SelectMany(s => s.Survey.QuestionList).ToList();
        var sampleQuestionLabels = allQuestions.Take(10).Select(q => q.QstLabel).ToList();
        var questionTypes = allQuestions.Select(q => ((int)q.VariableType) switch
        {
            1 => "Single Choice",
            2 => "Multiple Choice",
            3 => "Numeric",
            4 => "Open-ended",
            _ => "Other"
        }).ToList();

        // Generate AI summary (AI explains what your calculations mean)
        var aiSummary = await aiService.GenerateSurveySummary(
            totalSurveys, 
            totalQuestions, 
            totalCases, 
            currentProject?.ProjectName,
            sampleQuestionLabels,
            questionTypes
        );

        var result = new
        {
            // Summary statistics (calculated by Porpoise)
            TotalSurveys = totalSurveys,
            TotalQuestions = totalQuestions,
            TotalCases = totalCases,
            
            // Project info
            ProjectName = currentProject?.ProjectName,
            
            // AI-generated insights (explains the statistics above)
            AISummary = aiSummary,
            
            // Surveys detail
            Surveys = currentSurveys.Select(s => new
            {
                SurveyName = s.Survey.SurveyName,
                QuestionCount = s.Survey.QuestionList.Count,
                CaseCount = s.Data.DataList.Count - 1,
                Questions = s.Survey.QuestionList.Select(q => new
                {
                    q.Id,
                    q.QstLabel,
                    BlockLabel = q.BlkLabel ?? "",
                    IsBlockStart = q.BlkQstStatus == BlkQuestionStatusType.FirstQuestionInBlock,
                    q.VariableType,
                    q.DataType
                })
            })
        };

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// NEW: Topline analysis with AI insights
app.MapGet("/api/survey/{surveyIndex}/topline", async (int surveyIndex, AIInsightsService aiService) =>
{
    try
    {
        if (currentSurveys.Count == 0)
        {
            // Load surveys if not already loaded
            foreach (var (surveyPath, dataPath) in surveyFileSets)
            {
                if (File.Exists(surveyPath) && File.Exists(dataPath))
                {
                    var (survey, project, data) = ProjectLoader.LoadProject(surveyPath, projectPath, dataPath);
                    if (currentProject == null)
                        currentProject = project;
                    currentSurveys.Add((survey, data));
                }
            }
        }

        if (surveyIndex < 0 || surveyIndex >= currentSurveys.Count)
            return Results.NotFound($"Survey index {surveyIndex} not found");

        var (selectedSurvey, selectedData) = currentSurveys[surveyIndex];
        
        // Link the data to the survey (required for statistics calculation)
        selectedSurvey.Data = selectedData;
        
        // Calculate statistics for first few questions (YOUR STATISTICAL ENGINE WORKING!)
        var questionsToAnalyze = selectedSurvey.QuestionList.Take(5).ToList();
        var toplineResults = new List<object>();
        
        foreach (var question in questionsToAnalyze)
        {
            // Skip if question has no responses defined
            if (question.Responses == null || question.Responses.Count == 0)
                continue;
                
            // This is your real statistical engine calculating frequencies and percentages
            QuestionEngine.CalculateQuestionAndResponseStatistics(selectedSurvey, question);
            
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

        // Generate AI insights about these actual statistics
        var statsContext = string.Join("\n\n", toplineResults.Select(r => {
            var q = r.GetType().GetProperty("QstLabel")?.GetValue(r);
            var responses = r.GetType().GetProperty("Responses")?.GetValue(r);
            return $"Q: {q}\n{string.Join("\n", ((IEnumerable<object>)responses!).Take(3).Select(resp => {
                var label = resp.GetType().GetProperty("Label")?.GetValue(resp);
                var pct = resp.GetType().GetProperty("Percentage")?.GetValue(resp);
                return $"  - {label}: {pct}%";
            }))}";
        }));

        var aiInsights = await aiService.CallAIAPI($@"You are a survey research analyst. Analyze these topline results and provide 2-3 key insights:

{statsContext}

REQUIREMENTS:
- Always cite specific percentages when discussing findings (e.g., ""45% of respondents..."")
- Identify notable patterns, surprising findings, or actionable insights
- Keep each insight focused and specific to the data shown
- Format as numbered points for clarity", 
            temperature: 0.3); // Lower temperature = more consistent, data-focused responses

        return Results.Ok(new
        {
            SurveyName = selectedSurvey.SurveyName,
            QuestionsAnalyzed = questionsToAnalyze.Count,
            ToplineResults = toplineResults,
            AIInsights = aiInsights
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Helper function to validate AI insights for obvious numerical errors
static string ValidateAIInsights(string insights, string rawData)
{
    var lines = insights.Split('\n').ToList();
    var warnings = new List<string>();
    
    // Extract all percentages from the raw data
    var dataPercentages = new Dictionary<string, decimal>();
    var dataLines = rawData.Split('\n');
    foreach (var line in dataLines)
    {
        // Match pattern like "- Label: 42.7%"
        var match = System.Text.RegularExpressions.Regex.Match(line, @"- (.+?):\s*([\d.]+)%");
        if (match.Success)
        {
            var label = match.Groups[1].Value.Trim();
            var percentage = decimal.Parse(match.Groups[2].Value);
            dataPercentages[label.ToLower()] = percentage;
        }
    }
    
    // Check for obvious comparison errors like "X is higher than Y" when X < Y
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
                // Check if claim of "higher" is actually wrong
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

// FORMATTED TEXT TOPLINE — Returns formatted report with AI insights
app.MapGet("/api/survey/{surveyIndex}/topline/formatted", async (int surveyIndex, AIInsightsService aiService) =>
{
    try
    {
        if (currentProject == null || currentSurveys.Count == 0)
            return Results.NotFound("No project loaded. Call /api/survey first.");

        if (surveyIndex < 0 || surveyIndex >= currentSurveys.Count)
            return Results.NotFound($"Survey index {surveyIndex} not found");

        var (selectedSurvey, selectedData) = currentSurveys[surveyIndex];
        selectedSurvey.Data = selectedData;

        // Calculate statistics for first 10 questions
        var questionsToAnalyze = selectedSurvey.QuestionList.Take(10).ToList();
        var report = new System.Text.StringBuilder();

        // Header
        report.AppendLine("═══════════════════════════════════════════════════════════════════════");
        report.AppendLine($"                    TOPLINE RESULTS");
        report.AppendLine($"                    {selectedSurvey.SurveyName}");
        report.AppendLine($"                    {DateTime.Now:MMMM dd, yyyy}");
        report.AppendLine($"                    Base: {selectedData.DataList.Count - 1} Respondents");
        report.AppendLine("═══════════════════════════════════════════════════════════════════════");
        report.AppendLine();

        // Build statistics for AI analysis
        var statsForAI = new System.Text.StringBuilder();

        foreach (var question in questionsToAnalyze)
        {
            if (question.Responses == null || question.Responses.Count == 0)
                continue;

            QuestionEngine.CalculateQuestionAndResponseStatistics(selectedSurvey, question);

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

            // Build statistics context for AI (top 3 responses per question)
            statsForAI.AppendLine($"Q: {question.QstLabel}");
            foreach (var response in question.Responses.Take(3))
            {
                var pct = response.ResultPercent * 100;
                statsForAI.AppendLine($"  - {response.Label}: {pct:F1}%");
            }
            statsForAI.AppendLine();
        }

        // Generate AI insights with improved prompt
        var aiInsights = await aiService.CallAIAPI($@"You are a survey research analyst. Analyze these topline results and provide 3-5 key insights.

CRITICAL INSTRUCTIONS:
- Always cite specific percentages from the data
- When comparing numbers, verify which is actually higher (e.g., 19.0% IS higher than 16.2%)
- Double-check all numerical comparisons before making claims
- Format as numbered points
- Be precise and accurate with all statistics

DATA:
{statsForAI}

Provide data-driven insights about patterns, notable findings, and implications:", 0.1);

        // Validate AI insights for obvious errors
        var validatedInsights = ValidateAIInsights(aiInsights, statsForAI.ToString());

        // Add AI insights section to report
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
                // Wrap long lines at word boundaries
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

        return Results.Text(report.ToString(), "text/plain; charset=utf-8");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// ═══════════════════════════════════════════════════════════════════════════════
// SURVEY CRUD API ENDPOINTS (Using DataAccess Layer)
// ═══════════════════════════════════════════════════════════════════════════════

// GET /api/surveys - Get all surveys
app.MapGet("/api/surveys", async (ISurveyRepository repo) =>
{
    try
    {
        var surveys = await repo.GetAllAsync();
        return Results.Ok(surveys);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving surveys: {ex.Message}");
    }
});

// GET /api/surveys/{id} - Get survey by ID
app.MapGet("/api/surveys/{id:guid}", async (Guid id, ISurveyRepository repo) =>
{
    try
    {
        var survey = await repo.GetByIdAsync(id);
        return survey is null ? Results.NotFound($"Survey with ID {id} not found") : Results.Ok(survey);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving survey: {ex.Message}");
    }
});

// GET /api/surveys/name/{name} - Get survey by name
app.MapGet("/api/surveys/name/{name}", async (string name, ISurveyRepository repo) =>
{
    try
    {
        var survey = await repo.GetByNameAsync(name);
        return survey is null ? Results.NotFound($"Survey '{name}' not found") : Results.Ok(survey);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving survey: {ex.Message}");
    }
});

// GET /api/surveys/search?term={term} - Search surveys by name
app.MapGet("/api/surveys/search", async (string term, ISurveyRepository repo) =>
{
    try
    {
        var surveys = await repo.SearchByNameAsync(term);
        return Results.Ok(surveys);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error searching surveys: {ex.Message}");
    }
});

// GET /api/surveys/status/{status} - Get surveys by status
app.MapGet("/api/surveys/status/{status:int}", async (int status, ISurveyRepository repo) =>
{
    try
    {
        var surveys = await repo.GetByStatusAsync((SurveyStatus)status);
        return Results.Ok(surveys);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving surveys by status: {ex.Message}");
    }
});

// POST /api/surveys - Create new survey
app.MapPost("/api/surveys", async (Survey survey, ISurveyRepository repo) =>
{
    try
    {
        // Validate survey name
        survey.ValidateSurveyName();
        
        // Check if name already exists
        if (await repo.SurveyNameExistsAsync(survey.SurveyName))
        {
            return Results.Conflict($"Survey with name '{survey.SurveyName}' already exists");
        }

        var created = await repo.AddAsync(survey);
        return Results.Created($"/api/surveys/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error creating survey: {ex.Message}");
    }
});

// PUT /api/surveys/{id} - Update existing survey
app.MapPut("/api/surveys/{id:guid}", async (Guid id, Survey survey, ISurveyRepository repo) =>
{
    try
    {
        // Ensure ID matches
        if (id != survey.Id)
        {
            return Results.BadRequest("Survey ID mismatch");
        }

        // Check if survey exists
        if (!await repo.ExistsAsync(id))
        {
            return Results.NotFound($"Survey with ID {id} not found");
        }

        // Validate survey name
        survey.ValidateSurveyName();

        // Check if name already exists (excluding current survey)
        if (await repo.SurveyNameExistsAsync(survey.SurveyName, id))
        {
            return Results.Conflict($"Another survey with name '{survey.SurveyName}' already exists");
        }

        var updated = await repo.UpdateAsync(survey);
        return Results.Ok(updated);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error updating survey: {ex.Message}");
    }
});

// DELETE /api/surveys/{id} - Delete survey
app.MapDelete("/api/surveys/{id:guid}", async (Guid id, ISurveyRepository repo) =>
{
    try
    {
        if (!await repo.ExistsAsync(id))
        {
            return Results.NotFound($"Survey with ID {id} not found");
        }

        var deleted = await repo.DeleteAsync(id);
        return deleted ? Results.NoContent() : Results.Problem("Failed to delete survey");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error deleting survey: {ex.Message}");
    }
});

// GET /api/surveys/{id}/stats - Get survey statistics
app.MapGet("/api/surveys/{id:guid}/stats", async (Guid id, ISurveyRepository repo) =>
{
    try
    {
        if (!await repo.ExistsAsync(id))
        {
            return Results.NotFound($"Survey with ID {id} not found");
        }

        var questionCount = await repo.GetQuestionCountAsync(id);
        var responseCount = await repo.GetResponseCountAsync(id);

        return Results.Ok(new
        {
            SurveyId = id,
            QuestionCount = questionCount,
            ResponseCount = responseCount
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving survey stats: {ex.Message}");
    }
});

app.Run();