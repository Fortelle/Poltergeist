using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class LoopCheckContinueHook : MacroHook
{
    public int IterationIndex { get; init; }

    public IterationResult IterationResult { get; init; }

    public CheckContinueResult Result { get; set; }
}
