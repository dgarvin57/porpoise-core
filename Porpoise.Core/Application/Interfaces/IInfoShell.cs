// Porpoise.Core/Application/Interfaces/IInfoShell.cs
#nullable enable

using System;

namespace Porpoise.Core.Application.Interfaces;

public interface IInfoShell
{
    string RtfPath { get; }

    void LoadRtf(string rtfContent);

    event EventHandler OnLoad;
}