using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Macros.Loops;

public class LoopEndedHook : MacroHook
{
    public required int Iterations { get; init; }
}
