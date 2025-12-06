# AI Prompt Externalization - Implementation Summary

## What Was Done

Successfully externalized AI analysis prompts from hardcoded C# strings to JSON configuration files.

## Files Created

1. **`Configuration/ai-prompts.json`** - JSON file containing all AI prompt templates
2. **`Configuration/AiPromptConfiguration.cs`** - C# classes for deserializing JSON config
3. **`Services/PromptTemplateEngine.cs`** - Simple template engine for variable substitution
4. **`Configuration/README.md`** - Documentation for prompt configuration

## Files Modified

1. **`Program.cs`** - Added configuration loading and service registration
2. **`Controllers/SurveyAnalysisController.cs`** - Refactored to use template engine instead of string building

## How It Works

### 1. Configuration Loading (Program.cs)
```csharp
// Load AI prompt configuration from JSON file
var promptConfigPath = Path.Combine(builder.Environment.ContentRootPath, "Configuration", "ai-prompts.json");
var promptConfig = new AiPromptConfiguration();
if (File.Exists(promptConfigPath))
{
    var promptJson = File.ReadAllText(promptConfigPath);
    promptConfig = System.Text.Json.JsonSerializer.Deserialize<AiPromptConfiguration>(...);
    Console.WriteLine("✅ Loaded AI prompt configuration from ai-prompts.json");
}
builder.Services.AddSingleton(promptConfig);
builder.Services.AddSingleton<PromptTemplateEngine>();
```

### 2. Template Format
Templates support two operations:

**Variable Substitution:**
```
Question: {{QuestionLabel}}
Total N: {{TotalN}}
```

**Loops:**
```
{{#each Responses}}
- {{label}}: {{frequency}} ({{percent}}%)
{{/each}}
```

### 3. Usage in Controller
```csharp
private string BuildQuestionAnalysisPrompt(QuestionAnalysisRequest request)
{
    var context = new Dictionary<string, object>
    {
        { "QuestionLabel", request.QuestionLabel },
        { "TotalN", request.TotalN },
        { "Responses", request.Responses.Select(r => new Dictionary<string, object>
            {
                { "label", r.Label },
                { "frequency", r.Frequency },
                { "percent", $"{r.Percent:F1}" }
            }).ToList()
        }
    };
    
    return _templateEngine.Render(_promptConfig.QuestionAnalysis.Template, context);
}
```

## Benefits

✅ **No Code Changes Required** - Edit prompts without recompiling
✅ **Version Control** - Track prompt changes in git
✅ **Easy Testing** - Test different prompt variations quickly
✅ **Documentation** - Prompts are self-documenting in JSON
✅ **Future-Ready** - Foundation for database storage and admin UI

## Future Enhancements

### Short Term
- Add prompt validation on startup
- Add hot-reload capability (watch file changes)
- Add logging for which template was used

### Medium Term
- Database storage for per-tenant customization
- Admin UI for editing prompts through the web interface
- Prompt versioning and rollback capability

### Long Term
- A/B testing framework for prompt variations
- AI-powered prompt optimization
- Multi-language prompt support
- Prompt effectiveness metrics and analytics

## Testing

The implementation was tested and verified:
- ✅ Build successful with no errors
- ✅ Configuration loads on startup (`✅ Loaded AI prompt configuration from ai-prompts.json`)
- ✅ Existing functionality preserved
- ✅ Template engine works with variable substitution and loops

## Usage

### Editing Prompts

1. Open `/Users/dgarvin/Documents/Source Code/porpoise-core/Porpoise.Api/Configuration/ai-prompts.json`
2. Edit the `template` field for either `questionAnalysis` or `crosstabAnalysis`
3. Save the file
4. Restart the API (prompts load on startup)
5. Test by generating AI analysis in the UI

### Available Variables

**Question Analysis:**
- `{{QuestionLabel}}` - Question text
- `{{TotalN}}` - Total responses
- `{{#each Responses}}` - Loop through responses
  - `{{label}}` - Response option text
  - `{{frequency}}` - Response count
  - `{{percent}}` - Response percentage

**Crosstab Analysis:**
- `{{DependentVariable}}` - Column variable (outcome measured)
- `{{IndependentVariable}}` - Row variable (grouping factor)
- `{{TotalN}}` - Sample size
- `{{ChiSquare}}` - Chi-square statistic
- `{{PValue}}` - P-value
- `{{CramersV}}` - Effect size
- `{{#each Rows}}` - Loop through index data
  - `{{rowLabel}}` - Category name
  - `{{index}}` - Index value
  - `{{sentiment}}` - positive/neutral/negative
  - `{{posIndex}}` - Positive percentage
  - `{{negIndex}}` - Negative percentage

## Notes

- Prompts are loaded once at startup (restart API after changes)
- Template syntax is case-sensitive for variable names
- Whitespace in templates is preserved
- JSON strings must escape special characters (use `\n` for newlines)
- The template engine is lightweight and focused on this specific use case

## Migration Path to Database

When ready to move prompts to database:

1. Create `PromptTemplates` table with columns:
   - `Id` (PK)
   - `TenantId` (FK)
   - `Name` (e.g., "questionAnalysis")
   - `Template` (TEXT)
   - `Version` (INT)
   - `IsActive` (BOOL)
   - `CreatedDate`, `ModifiedDate`

2. Update `Program.cs` to load from database instead of JSON file

3. Keep JSON as fallback/default templates

4. Add admin UI endpoints to manage templates

This implementation provides a clean foundation for that future enhancement.
