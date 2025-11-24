#nullable enable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Porpoise.Core.Models;
using Word = Microsoft.Office.Interop.Word;

namespace Porpoise.Core.Engines;

/// <summary>
/// Generates Survey Notes and Question Notes reports in Microsoft Word.
/// Runs asynchronously with progress reporting and cancellation support.
/// </summary>
public class SurveyNotesEngine : IDisposable
{
    private readonly Survey _survey;
    private readonly string _clientName;
    private readonly string _researcherName;
    private readonly Image _smallPorpoiseLogo;
    private readonly string _basePorpoiseDir;

    private readonly BackgroundWorker _bgWorker;
    private readonly Stopwatch _stopwatch = new();

    private int _notesQuestions;
    private bool _surveyNotesExist;

    public event EventHandler<ProgressStartedEventArgs>? ProgressStarted;
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
    public event EventHandler<ProgressCompleteEventArgs>? ProgressComplete;
    public event EventHandler<TopLineErrorEventArgs>? TopLineError;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public SurveyNotesEngine(Survey survey, string clientName, string researcherName, Image smallPorpoiseLogo, string basePorpoiseDir)
    {
        _survey = survey ?? throw new ArgumentNullException(nameof(survey));
        _clientName = clientName ?? "";
        _researcherName = researcherName ?? "";
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
        if (e.Argument is not NotesType notesType)
            return;

        Word.Application? wordApp = null;
        Word.Document? doc = null;

        try
        {
            try
            {
                wordApp = (Word.Application)Marshal.GetActiveObject("Word.Application");
            }
            catch
            {
                wordApp = new Word.Application();
            }

            wordApp.Visible = true;
            doc = wordApp.Documents.Add(Visible: true);
            doc.Activate();
            wordApp.System.Cursor = Word.WdCursorType.wdCursorWait;

            InsertFooter(doc);

            // Title
            var titlePara = doc.Paragraphs.Add();
            titlePara.Range.Text = notesType == NotesType.Survey
                ? $"Survey Notes Document for {_survey.SurveyName} Survey"
                : $"Question Notes Document for {_survey.SurveyName} Survey";
            titlePara.Range.Style = Word.WdBuiltinStyle.wdStyleHeading1;
            titlePara.Range.InsertParagraphAfter();

            // Subtitle
            var sb = new StringBuilder($"Created on {DateTime.Now:MMMM d, yyyy}");
            if (!string.IsNullOrEmpty(_researcherName))
                sb.Append($" by {_researcherName}");
            if (!string.IsNullOrEmpty(_clientName))
                sb.Append($" regarding client {_clientName}");

            var subPara = doc.Paragraphs.Add();
            subPara.Range.Text = sb.ToString();
            subPara.Range.InsertParagraphAfter();
            subPara.Range.InsertParagraphAfter();

            if (notesType == NotesType.Survey && _surveyNotesExist)
            {
                var heading = doc.Content.Paragraphs.Add(doc.Bookmarks["\\endofdoc"].Range);
                heading.Range.Text = "Survey Notes";
                heading.Range.Style = Word.WdBuiltinStyle.wdStyleHeading2;
                heading.Range.InsertParagraphAfter();

                var notesPara = doc.Paragraphs.Add();
                notesPara.Range.Text = _survey.SurveyNotes;
                notesPara.Range.InsertParagraphAfter();
                notesPara.Range.InsertParagraphAfter();
            }
            else if (notesType == NotesType.Question)
            {
                int processed = 0;
                foreach (var q in _survey.QuestionList.Where(q => !string.IsNullOrEmpty(q.QuestionNotes)))
                {
                    if (_bgWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        UpdateProgress("Cancelling Survey Notes process...", processed);
                        break;
                    }

                    processed++;
                    UpdateProgress(q, processed);

                    var heading = doc.Content.Paragraphs.Add(doc.Bookmarks["\\endofdoc"].Range);
                    heading.Range.Text = $"{q.QstNumber}. {q.QstLabel}";
                    heading.Range.Style = Word.WdBuiltinStyle.wdStyleHeading2;
                    heading.Range.InsertParagraphAfter();

                    var notesPara = doc.Paragraphs.Add();
                    notesPara.Range.Text = q.QuestionNotes;
                    notesPara.Range.InsertParagraphAfter();
                    notesPara.Range.InsertParagraphAfter();
                }
            }

            BringWordToFront();
        }
        catch (Exception ex)
        {
            e.Result = ex;
        }
        finally
        {
            if (doc != null && !_bgWorker.CancellationPending)
            {
                // Leave Word open — user owns it now
            }
            else
            {
                doc?.Close(SaveChanges: false);
                wordApp?.Quit();
            }

            Marshal.ReleaseComObject(doc);
            Marshal.ReleaseComObject(wordApp);
        }
    }

