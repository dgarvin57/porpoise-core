// Porpoise.Core/Application/Interfaces/IProjectInfoShell.cs
#nullable enable

using Porpoise.Core.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Porpoise.Core.Application.Interfaces;

public interface IProjectInfoShell
{
    Project Project { get; set; }
    DialogResult DialogResult { get; set; }
    bool IsDirty { get; }

    void BindProject(Project project);
    void SetProjectPath(string path);
    void SetLogo(Bitmap? logo, string? path);
    void ShowLogo(Bitmap? logo);
    void ClearLogo();
    void ShowClearLogo(bool visible);
    void ShowExported(bool exported);
    void SetReadOnly(bool readOnly);

    event EventHandler OnLoad;
    event EventHandler<FormClosingEventArgs> OnClosing;
    event EventHandler OnSave;
    event Action<string?> OnLogoSelected;
    event EventHandler OnLogoCleared;
}