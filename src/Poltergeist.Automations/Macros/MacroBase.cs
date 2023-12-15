using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Common;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

/// <summary>
/// Provides the base class of a macro.
/// </summary>
public abstract class MacroBase : IMacroBase, IMacroInitializer
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

    public Action<ServiceCollection, IConfigureProcessor>? Configure { get; set; }
    public Action<MacroProcessor>? Process { get; set; }

    private string? _title;
    public string Title { get => _title ?? Key; set => _title = value; }

    public MacroGroup? Group { get; set; }

    private bool _requiresAdmin;
    public bool RequiresAdmin { get => _requiresAdmin; set => _requiresAdmin |= value; }

    public bool MinimizeApplication { get; set; }

    public string? PrivateFolder { get; set; }
    public string? SharedFolder { get; set; }

    public bool IsAvailable => IsInitialized;

    private bool UseFile => !string.IsNullOrEmpty(PrivateFolder);

    protected virtual void OnInitialize() { }
    protected virtual void OnLoad() { }
    protected virtual void OnConfigure(ServiceCollection services, IConfigureProcessor processor) { }
    protected virtual void OnProcess(MacroProcessor processor) { }

    private bool IsInitialized { get; set; }
    private bool IsLoaded { get; set; }

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
        if (IsInitialized)
        {
            return;
        }

        if (UseFile)
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

        OnInitialize();

        foreach (var module in Modules)
        {
            module.OnMacroInitialized(this);
        }

        IsInitialized = true;
    }

    void IMacroBase.Load()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException();
        }

        if (IsLoaded)
        {
            return;
        }

        if (!string.IsNullOrEmpty(PrivateFolder))
        {
            UserOptions.Load(Path.Combine(PrivateFolder, "useroptions.json"));

            Statistics.Load(Path.Combine(PrivateFolder, "statistics.json"));

            History.Load(Path.Combine(PrivateFolder, "history.json"));
        }

        IsLoaded = true;
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

    void IMacroBase.ConfigureServices(ServiceCollection services, IConfigureProcessor processor)
    {
        Configure?.Invoke(services, processor);
        OnConfigure(services, processor);
    }

    void IMacroBase.Process(MacroProcessor processor)
    {
        Process?.Invoke(processor);
        OnProcess(processor);
    }

    void IMacroBase.ExecuteAction(MacroAction action, IReadOnlyDictionary<string, object?> options, IReadOnlyDictionary<string, object?> environments)
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException();
        }

        if (!IsLoaded)
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

        var args = new MacroActionArguments(this)
        {
            Options = options,
            Environments = environments,
        };
        if (action.Execute is not null)
        {
            try
            {
                action.Execute(args);

                if (!string.IsNullOrEmpty(args.Message))
                {
                    _ = InteractionService.UIShowAsync(new TipModel()
                    {
                        Text = args.Message,
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
                    args.CancellationToken = cts.Token;
                }

                _ = InteractionService.UIShowAsync(new ProgressModel()
                {
                    IsOn = true,
                    Title = action.ProgressTitle ?? action.Text,
                    CancellationTokenSource = cts,
                });

                try
                {
                    await action.ExecuteAsync(args);

                    if (!string.IsNullOrEmpty(args.Message))
                    {
                        _ = InteractionService.UIShowAsync(new TipModel()
                        {
                            Text = args.Message,
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
