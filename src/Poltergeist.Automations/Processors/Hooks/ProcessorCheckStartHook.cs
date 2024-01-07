using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Processors;

public class ProcessorCheckStartHook : MacroHook
{
    public bool CanStart { get; set; }
}
