using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public abstract class MacroModule
{
    public static List<IOptionItem> GlobalOptions { get; } = new();
    public static List<IParameterEntry> GlobalStatistics { get; } = new();

    public virtual void OnMacroInitialized(IMacroInitializer macro)
    {
    }

    public virtual void OnMacroConfiguring(ServiceCollection services, IConfigureProcessor processor)
    {
    }

    public virtual void OnMacroProcessing(MacroProcessor processor)
    {
    }
}
