using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
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
    IconInfo? Icon { get; }
    Version? Version { get; }

    List<MacroAction> Actions { get; }
    OptionDefinitionCollection OptionDefinitions { get; }
    StatisticDefinitionCollection StatisticDefinitions { get; }
    ParameterDefinitionCollection Metadata { get; }
    ParameterValueCollection? OptionPresets { get; }
    ParameterValueCollection? EnvironmentPresets { get; }
    ParameterValueCollection ExtraData { get; }
    List<ConfigVariation> ConfigVariations { get; }
    List<ProcessorIntervention> Interventions { get; }

    bool RequiresAdmin { get; }
    bool PreventsSystemSleep { get; }
    bool PreventsDisplaySleep { get; }

    Exception? Exception { get; }
}
