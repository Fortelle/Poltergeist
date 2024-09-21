﻿using Poltergeist.Automations.Macros;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroCompletedHandler(MacroShell shell) : AppEventHandler
{
    public MacroShell Shell => shell;
}
