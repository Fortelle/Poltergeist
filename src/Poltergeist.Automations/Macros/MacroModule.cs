using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public abstract class MacroModule
{
    public string Name => GetType().Name;

    public static List<IParameterDefinition> GlobalOptions { get; } = new();
    public static List<IParameterDefinition> GlobalStatistics { get; } = new();

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
