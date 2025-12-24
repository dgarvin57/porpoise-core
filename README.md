# Porpoise.Core 1.0 — The Political Polling Engine Reborn

**The analytics brain trusted by pollsters and researchers for over 20 years — now clean, modern, and ready to sell.**

- 100 % WinForms-free  
- Pure C# • .NET 9 • NuGet-ready  
- No UI • No legacy • No compromises  

This is **the product**.

```text
Porpoise.Core/
├── Models/                  ← Survey, Question, Response, Project, Block
├── Services/                ← QuestionTreeService, SelectService, PoolTrendValidationService, etc.
├── Data/                    ← .porps file loader (your real data)
├── Exceptions/              ← Domain-specific errors
├── Extensions/              ← C# helpers
├── Utilities/               ← Small pure functions
└── Porpoise.Core.csproj     ← Versioned, publishable package
```

## Purpose

Porpoise.Core exists for one reason: to turn raw survey data into instant, trustworthy, client-ready insights — exactly the way political pollsters and market researchers have needed it since 2004.
It is not a generic survey tool.

It is a specialised weapon for people who live and die by crosstabs, indexes, significance, and topline reports.
No Excel. No manual weighting. No “did I mess up the index?”

Just load the data → ask the question → get the answer in 3 clicks.

### Major Features — All Preserved, All Pure Logic

| Feature                            | What it does                                                                 | Why pollsters love it (and competitors don’t have it)                          |
|------------------------------------|------------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| **Question Tree with Blocks**      | Groups questions into logical blocks with collapse/expand                     | Instantly find any question in a 200+ question survey                          |
| **DV / IV Selection**              | One-click Dependent / Independent variable selection with live visual cues    | The foundation of every crosstab                                               |
| **Select / Select-Plus**           | Multi-level filtering with live status labels (“Select: Males 18–34”)         | “Show me only likely voters who watched the debate”                             |
| **Weighting**                      | Apply census, custom, or simulation weights with live status                  | Accurate representation every single time                                         |
| **Pool & Trend**                   | Combine or compare multiple surveys with full validation                       | “How has approval changed since 2020?” — with zero manual work                  |
| **Full Crosstab Engine**           | Chi-square, Phi, Cramer’s V, Contingency Coefficient, significance, marginals  | One table answers the entire question — no extra software needed                 |
| **Index Calculations**              | Automatic +/0/− indexing with custom positive/negative labels                  | No more Excel formulas — perfect indexes every time                               |
| **Preference Blocks**               | Forced-choice analysis with pattern detection and ranking                       | “When voters are forced to choose, what do they really prefer?”                 |
| **Univariate & Bivariate**         | Full descriptive stats, ANOVA, correlation, confidence intervals                | Deep dive without leaving the tool                                               |
| **Topline Word Export**            | One-click, beautifully formatted, branded Word reports                          | Client-ready in 3 seconds — no copy-paste hell                                  |
| **All Validation Rules**            | Catches mismatched responses, index types, block consistency, etc.             | Garbage in → caught before it becomes garbage out                                 |
| **Smart Tooltips & Messages**       | Every disabled question, every validation error explained in plain English      | Users never wonder “why can’t I select this?”                                  |
| **Researcher Logo & Branding**      | Custom logo and title on every report                                         | White-label ready out of the box                                                |
---

## Design Philosophy: User Acceptance Over Academic Precision

Porpoise prioritizes **practitioner usability** over statistical jargon. Features must be immediately understood by working researchers, consultants, and decision-makers — not just academics.

### Strategic Decisions on Terminology & Features

**Variable Type** ("Who They Are / What They Think")
- **Issue**: The term "Variable Type" is not intuitive to most users
- **Solution**: Added clickable info icons with plain-language explanations and examples
- **Future**: Name may be revisited; tooltip ensures current usability

**Data Type** (Nominal, Ordinal, Interval, Ratio)
- **Issue**: Auto-generated, technically correct, but rarely understood by clients
- **Usage**: Minimal throughout the application; users don't reference it
- **Future**: Inclusion is questionable; may be removed or hidden in future versions
- **Rationale**: Academic classification adds little practical value for end users

**Favorability Index** (formerly "Index Type")
- **Issue**: Original terminology was unclear; users questioned if it was proprietary methodology
- **Clarification**: Standard research technique (net favorability = % Positive − % Negative), not a custom invention
- **Solution**: Renamed from "Index Type" to "Favorability Index" with helpful tooltip and real-time net score display
- **Outcome**: Familiar concept presented in immediately understandable terms

### Guiding Principle
If a feature requires academic training to understand, it either needs better terminology, helpful context (tooltips/examples), or should be reconsidered entirely. **High-value features presented in accessible language** — that's the product strategy.