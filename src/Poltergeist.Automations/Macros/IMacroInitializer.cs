using Poltergeist.Automations.Parameters;

namespace Poltergeist.Automations.Macros;

public interface IMacroInitializer
{
    public string Key { get; }
    public MacroStorage Storage { get; }
    public List<MacroAction> Actions { get; }
    public OptionCollection UserOptions { get; }
    public ParameterCollection Statistics { get; }
    public List<ConfigVariation> ConfigVariations { get; }

    public string? PrivateFolder { get; }
    public string? SharedFolder { get; }

    public bool RequiresAdmin { get; set; }
}
