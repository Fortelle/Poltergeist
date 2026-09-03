using Poltergeist.Automations.Components;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Macros.Loops;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Examples;

[ExampleMacro]
[ModuleDependency<TriggerModule>]
[ModuleDependency<LoopModule>]
[ModuleDependency<LoopVisualizationModule>]
public class PeriodicMacroExample : CommonMacroBase
{
    public PeriodicMacroExample() : base()
    {
        Title = "Periodic Example";

        Category = "Macros";

        Description = $"A macro that demonstrates the use of {nameof(PeriodicTrigger)}. The macro will wait for a specified interval before starting each iteration.";

        OptionDefinitions.Add(new OptionDefinition<int>("interval", 10)
        {
            DisplayLabel = "Interval (seconds)",
        });
    }

    [MacroHook]
    public static void OnProcessorStartup(IMacroProcessorShared processor, ProcessorStartupHook hook)
    {
        var interval = processor.Options.Get<int>("interval");
        var periodicTrigger = processor.GetService<PeriodicTrigger>();
        periodicTrigger.Interval = TimeSpan.FromSeconds(interval);
        processor.SessionStorage.Add(nameof(PeriodicTrigger), periodicTrigger);
    }

    [MacroHook]
    public static void OnLoopSetup(IMacroProcessorShared processor, LoopSetupHook hooks)
    {
        hooks.Pattern = LoopPattern.Multiple;
        hooks.MaxIterations = 3;
    }

    [MacroHook]
    public static void OnIterationStarting(IMacroProcessorShared processor, IterationStartingHook hook)
    {
        var periodicTrigger = processor.SessionStorage.Get<PeriodicTrigger>(nameof(PeriodicTrigger));
        periodicTrigger.WaitOne().GetAwaiter().GetResult();
    }
}
