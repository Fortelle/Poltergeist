namespace Poltergeist.Automations.Parameters;

public class OptionItem<T> : ParameterEntry<T>, IOptionItem 
{
    private T _value;
    public override T Value
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

    public bool IsReadonly { get; set; }
    public bool IsBrowsable { get; set; } = true;

    public event EventHandler<ChangedEventArgs>? Changed;

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

    public override string DisplayValue => DefaultValue?.ToString() ?? base.DisplayValue;

    public OptionItem(string key, T defaultValue) : base(key)
    {
        DefaultValue = defaultValue;

        _value = defaultValue;
    }

    public OptionItem(string key) : base(key)
    {
        DefaultValue = default;
        _value = default;
    }

    public OptionItem(string key, string title, T defaultValue) : this(key, defaultValue)
    {
        DisplayLabel = title;
    }

    public class ChangedEventArgs : EventArgs
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public ChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
