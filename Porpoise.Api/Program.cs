// Porpoise.Api/Program.cs — NO SWAGGER, JUST PURE API
using System.Reflection;
using Porpoise.Api.Configuration;
using Porpoise.Api.Database;
using Porpoise.Api.Middleware;
using Porpoise.Api.Mocks;
using Porpoise.Api.Services;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.DataAccess.Context;
using Porpoise.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Get version information
var version = Assembly.GetExecutingAssembly().GetName().Version;
var versionString = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "unknown";
Console.WriteLine($"🚀 Porpoise API v{versionString} starting up...");

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()  // Allow all origins for now - we'll tighten this later
        .AllowAnyHeader()
        .AllowAnyMethod()
    )
);

// Add controllers
builder.Services.AddControllers();

// Load AI prompt configuration from JSON file
var promptConfigPath = Path.Combine(builder.Environment.ContentRootPath, "Configuration", "ai-prompts.json");
var promptConfig = new AiPromptConfiguration();
if (File.Exists(promptConfigPath))
{
    var promptJson = File.ReadAllText(promptConfigPath);
    promptConfig = System.Text.Json.JsonSerializer.Deserialize<AiPromptConfiguration>(
        promptJson,
        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
    ) ?? new AiPromptConfiguration();
    Console.WriteLine("✅ Loaded AI prompt configuration from ai-prompts.json");
}
else
{
    Console.WriteLine("⚠️ AI prompts config file not found, using defaults");
}
builder.Services.AddSingleton(promptConfig);

// Register template engine
builder.Services.AddSingleton<PromptTemplateEngine>();

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
    
    // Add AllowUserVariables=true for migration scripts that use @variables
    if (!connectionString.Contains("AllowUserVariables", StringComparison.OrdinalIgnoreCase))
    {
        connectionString += ";AllowUserVariables=true";
    }
    
    // Run database migrations BEFORE setting up repositories
    try
    {
        DatabaseMigrationRunner.RunMigrations(connectionString);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Failed to run database migrations: {ex.Message}");
        Console.ResetColor();
        // Continue anyway - migrations might have already run
    }
    
    builder.Services.AddSingleton(new DapperContext(connectionString));
    
    // Register TenantContext as scoped (per-request)
    builder.Services.AddScoped<TenantContext>();
    
    // Register repositories with tenant support
    builder.Services.AddScoped<ITenantRepository, TenantRepository>();
    builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
    builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
    builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
    builder.Services.AddScoped<IResponseRepository, ResponseRepository>();
    builder.Services.AddScoped<ISurveyDataRepository, SurveyDataRepository>();
    builder.Services.AddScoped<IQuestionBlockRepository, QuestionBlockRepository>();
    builder.Services.AddScoped<ProjectRepository>(); // UnitOfWork needs concrete type
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<SurveyPersistenceService>();
    Console.WriteLine("✅ Using MYSQL database");
}

// Register AI service
builder.Services.AddHttpClient<AIInsightsService>();
builder.Services.AddSingleton<AIInsightsService>(sp => 
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    
    // Read provider from config or environment variable
    var provider = builder.Configuration["AI:Provider"] ?? 
                   Environment.GetEnvironmentVariable("AI_PROVIDER") ?? 
                   "anthropic"; // Default to anthropic
    
    // Read API key from config first, then environment variable based on provider
    var apiKey = builder.Configuration["AI:ApiKey"] ?? 
                 Environment.GetEnvironmentVariable(provider == "anthropic" ? "ANTHROPIC_API_KEY" : "OPENAI_API_KEY");
    
    Console.WriteLine($"AI Service Configuration: Provider={provider}, ApiKey={(apiKey != null ? "SET" : "NOT SET")}");
    
    return new AIInsightsService(httpClient, apiKey, provider);
});

var app = builder.Build();

// CORS must come first to handle preflight requests
app.UseCors();

// Add version endpoint (before tenant middleware)
app.MapGet("/version", () => 
{
    var version = Assembly.GetExecutingAssembly().GetName().Version;
    var versionString = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "unknown";
    return Results.Ok(new 
    { 
        version = versionString,
        environment = app.Environment.EnvironmentName,
        timestamp = DateTime.UtcNow
    });
});

// Add health check endpoint (before tenant middleware)
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

// Add tenant middleware (after CORS, before MapControllers)
app.UseMiddleware<TenantMiddleware>();

app.MapControllers();

// ═══════════════════════════════════════════════════════════════════════════════
// All API endpoints are now in Controllers:
// - SurveysController: CRUD operations on surveys
// - ProjectsController: CRUD operations on projects  
// - SurveyAnalysisController: Statistical analysis and AI insights
// - SurveyImportController: File import operations (.porps, .porpz, SPSS, CSV)
// ═══════════════════════════════════════════════════════════════════════════════

app.Run();