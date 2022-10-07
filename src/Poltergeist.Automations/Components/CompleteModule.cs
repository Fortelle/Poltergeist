using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Components.Loops;

public class CompleteModule : MacroModule
{
    public override void OnMacroInitialize(IMacroInitializer macro)
    {
        macro.UserOptions.Add(new OptionItem<CompleteAction>("CompleteAction", CompleteAction.None)
        {
            DisplayLabel = "After complete",
            Category = "Common",
        });

    }

}
