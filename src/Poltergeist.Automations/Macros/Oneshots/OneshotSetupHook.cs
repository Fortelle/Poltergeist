using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros.Oneshots;

public class OneshotSetupHook : MacroHook
{
    public Func<IMacroProcessorShared, Task>? ExecuteAsync { get; set; }

    public Action<IMacroProcessorShared>? Finalize { get; set; }
}
