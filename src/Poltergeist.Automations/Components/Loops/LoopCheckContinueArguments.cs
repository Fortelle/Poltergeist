using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopCheckContinueArguments(MacroProcessor processor) : ArgumentService(processor)
{
    public int IterationIndex { get; set; }

    public IterationResult IterationResult { get; set; }

    public CheckContinueResult Result { get; set; }
}
