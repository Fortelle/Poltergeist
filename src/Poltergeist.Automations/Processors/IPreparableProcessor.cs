using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IPreparableProcessor
{
    IUserMacro Macro { get; }

    ParameterValueCollection Options { get; }
    ParameterValueCollection Environments { get; }
    ParameterValueCollection SessionStorage { get; }
    ParameterValueCollection OutputStorage { get; }

    T GetService<T>() where T : class;
    void AddStep(WorkflowStep step);

    HookService Hooks => GetService<HookService>();
}
