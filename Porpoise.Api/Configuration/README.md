# AI Prompt Configuration

This directory contains externalized AI prompt templates used by the Porpoise API for generating survey analysis insights.

## Files

- **ai-prompts.json** - Configuration file containing prompt templates

## Template Format

Prompts use a simple template engine that supports:

1. **Variable substitution**: `{{VariableName}}`
2. **Loops**: `{{#each Collection}}...{{/each}}`

### Example:
```json
{
  "questionAnalysis": {
    "name": "Question Analysis",
    "description": "Analyzes single question response distributions",
    "template": "Question: {{QuestionLabel}}\n\n{{#each Responses}}- {{label}}: {{frequency}} ({{percent}}%){{/each}}"
  }
}
```

## Available Templates

### 1. Question Analysis (`questionAnalysis`)
Analyzes single survey question response distributions.

**Variables:**
- `QuestionLabel` - The question text
- `TotalN` - Total number of responses
- `Responses` - Array of response objects with:
  - `label` - Response option text
  - `frequency` - Number of respondents
  - `percent` - Percentage (formatted)

### 2. Crosstab Analysis (`crosstabAnalysis`)
Analyzes relationships between two survey variables using crosstab data.

**Variables:**
- `DependentVariable` - The outcome being measured (column variable)
- `IndependentVariable` - The grouping factor (row variable)
- `TotalN` - Sample size
- `ChiSquare` - Chi-square test statistic
- `PValue` - Statistical significance (p-value)
- `CramersV` - Effect size measure (0-1)
- `Rows` - Array of index data objects with:
  - `rowLabel` - Category name
  - `index` - Index value (0-200 scale)
  - `sentiment` - Calculated sentiment (positive/neutral/negative)
  - `posIndex` - Positive percentage
  - `negIndex` - Negative percentage

## Editing Prompts

1. Edit `ai-prompts.json` directly
2. No code changes required - API automatically reloads on restart
3. Test your changes by triggering AI analysis in the UI
4. Prompts support markdown formatting for AI output structure

## Future Enhancements

- [ ] Database storage for per-tenant customization
- [ ] Admin UI for editing prompts
- [ ] Version control and rollback
- [ ] A/B testing different prompt variations
- [ ] Prompt validation and testing framework

## Notes

- Keep prompts concise - longer prompts increase API costs
- Include clear output format instructions
- Use exact variable names from the code
- Test prompts with various data patterns
- Validate AI output matches expected format
