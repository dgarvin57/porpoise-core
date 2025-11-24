#nullable enable

using Porpoise.Core.Model;
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
using Word = Microsoft.Office.Interop.Word;

namespace Porpoise.Core.Engines;

/// <summary>
/// Generates full Top Line reports (IV, DV, or both) in Microsoft Word.
/// Runs asynchronously with progress reporting, cancellation, and beautiful formatting.
/// </summary>
public class ToplineEngine : IDisposable
{
    private readonly Project _project;
    private readonly Survey _survey;
    private readonly string _clientName;
    private readonly string _researcherName;
    private readonly string _researcherSubName;
    private readonly Image _smallPorpoiseLogo;
    private readonly string _basePorpoiseDir;

    private readonly BackgroundWorker _bgWorker;
    private readonly Stopwatch _stopwatch = new();

    private int _topLineQuestions;
    private int _questionCounter = 1;

    public event EventHandler<ProgressStartedEventArgs>? ProgressStarted;
    public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;
    public event EventHandler<ProgressCompleteEventArgs>? ProgressComplete;
    public event EventHandler<TopLineErrorEventArgs>? TopLineError;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public ToplineEngine(Project project, Survey survey, string clientName, string researcherName,
        string researcherSubName, Image smallPorpoiseLogo, string basePorpoiseDir)
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
        if (e.Argument is not DVOrIV dvIv) return;

        _questionCounter = 1;
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
            InsertHeader(doc);

            // Title
            var titlePara = doc.Paragraphs.Add();
            titlePara.Range.Text = dvIv switch
            {
                DVOrIV.IV => $"IV Top Line Report for {_survey.SurveyName} Survey",
                DVOrIV.DV => $"DV Top Line Report for {_survey.SurveyName} Survey",
                _ => $"DV/IV Top Line Report for {_survey.SurveyName} Survey"
            };
            titlePara.Range.Style = Word.WdBuiltinStyle.wdStyleHeading1;
            titlePara.Range.InsertParagraphAfter();

            // Subtitle
            var sb = new StringBuilder($"Created on {DateTime.Now:MMMM d, yyyy}");
            if (!string.IsNullOrEmpty(_researcherName))
                sb.Append($" by {_researcherName}");
            if (!string.IsNullOrEmpty(_clientName))
                sb.Append($" for {_clientName}");

            var subPara = doc.Paragraphs.Add();
            subPara.Range.Text = sb.ToString();
            subPara.Range.InsertParagraphAfter();
            subPara.Range.InsertParagraphAfter();

            int processed = 0;
            foreach (var q in _survey.QuestionList)
            {
                if ((dvIv == DVOrIV.IV && q.VariableType != QuestionVariableType.Independent) ||
                    (dvIv == DVOrIV.DV && q.VariableType != QuestionVariableType.Dependent) ||
                    dvIv == DVOrIV.Both && false)
                    continue;

                processed++;

                if (_bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    UpdateProgress("Cancelling Top Line process...", processed);
                    break;
                }

                UpdateProgress(q, processed);

                if (q.BlkQstStatus == BlkQuestionStatusType.FirstQuestionInBlock)
                {
                    var blockHeading = doc.Content.Paragraphs.Add(doc.Bookmarks["\\endofdoc"].Range);
                    blockHeading.Range.Text = $"Block: {q.BlkLabel}";
                    blockHeading.Range.Style = Word.WdBuiltinStyle.wdStyleHeading2;
                    blockHeading.Range.InsertParagraphAfter();

                    var stemPara = doc.Paragraphs.Add();
                    stemPara.Range.Text = q.BlkStem;
                    stemPara.Range.InsertParagraphAfter();
                }

                InsertBoldCaptionWordParagraph(doc, $"{q.QstNumber}. {q.QstLabel}: {q.QstStem}");
                InsertResponses(doc, q);
                doc.Paragraphs.Add().Range.InsertParagraphAfter();
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
                // Leave Word open
            }
            else
            {
                doc?.Close(SaveChanges: false);
                wordApp?.Quit();
            }

