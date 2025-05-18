namespace Poltergeist.Automations.Processors;

public class MacroProcessorArguments
{
    public required LaunchReason LaunchReason { get; set; }

    public Dictionary<string, object?>? Options { get; set; }

    public Dictionary<string, object?>? Environments { get; set; }

    public Dictionary<string, object?>? Statistics { get; set; }

    public Dictionary<string, object?>? SessionStorage { get; set; }
}
