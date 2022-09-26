using System;

namespace Poltergeist.Automations.Configs;

public class OptionItem<T> : IOptionItem
{
    public string Key { get; }
    public T Value { get; set; }
    private T DefaultValue { get; }

    public string DisplayLabel { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsBrowsable { get; set; } = true;

    public bool HasChanged { get; set; }



    public OptionItem(string key, T defaultValue)
    {
        Key = key;
        DefaultValue = defaultValue;

        Value = defaultValue;
    }

    public OptionItem(string key) : this(key, default)
    {
        Key = key;
    }

    public bool IsDefault =>
        Value is null ? DefaultValue is not null
        : Value is IEquatable<T> ie ? ie.Equals(DefaultValue)
        : Value.ToString() == DefaultValue.ToString();

    public Type Type => typeof(T);

    object IOptionItem.Value
    {
        get => Value;
        set => Value = (T)value;
    }
    object IOptionItem.Default => DefaultValue;
}
