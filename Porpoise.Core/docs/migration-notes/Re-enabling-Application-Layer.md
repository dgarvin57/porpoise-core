# Re-enabling Application Layer - Step-by-Step Guide

> **Historical Note:** This document describes a migration that has been completed. The Application layer is now fully enabled and functional. Keep for reference if similar refactoring is needed in the future.

## When You're Ready to Work on Application Layer Services

Follow these steps to incrementally re-enable and fix the Application layer.

## Current State

- ? **Core library compiling**: Models, Engines, DataAccess all working
- ? **Application layer excluded**: Has ~21 compilation errors
- ? **UI layer excluded**: Has ~12 compilation errors

## Step 1: Enable Application Layer Compilation

In `Porpoise.Core.csproj`, **remove or comment out** these lines:

```xml
<!-- COMMENT OUT TO RE-ENABLE: -->
<!-- <Compile Remove="Application\**\*.cs" /> -->
<!-- <None Include="Application\**\*.cs" /> -->
```

Expected result: **~21 errors**

## Step 2: Fix the 21 Errors Systematically

### Group 1: FormClosingEventArgs (5 errors)

**Files affected:**
- `IMainShell.cs`
- `IOptionsShell.cs`
- `IProjectInfoShell.cs`
- `IQuestionDefinitionShell.cs`
- Various service classes

**Solution:** Create custom `FormClosingEventArgs` in `IMainShell.cs`:

```csharp
public class FormClosingEventArgs : CancelEventArgs
{
    public CloseReason CloseReason { get; set; }
}

public enum CloseReason
{
    None,
    UserClosing,
    ApplicationExitCall,
    FormOwnerClosing
}
```

### Group 2: PoolTrendManager (1 error)

**File:** `PoolTrendSetupService.cs`

**Solution:** Create `Application/Services/PoolTrendManager.cs`:

```csharp
public class PoolTrendManager
{
    private readonly PoolTrendList _poolTrendList;
    
    public PoolTrendManager(PoolTrendList poolTrendList)
    {
        _poolTrendList = poolTrendList ?? throw new ArgumentNullException(nameof(poolTrendList));
    }
    
    public PoolTrendList PoolTrendList => _poolTrendList;
    
    // Add methods as needed by PoolTrendSetupService
    public ConsistencyChecker Checker { get; private set; }
    public PoolTrendItem? SurrogateSurveyItem { get; private set; }
    
    public object GetSummaryData(PoolTrendType type)
    {
        // TODO: Implement
        return new object();
    }
    
    public void CreateSurrogateSurvey(PoolTrendType type)
    {
        // TODO: Implement
    }
}
```

### Group 3: IQuestionTreeShell (3 errors)

**File:** `PoolTrendSetupService.cs`

**Solution:** Create `Application/Interfaces/IQuestionTreeShell.cs`:

```csharp
public interface IQuestionTreeShell
{
    DialogResult DialogResult { get; set; }
    Question? SelectedQuestion { get; }
    
    void LoadQuestions(Survey survey);
    void Initialize(bool selectOnEnabled);
    void SelectDV(Question? question);
    void SelectIV(Question? question);
    void Clear();
    
    event EventHandler? OnQuestionSelected;
}
```

### Group 4: IResultsShell (4 errors)

**Files:** `PoolTrendSetupService.cs`

**Solution:** Create `Application/Interfaces/IResultsShell.cs`:

```csharp
public interface IResultsShell
{
    void LoadQuestion(Question question);
    void Clear(string label);
    void ShowResults(object dataSource);
    
    event EventHandler? OnResultsChanged;
}
```

### Group 5: IQuestionTreeNode (~7 errors)

**Files:** `IQuestonTreeview.cs`, `QuestionTreeService.cs`

**Solution:** Create `Application/Interfaces/IQuestionTreeNode.cs`:

```csharp
public interface IQuestionTreeNode
{
    Question? Question { get; }
    string Text { get; set; }
    bool IsSelected { get; set; }
    bool IsExpanded { get; set; }
    bool IsChecked { get; set; }
    bool IsBlockNode { get; }
    IQuestionTreeNode? Parent { get; }
    IEnumerable<IQuestionTreeNode> Children { get; }
    
    void AddChild(IQuestionTreeNode child);
    void SetIcon(object icon);
    void SetEnabled(bool enabled);
}
```

### Group 6: HelpNavigator (1 error)

**File:** `MainApplicationService.cs`

**Solution:** Add enum to `MainApplicationService.cs`:

```csharp
public enum HelpNavigator
{
    TableOfContents,
    Index,
    Find,
    Topic
}
```

## Step 3: Build and Verify

After each group, build to ensure no new errors:

```bash
dotnet build Porpoise.Core/Porpoise.Core.csproj --no-restore
```

## Step 4: Re-enable UI Layer (Optional)

Once Application layer is clean:

1. Remove UI exclusion from `.csproj`
2. Fix TextRenderer dependencies (add WinForms package or create abstractions)
3. Fix SurveyManager references

## Tips

1. **Work incrementally** - Fix one group at a time
2. **Build after each change** - Catch cascading errors early
3. **Keep stubs minimal** - Only add what's actually called
4. **Consider abstractions** - Replace WinForms types with platform-agnostic interfaces

## Alternative: Separate Projects

Consider creating:
- `Porpoise.Core` - Models, Engines, DataAccess ? (already clean)
- `Porpoise.Application` - Service layer (references Core)
- `Porpoise.UI.WinForms` - WinForms-specific UI code
- `Porpoise.UI.Blazor` - Future Blazor implementation

This separation makes dependencies explicit and allows multiple UI implementations.

---

**Current Status**: Core is clean. Application/UI layers ready to be fixed when needed.
