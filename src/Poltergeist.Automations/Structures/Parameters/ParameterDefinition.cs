namespace Poltergeist.Automations.Structures.Parameters;

public class ParameterDefinition<T> : ParameterDefinitionBase, IParameterDefinition
{
    public Type BaseType => typeof(T);

    public Func<T, string>? Format { get; set; }

    public object? DefaultValue { get; }

    public ParameterDefinition(string key) : base(key)
    {
        DefaultValue = default(T);
    }

    public ParameterDefinition(string key, T? value) : base(key)
    {
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
            throw new ArgumentException($"The value \"${value}\" is not of type \"{nameof(T)}\".");
        }

        if (Format is not null)
        {
            return Format(valueOfT);
        }

        return value.ToString() ?? "";
    }
}
