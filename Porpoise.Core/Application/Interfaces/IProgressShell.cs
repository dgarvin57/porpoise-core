// Porpoise.Core/Application/Interfaces/IProgressShell.cs
#nullable enable

using System;

namespace Porpoise.Core.Application.Interfaces;

public interface IProgressShell
{
    void Reset();
    void SetTask(string text);
    void SetProgress(int percent, string text);
    void SetRemaining(string text);
    void ClearRemaining();
    void Close();

    event EventHandler OnLoad;
}