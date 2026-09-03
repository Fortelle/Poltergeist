using System.Diagnostics;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Helpers;
using Poltergeist.Modules.Navigation;

namespace Poltergeist.Modules.Macros;

public class MacroActionService : ServiceBase
{
    private readonly MacroInstanceManager InstanceManager;
    private readonly MacroManager MacroManager;
    private readonly GlobalOptionsService GlobalOptionsService;

    public MacroActionService(
        MacroInstanceManager macroInstanceManager,
        MacroManager macroManager,
        GlobalOptionsService globalOptionsService
        )
    {
        InstanceManager = macroInstanceManager;
        MacroManager = macroManager;
        GlobalOptionsService = globalOptionsService;
    }

    public ParameterValueCollection GetEnvironments(MacroInstance instance)
    {
        var environments = new ParameterValueCollection();
        foreach (var (key, value) in MacroManager.GlobalEnvironments)
        {
            environments[key] = value;
        }
        foreach (var (key, value) in instance.GetEnvironments())
        {
            environments[key] = value;
        }
        return environments;
    }

    public ParameterValueCollection GetOptions(MacroInstance instance)
    {
        var options = new ParameterValueCollection();
        foreach (var (key, value) in GlobalOptionsService.GlobalOptions.GetValueDictionary())
        {
            options[key] = value;
        }
        foreach (var (key, value) in instance.GetOptions())
        {
            options[key] = value;
        }

        return options;
    }

    public void Execute(MacroInstance instance, MacroAction action)
    {
        try
        {
            switch (action)
            {
                case SyncAction syncAction:
                    ExecuteSyncAction(instance, syncAction);
                    break;
                case AsyncAction asyncAction:
                    _ = ExecuteAsyncAction(instance, asyncAction);
                    break;
                case UriAction uriAction:
                    ExecuteUriAction(instance, uriAction);
                    break;
                case TerminalAction terminalAction:
                    ExecuteTerminalAction(instance, terminalAction);
                    break;
                case ExternalProcessAction externalProcessAction:
                    ExecuteExternalProcessAction(instance, externalProcessAction);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        catch (Exception exception)
        {
            PoltergeistApplication.ShowException(exception);
        }
    }

    private void ExecuteSyncAction(MacroInstance instance, SyncAction syncAction)
    {
        var context = new SyncActionContext()
        {
            Macro = instance.Template!,
            Options = GetOptions(instance),
            Environments = GetEnvironments(instance),
        };

        syncAction.Execute(context);

        if (!string.IsNullOrEmpty(context.Message))
        {
            PoltergeistApplication.ShowTeachingTip(context.Message);
        }
    }

    private async Task ExecuteAsyncAction(MacroInstance instance, AsyncAction asyncAction)
    {
        using var cts = new CancellationTokenSource();

        var context = new AsyncActionContext()
        {
            Macro = instance.Template!,
            Options = GetOptions(instance),
            Environments = GetEnvironments(instance),
            CancellationToken = cts.Token,
        };

        _ = InteractionHelper.Interact(new ProgressModel()
        {
            IsOn = true,
            Title = asyncAction.ProgressTitle ?? asyncAction.Text,
            CancellationTokenSource = cts,
        });

        await asyncAction.ExecuteAsync(context);

        if (!string.IsNullOrEmpty(context.Message))
        {
            PoltergeistApplication.ShowTeachingTip(context.Message);
        }

        _ = InteractionHelper.Interact(new ProgressModel()
        {
            IsOn = false,
        });
    }

    private void ExecuteUriAction(MacroInstance instance, UriAction uriAction)
    {
        var uri = uriAction.Uri;
        if (uriAction.GetUri is not null)
        {
            var context = new ActionContext()
            {
                Macro = instance.Template!,
                Options = GetOptions(instance),
                Environments = GetEnvironments(instance),
            };
            uri = uriAction.GetUri(context);
        }

        if (uri is not null)
        {
            Process.Start("explorer", uri);
        }
    }

    private static void ExecuteTerminalAction(MacroInstance instance, MacroAction action)
    {
        var actionIndex = instance.Template!.Actions.IndexOf(action);
        var pageKey = $"quest:{instance.InstanceId}:{actionIndex}";

        var navigationService = PoltergeistApplication.GetService<NavigationService>();
        navigationService.NavigateTo(pageKey, instance);
    }

    private void ExecuteExternalProcessAction(MacroInstance instance, ExternalProcessAction processAction)
    {
        var context = new ActionContext()
        {
            Macro = instance.Template!,
            Options = GetOptions(instance),
            Environments = GetEnvironments(instance),
        };
        var startInfo = processAction.GetStartInfo(context);

        _ = Process.Start(startInfo);
    }
}
