#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;

namespace Porpoise.Core.Utilities;

/// <summary>
/// Opens an RTF or DOC file in Microsoft Word (reuses existing instance if running).
/// Used by Topline, Crosstab, and other report exports.
/// </summary>
public sealed class WordDocumentOpener : IDisposable
{
    private readonly string _filePath;
    private Word.Application? _wordApp;
    private bool _createdNewInstance;

    public WordDocumentOpener(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// Opens the RTF/DOC file in Word and brings it to the foreground.
    /// </summary>
    public void OpenInWord()
    {
        try
        {
            // Try to reuse existing Word instance
            _wordApp = (Word.Application)Marshal.GetActiveObject("Word.Application");
            _createdNewInstance = false;
        }
        catch (COMException)
        {
            // Word not running — create new instance
            try
            {
                _wordApp = new Word.Application();
                _createdNewInstance = true;
            }
            catch (COMException ex)
            {
                throw new InvalidOperationException("Microsoft Word is not installed or cannot be started.", ex);
            }
        }

        if (_wordApp == null)
            throw new InvalidOperationException("Failed to initialize Microsoft Word.");

        _wordApp.Visible = true;

        // Open the document (read-only recommended for reports)
        _wordApp.Documents.Open(
            FileName: _filePath,
            ReadOnly: true,
            Visible: true);

        BringWordToFront();
    }

    private void BringWordToFront()
    {
        var wordProcess = Process.GetProcessesByName("WINWORD")
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

        if (wordProcess != null)
            SetForegroundWindow(wordProcess.MainWindowHandle);
    }

    public void Dispose()
    {
        // Only quit if we created the instance
        if (_createdNewInstance && _wordApp != null)
        {
            try { _wordApp.Quit(); }
            catch { /* ignore */ }
        }

        if (_wordApp != null)
            Marshal.ReleaseComObject(_wordApp);
    }
}