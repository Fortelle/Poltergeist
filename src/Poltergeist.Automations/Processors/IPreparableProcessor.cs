using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IPreparableProcessor
{
    public IUserMacro Macro { get; }

    public ParameterValueCollection Options { get; }
    public ParameterValueCollection Environments { get; }
    public ParameterValueCollection Statistics { get; }

    public Action? WorkProc { set; }
    public Func<Task>? AsyncWorkProc { set; }

    public T GetService<T>() where T : class;

    public HookService Hooks => GetService<HookService>();
}
