using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Common;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

/// <summary>
/// Provides the base class of a macro.
/// </summary>
public abstract class MacroBase : IMacroBase, IInitializableMacro
{
    public const string UseStatisticsKey = "macro.usestatistics";

    public string Key { get; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string[]? Details { get; set; }
    public string[]? Tags { get; set; }

    public OptionCollection UserOptions { get; } = new();
    public ParameterCollection Statistics { get; } = new();
    public ProcessHistoryCollection History { get; } = new();

    public List<MacroAction> Actions { get; } = new();
    public List<MacroModule> Modules { get; } = new();
    public MacroStorage Storage { get; } = new();
    public List<ConfigVariation> ConfigVariations { get; } = new();

    private string? _title;
    public string Title { get => _title ?? Key; set => _title = value; }

    public MacroGroup? Group { get; set; }

    private bool _requiresAdmin;
    public bool RequiresAdmin { get => _requiresAdmin; set => _requiresAdmin |= value; }

    public bool MinimizeApplication { get; set; }

    public string? PrivateFolder { get; set; }
    public string? SharedFolder { get; set; }

    protected virtual void OnConfigure(IConfigurableProcessor processor) { }
    void IMacroBase.OnConfigure(IConfigurableProcessor processor) => OnConfigure(processor);

    protected virtual void OnPrepare(IPreparableProcessor processor) { }
    void IMacroBase.OnPrepare(IPreparableProcessor processor) => OnPrepare(processor);

    public MacroStatus Status { get; set; }
    public Exception? Exception { get; set; }

    private static readonly char[] InvalidKeyChars = new[]
        {
            ' ',
            '@',
            ':',
        }
        .Concat(Path.GetInvalidFileNameChars())
        .ToArray();

    public MacroBase()
    {
        Key = GetType().Name;
    }

    public MacroBase(string name)
    {
        Key = string.Join(null, name.Select(c => InvalidKeyChars.Contains(c) ? '_' : c));
    }

    public T As<T>() where T : MacroBase
    {
        return (T)this;
    }

    void IMacroBase.Initialize()
    {
        if (Status != MacroStatus.Uninitialized)
        {
            return;
        }

        if (Exception is not null)
        {
            Status = MacroStatus.InitializationFailed;
            return;
        }

        try
        {
            if (!string.IsNullOrEmpty(PrivateFolder))
            {
                Actions.Add(ActionHelper.OpenLocalFolder);
            }
            Actions.Add(ActionHelper.CreateShortcut);

            Statistics.Add(new ParameterEntry<int>("total_run_count")
            {
                DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/Statistic_TotalRunCount"),
            });
            Statistics.Add(new ParameterEntry<TimeSpan>("total_run_duration")
            {
                DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/Statistic_TotalRunDuration"),
                Format = x => $"{x.TotalHours:00}:{x.Minutes:00}:{x.Seconds:00}",
            });
            Statistics.Add(new ParameterEntry<DateTime?>("last_run_time")
            {
                DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/Statistic_LastRunTime"),
                Format = x => x?.ToString() ?? "-",
            });

            foreach (var module in Modules)
            {
                module.OnMacroInitialize(this);
            }

            Status = MacroStatus.Initialized;
        }
        catch (Exception ex)
        {
            Status = MacroStatus.InitializationFailed;
            Exception = ex;
            if (Debugger.IsAttached)
            {
                throw;
            }
        }
    }

    void IMacroBase.Load()
    {
        if (Status == MacroStatus.Uninitialized)
        {
            ((IMacroBase)this).Initialize();
        }

        if (Status != MacroStatus.Initialized)
        {
            return;
        }

        try
        {
            if (!string.IsNullOrEmpty(PrivateFolder))
            {
                UserOptions.Load(Path.Combine(PrivateFolder, "useroptions.json"));

                Statistics.Load(Path.Combine(PrivateFolder, "statistics.json"));

                History.Load(Path.Combine(PrivateFolder, "history.json"));
            }

            Status = MacroStatus.Loaded;
        }
        catch (Exception ex)
        {
            Status = MacroStatus.LoadFailed;
            Exception = ex;
            if (Debugger.IsAttached)
            {
                throw;
            }
        }
    }

    public virtual string? CheckValidity()
    {
        if (Status == MacroStatus.Uninitialized)
        {
            ((IMacroBase)this).Initialize();
        }

        if (Status == MacroStatus.Initialized)
        {
            ((IMacroBase)this).Load();
        }

        if (Exception is not null)
        {
            return Exception.Message;
        }

        if (RequiresAdmin)
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                return ResourceHelper.Localize("Poltergeist.Automations/Resources/Validation_RequiresAdmin");
            }
        }

