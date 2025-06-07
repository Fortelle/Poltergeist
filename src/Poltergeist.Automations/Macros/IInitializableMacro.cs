using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IInitializableMacro : IMacroBase
{
    public List<MacroAction> Actions { get; }
    public ParameterDefinitionCollection UserOptions { get; }
    public ParameterDefinitionCollection Statistics { get; }
    public ParameterValueCollection ExtraData { get; }
    public List<ConfigVariation> ConfigVariations { get; }

    public bool RequiresAdmin { get; set; }
}
