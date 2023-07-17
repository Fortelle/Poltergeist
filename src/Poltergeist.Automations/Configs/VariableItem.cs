using System;

namespace Poltergeist.Automations.Configs;

public class VariableItem
{
    public string Key { get; }

    public object Value { get; set; }

    private string? _title;
    public string Title { get => _title ?? Key; set => _title = value; }

    public string? Description { get; set; }

    public bool IsBrowsable { get; set; }

    public Type BaseType { get; }

    public VariableItem(string key, object value)
    {
        Key = key;
        Value = value;
        BaseType = value.GetType();
    }
}
