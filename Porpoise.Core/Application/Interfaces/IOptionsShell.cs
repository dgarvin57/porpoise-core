// Porpoise.Core/Application/Interfaces/IOptionsShell.cs
#nullable enable

using System;
using System.Windows.Forms;

namespace Porpoise.Core.Application.Interfaces;

public interface IOptionsShell
{
    string ProjectPath { get; set; }
    string TemplatePath { get; set; }
    string CompanyPath { get; set; }

    DialogResult DialogResult { get; set; }
    bool ApplyEnabled { get; }

    void SetApplyEnabled(bool enabled);

    event EventHandler OnLoad;
    event EventHandler<FormClosingEventArgs> OnClosing;
    event EventHandler OnApply;
    event EventHandler OnCancel;
    event EventHandler OnProjectPathChanged;
    event EventHandler OnTemplatePathChanged;
}