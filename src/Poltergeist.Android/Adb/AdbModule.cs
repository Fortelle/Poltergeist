using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Android.Adb;

public class AdbModule : MacroModule
{
    public override void OnMacroInitialize(IInitializableMacro macro)
    {
        base.OnMacroInitialize(macro);

        macro.OptionDefinitions.Add(new OptionDefinition<string>(AdbService.IpAddressKey)
        {
            DisplayLabel = "IP Address",
            Category = "ADB",
            IsGlobal = true,
        });

        macro.OptionDefinitions.Add(new PathOption(AdbService.ExePathKey)
        {
            DisplayLabel = "Exe file",
            Category = "ADB",
            IsGlobal = true,
        });

        macro.OptionDefinitions.Add(new OptionDefinition<bool>(AdbService.KeepAliveKey, true)
        {
            DisplayLabel = "Keep adb server alive",
            Description = "Skips killing the adb server when the macro is completed. " +
                "This helps when you are planning to launch the macro frequently in a short time. " +
                "You can use the \"kill-server\" action to kill the adb server manually.",
            Category = "ADB",
        });

        macro.Actions.Add(KillServerAction);
    }

    public override void OnProcessorConfigure(IConfigurableProcessor processor)
    {
        base.OnProcessorConfigure(processor);

        processor.Services.AddSingleton<TerminalService>();
        processor.Services.AddSingleton<AdbService>();
        processor.Services.AddSingleton<AdbLocatingService>();
        processor.Services.AddSingleton<AdbInputService>();
        processor.Services.AddSingleton<AdbCapturingService>();

        processor.Services.AddOptions<AdbInputOptions>();
    }

    // todo: support EmulatorDetectionModule
    public static readonly MacroAction KillServerAction = new()
    {
        Text = "Kill ADB server",
        Description = "Runs \"adb kill-server\" via Command Prompt to kill the adb server.",
        Icon = "\uE756",
        Execute = args =>
        {
            if (!args.Options.TryGetValue(AdbService.ExePathKey, out var value) || value is not string exepath || !File.Exists(exepath))
            {
                args.Message = $"ADB executable file is not set or does not exist.";
                return;
            }

            Process.Start(new ProcessStartInfo()
            {
                FileName = exepath,
                Arguments = "kill-server",
            });

            args.Message = $"Killed adb server.";
        },
    };

}
