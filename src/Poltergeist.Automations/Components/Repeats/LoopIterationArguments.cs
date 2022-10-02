using System;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Components.Loops;

public class LoopIterationArguments : ArgumentService
{
    public int Index { get; internal set; }
    public DateTime BeginTime { get; internal set; }

    public IterationResult Result { get; set; }

    public LoopIterationArguments(MacroProcessor processor) : base(processor)
    {
    }
}
