#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

namespace Porpoise.Core.Models;

/// <summary>
/// Base class for all business objects in the original Porpoise system.
/// Provides INotifyPropertyChanged, dirty tracking, audit fields, and deep cloning.
/// </summary>
[Serializable]
public class ObjectBase : INotifyPropertyChanged, ICloneable
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void SetProperty<T>(ref T field, T value, string name)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            _isDirty = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDirty)));
        }
    }

    protected ObjectBase() { }

    protected internal bool _isDirty;
    private bool _isNew = true;
    private DateTime _createdOn = DateTime.Now;
    private string _createdBy = Environment.UserName;
    private DateTime _modifiedOn;
    private string _modifiedBy = string.Empty;

    [XmlIgnore]
    public virtual bool IsDirty
    {
        get => _isDirty;
        set
        {
            _isDirty = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDirty)));
        }
    }

    public bool IsNew => _isNew;

    public DateTime CreatedOn
    {
        get => _createdOn;
        set => SetProperty(ref _createdOn, value, nameof(CreatedOn));
    }

    public string CreatedBy
    {
        get => _createdBy;
        set => SetProperty(ref _createdBy, value, nameof(CreatedBy));
    }

    public DateTime ModifiedOn
    {
        get => _modifiedOn;
        set => SetProperty(ref _modifiedOn, value, nameof(ModifiedOn));
    }

    public string ModifiedBy
    {
        get => _modifiedBy;
        set => SetProperty(ref _modifiedBy, value, nameof(ModifiedBy));
    }

    public virtual void MarkDirty() => IsDirty = true;

    public virtual void MarkClean()
    {
        if (!IsDirty) return;
        IsDirty = false;
    }

    public virtual void MarkAsNew()
    {
        if (_isNew) return;
        _isNew = true;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNew)));
    }

    public virtual void MarkAsOld()
    {
        if (!_isNew) return;
        _isNew = false;
        _isDirty = false;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNew)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDirty)));
    }

    public virtual object Clone() => Clone(this)!;

    private object? Clone(object? vObj)
    {
        if (vObj is null) return null;

        var type = vObj.GetType();
        if (type.IsValueType || type == typeof(string)) return vObj;

        bool origIsNew = IsNew;
        bool origIsDirty = IsDirty;

        var newObject = Activator.CreateInstance(type)!;

        // Handle IEnumerable that also implements ICloneable
        if (type.GetInterface("IEnumerable") != null && type.GetInterface("ICloneable") != null)
            return ((ICloneable)vObj).Clone();

        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanWrite) continue;

            object? value = prop.GetValue(vObj, null);

            if (value is ICloneable cloneable && cloneable != null)
                prop.SetValue(newObject, cloneable.Clone(), null);
            else
                prop.SetValue(newObject, Clone(value), null);
        }

        // Restore flags on the clone
        if (newObject is ObjectBase baseObj)
        {
            if (origIsNew) baseObj.MarkAsNew(); else baseObj.MarkAsOld();
            if (origIsDirty) baseObj.MarkDirty(); else baseObj.MarkClean();
        }

        return newObject;
    }
}