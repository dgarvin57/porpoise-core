// Porpoise.Core/Application/Services/OptionsService.cs
#nullable enable

using System;
using System.IO;
using Porpoise.Core.Application.Interfaces;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Handles the Options/Settings dialog
/// Changes project/template paths, copies projects, updates MRU
/// Fully decoupled from WinForms/Telerik
/// </summary>
public class OptionsService : ApplicationServiceBase
{
    private readonly IOptionsShell _view;
    private readonly RecentProjectsService _mru;
    private string _oldProjectPath = "";
    private string _oldTemplatePath = "";

    public OptionsService(IOptionsShell view, IApplicationShell shell, RecentProjectsService mru)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _mru = mru ?? throw new ArgumentNullException(nameof(mru));
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
        _view.OnClosing += OnClosing;
        _view.OnApply += OnApply;
        _view.OnCancel += OnCancel;
        _view.OnProjectPathChanged += OnProjectPathChanged;
        _view.OnTemplatePathChanged += OnTemplatePathChanged;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            _view.ProjectPath = Shell.SettingsBaseProjectDir;
            _view.TemplatePath = Shell.SettingsBaseTemplateDir;
            _view.CompanyPath = Shell.SettingsLocalDataPath;

            _oldProjectPath = _view.ProjectPath;
            _oldTemplatePath = _view.TemplatePath;

            _view.SetApplyEnabled(false);
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred loading Options.");
        }
    }

    private void OnClosing(object? sender, FormClosingEventArgs e)
    {
        if (_view.ApplyEnabled)
        {
            var result = Shell.AskYesNoCancel("Save changes before closing?", "Options");
            if (result == DialogResult.Yes)
            {
                OnApply(sender, EventArgs.Empty);
                if (_view.DialogResult == DialogResult.Abort)
                    e.Cancel = true;
            }
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }

    private void OnApply(object? sender, EventArgs e)
    {
        try
        {
            Shell.Hourglass(true);

            if (!ValidatePaths(out var error))
            {
                Shell.ShowMessage(error, "Invalid Path");
                _view.DialogResult = DialogResult.Abort;
                return;
            }

            var msg = $"Change project folder to:\n{_view.ProjectPath}\n\n" +
                      $"This will copy all projects and update recent list.\n" +
                      $"Original projects remain in:\n{_oldProjectPath}\n\nContinue?";

            if (Shell.AskYesNo(msg, "Confirm Project Path Change") != DialogResult.Yes)
            {
                _view.DialogResult = DialogResult.Abort;
                return;
            }

            // Copy projects and update MRU
            _mru.ChangeProjectPath(_oldProjectPath, _view.ProjectPath, copyFiles: true);

            // Save settings
            Shell.SettingsBaseProjectDir = _view.ProjectPath;
            Shell.SettingsBaseTemplateDir = _view.TemplatePath;

            _view.SetApplyEnabled(false);
            _view.DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            HandleError(ex, "Failed to apply options.");
            _view.DialogResult = DialogResult.Abort;
        }
        finally
        {
            Shell.Hourglass(false);
        }
    }

    private void OnCancel(object? sender, EventArgs e)
    {
        _view.DialogResult = DialogResult.Cancel;
    }

    private void OnProjectPathChanged(object? sender, EventArgs e)
    {
        if (_view.ProjectPath == _oldProjectPath) return;

        // Auto-update template path
        var newTemplate = Path.Combine(_view.ProjectPath, Shell.SettingsBaseTemplateFolder);
        Directory.CreateDirectory(newTemplate);
        _view.TemplatePath = newTemplate;

        _view.SetApplyEnabled(true);
    }

    private void OnTemplatePathChanged(object? sender, EventArgs e)
    {
        if (_view.TemplatePath != _oldTemplatePath)
            _view.SetApplyEnabled(true);
    }

    private bool ValidatePaths(out string error)
    {
        error = "";

        if (!Directory.Exists(_view.ProjectPath))
            error = $"Project path does not exist:\n{_view.ProjectPath}";

        else if (!Directory.Exists(_view.TemplatePath))
            error = $"Template path does not exist:\n{_view.TemplatePath}";

        return string.IsNullOrEmpty(error);
    }
}