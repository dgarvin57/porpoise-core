// Porpoise.Core/Application/Services/ProjectInfoService.cs
#nullable enable

using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;
using System;
using System.Drawing;
using System.IO;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Handles the Project Information dialog
/// Edit name, client, logo, path — with full validation and rename support
/// Fully decoupled from WinForms/Telerik
/// </summary>
public class ProjectInfoService : ApplicationServiceBase
{
    private readonly IProjectInfoShell _view;
    private readonly RecentProjectsService _mru;
    private Project _originalProject = null!;
    private string _originalProjectName = "";
    private bool _readOnly;

    public ProjectInfoService(IProjectInfoShell view, IApplicationShell shell, RecentProjectsService mru, bool readOnly = false)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _mru = mru ?? throw new ArgumentNullException(nameof(mru));
        _readOnly = readOnly;
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
        _view.OnClosing += OnClosing;
        _view.OnSave += OnSave;
        _view.OnLogoSelected += OnLogoSelected;
        _view.OnLogoCleared += OnLogoCleared;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            _originalProject = _view.Project.Clone();
            _originalProjectName = _view.Project.ProjectName;

            _view.BindProject(_view.Project);
            _view.SetProjectPath(_view.Project.FullPath);
            _view.SetLogo(_view.Project.ResearcherLogo, _view.Project.ResearcherLogoPath);
            _view.ShowExported(_view.Project.IsExported);
            _view.SetReadOnly(_readOnly);
        }
        catch (Exception ex)
        {
            HandleError(ex, "Error loading project information.");
        }
    }

    private void OnClosing(object? sender, FormClosingEventArgs e)
    {
        if (_view.IsDirty)
        {
            var result = Shell.AskYesNoCancel("Save changes before closing?", "Project Info");
            if (result == DialogResult.Yes)
            {
                OnSave(sender, EventArgs.Empty);
                if (_view.DialogResult == DialogResult.Abort)
                    e.Cancel = true;
            }
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                // Revert
                _view.Project = _originalProject;
            }
        }
    }

    private void OnSave(object? sender, EventArgs e)
    {
        try
        {
            Shell.Hourglass(true);

            if (!ValidateProject())
            {
                _view.DialogResult = DialogResult.Abort;
                return;
            }

            if (_view.Project.ProjectName != _originalProjectName)
            {
                var newPath = Path.Combine(
                    Path.GetDirectoryName(_view.Project.FullPath)!,
                    _view.Project.ProjectName);

                if (!ProjectEngine.IsProjectNameUnique(newPath, _view.Project.ProjectName))
                {
                    Shell.ShowMessage($"Project name '{_view.Project.ProjectName}' is already used.", "Invalid Name");
                    _view.DialogResult = DialogResult.Abort;
                    return;
                }

                ProjectEngine.RenameProject(_view.Project, newPath);
                _mru.Remove(Path.Combine(Path.GetDirectoryName(_view.Project.FullPath)!, $"{_originalProjectName}.porp"));
            }

            // Update logo path
            _view.Project.ResearcherLogoPath = string.IsNullOrEmpty(_view.Project.ResearcherLogoFilename)
                ? ""
                : Path.Combine(_view.Project.FullFolder, _view.Project.ResearcherLogoFilename);

            ProjectEngine.SaveProject(_view.Project);
            _view.DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            HandleError(ex, "Failed to save project information.");
            _view.DialogResult = DialogResult.Abort;
        }
        finally
        {
            Shell.Hourglass(false);
        }
    }

    private void OnLogoSelected(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        try
        {
            using var original = new Bitmap(path);
            _view.Project.ResearcherLogo = new Bitmap(original);
            _view.Project.ResearcherLogoFilename = Path.GetFileName(path);
            _view.Project.ResearcherLogoPath = path;
            _view.ShowLogo(_view.Project.ResearcherLogo);
            _view.ShowClearLogo(true);
        }
        catch (Exception ex)
        {
            HandleError(ex, "Failed to load logo image.");
        }
    }

    private void OnLogoCleared(object? sender, EventArgs e)
    {
        _view.Project.ResearcherLogo = null;
        _view.Project.ResearcherLogoFilename = "";
        _view.Project.ResearcherLogoPath = "";
        _view.ClearLogo();
        _view.ShowClearLogo(false);
    }

    private bool ValidateProject()
    {
        if (string.IsNullOrWhiteSpace(_view.Project.ProjectName))
        {
            Shell.ShowMessage("Project name is required.", "Validation");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_view.Project.ClientName))
        {
            Shell.ShowMessage("Client name is required.", "Validation");
            return false;
        }

        return true;
    }
}