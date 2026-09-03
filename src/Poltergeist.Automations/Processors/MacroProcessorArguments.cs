namespace Poltergeist.Automations.Processors;

public class MacroProcessorArguments
{
    public LaunchReason LaunchReason { get; set; }

    public Dictionary<string, object?>? Options { get; set; }

    public Dictionary<string, object?>? Environments { get; set; }

    public Dictionary<string, object?>? Inputs { get; set; }
}
