using Poltergeist.Automations.Common;
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
            DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_Action"),
            Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Category"),
            GetText = x => ResourceHelper.Localize($"Poltergeist.Automations/Resources/AfterCompletion_CompletionAction_{x}"),
        });

        macro.UserOptions.Add(new OptionItem<int>("aftercompletion.minimumseconds", 0)
        {
            DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_MinimumTime"),
            Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Category"),
            Description = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_MinimumTime_Description"),
        });

        macro.UserOptions.Add(new OptionItem<bool>("aftercompletion.allowerror", false)
        {
            DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_AllowError"),
            Category = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Category"),
            Description = ResourceHelper.Localize("Poltergeist.Automations/Resources/AfterCompletion_Option_AllowError_Description"),
        });
    }

}
