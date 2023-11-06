using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Macros;

/// <summary>
/// Provides the base class of a macro.
/// </summary>
public abstract class MacroBase : IMacroBase, IMacroInitializer
{
    public string Name { get; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string[]? Details { get; set; }
    public string[]? Tags { get; set; }

    public MacroOptions UserOptions { get; } = new();
    public VariableCollection Statistics { get; } = new();
    public List<ProcessSummary> Summaries { get; } = new();

    public List<MacroAction> Actions { get; } = new();
    public List<MacroModule> Modules { get; } = new();
    public MacroStorage Storage { get; } = new();

    public Action<ServiceCollection, IConfigureProcessor>? Configure { get; set; }
    public Action<MacroProcessor>? Process { get; set; }

    private string? _title;
    public string Title { get => _title ?? Name; set => _title = value; }

    MacroGroup? IMacroBase.Group { get; set; }

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

    private static readonly char[] InvalidKeyChars;

    static MacroBase()
    {
        InvalidKeyChars = new[]
        {
            ' ',
            '@',
            ':',
        }
        .Concat(Path.GetInvalidFileNameChars())
        .ToArray();
    }

    public MacroBase()
    {
        Name = GetType().Name;
    }

    public MacroBase(string name)
    {
        Name = string.Join(null, name.Select(c => InvalidKeyChars.Contains(c) ? '_' : c));
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
        
        Statistics.Add(new("total_run_count", 0)
        {
            Title = "Total run count",
        });
        Statistics.Add(new("total_run_duration", default(TimeSpan))
        {
            Title = "Total run duration",
        });
        Statistics.Add(new("last_run_time", default(DateTime))
        {
            Title = "Last run time",
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

        LoadOptions();
        LoadStatistics();
        LoadSummaries();

        IsLoaded = true;
    }

    private void LoadOptions()
    {
        if (string.IsNullOrEmpty(PrivateFolder))
        {
            return;
        }

        var path = Path.Combine(PrivateFolder, "useroptions.json");
        if (!File.Exists(path))
        {
            return;
        }

        UserOptions.Load(path);
    }

    void IMacroBase.SaveOptions()
    {
        if (!IsInitialized)
        {
            return;
        }

        if (string.IsNullOrEmpty(PrivateFolder))
        {
            return;
        }

        if (!UserOptions.HasChanged)
        {
            return;
        }

        var path = Path.Combine(PrivateFolder, "useroptions.json");
        UserOptions.Save(path);
    }

    private void LoadStatistics()
    {
        if (string.IsNullOrEmpty(PrivateFolder))
        {
            return;
        }

        var path = Path.Combine(PrivateFolder, "statistics.json");
        if (!File.Exists(path))
        {
            return;
        }

        Statistics.Load(path);
    }

    void IMacroBase.SaveStatistics()
    {
        if (!IsInitialized)
        {
            return;
        }

        if (string.IsNullOrEmpty(PrivateFolder))
        {
            return;
        }

        var path = Path.Combine(PrivateFolder, "statistics.json");
        Statistics.Save(path);
    }

    private void LoadSummaries()
    {
        if (string.IsNullOrEmpty(PrivateFolder))
        {
            return;
        }

        var path = Path.Combine(PrivateFolder, "summaries.json");
        if (!File.Exists(path))
        {
            return;
        }

        SerializationUtil.JsonPopulate(path, Summaries);
    }

    void IMacroBase.SaveSummaries()
    {
        if (!IsInitialized)
        {
            return;
        }

        if (string.IsNullOrEmpty(PrivateFolder))
        {
            return;
        }

        var path = Path.Combine(PrivateFolder, "summaries.json");
        SerializationUtil.JsonSave(path, Summaries);
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

    void IMacroBase.ExecuteAction(MacroAction action, Dictionary<string, object?>? options, Dictionary<string, object?>? environments)
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
            action.Execute(args);
            if (!string.IsNullOrEmpty(args.Message))
            {
                _ = InteractionService.UIShowAsync(new TipModel()
                {
                    Text = args.Message,
                });
            }
        }
        else if(action.ExecuteAsync is not null)
        {
            Task.Run(async () => {
                await action.ExecuteAsync(args);

                if (!string.IsNullOrEmpty(args.Message))
                {
                    _ = InteractionService.UIShowAsync(new TipModel()
                    {
                        Text = args.Message,
                    });
                }
            });
        }
        else
        {
            throw new ArgumentNullException();
        }
    }

    public IMacroBase CreateDuplicate()
    {
        var macrokey = Name.Split('@')[0];
        var cloneGuid = Guid.NewGuid().ToString();
        var cloneKey = $"{macrokey}@{cloneGuid}";

        var type = GetType();
        var clone = (MacroBase)Activator.CreateInstance(type, cloneKey)!;

        return clone;
    }


}
 