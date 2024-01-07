using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IPreparableProcessor
{
    public IMacroBase Macro { get; }

    public VariableCollection Options { get; }
    public VariableCollection Environments { get; }
    public VariableCollection Statistics { get; }

    public Action? WorkProc { set; }
    public Func<Task>? AsyncWorkProc { set; }

    public T GetService<T>() where T : class;

    public HookService Hooks => GetService<HookService>();
}
