using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Tests.UnitTests;

[ModuleDependency<OneshotModule>]
public class TestMacro : MacroBase
{
    public Action<IMacroProcessorShared>? Execute { get; set; }
    public Func<IMacroProcessorShared, Task>? ExecuteAsync { get; set; }

    public TestMacro() : base()
    {
    }

    public TestMacro(string name) : base(name)
    {
    }

    [MacroHook]
    private void OnOneshotSetup(IMacroProcessorShared processor, OneshotSetupHook hooks)
    {
        hooks.ExecuteAsync = async (processor) =>
        {
            if (Execute is not null)
            {
                Execute(processor);
            }
            else if (ExecuteAsync is not null)
            {
                await ExecuteAsync(processor);
            }
        };
    }
}

