using Poltergeist.Automations.Processors;

namespace Poltergeist.Components.Loops;

public class LoopBeginArguments : ArgumentService
{
    public bool Cancel { get; set; }

    public LoopBeginArguments(MacroProcessor processor) : base(processor)
    {
    }
}
