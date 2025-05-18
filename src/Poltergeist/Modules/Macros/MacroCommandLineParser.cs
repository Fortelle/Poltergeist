using Poltergeist.Automations.Processors;
using Poltergeist.Helpers;
using Poltergeist.Modules.App;
using Poltergeist.Modules.CommandLine;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Navigation;
using Poltergeist.UI.Pages.Macros;

namespace Poltergeist.Modules.Macros;

public class MacroCommandLineParser : CommandLineParser
{
    [CommandLineOption(LongName = "Macro")]
    public required string ShellKey { get; set; }

    [CommandLineOption]
    public bool AutoStart { get; set; }

    [CommandLineOption]
    public bool AutoClose { get; set; }

    [CommandLineOption]
    public bool SingleMode { get; set; }

    public override bool AllowsPassed => true;

    public override void Parse(CommandLineOptionArguments args)
    {
        if (string.IsNullOrEmpty(ShellKey))
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
            PoltergeistApplication.GetService<AppEventService>().Subscribe<AppWindowLoadedHandler>(_ =>
            {
                Parse(args.IsPassed);
            });
        }
    }

    private void OnAppContentLoading()
    {
        var macroManager = PoltergeistApplication.GetService<MacroManager>();
        var shell = macroManager.GetShell(ShellKey);
        if (shell is null)
        {
            return;
        }

        if (SingleMode)
        {
            PoltergeistApplication.SingleMacroMode = ShellKey;
            PoltergeistApplication.StartPageKey = "macro:" + ShellKey;
        }

        RestorePreviousTabsHelper.IsEnabled = false;
    }

    private void Parse(bool isPassed)
    {
        var macroManager = PoltergeistApplication.GetService<MacroManager>();
        var shell = macroManager.GetShell(ShellKey);
        if (shell is null)
        {
            return;
        }

        var nav = PoltergeistApplication.GetService<INavigationService>();
        if (!nav.NavigateTo("macro:" + shell.ShellKey))
        {
            return;
        }

        if (AutoStart)
        {
            var startArguments = new MacroStartArguments()
            {
                ShellKey = shell.ShellKey,
                Reason = LaunchReason.ByCommandLine,
            };
            if (AutoClose && !isPassed)
            {
                startArguments.OptionOverrides = new()
                {
                    ["aftercompletion.action"] = CompletionAction.ExitApplication,
                };
            }

            ActionService.RunMacro(startArguments);
        }
    }
}
