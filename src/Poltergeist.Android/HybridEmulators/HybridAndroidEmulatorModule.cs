using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Capturing;
using Poltergeist.Operations.Hybrid;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Android.HybridEmulators;

[ModuleDependency<AdbModule>]
public class HybridAndroidEmulatorModule : HybridOperationModule
{
    public HybridAndroidEmulatorModule() : base()
    {
        CapturingModes.AddRange("adb");
        MouseModes.AddRange("adb");
    }

    public override void OnMacroInitialized(IMacroInformation macro)
    {
        base.OnMacroInitialized(macro);
    }

    public override void RegisterServices(IServiceCollection services, RegisterServicesArguments args)
    {
        base.RegisterServices(services, args);

        services.AddSingleton<HybridOperator>();

        var capturingMode = args.Options.Get<string>(CapturingModeKey);
        var mouseMode = args.Options.Get<string>(MouseModeKey);

        if (capturingMode == "adb")
        {
            services.AddSingleton<CapturingProvider>(x => x.GetRequiredService<AdbCapturingService>());
        }

        if (mouseMode == "sendinput")
        {
            services.AddSingleton<IHybridInputService, ScreenInputWrapper>();
        }
        else if (mouseMode == "sendmessage")
        {
            services.AddSingleton<IHybridInputService, WindowInputWrapper>();
        }
        else if(mouseMode == "adb")
        {
            services.AddSingleton<IHybridInputService, AdbInputWrapper>();
        }

    }

    [MacroHook]
    public static void OnProcessorStartup(IMacroProcessorShared processor, ProcessorStartupHook hook)
    {
        if (processor.Macro.ExtraData.TryGetValue("window_region_config", out RegionConfig? config)) {
            processor.SessionStorage.TryAdd("window_region_config", config);
        }
    }

    [MacroHook]
    public static void OnHybridOperationStart2(IMacroProcessorShared processor, HybridOperationStartHook hook)
    {
        var capturingMode = processor.Options.Get<string>(CapturingModeKey);
        var mouseMode = processor.Options.Get<string>(MouseModeKey);

        if (capturingMode == "adb" || mouseMode == "adb")
        {
            var adb = processor.GetService<AdbService>();
            if (!adb.Connect())
            {
                throw new Exception("Failed to connect to adb server.");
            }
        }
    }

    [MacroHook]
    public static void OnHybridOperationStop(IMacroProcessorShared processor, HybridOperationStopHook hook)
    {
        var capturingMode = processor.Options.Get<string>(CapturingModeKey);
        var mouseMode = processor.Options.Get<string>(MouseModeKey);

        if (capturingMode == "adb" || mouseMode == "adb")
        {
            var adb = processor.GetService<AdbService>();
            adb.Close();
        }
    }
}
