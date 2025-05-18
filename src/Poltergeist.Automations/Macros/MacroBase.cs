using System.Diagnostics;
using System.Security.Principal;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;

namespace Poltergeist.Automations.Macros;

/// <summary>
/// Provides the base class of a macro.
/// </summary>
public abstract class MacroBase : IMacroBase, IBackMacro, IFrontMacro, IConfigurableMacro, IInitializableMacro, IUserMacro
{
    public string Key { get; }

    private string? _title;
    public string Title { get => _title ?? Key; set => _title = value; }

    public string? Category { get; set; }
    public string? Description { get; set; }
    public string[]? Details { get; set; }
    public string[]? Tags { get; set; }
    public string? Icon { get; set; }
    public Version? Version { get; set; }

    public ParameterDefinitionCollection UserOptions { get; } = new();
    public ParameterDefinitionCollection Statistics { get; } = new();
    public ParameterDefinitionCollection Properties { get; } = new();

    public List<MacroAction> Actions { get; } = new();
    public List<MacroModule> Modules { get; } = new();
    public MacroStorage Storage { get; } = new();
    public List<ConfigVariation> ConfigVariations { get; } = new();

    private bool _requiresAdmin;
    public bool RequiresAdmin { get => _requiresAdmin; set => _requiresAdmin |= value; }

    public MacroStatus Status { get; protected set; }
    public Exception? Exception { get; protected set; }

    public bool IsSingleton { get; set; }

    protected virtual void OnConfigure(IConfigurableProcessor processor) { }
    protected virtual void OnPrepare(IPreparableProcessor processor) { }

    private static readonly char[] InvalidKeyChars = [
        ' ',
        '@',
        ':',
        .. Path.GetInvalidFileNameChars()
    ];

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

    protected virtual string? OnValidating()
    {
        if (Status == MacroStatus.Uninitialized)
        {
            ((IBackMacro)this).Initialize();
        }

        if (Exception is not null)
        {
            return ResourceHelper.Localize("Poltergeist.Automations/Resources/Validation_ExceptionOccurred", Exception.Message);
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

    void IFrontBackMacro.Initialize()
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
            Actions.Add(ActionHelper.OpenLocalFolder);
            Actions.Add(ActionHelper.CreateShortcut);

            Statistics.Add(new ParameterDefinition<int>("total_run_count")
            {
                DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/Statistic_TotalRunCount"),
            });
            Statistics.Add(new ParameterDefinition<TimeSpan>("total_run_duration")
            {
                DisplayLabel = ResourceHelper.Localize("Poltergeist.Automations/Resources/Statistic_TotalRunDuration"),
                Format = x => $"{x.TotalHours:00}:{x.Minutes:00}:{x.Seconds:00}",
            });
            Statistics.Add(new ParameterDefinition<DateTime?>("last_run_time")
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

    string? IFrontBackMacro.CheckValidity()
    {
        return OnValidating();
    }

    void IBackMacro.Configure(IConfigurableProcessor processor) => OnConfigure(processor);
    void IBackMacro.Prepare(IPreparableProcessor processor) => OnPrepare(processor);

}
