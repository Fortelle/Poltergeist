using System;

namespace Poltergeist.Automations.Configs;

public interface IOptionItem
{
    public string Key { get; }

    public string DisplayLabel { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsBrowsable { get; set; }

    public bool HasChanged { get; set; }

    public Type Type { get; }
    public string ToString();
    public bool IsDefault { get; }

    public object Value { get; set; }
    public object Default { get; }
}
