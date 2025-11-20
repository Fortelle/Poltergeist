using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class LoopEndedHook : MacroHook
{
    public required LoopResult Result { get; init; }
    public required int Iterations { get; init; }
}