        return null;
    }

    VariableCollection IMacroBase.GetOptionCollection()
    {
        var vc = new VariableCollection();

        foreach (var item in UserOptions)
        {
            vc.Add(item.Key, item.Value, ParameterSource.Macro);
        }

        if (Group is not null)
        {
            foreach (var item in Group.Options)
            {
                if (vc.Contains(item.Key))
                {
                    continue;
                }
                vc.Add(item.Key, item.Value, ParameterSource.Group);
            }
        }

        return vc;
    }

    VariableCollection IMacroBase.GetStatisticCollection()
    {
        var vc = new VariableCollection();

        foreach (var item in Statistics)
        {
            vc.Add(item.Key, item.Value, ParameterSource.Macro);
        }

        if (Group is not null)
        {
            foreach (var item in Group.Statistics)
            {
                if (vc.Contains(item.Key))
                {
                    continue;
                }
                vc.Add(item.Key, item.Value, ParameterSource.Group);
            }
        }

        return vc;
    }

    void IMacroBase.ExecuteAction(MacroAction action, MacroActionArguments arguments)
    {
        if (Status <= MacroStatus.Loaded)
        {
            throw new InvalidOperationException();
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (!Actions.Contains(action))
        {
            throw new ArgumentException();
        }

        if (action.Execute is not null)
        {
            try
            {
                action.Execute(arguments);

                if (!string.IsNullOrEmpty(arguments.Message))
                {
                    _ = InteractionService.UIShowAsync(new TipModel()
                    {
                        Text = arguments.Message,
                    });
                }
            }
            catch (Exception exception)
            {
                _ = InteractionService.UIShowAsync(new TipModel()
                {
                    Text = exception.Message,
                });
            }
        }
        else if(action.ExecuteAsync is not null)
        {
            Task.Run(async () => {
                CancellationTokenSource? cts = null;

                if (action.IsCancellable)
                {
                    cts = new();
                    arguments.CancellationToken = cts.Token;
                }

                _ = InteractionService.UIShowAsync(new ProgressModel()
                {
                    IsOn = true,
                    Title = action.ProgressTitle ?? action.Text,
                    CancellationTokenSource = cts,
                });

                try
                {
                    await action.ExecuteAsync(arguments);

                    if (!string.IsNullOrEmpty(arguments.Message))
                    {
                        _ = InteractionService.UIShowAsync(new TipModel()
                        {
                            Text = arguments.Message,
                        });
                    }
                }
                catch (Exception exception)
                {
                    _ = InteractionService.UIShowAsync(new TipModel()
                    {
                        Text = exception.Message,
                    });
                }

                _ = InteractionService.UIShowAsync(new ProgressModel()
                {
                    IsOn = false,
                });

                cts?.Dispose();
            });
        }
        else
        {
            throw new ArgumentNullException();
        }
    }

    public IMacroBase CreateDuplicate()
    {
        var macrokey = Key.Split('@')[0];
        var cloneGuid = Guid.NewGuid().ToString();
        var cloneKey = $"{macrokey}@{cloneGuid}";

        var type = GetType();
        var clone = (MacroBase)Activator.CreateInstance(type, cloneKey)!;

        return clone;
    }

}
