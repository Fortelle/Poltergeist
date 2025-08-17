using Poltergeist.Automations.Processors;
using Poltergeist.Helpers;
using Poltergeist.Modules.App;
using Poltergeist.Modules.CommandLine;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroCommandLineParser : CommandLineParser
{
    [CommandLineOption(LongName = "Macro")]
    public required string InstanceIdentifier { get; set; }

    [CommandLineOption]
    public bool AutoStart { get; set; }

    [CommandLineOption]
    public bool AutoClose { get; set; }

    [CommandLineOption]
    public bool ExclusiveMode { get; set; }

    public override bool AllowsPassed => true;

    public override void Parse(CommandLineOptionArguments args)
    {
        if (string.IsNullOrEmpty(InstanceIdentifier))
        {
            return;
        }

        if (args.IsPassed)
        {
            Parse(args.IsPassed);
        }
        else
        {
            OnAppContentLoading();
            PoltergeistApplication.GetService<AppEventService>().Subscribe<AppShellPageLoadedEvent>(_ =>
            {
                Parse(args.IsPassed);
            });
        }
    }

    private void OnAppContentLoading()
    {
        var instanceManager = PoltergeistApplication.GetService<MacroInstanceManager>();
        var instance = instanceManager.GetInstance(InstanceIdentifier);
        if (instance is null)
        {
            return;
        }

        if (ExclusiveMode)
        {
            PoltergeistApplication.Current.ExclusiveMacroMode = instance.InstanceId;
            PoltergeistApplication.Current.StartPageKey = MacroManager.GetPageKey(instance.InstanceId);
        }

        RestorePreviousTabsHelper.IsEnabled = false;
    }

    private void Parse(bool isPassed)
    {
        var instanceManager = PoltergeistApplication.GetService<MacroInstanceManager>();
        var instance = instanceManager.GetInstance(InstanceIdentifier);
        if (instance is null)
        {
            return;
        }

        var macroManager = PoltergeistApplication.GetService<MacroManager>();
        if (!macroManager.OpenPage(instance, out var viewmodel))
        {
            return;
        }

        if (AutoStart)
        {
            var startArguments = new MacroStartArguments();
            if (AutoClose && !isPassed)
            {
                startArguments.OptionOverrides = new()
                {
                    ["aftercompletion.action"] = CompletionAction.ExitApplication,
                };
            }

            var launchReason = isPassed ? LaunchReason.PipeMessage : LaunchReason.CommandLine;

            viewmodel.Start(startArguments, launchReason);
        }
    }
}
