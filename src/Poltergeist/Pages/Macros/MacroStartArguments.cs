using Poltergeist.Automations.Processors;

namespace Poltergeist.Pages.Macros;

public class MacroStartArguments
{
    public required string ShellKey { get; init; }

    public required LaunchReason Reason { get; init; }

    public bool IgnoresUserOptions { get; set; }

    public Dictionary<string, object?>? OptionOverrides { get; set; }

    public Dictionary<string, object?>? EnvironmentOverrides { get; set; }

    public Dictionary<string, object?>? SessionStorage { get; set; }
}
