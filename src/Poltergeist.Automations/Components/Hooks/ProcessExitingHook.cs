using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Hooks;

public class ProcessExitingHook : MacroHook
{
    public EndReason Reason { get; set; }

    public ProcessExitingHook(EndReason reason)
    {
        Reason = reason;
    }
}
