#nullable enable

using System;

namespace Porpoise.Core.Models;

/// <summary>
/// Part of Porpoise’s exclusive Preference Block analysis — a feature almost no competitor has ever matched.
/// 
/// Enables true forced-choice preference ranking across 4–6 items with full statistical rigor,
/// automatic syncing, and visual heatmaps. A genuine analytical superpower.
/// </summary>
 
[Serializable]
public class PreferenceItem : ObjectBase
{
    private string _itemId = string.Empty;
    private string _itemName = string.Empty;

    public PreferenceItem() { }

    public PreferenceItem(string id, string name)
    {
        ItemId = id;
        ItemName = name;
    }

    public string ItemId
    {
        get => _itemId;
        set => SetProperty(ref _itemId, value, nameof(ItemId));
    }

    public string ItemName
    {
        get => _itemName;
        set => SetProperty(ref _itemName, value, nameof(ItemName));
    }
}