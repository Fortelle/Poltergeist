namespace Poltergeist.Automations.Structures.Parameters;

public class ObservableParameterItem
{
    public IParameterDefinition Definition { get; }
    public bool HasChanged { get; set; }
    public event ObservableParameterCollection.ChangedEventHandler? Changed;

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

            var oldValue = _value;
            _value = value;
            Changed?.Invoke(Definition.Key, oldValue, value);
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
