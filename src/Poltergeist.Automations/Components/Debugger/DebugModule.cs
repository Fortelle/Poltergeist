using System.Reflection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Components.Debugger;

public class DebugModule : MacroModule
{
    public const string DebugModeKey = "debug_mode";

    public DebugModule()
    {
    }

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.ConfigVariations.Add(new()
        {
            Title = ResourceHelper.Localize("Poltergeist.Automations/Resources/DebugModule_ConfigTitle"),
            Icon = "\uEBE8",
            IgnoresUserOptions = true,
            OptionOverrides = new()
            {
                {LoopService.ConfigEnableKey, false},
            },
            EnvironmentOverrides = new()
            {
                {DebugModeKey, true },
                {MacroBase.UseStatisticsKey, false},
            }
        });
    }

    public override void OnProcessorPrepare(IPreparableProcessor processor)
    {
        base.OnProcessorPrepare(processor);

        var methods = processor.Macro.GetType()
            .GetMethods()
            .Where(x => x.GetCustomAttribute<DebugMethodAttribute>() is not null)
            .ToArray()
            ;

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<DebugMethodAttribute>()!;
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                processor.Hooks.Register<ProcessorPreparedHook>(() =>
                {
                    method.Invoke(processor.Macro, null);
                });
                if (attr.PreventsStart)
                {
                    processor.Hooks.Register<ProcessorCheckStartHook>(e =>
                    {
                        e.CanStart = false;
                    });
                }
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableTo(typeof(MacroHook)))
            {
                var register = typeof(HookService).GetMethods().FirstOrDefault(x => x.Name == nameof(HookService.Register) && x.GetParameters().FirstOrDefault()?.ParameterType.Name == "Action`1");
                if (register is null)
                {
                    continue;
                }
                // todo
            }
        }
    }
}
