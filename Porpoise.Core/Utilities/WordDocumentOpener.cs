#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Porpoise.Core.Utilities;

/// <summary>
/// Opens an RTF or DOC file in Microsoft Word (reuses existing instance if running).
/// Used by Topline, Crosstab, and other report exports.
/// 
/// NOTE: This utility requires Microsoft Office and COM Interop support.
/// Marshal.GetActiveObject was removed in .NET 5+. To enable this feature:
/// 1. Install Microsoft.Office.Interop.Word NuGet package
/// 2. Add System.Runtime.InteropServices.Marshal.GetActiveObject support
/// Or consider modern alternatives like Open XML SDK, Syncfusion, or export to PDF/HTML.
/// </summary>
public sealed class WordDocumentOpener(string filePath) : IDisposable
{
    private readonly string _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

    // DllImport is used instead of LibraryImport to avoid requiring unsafe code blocks.
    // LibraryImport would require <AllowUnsafeBlocks>true</AllowUnsafeBlocks> in the project file.
    // For this simple boolean P/Invoke, the performance benefit is negligible.
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
#pragma warning restore SYSLIB1054

    /// <summary>
    /// Opens the RTF/DOC file in Word and brings it to the foreground.
    /// </summary>
    public void OpenInWord()
    {
        throw new NotImplementedException(
            "Word document opening requires COM Interop which is not available in .NET 5+. " +
            "Consider using Process.Start to open documents in the default application, " +
            "or use modern document generation libraries like Open XML SDK for Office documents.");
    }

    private static void BringWordToFront()
    {
        var wordProcess = Process.GetProcessesByName("WINWORD")
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);

        if (wordProcess is not null)
            SetForegroundWindow(wordProcess.MainWindowHandle);
    }

    public void Dispose()
    {
        // Stub - no cleanup needed
    }
}