using System;

namespace Poltergeist.Automations.Macros;

public class OptionItem
{
    public string Key { get; }

    public string DisplayLabel { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsBrowsable { get; set; }

    public object Value { get; set; }

    internal string DefaultValueString { get; }
    internal string SavedValueString { get; set; }
    internal Type Type => Value.GetType();

    public OptionItem(string key, object defaultValue)
    {
        Key = key;
        DefaultValueString = defaultValue.ToString();

        Value = defaultValue;
    }

    public bool HasChanged { get; set; }
}
