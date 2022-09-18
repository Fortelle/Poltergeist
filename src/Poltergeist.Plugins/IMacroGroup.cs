using System;
using System.Collections.Generic;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Plugins;

public interface IMacroGroup
{
    public string Name { get; }
    public string Description { get; }

    public IEnumerable<MacroBase> GetMacros();
}
