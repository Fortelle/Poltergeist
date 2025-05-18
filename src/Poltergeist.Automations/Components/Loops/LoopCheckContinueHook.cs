using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class LoopCheckContinueHook : MacroHook
{
    public required IterationData Data { get; init; }

    public CheckContinueResult Result { get; set; }
}
