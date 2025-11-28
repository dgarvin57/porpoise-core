// Porpoise.Api/Program.cs — NO SWAGGER, JUST PURE API
using Porpoise.Core.Data;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using Porpoise.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

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

        var aiInsights = await aiService.CallAIAPI($@"You are a survey research analyst. Analyze these topline results and provide 2-3 sentences of key insights:

{statsContext}

Focus on: notable patterns, surprising findings, or actionable insights for decision-makers.");

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

app.Run();