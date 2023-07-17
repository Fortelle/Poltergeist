using System;

namespace Poltergeist.Automations.Configs;

public interface IOptionItem
{
    public string Key { get; }

    public string? DisplayLabel { get; }
    public string? Category { get; }
    public string? Description { get; }
    public bool IsReadonly { get; set; }
    public bool IsBrowsable { get; set; }

    public bool HasChanged { get; set; }

    public Type BaseType { get; }

    public bool IsDefault { get; }

    public object? Value { get; set; }
    public object? Default { get; }
}
