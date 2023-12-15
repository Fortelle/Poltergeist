namespace Poltergeist.Automations.Parameters;

public class ParameterEntry<T> : IParameterEntry
{
    public string Key { get; }

    public virtual T Value { get; set; }

    private string? _displayLabel;
    public string DisplayLabel
    {
        get => _displayLabel ?? Key;
        set => _displayLabel = value;
    }

    public string? Category { get; set; }
    public string? Description { get; set; }

    public bool HasChanged { get; set; }

    public Type BaseType => typeof(T);

    public Func<T, string>? Format { get; set; }

    object? IParameterEntry.Value
    {
        get => Value;
        set => Value = (T)value;
    }

    public virtual string DisplayValue
    {
        get
        {
            if (Format is not null)
            {
                return Format(Value);
            }

            if (Value is not null)
            {
                return Value.ToString() ?? "";
            }

            return "(null)";
        }
    }

    public ParameterEntry(string key)
    {
        Key = key;
        Value = default;
    }

    public ParameterEntry(string key, T value)
    {
        Key = key;
        Value = value;
    }
}
