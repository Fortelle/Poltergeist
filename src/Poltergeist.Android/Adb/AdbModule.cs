using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Storages;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Android.Adb;

public class AdbModule : MacroModule
{
    public static readonly OptionDefinition<string> IpAddressOption = new("adb.ip_address")
    {
        DisplayLabel = "IP Address",
        Category = "ADB",
        IsGlobal = true,
    };

    public static readonly PathOption AdbPathOption = new("adb.exepath")
    {
        DisplayLabel = "Exe file",
        Category = "ADB",
        IsGlobal = true,
    };

    public static readonly OptionDefinition<bool> KeepAliveOption = new("adb.keep_alive")
    {
        DisplayLabel = "Keep adb server alive",
        Description = "Skips killing the adb server when the macro is completed. " +
                "This helps when you are planning to launch the macro frequently in a short time. " +
                "You can use the \"kill-server\" action to kill the adb server manually.",
        Category = "ADB",
    };

    public override void OnMacroInitialized(IMacroInformation macro)
    {
        base.OnMacroInitialized(macro);

        macro.OptionDefinitions.Add(IpAddressOption);
        macro.OptionDefinitions.Add(AdbPathOption);
        macro.OptionDefinitions.Add(KeepAliveOption);

        macro.Actions.Add(KillServerAction);
    }

    public override void RegisterServices(IServiceCollection services, RegisterServicesArguments args)
    {
        base.RegisterServices(services, args);

        services.AddSingleton<TerminalService>();
        services.AddSingleton<AdbService>();
        services.AddSingleton<AdbLocatingService>();
        services.AddSingleton<AdbInputService>();
        services.AddSingleton<AdbCapturingService>();

        services.AddOptions<AdbInputOptions>();
    }

    [MacroHook]
    public static void OnProcessorStartup(IMacroProcessorShared processor, ProcessorStartupHook hook)
    {
        var adbService = processor.GetService<AdbService>();
        adbService.Address ??= processor.Options.GetValueOrDefault(IpAddressOption, null);
        adbService.ExePath ??= processor.Options.GetValueOrDefault(AdbPathOption, null);

        var keepAlive = processor.Options.GetValueOrDefault(KeepAliveOption);

        if (keepAlive && adbService.ExePath is not null)
        {
            var localStorage = processor.GetService<LocalStorageService>();
            localStorage.Storage.AddOrUpdate(AdbPathOption, adbService.ExePath);
        }
    }

    [MacroHook]
    public static void OnProcessorCompleted(IMacroProcessorShared processor, ProcessorCompletedHook hook)
    {
        var adbService = processor.GetService<AdbService>();
        if (!adbService.IsConnected)
        {
            return;
        }

        var keepAlive = processor.Options.GetValueOrDefault(KeepAliveOption);

        if (!keepAlive)
        {
            adbService.Close();
        }

        if (!keepAlive)
        {
            var localStorage = processor.GetService<LocalStorageService>();
            localStorage.Storage.TryRemove(AdbPathOption);
        }
    }

    private static readonly MacroAction KillServerAction = new ExternalProcessAction()
    {
        Text = "Kill ADB server",
        Description = "Runs \"adb kill-server\" via Command Prompt to kill the adb server.",
        Icon = new GlyphIcon("\uE756"),
        GetStartInfo = context =>
        {
            var localStorage = LocalStorageService.Load(context.Environments);

            if (!localStorage.TryGetValue(AdbPathOption, out var exePath))
            {
                throw new Exception($"{AdbPathOption.Key} does not found.");
            }

            return new ProcessStartInfo()
            {
                FileName = exePath,
                Arguments = "kill-server",
            };
        },

    };
}
