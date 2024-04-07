namespace Poltergeist.Automations.Parameters;

public class ObservableParameterItem
{
    public IParameterDefinition Definition { get; }
    public bool HasChanged { get; set; }
    public event Action? Changed;

    private object? _value;
    public object? Value
    {
        get => _value;

        set
        {
            if (_value == value)
            {
                return;
            }

            _value = value;
            Changed?.Invoke();
            HasChanged = true;
        }
    }

    public ObservableParameterItem(IParameterDefinition definition)
    {
        Definition = definition;
        _value = definition.DefaultValue;
    }

    public ObservableParameterItem(IParameterDefinition definition, object? values)
    {
        Definition = definition;
        _value = values;
    }

    public string DisplayValue => Definition.FormatValue(Value);
}
