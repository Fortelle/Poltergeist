using Poltergeist.Automations.Processors;

namespace Poltergeist.Modules.Macros;

public class MacroStartArguments
{
    public bool IncognitoMode { get; set; }

    public Dictionary<string, object?>? OptionOverrides { get; set; }

    public Dictionary<string, object?>? EnvironmentOverrides { get; set; }

    public Dictionary<string, object?>? SessionStorage { get; set; }

    public Action<ProcessorResult>? Callback { get; set; }
}
