# AI-Powered Insights for Porpoise

## Overview

Porpoise now includes AI-powered insights that **explain** your statistical calculations in plain English. The AI doesn't replace your statistical engines - it makes them more accessible.

## How It Works

```
Your Statistical Engines → Calculate precise numbers
         ↓
AI Service → Explains what the numbers mean
         ↓
API Response → Numbers + Natural language insights
```

## Setup

### Option 1: Using OpenAI (Recommended)

1. Get an API key from [OpenAI](https://platform.openai.com/api-keys)

2. Set the environment variable:
   ```bash
   export OPENAI_API_KEY="sk-your-key-here"
   ```

3. Or add to `appsettings.json`:
   ```json
   {
     "OpenAI": {
       "ApiKey": "sk-your-key-here"
     }
   }
   ```

### Option 2: Without AI (Fallback Mode)

If no API key is configured, the service automatically provides rule-based summaries instead. No setup needed!

## Example Response

### Before (Numbers Only):
```json
{
  "totalSurveys": 1,
  "totalQuestions": 45,
  "totalCases": 500,
  "projectName": "Demo 2015"
}
```

### After (Numbers + AI Insights):
```json
{
  "totalSurveys": 1,
  "totalQuestions": 45,
  "totalCases": 500,
  "projectName": "Demo 2015",
  "aiSummary": "The 'Demo 2015' project contains a comprehensive survey with 45 questions and 500 respondents, providing strong statistical reliability for most analyses. The sample size enables robust significance testing and detailed subgroup analysis. This dataset is well-suited for advanced statistical techniques including crosstabs, regression, and multivariate analysis."
}
```

## Cost Estimates

Using `gpt-4o-mini` (fast and cheap):
- Per summary: ~$0.0001 (1/100th of a cent)
- 1000 summaries: ~$0.10
- 10,000 summaries: ~$1.00

## Future AI Features

✅ **Currently Available:**
- Executive summaries of survey statistics
- Sample size quality assessments
- Survey complexity analysis

🚧 **Coming Soon:**
- Question wording improvements
- Open-end response coding
- Statistical insight narratives (explaining crosstabs, significance tests)
- Trend analysis across waves
- Automated report generation

## Privacy & Security

- API calls include only **aggregate statistics** (counts, percentages)
- No raw respondent data is sent to AI services
- No personally identifiable information (PII) is shared
- Calculations are always done locally by your Porpoise engines
- AI only receives the final numbers to explain

## Testing

Test the API with and without AI:

```bash
# Run the API
dotnet run

# Call the endpoint
curl http://localhost:5107/api/survey
```

The response will include `"aiSummary"` field with insights about your data.

## Architecture

```
┌─────────────────────────────────────────────────┐
│  Vue 3 Frontend                                 │
│  - Displays statistics + AI insights           │
└───────────────────┬─────────────────────────────┘
                    │ HTTP Request
┌───────────────────▼─────────────────────────────┐
│  Porpoise.Api                                   │
│  - Calls ProjectLoader                          │
│  - Calculates statistics (your engines)         │
│  - Calls AIInsightsService                      │
└───────────────────┬─────────────────────────────┘
                    │ 
        ┌───────────┴──────────┐
        │                      │
┌───────▼──────────┐  ┌────────▼──────────┐
│ Statistical      │  │ AIInsightsService │
│ Engines          │  │                   │
│ - ToplineEngine  │  │ - Calls OpenAI    │
│ - VarCovar       │  │ - Generates text  │
│ - Crosstabs      │  │ - Handles errors  │
└──────────────────┘  └───────────────────┘
```

## The Vision

**Porpoise provides the precision. AI provides the clarity.**

Researchers trust the numbers (your statistical engines), and understand them instantly (AI explanations). Best of both worlds.
