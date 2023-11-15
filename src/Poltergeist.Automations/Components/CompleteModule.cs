using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Components.Loops;

public class CompleteModule : MacroModule
{

    public override void OnMacroInitialized(IMacroInitializer macro)
    {
        macro.UserOptions.Add(new EnumOption<CompletionAction>("aftercompletion.action", CompletionAction.None)
        {
            DisplayLabel = "Action",
            Category = "After Completion",
        });

        macro.UserOptions.Add(new OptionItem<int>("aftercompletion.minimumseconds", 0)
        {
            DisplayLabel = "Minimum run time (seconds)",
            Category = "After Completion",
        });

        macro.UserOptions.Add(new OptionItem<bool>("aftercompletion.allowerror", false)
        {
            DisplayLabel = "Allow error",
            Category = "After Completion",
        });
    }

}
