#nullable enable

using Porpoise.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Porpoise.Core.Models;

/// <summary>
/// The intelligent change-tracking and user-communication engine of Porpoise.
/// 
/// WhatChanged is not just a list — it's a **smart, user-friendly audit trail** that:
/// • Tracks every automatic correction made during data import/validation
/// • Prevents silent data changes by clearly explaining what was fixed and why
/// • Builds beautiful, human-readable messages for the user
/// • Powers one of Porpoise’s most beloved features: transparency and trust
/// 
/// </summary>
public class WhatChanged
{
    private List<WhatChangedItem> _items = [];
    private ChangesApplyTo _appliedTo = (ChangesApplyTo)(-1);
    private string _messageTitle = string.Empty;
    private string _messageDetail = string.Empty;

    #region Constructors

    public WhatChanged() { }

    public WhatChanged(ChangesApplyTo appliedTo, string messageTitle)
    {
        _appliedTo = appliedTo;
        _messageTitle = messageTitle;
    }

    public WhatChanged(ChangesApplyTo appliedTo)
    {
        _appliedTo = appliedTo;
    }

    #endregion

    #region Public Properties

    public ChangesApplyTo AppliedTo
    {
        get => _appliedTo;
        set => _appliedTo = value;
    }

    public string MessageTitle
    {
        get
        {
            FormatMessage();
            return _messageTitle;
        }
        set => _messageTitle = value;
    }

    public string MessageDetail
    {
        get
        {
            FormatMessage();
            return _messageDetail;
        }
        set => _messageDetail = value;
    }

    public List<WhatChangedItem> Items
    {
        get => _items;
        set => _items = value;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add a new change item. Only adds if an identical change doesn't already exist.
    /// </summary>
    public void AddItem(WhatChangedItem item)
    {
        if (_items.Any(i => i.ItemChanged == item.ItemChanged && i.FromValue == item.FromValue))
            return;

        _items.Add(item);
    }

    private void FormatMessage()
    {
        if (_items.Count == 0 && _appliedTo == (ChangesApplyTo)(-1)) return;

        _messageTitle = "Automatic corrections were made";

        var sb = new StringBuilder();
        sb.Append("Changes have been automatically applied to ")
          .Append(_appliedTo.Description().ToLower());

        foreach (var item in _items)
        {
            sb.Append($"\n• {item.ItemChanged.Description()} from '{item.FromValue}' to '{item.ToValue}'");
        }

        _messageDetail = sb.ToString();
    }

    #endregion
}