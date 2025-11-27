#nullable enable

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Generates Survey Notes and Question Notes reports in Microsoft Word.
/// Runs asynchronously with progress reporting and cancellation support.
/// 
/// NOTE: This feature requires Microsoft Office Interop assemblies to be installed.
/// To enable this feature, install the Microsoft.Office.Interop.Word NuGet package
/// and uncomment the implementation below.
/// </summary>
public class SurveyNotesEngine : IDisposable
{
    private readonly Survey _survey;
    private readonly string _clientName;
    private readonly string _researcherName;
    //TODO: Re-implement image handling for web version
//    private readonly Image _smallPorpoiseLogo;
    private readonly string _basePorpoiseDir;

    private readonly BackgroundWorker _bgWorker;
    private readonly Stopwatch _stopwatch = new();

    private int _notesQuestions;
    private bool _surveyNotesExist;

    public event EventHandler<ProgressStartedEventArgs>? ProgressStarted;
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
    public event EventHandler<ProgressCompleteEventArgs>? ProgressComplete;
    public event EventHandler<TopLineErrorEventArgs>? TopLineError;

    // DllImport is used instead of LibraryImport to avoid requiring unsafe code blocks.
    // LibraryImport would require <AllowUnsafeBlocks>true</AllowUnsafeBlocks> in the project file.
    // For this simple boolean P/Invoke, the performance benefit is negligible.
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
#pragma warning restore SYSLIB1054

//TODO: To enable Word report generation, install Microsoft.Office.Interop.Word NuGet package
//TODO: Reimplement Image researcher logo handling for web version
//    public SurveyNotesEngine(Survey survey, string clientName, Image? researcherName, Image smallPorpoiseLogo, string basePorpoiseDir)
    public SurveyNotesEngine(Survey survey, string clientName, string researcherName, string basePorpoiseDir)
    {
        _survey = survey ?? throw new ArgumentNullException(nameof(survey));
        _clientName = clientName ?? "";
        _researcherName = researcherName ?? "";
//        _smallPorpoiseLogo = smallPorpoiseLogo ?? throw new ArgumentNullException(nameof(smallPorpoiseLogo));
        _basePorpoiseDir = basePorpoiseDir ?? throw new ArgumentNullException(nameof(basePorpoiseDir));

        _bgWorker = new BackgroundWorker
        {
            WorkerSupportsCancellation = true,
            WorkerReportsProgress = true
        };
        _bgWorker.DoWork += BackgroundWorker_DoWork;
        _bgWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
        _bgWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
    }

    public void PrintNotes(NotesType notesType)
    {
        try
        {
            StartProgress();

            _surveyNotesExist = !string.IsNullOrEmpty(_survey.SurveyNotes);
            _notesQuestions = _survey.QuestionList.Count(q => !string.IsNullOrEmpty(q.QuestionNotes));

            if (!_bgWorker.IsBusy)
                _bgWorker.RunWorkerAsync(notesType);
        }
        catch (Exception ex)
        {
            RaiseTopLineError("An error occurred starting the survey notes process.", ex);
        }
    }

    public void CancelNotesReport()
    {
        _bgWorker.CancelAsync();
    }

    private void StartProgress()
    {
        var taskTitle = $"Creating Notes report for {_survey.SurveyName}";
        var progressText = "Starting Word ...";
        _stopwatch.Restart();
        ProgressStarted?.Invoke(this, new ProgressStartedEventArgs(taskTitle, progressText));
    }

    private void CompleteProgress()
    {
        var progressText = $"Successfully created Survey Notes report for {_notesQuestions} questions in survey";
        _stopwatch.Stop();
        ProgressComplete?.Invoke(this, new ProgressCompleteEventArgs(progressText));
    }

    private void RaiseTopLineError(string message, Exception ex)
        => TopLineError?.Invoke(this, new TopLineErrorEventArgs(message, ex));

    private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        // TODO: To enable Word report generation, install Microsoft.Office.Interop.Word NuGet package
        // and implement the Word automation logic here.
        
        throw new NotImplementedException(
            "Survey Notes report generation requires Microsoft Office Interop. " +
            "Install the Microsoft.Office.Interop.Word NuGet package to enable this feature. " +
            "Alternatively, consider implementing export to PDF, HTML, or other modern document formats.");
    }

    private void BackgroundWorker_ProgressChanged(object? sender, System.ComponentModel.ProgressChangedEventArgs e)
    {
        if (e.UserState is ProgressArgs args)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(e.ProgressPercentage, args.ProgressText, args.ElapsedTime));
        }
    }

    private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Error != null)
        {
            RaiseTopLineError("An error occurred completing the survey notes process.", e.Error);
        }
        else if (!e.Cancelled)
        {
            CompleteProgress();
        }
    }

    public void Dispose()
    {
        _bgWorker.Dispose();
        GC.SuppressFinalize(this);
    }

    private class ProgressArgs
    {
        public string ProgressText { get; set; } = "";
        public TimeSpan ElapsedTime { get; set; }
    }
}

public class ProgressStartedEventArgs(string taskTitle, string progressText) : EventArgs
{
    public string TaskTitle { get; } = taskTitle;
    public string ProgressText { get; } = progressText;
}

public class ProgressChangedEventArgs(int percentage, string progressText, TimeSpan elapsedTime) : EventArgs
{
    public int Percentage { get; } = percentage;
    public string ProgressText { get; } = progressText;
    public TimeSpan ElapsedTime { get; } = elapsedTime;
}

public class ProgressCompleteEventArgs(string progressText) : EventArgs
{
    public string ProgressText { get; } = progressText;
}

public class TopLineErrorEventArgs(string message, Exception ex) : EventArgs
{
    public string Message { get; } = message;
    public Exception Exception { get; } = ex;
}

public enum NotesType
{
    Survey,
    Question
}