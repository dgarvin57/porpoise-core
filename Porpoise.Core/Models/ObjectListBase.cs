#nullable enable

using System.Collections.Generic;
using System.ComponentModel;

namespace Porpoise.Core.Models;

[Serializable]
public class ObjectListBase<T> : List<T>, INotifyPropertyChanged, ICloneable
    where T : ObjectBase
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsDirty => this.Any(item => item.IsDirty);

    public BindingList<T> BindingList
    {
        get
        {
            var bl = new BindingList<T>();
            foreach (var x in this) bl.Add(x);
            return bl;
        }
    }

    public new void Add(T listItem)
    {
        base.Add(listItem);
        if (listItem != null)
        {
            listItem.PropertyChanged -= Item_IsDirtyChanged;
            listItem.PropertyChanged += Item_IsDirtyChanged;
        }
    }

    private void Item_IsDirtyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ObjectBase.IsDirty))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDirty)));
    }

    public new void Clear()
    {
        for (int i = Count - 1; i >= 0; i--)
            RemoveAt(i);
    }

    public object Clone()
    {
        var newList = new ObjectListBase<T>();

        if (Count == 0) return newList;

        foreach (T item in this)
        {
            newList.Add((T)(item.Clone() ?? throw new InvalidOperationException("Clone returned null")));
        }

        return newList;
    }
}