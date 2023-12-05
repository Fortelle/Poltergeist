namespace Poltergeist.Automations.Configs;

public class OptionItem<T> : IOptionItem 
{
    public string Key { get; }

    private T? _value;
    public T? Value
    { 
        get => _value;
        set
        {
            var oldValue = _value;
            _value = value;

            HasChanged = true;

            Changed?.Invoke(this, new ChangedEventArgs(oldValue, value));
        }
    }

    private T? DefaultValue { get; }

    private string? _displayLabel;
    public string DisplayLabel {
        get => _displayLabel ?? Key;
        set => _displayLabel = value;
    }

    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsBrowsable { get; set; } = true;

    public bool HasChanged { get; set; }

    public event EventHandler<ChangedEventArgs>? Changed;

    public OptionItem(string key, T defaultValue)
    {
        Key = key;
        DefaultValue = defaultValue;

        _value = defaultValue;
    }

    public OptionItem(string key)
    {
        Key = key;

        if (BaseType.IsClass)
        {
            DefaultValue = default;
        }
        else
        {
            DefaultValue = (T?)Activator.CreateInstance(BaseType);
        }

        _value = DefaultValue;
    }

    public OptionItem(string key, string title, T defaultValue) : this(key, defaultValue)
    {
        DisplayLabel = title;
    }

    public Type BaseType => typeof(T);

    object? IOptionItem.Default => DefaultValue;

    public bool IsDefault
    {
        get
        {
            if(Value is null)
            {
                return DefaultValue is not null;
            }

            if (Value is IEquatable<T> ie)
            {
                return ie.Equals(DefaultValue);
            }

            if (BaseType.IsClass)
            {
                return false;
            }

            if (DefaultValue is null)
            {
                return false;
            }

            return Value.ToString() == DefaultValue.ToString();
        }
    }

    object? IOptionItem.Value
    {
        get => Value;
        set => Value = (T?)value;
    }

    public override string? ToString() => Value?.ToString();

    public class ChangedEventArgs : EventArgs
    {
        public T? OldValue { get; }
        public T? NewValue { get; }

        public ChangedEventArgs(T? oldValue, T? newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

    }
}