            if (doc != null) Marshal.ReleaseComObject(doc);
            if (wordApp != null) Marshal.ReleaseComObject(wordApp);
        }
    }

    private void InsertHeader(Word.Document doc)
    {
        string logoPath = _project.ResearcherLogoFilename != null
            ? Path.Combine(_project.FullFolder, _project.ResearcherLogoFilename)
            : CreateBlankLogo();

        if (!File.Exists(logoPath)) return;

        var tempPath = Path.Combine(Path.GetDirectoryName(logoPath)!,
            Path.GetFileNameWithoutExtension(logoPath) + "_tmp" + Path.GetExtension(logoPath));

        using var original = new Bitmap(logoPath);
        using var smallLogo = ImageUtils.ResizeImage(original, 85, 85, ImageVertAlign.Bottom, ImageHorizAlign.Right);
        using var headerBitmap = new Bitmap(400, 85);

        using (var g = Graphics.FromImage(headerBitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            g.DrawImage(smallLogo, 400 - smallLogo.Width, 85 - smallLogo.Height);

            if (!string.IsNullOrEmpty(_researcherName))
            {
                using var nameFont = new Font("Segoe UI", 10, FontStyle.Bold);
                using var brush = new SolidBrush(Color.Black);
                var nameSize = g.MeasureString(_researcherName, nameFont);
                var maxWidth = Math.Max(nameSize.Width, headerBitmap.Width - smallLogo.Width - 10);
                var left = headerBitmap.Width - smallLogo.Width - maxWidth + 5;
                g.DrawString(_researcherName, nameFont, brush, new RectangleF(left, 50, maxWidth, nameSize.Height));

                if (!string.IsNullOrEmpty(_researcherSubName))
                {
                    using var subFont = new Font("Segoe UI", 8);
                    var subSize = g.MeasureString(_researcherSubName, subFont);
                    var subLeft = headerBitmap.Width - smallLogo.Width - Math.Max(subSize.Width, maxWidth) + 5;
                    g.DrawString(_researcherSubName, subFont, brush, new RectangleF(subLeft, 68, subSize.Width + 10, subSize.Height + 5));
                }
            }
        }

        headerBitmap.Save(tempPath);
        doc.Sections[1].Headers[Word.WdHeaderFooterIndex.wdHeaderFooterFirstPage].Range
            .InlineShapes.AddPicture(tempPath);
        File.Delete(tempPath);

        if (Path.GetFileNameWithoutExtension(logoPath) == "BlankLogo")
            File.Delete(logoPath);
    }

    private string CreateBlankLogo()
    {
        var path = Path.Combine(_project.FullFolder, "BlankLogo.tmp");
        using var bmp = new Bitmap(85, 85);
        bmp.MakeTransparent();
        bmp.Save(path);
        return path;
    }

    private void InsertFooter(Word.Document doc)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "PorpoiseSmall_tmp.png");
        if (File.Exists(tempPath)) File.Delete(tempPath);

        using var footerBitmap = new Bitmap(350, 16);
        using (var g = Graphics.FromImage(footerBitmap))
        {
            g.DrawImage(_smallPorpoiseLogo, 0, 0);

            using var font = new Font("Segoe UI", 8);
            using var brush = new SolidBrush(Color.Black);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            var text = $"Top Line report created by Porpoise {DateTime.Now:MMMM d, yyyy h:mm tt}";
            var size = g.MeasureString(text, font);
            g.DrawString(text, font, brush, new RectangleF(20, 0, size.Width + 5, size.Height));
        }

        footerBitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

        doc.Sections[1].PageSetup.DifferentFirstPageHeaderFooter = true;

        var firstFooter = doc.Sections[1].Footers[Word.WdHeaderFooterIndex.wdHeaderFooterFirstPage].Range;
        firstFooter.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
        firstFooter.InlineShapes.AddPicture(tempPath);

        var primaryFooter = doc.Sections[1].Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
        primaryFooter.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
        primaryFooter.InlineShapes.AddPicture(tempPath);

        doc.Sections[1].Footers[1].PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberRight);

        if (File.Exists(tempPath)) File.Delete(tempPath);
    }

    private static void InsertBoldCaptionWordParagraph(Word.Document doc, string text)
    {
        text = text.Replace("\r\n\r\n", "\r\n").Replace("\r\n", "").Replace("\r", "").Replace("\n", "");

        var para = doc.Content.Paragraphs.Add();
        para.Range.Text = text;
        para.Range.Font.Bold = 0;

        int colonIndex = text.IndexOf(':');
        if (colonIndex > 0)
        {
            var boldRange = doc.Range(para.Range.Start, para.Range.Start + colonIndex + 1);
            boldRange.Bold = 1;
        }
    }

    private void InsertResponses(Word.Document doc, Question q)
    {
        var range = doc.Bookmarks["\\endofdoc"].Range;
        doc.Tables.Add(range, q.Responses.Count + 1, 3);
        var table = doc.Tables[_questionCounter];

        try
        {
            table.Range.Font.Size = 10;
            table.Style = "Table Grid Light";
        }
        catch
        {
            table.Range.Font.Size = 10;
        }

        table.Cell(1, 1).Range.Text = "Value";
        table.Cell(1, 2).Range.Text = "Label";
        table.Cell(1, 3).Range.Text = "Frequency";

        for (int i = 0; i < q.Responses.Count; i++)
        {
            var r = q.Responses[i];
            table.Cell(i + 2, 1).Range.Text = r.RespValue.ToString();
            table.Cell(i + 2, 2).Range.Text = r.Label;
            table.Cell(i + 2, 2).Range.Bold = true;
            table.Cell(i + 2, 3).Range.Text = $"{r.ResultPercent:P1}";
            table.Cell(i + 2, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
        }

        table.Rows.LeftIndent = 25;
        table.Columns.PreferredWidthType = Word.WdPreferredWidthType.wdPreferredWidthPoints;
        table.Columns[1].Width = 40;
        table.Columns[2].Width = 180;
        table.Columns[3].Width = 60;
        table.PreferredWidth = 290;

        _questionCounter++;
    }

    private void UpdateProgress(Question q, int current)
    {
        var percentage = _topLineQuestions > 0 ? (int)Math.Ceiling(100.0 * current / _topLineQuestions) : 0;
        if (percentage > 100) percentage = 100;

        var text = $"Processing question {current} of {_topLineQuestions} ({q.QstNumber}. {q.QstLabel})";
        _bgWorker.ReportProgress(percentage, new ProgressArgs { ProgressText = text, ElapsedTime = _stopwatch.Elapsed });
    }

    private static void BringWordToFront()
    {
        var process = Process.GetProcessesByName("WINWORD")
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
        if (process != null)
            SetForegroundWindow(process.MainWindowHandle);
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