using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IInitializableMacro : IMacroBase
{
    List<MacroAction> Actions { get; }
    ParameterDefinitionCollection OptionDefinitions { get; }
    StatisticDefinitionCollection StatisticDefinitions { get; }
    ParameterValueCollection ExtraData { get; }
    List<ConfigVariation> ConfigVariations { get; }
    bool RequiresAdmin { get; set; }
}
