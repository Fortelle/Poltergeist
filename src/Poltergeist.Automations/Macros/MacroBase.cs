using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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

    public ParameterDefinitionCollection OptionDefinitions { get; } = new();
    public StatisticDefinitionCollection StatisticDefinitions { get; } = new();
    public ParameterDefinitionCollection Metadata { get; } = new();

    public List<MacroAction> Actions { get; } = new();
    public List<MacroModule> Modules { get; } = new();
    public ParameterValueCollection ExtraData { get; } = new();
    public List<ConfigVariation> ConfigVariations { get; } = new();

    private bool _requiresAdmin;
    public bool RequiresAdmin { get => _requiresAdmin; set => _requiresAdmin |= value; }

    public MacroStatus Status { get; protected set; }
    public Exception? Exception { get; protected set; }

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

    public MacroBase(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Key = GetType().Name;
        }
        else
        {
            Key = string.Join(null, name.Select(c => InvalidKeyChars.Contains(c) ? '_' : c));
        }
    }

    protected virtual bool OnValidating([MaybeNullWhen(true)] out string invalidationMessage)
    {
        if (Status == MacroStatus.Uninitialized)
        {
            ((IBackMacro)this).Initialize();
        }

        if (Exception is not null)
        {
            invalidationMessage = LocalizationUtil.Localize("Validation_ExceptionOccurred", Exception.Message);
            return false;
        }

        if (RequiresAdmin)
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!isAdmin)
            {
                invalidationMessage = LocalizationUtil.Localize("Validation_RequiresAdmin");
                return false;
            }
        }

        invalidationMessage = null;
        return true;
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
            var dependentModuleTypes = new List<Type>([
                GetType(),
                .. Modules.Select(x => x.GetType())
                ]);

            var tempTypes = new HashSet<Type>(dependentModuleTypes);
            while (tempTypes.Count > 0)
            {
                foreach (var type in tempTypes.ToArray())
                {
                    var dependencyAttributes = type.GetCustomAttributes(typeof(ModuleDependencyAttribute<>));
                    foreach (var dependencyAttribute in dependencyAttributes)
                    {
                        var moduleType = dependencyAttribute.GetType().GetGenericArguments()[0];
                        if (!dependentModuleTypes.Contains(moduleType))
                        {
                            dependentModuleTypes.Add(moduleType);
                            tempTypes.Add(moduleType);
                        }
                    }
                    tempTypes.Remove(type);
                }
            }

            dependentModuleTypes.Remove(GetType());
            foreach (var module in Modules)
            {
                dependentModuleTypes.Remove(module.GetType());
            }

            foreach (var moduleType in dependentModuleTypes)
            {
                var module = (MacroModule)Activator.CreateInstance(moduleType)!;
                Modules.Add(module);
            }

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

    bool IFrontBackMacro.CheckValidity([MaybeNullWhen(true)] out string invalidationMessage)
    {
        return OnValidating(out invalidationMessage);
    }

    void IBackMacro.Configure(IConfigurableProcessor processor) => OnConfigure(processor);
    void IBackMacro.Prepare(IPreparableProcessor processor) => OnPrepare(processor);

}
