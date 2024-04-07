namespace Poltergeist.Automations.Parameters;

public class ParameterValue
{
    public required string Key { get; init; }
    public object? Value { get; set; }

    public bool HasChanged { get; set; }
}
