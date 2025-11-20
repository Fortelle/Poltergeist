using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopCheckContinueArguments(MacroProcessor processor) : ArgumentService(processor)
{
    public int IterationIndex { get; set; }
    public bool Break { get; set; }
}
