using System.Reflection;
using Poltergeist.Automations.Macros;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroTemplateManager : ServiceBase
{
    private const string FilenameFormat = "Poltergeist.Plugin.*.dll";

    public List<IFrontMacro> Templates { get; } = new();

    private readonly MacroStatisticsService StatisticService;
    private readonly GlobalOptionsService GlobalOptionsService;

    public MacroTemplateManager(
        AppEventService eventService,
        MacroStatisticsService statisticService,
        GlobalOptionsService globalOptionsService
        )
    {
        StatisticService = statisticService;
        GlobalOptionsService = globalOptionsService;

        eventService.Subscribe<AppContentLoadingEvent>(OnAppContentLoading, new() { Priority = 200 }); // should go before MacroInstanceManager
    }

    public void Register(IFrontMacro macro)
    {
        RegisterInternal(macro);
    }

    private void RegisterInternal(IFrontMacro macro)
    {
        Templates.Add(macro);

        macro.Initialize();

        foreach (var definition in macro.StatisticDefinitions.Where(x => x.IsGlobal))
        {
            if (StatisticService.GlobalStatistics.TryGetDefinition(definition.Key, out var oldDefinition))
            {
                if (oldDefinition.BaseType != definition.BaseType)
                {
                    Logger.Warn($"Added macro template '{macro.Key}'({macro.GetType().Name}).");
                }
                continue;
            }

            StatisticService.GlobalStatistics.AddDefinition(definition);
        }

        foreach (var definition in macro.OptionDefinitions.Where(x => x.IsGlobal))
        {
            if (GlobalOptionsService.GlobalOptions.ContainsDefinition(definition.Key))
            {
                continue;
            }

            GlobalOptionsService.GlobalOptions.AddDefinition(definition);
        }

        Logger.Debug($"Added macro template '{macro.Key}'({macro.GetType().Name}).");
    }

    public IFrontMacro? GetTemplate(string macroKey)
    {
        return Templates.FirstOrDefault(x => x.Key == macroKey);
    }

    private void OnAppContentLoading(AppContentLoadingEvent _)
    {
        var assemblies = GetAssemblies();
        var macroType = typeof(MacroBase);

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAssignableTo(macroType))
                {
                    if (type.GetCustomAttribute<MacroTemplateAttribute>() is not null)
                    {
                        try
                        {
                            var macro = (MacroBase)Activator.CreateInstance(type)!;
                            RegisterInternal(macro);
                            Logger.Trace($"Created an instance of macro '{type.Name}'.");
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to create an instance of macro '{type.Name}': {ex.Message}");
                            continue;
                        }
                    }
                    else if (type.GetCustomAttribute<MacroInstanceAttribute>() is not null)
                    {
                        try
                        {
                            var macro = (MacroBase)Activator.CreateInstance(type)!;
                            var instance = MacroInstance.CreateStaticInstance(macro);
                            PoltergeistApplication.GetService<MacroInstanceManager>().AddInstance(instance);
                            Logger.Trace($"Created a static macro instance of template '{type.Name}'.");
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to create a macro instance of template '{type.Name}': {ex.Message}");
                            continue;
                        }
                    }
                }
            }
        }
    }

    private IEnumerable<Assembly> GetAssemblies()
    {
        var exe = Assembly.GetExecutingAssembly();
        //yield return exe;

        var entry = Assembly.GetEntryAssembly();
        if (entry is not null && entry != exe)
        {
            yield return entry;
        }

        if (PoltergeistApplication.Current.StartupOptions.Contains("DisablePlugins"))
        {
            yield break;
        }

        var pluginFolders = new string[]
        {
            PoltergeistApplication.Paths.AppFolder,
            Path.Combine(PoltergeistApplication.Paths.AppFolder, "Plugins"),
            Path.Combine(PoltergeistApplication.Paths.DocumentDataFolder, "Plugins"),
        };
        foreach (var folder in pluginFolders)
        {
            if (!Directory.Exists(folder))
            {
                continue;
            }

            var files = Directory.GetFiles(folder, FilenameFormat, SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                Assembly? assembly;
                try
                {
                    assembly = Assembly.LoadFrom(file);
                    Logger.Debug($"Loaded assembly '{file}'.");
                }
                catch (Exception exception)
                {
                    Logger.Error($"Failed to load assembly '{file}': {exception.Message}");
                    continue;
                }
                if (assembly is not null)
                {
                    yield return assembly;
                }
            }
        }
    }
}
