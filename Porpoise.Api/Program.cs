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

// Add controllers
builder.Services.AddControllers();

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
app.MapControllers();

// ═══════════════════════════════════════════════════════════════════════════════
// All API endpoints are now in Controllers:
// - SurveysController: CRUD operations on surveys
// - ProjectsController: CRUD operations on projects  
// - SurveyAnalysisController: Statistical analysis and AI insights
// - SurveyImportController: File import operations (.porps, .porpz, SPSS, CSV)
// ═══════════════════════════════════════════════════════════════════════════════

app.Run();