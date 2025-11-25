// Porpoise.Core/Application/Services/InfoService.cs
#nullable enable

using System;
using System.IO;
using Porpoise.Core.Application.Interfaces;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Loads and displays static RTF information (About, Help, Project Info, etc.)
/// Fully decoupled from Telerik/WinForms
/// </summary>
public class InfoService : ApplicationServiceBase
{
    private readonly IInfoShell _view;

    public InfoService(IInfoShell view, IApplicationShell shell)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        try
        {
            if (!File.Exists(_view.RtfPath))
                throw new FileNotFoundException($"Info file not found: {_view.RtfPath}");

            var rtfContent = File.ReadAllText(_view.RtfPath);
            _view.LoadRtf(rtfContent);
        }
        catch (Exception ex)
        {
            HandleError(ex, "An error occurred loading information.");
        }
    }
}