using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopCheckContinueArguments(MacroProcessor processor) : ArgumentService(processor)
{
    public int IterationIndex { get; set; }

    [AllowNull]
    public IterationData IterationData { get; set; }

    public CheckContinueResult Result { get; set; }
}
