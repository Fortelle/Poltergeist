using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IFrontBackMacro : IMacroBase
{
    public ParameterDefinitionCollection OptionDefinitions { get; }
    public StatisticDefinitionCollection StatisticDefinitions { get; }
    public ParameterDefinitionCollection Metadata { get; }
    public List<ConfigVariation> ConfigVariations { get; }
    public List<MacroAction> Actions { get; }
    public MacroStatus Status { get; }

    public void Initialize();
    public bool CheckValidity([MaybeNullWhen(true)] out string invalidationMessage);
}
