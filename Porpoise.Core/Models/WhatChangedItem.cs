#nullable enable

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a single automatic data correction made by Porpoise during import or validation.
/// 
/// Part of Porpoise's legendary transparency engine — one of the features that made users
/// trust the software like no other tool ever did.
/// 
/// Instead of silently "fixing" data (like every other tool), Porpoise told you:
/// • Exactly what was changed
/// • From what value to what value
/// • Why it was necessary
/// 
/// </summary>
/// <param name="itemChanged">The type of change that occurred</param>
/// <param name="fromValue">Original value (null becomes empty string)</param>
/// <param name="toValue">New corrected value (null becomes empty string)</param>
public class WhatChangedItem(WhatChangedEnum itemChanged, string? fromValue, string? toValue)
{
    private WhatChangedEnum _itemChanged = itemChanged;
    private string _fromValue = fromValue ?? string.Empty;
    private string _toValue = toValue ?? string.Empty;

    public WhatChangedEnum ItemChanged
    {
        get => _itemChanged;
        set => _itemChanged = value;
    }

    public string FromValue
    {
        get => _fromValue;
        set => _fromValue = value ?? string.Empty;
    }

    public string ToValue
    {
        get => _toValue;
        set => _toValue = value ?? string.Empty;
    }
}