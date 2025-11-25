// Porpoise.Core/Application/Services/AlertMessageBoxService.cs
#nullable enable

using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Utilities;
using System;
using System.Media;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Handles the legacy "Alert Message Box" behavior — icon + sound + preference saving
/// This is a thin wrapper that will be called from your new UI (Blazor, WinUI, etc.)
/// </summary>
public class AlertMessageBoxService : ApplicationServiceBase
{
    private readonly IAlertMessageBoxShell _view;

    public AlertMessageBoxService(IAlertMessageBoxShell view, IApplicationShell shell)
        : base(shell)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _view.OnLoad += OnViewLoaded;
        _view.OnPrimaryButtonClick += OnButtonClicked;
        _view.OnSecondaryButtonClick += OnButtonClicked;
    }

    private void OnViewLoaded(object? sender, EventArgs e)
    {
        PlaySoundAndSetIcon(_view.MessageBoxIcon);
    }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        // Both buttons do the same: save a preference so user can suppress this message
        var preferenceKey = GeneratePreferenceKey(_view.Title);
        // In future: save to user settings / localStorage
        // UserSettings.SetPreference(preferenceKey, "NeverShowAgain");
    }

    private void PlaySoundAndSetIcon(MessageBoxIcon icon)
    {
        switch (icon)
        {
            case MessageBoxIcon.Information:
                _view.SetIcon(_view.InformationIcon);
                SystemSounds.Beep.Play();
                break;
            case MessageBoxIcon.Error:
            case MessageBoxIcon.Stop:
                _view.SetIcon(_view.StopIcon);
                SystemSounds.Hand.Play();
                break;
            case MessageBoxIcon.Exclamation:
            case MessageBoxIcon.Warning:
                _view.SetIcon(_view.ExclamationIcon);
                SystemSounds.Exclamation.Play();
                break;
            case MessageBoxIcon.Question:
                _view.SetIcon(_view.QuestionIcon);
                SystemSounds.Question.Play();
                break;
            case MessageBoxIcon.None:
                _view.SetIcon(null);
                SystemSounds.Beep.Play();
                break;
        }
    }

    private static string GeneratePreferenceKey(string title)
    {
        return StringUtils.ProperCase(title.Replace("?", ""), false).Replace(" ", "");
    }
}