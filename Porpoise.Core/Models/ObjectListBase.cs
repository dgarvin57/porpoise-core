#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Porpoise.Core.Models;

/// <summary>
/// Custom observable/dirty-tracking list used throughout Porpoise.
/// Now fully modern, supports collection-initializer cloning, and zero warnings.
/// </summary>
[Serializable]
public class ObjectListBase<T> : List<T>, INotifyPropertyChanged where T : class
{
    public ObjectListBase() { }

    /// <summary>
    /// Constructor that accepts a collection — enables beautiful deep cloning
    /// </summary>
    public ObjectListBase(IEnumerable<T> collection) : this()
    {
        if (collection is not null)
            AddRange(collection);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event PropertyChangedEventHandler? IsDirtyChanged;

    private bool _isDirty;
    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (_isDirty != value)
            {
                _isDirty = value;
                OnPropertyChanged();
                IsDirtyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDirty)));
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public new void Add(T item)
    {
        base.Add(item);
        IsDirty = true;
    }

    public new void AddRange(IEnumerable<T> collection)
    {
        if (collection is null) return;
        base.AddRange(collection);
        IsDirty = true;
    }

    public new bool Remove(T item)
    {
        bool removed = base.Remove(item);
        if (removed) IsDirty = true;
        return removed;
    }

    public new void RemoveAt(int index)
    {
        base.RemoveAt(index);
        IsDirty = true;
    }

    public new void Insert(int index, T item)
    {
        base.Insert(index, item);
        IsDirty = true;
    }

    public new void Clear()
    {
        if (Count == 0) return;
        base.Clear();
        IsDirty = true;
    }

    /// <summary>
    /// Reset dirty flag — used when loading/saving project
    /// </summary>
    public void MarkClean() => IsDirty = false;
}