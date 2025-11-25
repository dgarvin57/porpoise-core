// Porpoise.Core/Application/Interfaces/IAlertMessageBoxShell.cs
#nullable enable

using System;
using System.Drawing;

namespace Porpoise.Core.Application.Interfaces;

public enum MessageBoxIcon
{
    None,
    Information,
    Question,
    Exclamation,
    Warning = Exclamation,
    Error,
    Stop = Error
}

public interface IAlertMessageBoxShell
{
    string Title { get; }
    MessageBoxIcon MessageBoxIcon { get; }

    Bitmap? InformationIcon { get; }
    Bitmap? QuestionIcon { get; }
    Bitmap? ExclamationIcon { get; }
    Bitmap? StopIcon { get; }

    void SetIcon(Bitmap? icon);

    event EventHandler OnLoad;
    event EventHandler OnPrimaryButtonClick;
    event EventHandler OnSecondaryButtonClick;
}