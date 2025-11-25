// Porpoise.Core/Application/Interfaces/IApplicationShell.cs
#nullable enable

using System.Drawing;
using System.Reflection;
using Porpoise.Core.Models;

namespace Porpoise.Core.Application.Interfaces;

/// <summary>
/// The shell abstraction — everything the UI needs to talk to the application layer.
/// This replaces IBase 1:1 and is UI-agnostic (works with WinForms, Blazor, MAUI, WPF, etc.)
/// </summary>
public interface IApplicationShell
{
    // Settings (read-only from shell)
    string SettingsBaseProjectDir { get; }
    string SettingsBaseTemplateDir { get; }
    string SettingsCompanyFolder { get; }
    string SettingsProjectsDir { get; }
    string SettingsTempPath { get; }
    string SettingsUnlockWebServiceBasePath { get; }
    string SettingsBasePorpoiseWebsiteUri { get; }
    string SettingsHelpFileName { get; }
    string ErrorLogPath { get; }

    Color SettingsBlockQstHighlightColor { get; }
    bool SettingsCheckBlockConsistencyInDefinition { get; }
    bool SettingsConfirmBlockDataChanges { get; }
    bool SettingsEULAAccepted { get; set; }

    Point SettingsMainViewPosition { get; set; }
    Size SettingsMainViewSize { get; set; }

    // UI Actions
    void Hourglass(bool enable);
    void ShowErrorMessage(string message, Exception? ex = null, string? title = null);
    void ShowMessage(string message, string title = "Porpoise");
    bool AskYesNo(string message, string title = "Confirm");
    bool AskYesNoCancel(string message, string title = "Confirm");
    DialogResult ShowFileOpenDialog(ref string filename, string initialDirectory, string filter, string title);
    DialogResult ShowFileSaveDialog(ref string filename, string initialDirectory, string filter, string title);

    // Events
    event EventHandler<ErrorOnShellEventArgs> ErrorOnShell;
}

/// <summary>
/// Standard dialog result — matches WinForms but works everywhere
/// </summary>
public enum DialogResult
{
    None,
    OK,
    Cancel,
    Yes,
    No
}

/// <summary>
/// Error event args — matches your old ErrorOnViewArgs
/// </summary>
public class ErrorOnShellEventArgs : EventArgs
{
    public ErrorLog ErrorLog { get; }

    public ErrorOnShellEventArgs(ErrorLog errorLog)
    {
        ErrorLog = errorLog;
    }
}