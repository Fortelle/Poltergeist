using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Components.Logging;

namespace Poltergeist.Automations.Processors;

public class WorkflowController
{
    public IMacroProcessorShared Processor { get; }
    public string? Comment { get; set; }

    public LoggerWrapper Logger { get; }

    public OutputService Outputer => Processor.GetService<OutputService>();

    private readonly string SenderName;

    public WorkflowController(MacroProcessor processor)
    {
        Processor = processor;

        SenderName = GetType().Name;

        Logger = new(processor.GetService<MacroLogger>(), SenderName);
    }

    [DoesNotReturn]
#pragma warning disable CA1822 // Mark members as static
    public void Exit(string? message = null)
#pragma warning restore CA1822 // Mark members as static
    {
        throw new WorkflowFailureException(message);
    }
}
