using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components.Debugger;

public class TrialModule : MacroModule
{
    public const string TrialModeKey = "trial_mode";

    public TrialModule()
    {
    }

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.ConfigVariations.Add(new()
        {
            Title = LocalizationUtil.Localize("TrialModule_ConfigTitle"),
            IncognitoMode = true,
            OptionOverrides = new()
            {
                {LoopService.ConfigEnableKey, false},
            },
            EnvironmentOverrides = new()
            {
                {TrialModeKey, true },
            }
        });
    }

}