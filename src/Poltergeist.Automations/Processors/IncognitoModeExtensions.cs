using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public static class IncognitoModeExtensions
{
    public static readonly EntryDefinition<bool> EnvironmentEntry = new("is_incognitomode");

    public static bool IsIncognitoMode(this IMacroProcessorInformation processor)
    {
        return processor.Environments.GetValueOrDefault(EnvironmentEntry) == true;
    }
}
