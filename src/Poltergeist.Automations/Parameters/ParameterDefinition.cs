namespace Poltergeist.Automations.Parameters;

public class ParameterDefinition<T> : IParameterDefinition
{
    public string Key { get; }

    private string? _displayLabel;
    public string DisplayLabel
    {
        get => _displayLabel ?? Key;
        set => _displayLabel = value;
    }

    public string? Category { get; set; }
    public string? Description { get; set; }

    public Type BaseType => typeof(T);

    public Func<T, string>? Format { get; set; }

    public ParameterStatus Status { get; set; }

    public object? DefaultValue { get; }

    public ParameterDefinition(string key)
    {
        Key = key;
        DefaultValue = default(T);
    }

    public ParameterDefinition(string key, T? value)
    {
        Key = key;
        DefaultValue = value;
    }

    public bool IsDefault(object? value)
    {
        if (value is null)
        {
            return DefaultValue is not null;
        }

        if (value is IEquatable<T> ie)
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

        return value.ToString() == DefaultValue.ToString();
    }

    public virtual string FormatValue(object? value)
    {
        if (value is null)
        {
            return "(null)";
        }

        if (value is not T valueOfT)
        {
            throw new ArgumentException();
        }

        if (Format is not null)
        {
            return Format(valueOfT);
        }

        return value.ToString() ?? "";
    }
}
