namespace Poltergeist.Automations.Parameters;

public interface IOptionItem : IParameterEntry
{
    public bool IsReadonly { get; set; }
    public bool IsBrowsable { get; set; }

    public bool IsDefault { get; }

    public object? Default { get; }
}
