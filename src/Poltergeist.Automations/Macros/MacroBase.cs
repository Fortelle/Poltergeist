using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

/// <summary>
/// Provides the base class of a macro.
/// </summary>
public abstract class MacroBase : IMacroBase, IMacroInformation, IMacroExecution
{
    public string Key { get; }

    private string? _title;
    public string Title { get => _title ?? Key; set => _title = value; }

    public string? Category { get; set; }
    public string? Description { get; set; }
    public string[]? Details { get; set; }
    public string[]? Tags { get; set; }
    public IconInfo? Icon { get; set; }
    public Version? Version { get; set; }

    public OptionDefinitionCollection OptionDefinitions { get; } = new();
    public StatisticDefinitionCollection StatisticDefinitions { get; } = new();
    public ParameterDefinitionCollection Metadata { get; } = new();

    public ParameterValueCollection? OptionPresets { get; set; }
    public ParameterValueCollection? EnvironmentPresets { get; set; }

    public List<MacroAction> Actions { get; } = new();
    public List<MacroModule> Modules { get; } = new();
    public ParameterValueCollection ExtraData { get; } = new();
    public List<ConfigVariation> ConfigVariations { get; } = new();
    public List<ProcessorIntervention> Interventions { get; } = new();

    private bool _requiresAdmin;
    public bool RequiresAdmin { get => _requiresAdmin; set => _requiresAdmin |= value; }

    public Exception? Exception { get; protected set; }

    protected virtual void OnInitializing() { }

    protected virtual void OnModuleInstalled(MacroModule module) { }

    protected virtual void OnProcessorCreated(IMacroProcessorInformation processor) { }

    protected virtual void RegisterServices(IServiceCollection services, RegisterServicesArguments arguments) { }

    private bool IsInitialized;

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

    protected virtual bool CanExecute([MaybeNullWhen(true)] out string invalidationMessage)
    {
        invalidationMessage = null;
        return true;
    }

    protected virtual bool CanExecute(IMacroProcessorInformation processor, [MaybeNullWhen(true)] out string invalidationMessage)
    {
        invalidationMessage = null;
        return true;
    }

    void IMacroBase.Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        if (Exception is not null)
        {
            return;
        }

        try
        {
            OnInitializing();

            LoadModuleDependencies();

            foreach (var module in Modules)
            {
                module.OnMacroInitialized(this);
            }
        }
        catch (Exception ex)
        {
            Exception = ex;
            if (Debugger.IsAttached)
            {
                throw;
            }
        }

        IsInitialized = true;
    }

    private void LoadModuleDependencies()
    {
        var dependentModuleTypes = new List<Type>([
            GetType(),
                .. Modules.Select(x => x.GetType())
            ]);

        var tempTypes = new HashSet<Type>([
            GetType(),
            .. Modules.Select(x => x.GetType())
        ]);

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

            OnModuleInstalled(module);
        }
    }

    void IMacroExecution.OnProcessorCreated(IMacroProcessorInformation processor) => OnProcessorCreated(processor);
    bool IMacroExecution.CanExecute([MaybeNullWhen(true)] out string invalidationMessage) => CanExecute(out invalidationMessage);
    bool IMacroExecution.CanExecute(IMacroProcessorInformation processor, [MaybeNullWhen(true)] out string invalidationMessage) => CanExecute(processor, out invalidationMessage);
    void IMacroExecution.RegisterServices(ServiceCollection services, RegisterServicesArguments arguments) => RegisterServices(services, arguments);
}
