// Porpoise.Core/Application/Services/RecentProjectsService.cs
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Porpoise.Core.Application.Interfaces;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Manages the Most Recently Used (MRU) project list
/// Persists to Registry (can be changed to JSON later)
/// Fully decoupled from WinForms/Telerik
/// </summary>
public class RecentProjectsService
{
    private const int MaxEntries = 10;
    private const string AppKey = "Porpoise";
    private const string ProjectsKey = "MruListProjects";

    private readonly List<string> _recentProjects = new();
    private readonly IMainShell _shell;

    public IReadOnlyList<string> RecentProjects => _recentProjects.AsReadOnly();

    public event Action<string>? ProjectSelected;

    public RecentProjectsService(IMainShell shell)
    {
        _shell = shell ?? throw new ArgumentNullException(nameof(shell));
        LoadFromRegistry();
        LoadSampleProjects();
    }

    public void Add(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath) || !File.Exists(projectPath))
            return;

        _recentProjects.RemoveAll(p => string.Equals(p, projectPath, StringComparison.OrdinalIgnoreCase));
        _recentProjects.Insert(0, projectPath);

        if (_recentProjects.Count > MaxEntries)
            _recentProjects.RemoveRange(MaxEntries, _recentProjects.Count - MaxEntries);

        SaveToRegistry();
        _shell.RefreshRecentProjects();
    }

    public void Remove(string projectPath)
    {
        _recentProjects.RemoveAll(p => string.Equals(p, projectPath, StringComparison.OrdinalIgnoreCase));
        SaveToRegistry();
        _shell.RefreshRecentProjects();
    }

    public void Clear()
    {
        _recentProjects.Clear();
        SaveToRegistry();
        _shell.RefreshRecentProjects();
    }

    public void OpenProject(string projectPath)
    {
        if (File.Exists(projectPath))
        {
            Add(projectPath);
            ProjectSelected?.Invoke(projectPath);
        }
        else
        {
            _shell.ShowMessage($"Project not found: {projectPath}\nIt has been removed from recent projects.", "Project Not Found");
            Remove(projectPath);
        }
    }

    public void ChangeProjectPath(string oldPath, string newPath, bool copyFiles)
    {
        var updated = new List<string>();

        foreach (var path in _recentProjects)
        {
            if (path.StartsWith(oldPath, StringComparison.OrdinalIgnoreCase))
            {
                var relative = path.Substring(oldPath.Length).TrimStart('\\', '/');
                var newFullPath = Path.Combine(newPath, relative);
                updated.Add(newFullPath);
            }
            else
            {
                updated.Add(path);
            }
        }

        if (copyFiles && Directory.Exists(oldPath))
        {
            CopyDirectory(oldPath, newPath);
        }

        _recentProjects.Clear();
        _recentProjects.AddRange(updated.Distinct());
        SaveToRegistry();
    }
    
    private void LoadFromRegistry()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey($@"Software\{AppKey}\{ProjectsKey}");
            if (key == null) return;

            for (int i = 1; i <= MaxEntries; i++)
            {
                var path = key.GetValue($"FileName{i}") as string;
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    _recentProjects.Add(path);
            }
        }
        catch { /* ignore registry errors */ }
    }

    private void SaveToRegistry()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey($@"Software\{AppKey}\{ProjectsKey}");
            key?.Close();
            using var writeKey = Registry.CurrentUser.OpenSubKey($@"Software\{AppKey}\{ProjectsKey}", true);

            // Clear old
            for (int i = 1; i <= MaxEntries; i++)
                writeKey?.DeleteValue($"FileName{i}", false);

            // Write new
            for (int i = 0; i < _recentProjects.Count; i++)
                writeKey?.SetValue($"FileName{i + 1}", _recentProjects[i]);
        }
        catch { /* ignore */ }
    }

    private void LoadSampleProjects()
    {
        var samplesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples");
        if (!Directory.Exists(samplesPath)) return;

        foreach (var dir in Directory.GetDirectories(samplesPath))
        {
            var porpFile = Directory.GetFiles(dir, "*.porp").FirstOrDefault();
            if (porpFile != null)
            {
                var destDir = Path.Combine(_shell.SettingsBaseProjectDir, Path.GetFileName(dir));
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                    CopyDirectory(dir, destDir);
                }

                var destFile = Path.Combine(destDir, Path.GetFileName(porpFile));
                if (File.Exists(destFile) && !_recentProjects.Contains(destFile))
                    _recentProjects.Add(destFile);
            }
        }

        SaveToRegistry();
    }

    private static void CopyDirectory(string source, string dest)
    {
        foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(source, dest));

        foreach (var file in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            File.Copy(file, file.Replace(source, dest), true);
    }

}