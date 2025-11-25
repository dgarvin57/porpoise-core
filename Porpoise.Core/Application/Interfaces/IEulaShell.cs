// Porpoise.Core/Application/Interfaces/IEulaShell.cs
#nullable enable

using System;

namespace Porpoise.Core.Application.Interfaces;

public interface IEulaShell
{
    bool Accepted { get; }

    void LoadLicense(string rtfContent);
    void CloseWithResult(DialogResult result);

    event EventHandler OnLoad;
    event EventHandler OnContinue;
    event EventHandler OnPrint;
}