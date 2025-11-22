using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Capturing;
using Poltergeist.Operations.Inputing;
using Poltergeist.Operations.Locating;
using Poltergeist.Operations.Hybrid;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.HybridEmulators;

[ModuleDependency<AdbModule>]
public class HybridAndroidEmulatorModule : HybridOperationModule
{
    public HybridAndroidEmulatorModule() : base()
    {
        CapturingModes.AddRange("adb");
        MouseModes.AddRange("adb");
    }

    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);
    }

    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<HybridOperator>();

        var capturingMode = processor.Options.Get<string>(CapturingModeKey);
        var mouseMode = processor.Options.Get<string>(MouseModeKey);

        if (capturingMode == "adb")
        {
            processor.Services.AddSingleton<CapturingProvider>(x => x.GetRequiredService<AdbCapturingService>());
        }

        if (mouseMode == "sendinput")
        {
            processor.Services.AddSingleton<IHybridInputService, ScreenInputWrapper>();
        }
        else if (mouseMode == "sendmessage")
        {
            processor.Services.AddSingleton<IHybridInputService, WindowInputWrapper>();
        }
        else if(mouseMode == "adb")
        {
            processor.Services.AddSingleton<IHybridInputService, AdbInputWrapper>();
        }

        processor.Services.Configure<AdbInputOptions>(options =>
        {
            options.SwipeTime = TimeSpanRange.FromMilliseconds(1000, 1500);
            options.MaxDeviationRadius = 32;
            options.DeviationDistribution = ShapeDistributionType.Gaussian;
            options.ShapeDistribution = ShapeDistributionType.Gaussian;
            options.MovingMotion = MouseMoveMotion.Linear;
            options.MovingInterval = 15;
        });
    }

    public override void OnProcessorPrepare(IPreparableProcessor processor)
    {
        base.OnProcessorPrepare(processor);

        if (processor.Macro.ExtraData.TryGetValue("window_region_config", out RegionConfig? config)) {
            processor.SessionStorage.TryAdd("window_region_config", config);
        }
    }

    [MacroHook]
    public static void OnStart2(HybridOperationStartHook hook)
    {
        var capturingMode = hook.Processor.Options.Get<string>(CapturingModeKey);
        var mouseMode = hook.Processor.Options.Get<string>(MouseModeKey);

        if (capturingMode == "adb" || mouseMode == "adb")
        {
            var adb = hook.Processor.GetService<AdbService>();
            if (!adb.Connect())
            {
                throw new Exception("Failed to connect to adb server.");
            }
        }
    }

    [MacroHook]
    public static void OnStop(HybridOperationStopHook hook)
    {
        var capturingMode = hook.Processor.Options.Get<string>(CapturingModeKey);
        var mouseMode = hook.Processor.Options.Get<string>(MouseModeKey);

        if (capturingMode == "adb"|| mouseMode == "adb")
        {
            var adb = hook.Processor.GetService<AdbService>();
            adb.Close();
        }
    }
}
