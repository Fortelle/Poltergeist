using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Modules;

public class RegisterServicesArguments
{
    public required IReadOnlyParameterValueCollection Options { get; init; }
    public required IReadOnlyParameterValueCollection Environments { get; init; }
}
