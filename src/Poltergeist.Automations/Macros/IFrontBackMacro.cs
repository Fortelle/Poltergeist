using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IFrontBackMacro : IMacroBase
{
    ParameterDefinitionCollection OptionDefinitions { get; }
    StatisticDefinitionCollection StatisticDefinitions { get; }
    ParameterDefinitionCollection Metadata { get; }
    List<ConfigVariation> ConfigVariations { get; }
    List<MacroAction> Actions { get; }
    MacroStatus Status { get; }

    void Initialize();
    bool CheckValidity([MaybeNullWhen(true)] out string invalidationMessage);
}