    private void UpdateProgress(Question question, int current)
    {
        var value = _notesQuestions > 0 ? (int)Math.Ceiling(100.0 * current / _notesQuestions) : 0;
        var text = $"Processing question {current} of {_notesQuestions} ({question.QstNumber}. {question.QstLabel})";
        _bgWorker.ReportProgress(value, new ProgressArgs { ProgressText = text, ElapsedTime = _stopwatch.Elapsed });
    }

    private void UpdateProgress(string text, int current)
    {
        var value = _notesQuestions > 0 ? (int)Math.Ceiling(100.0 * current / _notesQuestions) : 0;
        _bgWorker.ReportProgress(value, new ProgressArgs { ProgressText = text, ElapsedTime = _stopwatch.Elapsed });
    }

    private static void BringWordToFront()
    {
        var wordProcess = Process.GetProcessesByName("WINWORD")
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

        if (wordProcess != null)
            SetForegroundWindow(wordProcess.MainWindowHandle);
    }

    private void InsertFooter(Word.Document doc)
    {
        var tempPath = Path.GetTempPath();
        var footerImagePath = Path.Combine(tempPath, "PorpoiseSmall_tmp.png");

        if (File.Exists(footerImagePath))
            File.Delete(footerImagePath);

        using var footerBitmap = new Bitmap(350, 16);
        using (var g = Graphics.FromImage(footerBitmap))
        {
            g.DrawImage(_smallPorpoiseLogo, 0, 0);

            using var font = new Font("Segoe UI", 8);
            using var brush = new SolidBrush(Color.Black);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var text = $"Survey Notes report created by Porpoise {DateTime.Now:MMMM d, yyyy h:mm tt}";
            var size = g.MeasureString(text, font);
            g.DrawString(text, font, brush, new RectangleF(20, 0, size.Width + 5, size.Height));
        }

        footerBitmap.Save(footerImagePath, System.Drawing.Imaging.ImageFormat.Png);

        doc.Sections[1].PageSetup.DifferentFirstPageHeaderFooter = true;

        var firstFooter = doc.Sections[1].Footers[Word.WdHeaderFooterIndex.wdHeaderFooterFirstPage].Range;
        firstFooter.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
        firstFooter.InlineShapes.AddPicture(footerImagePath);

        var primaryFooter = doc.Sections[1].Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
        primaryFooter.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
        primaryFooter.InlineShapes.AddPicture(footerImagePath);

        doc.Sections[1].Footers[1].PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberRight);

        if (File.Exists(footerImagePath))
            File.Delete(footerImagePath);
    }

    private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
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
    }

    private class ProgressArgs
    {
        public string ProgressText { get; set; } = "";
        public TimeSpan ElapsedTime { get; set; }
    }
}

public class ProgressStartedEventArgs : EventArgs
{
    public string TaskTitle { get; }
    public string ProgressText { get; }
    public ProgressStartedEventArgs(string taskTitle, string progressText)
    {
        TaskTitle = taskTitle;
        ProgressText = progressText;
    }
}

public class ProgressChangedEventArgs : EventArgs
{
    public int Percentage { get; }
    public string ProgressText { get; }
    public TimeSpan ElapsedTime { get; }
    public ProgressChangedEventArgs(int percentage, string progressText, TimeSpan elapsedTime)
    {
        Percentage = percentage;
        ProgressText = progressText;
        ElapsedTime = elapsedTime;
    }
}

public class ProgressCompleteEventArgs : EventArgs
{
    public string ProgressText { get; }
    public ProgressCompleteEventArgs(string progressText) => ProgressText = progressText;
}

public class TopLineErrorEventArgs : EventArgs
{
    public string Message { get; }
    public Exception Exception { get; }
    public TopLineErrorEventArgs(string message, Exception ex)
    {
        Message = message;
        Exception = ex;
    }
}

public enum NotesType
{
    Survey,
    Question
}