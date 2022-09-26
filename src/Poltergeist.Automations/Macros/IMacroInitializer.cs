using System.Collections.Generic;
using Poltergeist.Automations.Configs;

namespace Poltergeist.Automations.Macros;

public interface IMacroInitializer
{
    public MacroStorage Storage { get; }
    public List<MacroMaintenance> Maintenances { get; }
    public MacroOptions UserOptions { get; }
    public VariableCollection Statistics { get; }

    public string PrivateFolder { get; }
    public string SharedFolder { get; }
}

