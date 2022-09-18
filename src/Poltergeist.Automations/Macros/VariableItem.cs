using System;

namespace Poltergeist.Automations.Macros;

public class VariableItem
{
    public string Key { get; }

    public string DisplayLabel { get; set; }
    public string Description { get; set; }

    public object Value { get; set; }

    public bool IsBrowsable { get; set; }

    internal Type Type => Value.GetType();

    public bool HasChanged { get; set; }

    public VariableItem(string key, object value)
    {
        Key = key;
        Value = value;
    }
}
