using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public abstract class MacroModule
{
    public string Name => GetType().Name;

    public virtual void OnMacroInitialize(IInitializableMacro macro)
    {
    }

    public virtual void OnProcessorConfigure(IConfigurableProcessor processor)
    {
    }

    public virtual void OnProcessorPrepare(IPreparableProcessor processor)
    {
    }

}
