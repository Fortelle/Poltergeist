using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class LoopStartingHook : MacroHook
{
    public bool _cancel;
    public bool Cancel { get => _cancel; set => _cancel |= value; }

    public string? CancelReason { get; set; }
}
