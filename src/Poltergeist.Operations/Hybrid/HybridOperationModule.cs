using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Operations.Capturing;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Operations.Hybrid;

[ModuleDependency<OperationModule>]
public class HybridOperationModule : MacroModule
{
    public const string CapturingModeKey = "hybrid_operation_capturing_mode";
    public const string MouseModeKey = "hybrid_operation_mouse_mode";
    public const string KeyboardModeKey = "hybrid_operation_keyboard_mode";

    protected List<string> CapturingModes { get; } = new();
    protected List<string> MouseModes { get; } = new();
    protected List<string> KeyboardModes { get; } = new();

    public HybridOperationModule() : base()
    {
        CapturingModes.AddRange("screen", "printwindow", "bitblt");
        MouseModes.AddRange("sendinput", "sendmessage");
        KeyboardModes.AddRange("sendinput", "sendmessage");
    }

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.OptionDefinitions.Add(new OptionDefinition<bool>(CapturingProvider.PreviewCaptureKey)
        {
            DisplayLabel = "Preview captured image",
            Category = "Debug",
            Status = ParameterStatus.DevelopmentOnly,
        });

        macro.OptionDefinitions.Add(new ChoiceOption<string>(CapturingModeKey, [.. CapturingModes])
        {
            DisplayLabel = "Capturing mode",
            Category = "Operation",
        });

        macro.OptionDefinitions.Add(new ChoiceOption<string>(MouseModeKey, [.. MouseModes])
        {
            DisplayLabel = "Mouse mode",
            Category = "Operation",
        });

        macro.OptionDefinitions.Add(new ChoiceOption<string>(KeyboardModeKey, [.. KeyboardModes])
        {
            DisplayLabel = "Keyboard mode",
            Category = "Operation",
        });
    }

    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<HybridOperationService>();

        var capturingMode = processor.Options.Get<string>(CapturingModeKey);
        
        if (capturingMode == "screen")
        {
            processor.Services.AddSingleton<CapturingProvider>(x => x.GetRequiredService<ScreenCapturingService>());
        }
        else if (capturingMode == "printwindow")
        {
            processor.Services.AddSingleton<CapturingProvider>(x => x.GetRequiredService<PrintWindowCapturingService>());
        }
        else if (capturingMode == "bitblt")
        {
            processor.Services.AddSingleton<CapturingProvider>(x => x.GetRequiredService<BitBltCapturingService>());
        }
    }

    [MacroHook]
    public static void OnStart(HybridOperationStartHook hook)
    {
        var config = hook.Processor.SessionStorage.GetValueOrDefault<RegionConfig>("window_region_config");
        var capturingMode = hook.Processor.Options.Get<string>(CapturingModeKey);
        var mouseMode = hook.Processor.Options.Get<string>(MouseModeKey);
        var keyboardMode = hook.Processor.Options.Get<string>(KeyboardModeKey);

        LocatedWindowInfo? info = null; // to avoid duplicate locating

        if (capturingMode == "printwindow" || capturingMode == "bitblt" || mouseMode == "sendmessage" || keyboardMode == "sendmessage")
        {
            var windowLocatingService = hook.Processor.GetService<WindowLocatingService>();
            if (config is null || !windowLocatingService.TryLocate(config, out info))
            {
                throw new Exception("Failed to find the requested window.");
            }
        }

        if (capturingMode == "screen" || mouseMode == "sendinput")
        {
            var screenLocatingService = hook.Processor.GetService<ScreenLocatingService>();
            if (config is null || !screenLocatingService.TryLocate(config, info))
            {
                throw new Exception("Failed to locate the screen region.");
            }
        }
    }
}
