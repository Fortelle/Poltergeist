using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Debugger;

public static class TrialModuleExtensions
{
    public static bool IsTrialMode(this ArgumentService args)
    {
        return args.Processor.Environments.Get<bool>(TrialModule.TrialModeKey);
    }
}
