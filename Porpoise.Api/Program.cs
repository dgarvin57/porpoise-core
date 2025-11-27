// Porpoise.Api/Program.cs — NO SWAGGER, JUST PURE API
using Porpoise.Core.Data;
using Porpoise.Core.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

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

app.MapGet("/api/survey", () =>
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

        var result = new
        {
            // Summary statistics
            TotalSurveys = currentSurveys.Count,
            TotalQuestions = currentSurveys.Sum(s => s.Survey.QuestionList.Count),
            TotalCases = currentSurveys.Sum(s => s.Data.DataList.Count - 1), // Subtract 1 for header row per survey
            
            // Project info
            ProjectName = currentProject?.ProjectName,
            
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

app.Run();