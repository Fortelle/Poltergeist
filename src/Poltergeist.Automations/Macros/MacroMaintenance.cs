using System;

namespace Poltergeist.Automations.Macros;

public class MacroMaintenance
{
    public string Text { get; set; }
    public Action<MacroBase> Execute;
}
