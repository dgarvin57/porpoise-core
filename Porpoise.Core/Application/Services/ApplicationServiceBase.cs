// Porpoise.Core/Application/Services/ApplicationServiceBase.cs
#nullable enable

using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Base class for all application services (formerly Presenters)
/// Handles error logging, user messaging, and shell interaction
/// </summary>
public abstract class ApplicationServiceBase
{
    protected IApplicationShell Shell { get; }

    protected ApplicationServiceBase(IApplicationShell shell)
    {
        Shell = shell ?? throw new ArgumentNullException(nameof(shell));
        Shell.ErrorOnShell += OnShellError;
    }

    private void OnShellError(object? sender, ErrorOnShellEventArgs e)
    {
        ErrorLogManager.LogError(e.ErrorLog);
    }

    protected bool HandleError(
        Exception ex,
        string userMessage,
        string? fromObject = null,
        [CallerMemberName] string? fromMethod = null)
    {
        var caller = fromObject ?? GetType().Name;
        var method = fromMethod ?? "Unknown";

        var errorLog = new ErrorLog(
            ex: ex,
            userMessage: userMessage,
            fromObject: caller,
            fromMethod: method,
            user: "CurrentUser", // Will come from shell later
            role: "CurrentRole",
            logPath: Shell.ErrorLogPath);

        try
        {
            Shell.ShowErrorMessage(userMessage, ex, "Porpoise Error");
            ErrorLogManager.LogError(Shell.ErrorLogPath, errorLog);
            return true;
        }
        catch
        {
            // If even error reporting fails, we let it bubble
            return false;
        }
    }

    protected void ShowHelp(string? topic = null)
    {
        Shell.Hourglass(true);
        try
        {
            var helpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", Shell.SettingsHelpFileName);

            if (!File.Exists(helpPath))
            {
                HandleError(
                    new FileNotFoundException($"Help file not found: {helpPath}"),
                    $"The help file '{Shell.SettingsHelpFileName}' was not found.",
                    nameof(ApplicationServiceBase));
                return;
            }

            // For web, this will open in browser. For desktop, use Help.ShowHelp
            Process.Start(new ProcessStartInfo(helpPath) { UseShellExecute = true });
        }
        finally
        {
            Shell.Hourglass(false);
        }
    }
}