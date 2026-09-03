using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IMacroInformation
{
    string Key { get; }
    string Title { get; }

    string? Category { get; }
    string? Description { get; }
    string[]? Details { get; }
    string[]? Tags { get; }
    string? Icon { get; }
    Version? Version { get; }

    List<MacroAction> Actions { get; }
    ParameterDefinitionCollection OptionDefinitions { get; }
    StatisticDefinitionCollection StatisticDefinitions { get; }
    ParameterDefinitionCollection Metadata { get; }
    ParameterValueCollection? OptionPresets { get; }
    ParameterValueCollection? EnvironmentPresets { get; }
    ParameterValueCollection ExtraData { get; }
    List<ConfigVariation> ConfigVariations { get; }
    List<ProcessorIntervention> Interventions { get; }
    bool RequiresAdmin { get; }

    Exception? Exception { get; }
}
