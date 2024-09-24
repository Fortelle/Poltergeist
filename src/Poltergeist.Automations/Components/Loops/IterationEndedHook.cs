using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Components.Loops;

public class IterationEndedHook(IterationResult result) : MacroHook
{
    public IterationResult Result => result;
}
