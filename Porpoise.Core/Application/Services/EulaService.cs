// Porpoise.Core/Application/Services/EulaService.cs
#nullable enable

using System;
using System.IO;
using Porpoise.Core.Application.Interfaces;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Handles the EULA (End User License Agreement) screen
/// Loads RTF license, handles accept/decline, prints
/// Fully decoupled from WinForms/Telerik
/// </summary>
public class EulaService : ApplicationServiceBase
{
    private readonly IEulaShell _view;
    private readonly string _eulaPath;

    public EulaService(IEulaShell view, IApplicationShell shell)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));

        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        _eulaPath = Path.Combine(exeDir, "Resources", shell.SettingsEULAFilename);

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
        _view.OnContinue += OnContinue;
        _view.OnPrint += OnPrint;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            if (!File.Exists(_eulaPath))
                throw new FileNotFoundException($"EULA file not found: {_eulaPath}");

            var rtfContent = File.ReadAllText(_eulaPath);
            _view.LoadLicense(rtfContent);
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred loading the End User License Agreement.");
        }
    }

    private void OnContinue(object? sender, EventArgs e)
    {
        try
        {
            Shell.Hourglass(true);

            if (_view.Accepted)
            {
                Shell.SettingsEULAAccepted = true;
                _view.CloseWithResult(DialogResult.OK);
            }
            else
            {
                _view.CloseWithResult(DialogResult.Abort);
            }
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred continuing past the End User License Agreement.");
        }
        finally
        {
            Shell.Hourglass(false);
        }
    }

    private void OnPrint(object? sender, EventArgs e)
    {
        try
        {
            Shell.Hourglass(true);

            var wordOpener = new OpenInWord(_eulaPath);
            wordOpener.OpenRTFInWord();
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred printing the End User License Agreement.");
        }
        finally
        {
            Shell.Hourglass(false);
        }
    }
}