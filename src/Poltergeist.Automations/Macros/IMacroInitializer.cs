using System.Collections.Generic;
using Poltergeist.Automations.Common.Structures;
using Poltergeist.Automations.Configs;

namespace Poltergeist.Automations.Macros;

public interface IMacroInitializer
{
    public string Key { get; }
    public MacroStorage Storage { get; }
    public List<MacroAction> Actions { get; }
    public MacroOptions UserOptions { get; }
    public VariableCollection Statistics { get; }

    public string? PrivateFolder { get; }
    public string? SharedFolder { get; }

    public bool RequiresAdmin { get; set; }
}
