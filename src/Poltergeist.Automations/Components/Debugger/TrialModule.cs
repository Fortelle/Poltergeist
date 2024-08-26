using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
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
            Title = ResourceHelper.Localize("Poltergeist.Automations/Resources/TrialModule_ConfigTitle"),
            OptionOverrides = new()
            {
                {LoopService.ConfigEnableKey, false},
            },
            EnvironmentOverrides = new()
            {
                {TrialModeKey, true },
                {MacroBase.UseStatisticsKey, false},
            }
        });
    }

}
