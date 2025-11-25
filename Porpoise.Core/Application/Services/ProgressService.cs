// Porpoise.Core/Application/Services/ProgressService.cs
#nullable enable

using System;
using System.Timers;
using Porpoise.Core.Application.Interfaces;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Manages a non-blocking progress dialog
/// Works with any long-running engine operation (Topline, PoolTrend, etc.)
/// Fully decoupled from WinForms/Telerik
/// </summary>
public class ProgressService : ApplicationServiceBase, IDisposable
{
    private readonly IProgressShell _view;
    private readonly System.Timers.Timer _closeTimer;

    public ProgressService(IProgressShell view, IApplicationShell shell)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));

        _closeTimer = new System.Timers.Timer(1500) { AutoReset = false };
        _closeTimer.Elapsed += CloseTimerElapsed;

        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnLoad;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        _view.Reset();
        Shell.Hourglass(true);
    }

    public void Start(string taskTitle, string initialText = "Starting...")
    {
        _view.SetTask(taskTitle);
        _view.SetProgress(0, initialText);
    }

    public void Update(int percent, string text, TimeSpan elapsed)
    {
        _view.SetProgress(percent, text);
        _view.SetRemaining(CalculateRemaining(percent, elapsed));
    }

    public void Complete(string finalText = "Complete")
    {
        _view.SetProgress(100, finalText);
        _view.ClearRemaining();
        _closeTimer.Start();
    }

    private void CloseTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            Shell.Hourglass(false);
            _view.Close();
        }
        catch { /* ignore if already closed */ }
    }

    private string CalculateRemaining(int done, TimeSpan elapsed)
    {
        if (done <= 0 || elapsed.TotalSeconds < 5)
            return "Estimating time remaining...";

        var secondsLeft = ((100.0 / done) * elapsed.TotalSeconds) - elapsed.TotalSeconds;

        return secondsLeft switch
        {
            < 60 => $"About {Math.Ceiling(secondsLeft)} seconds remaining",
            < 3600 => $"About {Math.Ceiling(secondsLeft / 60)} minutes remaining",
            _ => $"About {Math.Ceiling(secondsLeft / 3600)} hours remaining"
        };
    }

    public void Dispose()
    {
        _closeTimer.Stop();
        _closeTimer.Dispose();
    }
}