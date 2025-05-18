using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IPreparableProcessor
{
    public IUserMacro Macro { get; }

    public ParameterValueCollection Options { get; }
    public ParameterValueCollection Environments { get; }
    public ParameterValueCollection Statistics { get; }
    public ParameterValueCollection SessionStorage { get; }

    public T GetService<T>() where T : class;
    public void AddStep(WorkflowStep step);

    public HookService Hooks => GetService<HookService>();
}
