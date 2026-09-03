using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Modules;
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

    public override void OnMacroInitialized(IMacroInformation macro)
    {
        base.OnMacroInitialized(macro);

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

    public override void RegisterServices(IServiceCollection services, RegisterServicesArguments args)
    {
        base.RegisterServices(services, args);

        services.AddSingleton<HybridOperationService>();

        var capturingMode = args.Options.Get<string>(CapturingModeKey);
        
        if (capturingMode == "screen")
        {
            services.AddSingleton<CapturingProvider>(x => x.GetRequiredService<ScreenCapturingService>());
        }
        else if (capturingMode == "printwindow")
        {
            services.AddSingleton<CapturingProvider>(x => x.GetRequiredService<PrintWindowCapturingService>());
        }
        else if (capturingMode == "bitblt")
        {
            services.AddSingleton<CapturingProvider>(x => x.GetRequiredService<BitBltCapturingService>());
        }
    }

    [MacroHook]
    protected static void OnHybridOperationStart(IMacroProcessorShared processor, HybridOperationStartHook hook)
    {
        var config = processor.SessionStorage.GetValueOrDefault<RegionConfig>("window_region_config");
        var capturingMode = processor.Options.Get<string>(CapturingModeKey);
        var mouseMode = processor.Options.Get<string>(MouseModeKey);
        var keyboardMode = processor.Options.Get<string>(KeyboardModeKey);

        LocatedWindowInfo? info = null; // to avoid duplicate locating

        if (capturingMode == "printwindow" || capturingMode == "bitblt" || mouseMode == "sendmessage" || keyboardMode == "sendmessage")
        {
            var windowLocatingService = processor.GetService<WindowLocatingService>();
            if (config is null || !windowLocatingService.TryLocate(config, out info))
            {
                processor.ReportComment("Client not found");
                throw new Exception("Failed to find the requested window.");
            }
        }

        if (capturingMode == "screen" || mouseMode == "sendinput")
        {
            var screenLocatingService = processor.GetService<ScreenLocatingService>();
            if (config is null || !screenLocatingService.TryLocate(config, info))
            {
                processor.ReportComment("Client not found");
                throw new Exception("Failed to locate the screen region.");
            }
        }
    }
}
