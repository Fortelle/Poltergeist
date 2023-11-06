using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopCheckContinueArguments : ArgumentService
{
    public int IterationIndex { get; set; }

    [AllowNull]
    public IterationResult IterationResult { get; set; }

    public CheckContinueResult Result { get; set; }

    public LoopCheckContinueArguments(MacroProcessor processor) : base(processor)
    {
    }

}
