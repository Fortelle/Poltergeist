using Newtonsoft.Json;

namespace Poltergeist.Automations.Parameters;

public class VariableEntry
{
    public required string Key { get; init; }
    public object? Value { get; set; }
    public ParameterSource Source { get; set; }

    [JsonIgnore]
    public bool HasChanged { get; set; }
}
