#nullable enable

using Porpoise.Core.Models;
using Porpoise.Core.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Porpoise.Core.Engines;

/// <summary>
/// Generates full Top Line reports (IV, DV, or both) in Microsoft Word.
/// Runs asynchronously with progress reporting, cancellation, and beautiful formatting.
/// 
/// NOTE: This feature requires Microsoft Office Interop assemblies to be installed.
/// To enable this feature, install the Microsoft.Office.Interop.Word NuGet package
/// and implement the Word automation logic.
/// </summary>
public class ToplineEngine : IDisposable
{
    private readonly Project _project;
    private readonly Survey _survey;
    private readonly string _clientName;
    private readonly string _researcherName;
    private readonly string _researcherSubName;
    private readonly string _smallPorpoiseLogo;
    //TODO: Re-implement image handling for web version
    // private readonly Image _smallPorpoiseLogo;
    private readonly string _basePorpoiseDir;

    private readonly BackgroundWorker _bgWorker;
    private readonly Stopwatch _stopwatch = new();

    private int _topLineQuestions;

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

    //TODO: Implement Image handling for web version
    // public ToplineEngine(Project project, Survey survey, string clientName, string researcherName,
    //     string researcherSubName, Image smallPorpoiseLogo, string basePorpoiseDir)
    public ToplineEngine(Project project, Survey survey, string clientName, string researcherName,
        string researcherSubName, string smallPorpoiseLogo, string basePorpoiseDir)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _survey = survey ?? throw new ArgumentNullException(nameof(survey));
        _clientName = clientName ?? "";
        _researcherName = researcherName == "Researcher name here" ? "" : researcherName ?? "";
        _researcherSubName = researcherSubName == "Researcher title here" ? "" : researcherSubName ?? "";
        _smallPorpoiseLogo = smallPorpoiseLogo ?? throw new ArgumentNullException(nameof(smallPorpoiseLogo));
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

    public void PrintTopLine(DVOrIV dvIvOrBoth)
    {
        try
        {
            StartProgress(dvIvOrBoth);

            var wasDirty = _survey.IsDirty;

            _topLineQuestions = 0;
            foreach (var q in _survey.QuestionList)
            {
                if ((dvIvOrBoth == DVOrIV.IV && q.VariableType == QuestionVariableType.Independent) ||
                    (dvIvOrBoth == DVOrIV.DV && q.VariableType == QuestionVariableType.Dependent) ||
                    dvIvOrBoth == DVOrIV.Both)
                {
                    _topLineQuestions++;
                    QuestionEngine.CalculateQuestionAndResponseStatistics(_survey, q);
                }
            }

            if (!wasDirty)
            {
                _survey.MarkClean();
                _project.MarkClean();
            }

            if (!_bgWorker.IsBusy)
                _bgWorker.RunWorkerAsync(dvIvOrBoth);
        }
        catch (Exception ex)
        {
            RaiseTopLineError("An error occurred starting the Top Line process.", ex);
        }
    }

    public void CancelTopLineReport()
    {
        _bgWorker.CancelAsync();
    }

    private void StartProgress(DVOrIV dvIv)
    {
        var taskTitle = $"Creating {dvIv} Top Line report";
        var progressText = "Starting Word ...";
        _stopwatch.Restart();
        ProgressStarted?.Invoke(this, new ProgressStartedEventArgs(taskTitle, progressText));
    }

    private void CompleteProgress()
    {
        var progressText = $"Successfully created Top Line for all {_topLineQuestions} questions in survey";
        _stopwatch.Stop();
        ProgressComplete?.Invoke(this, new ProgressCompleteEventArgs(progressText));
    }

    private void RaiseTopLineError(string message, Exception ex)
        => TopLineError?.Invoke(this, new TopLineErrorEventArgs(message, ex));

    private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        // TODO: To enable Top Line Word report generation, install Microsoft.Office.Interop.Word NuGet package
        // and implement the Word automation logic here.
        
        throw new NotImplementedException(
            "Top Line report generation requires Microsoft Office Interop. " +
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
            RaiseTopLineError("An error occurred completing the Top Line process.", e.Error);
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

public enum DVOrIV
{
    IV,
    DV,
    Both
}