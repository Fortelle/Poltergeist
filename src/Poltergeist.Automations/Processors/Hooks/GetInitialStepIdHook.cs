using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Processors;

public class GetInitialStepIdHook : MacroHook
{
    public string? StepId { get; set; }
}
