using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IFrontBackMacro : IMacroBase
{
    public ParameterDefinitionCollection UserOptions { get; }
    public ParameterDefinitionCollection Statistics { get; }
    public ParameterDefinitionCollection Properties { get; }
    public List<ConfigVariation> ConfigVariations { get; }
    public List<MacroAction> Actions { get; }
    public MacroStatus Status { get; }

    public void Initialize();
    public bool CheckValidity([MaybeNullWhen(false)] out string invalidationMessage);
}
