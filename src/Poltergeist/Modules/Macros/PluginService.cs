using System.Reflection;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class PluginService : ServiceBase
{
    private const string FilenameFormat = "Poltergeist.Plugin*.dll";

    public PluginService(AppEventService eventService)
    {
        eventService.Subscribe<AppContentLoadingHandler>(OnAppContentLoading);
    }

    private void OnAppContentLoading(AppContentLoadingHandler e)
    {
        var manager = PoltergeistApplication.GetService<MacroManager>();
        var assemblies = GetAssemblies();
        var groupType = typeof(MacroGroup);
        var macroType = typeof(MacroBase);

        foreach (var assembly in assemblies)
        {
            var name = assembly.GetName();
            var assemblyName = name.Name;
            var assemblyVersion = assembly.GetName().Version;
            var assemblyLocation = assembly.Location;

            void AddMacro(MacroBase macro)
            {
                macro.Properties.Add("assembly_name", assemblyName);
                macro.Properties.Add("assembly_version", assemblyVersion);
                macro.Properties.Add("assembly_location", assemblyLocation);
                manager.AddMacro(macro);
            }

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAssignableTo(groupType))
                {
                    if (type.GetCustomAttribute<AutoLoadAttribute>() is null)
                    {
                        continue;
                    }

                    MacroGroup? group = null;
                    try
                    {
                        group = (MacroGroup)Activator.CreateInstance(type)!;
                        Logger.Trace($"Created an instance of macro group '{type.Name}'.", new
                        {
                            GroupType = type,
                            AssemblyName = assembly.FullName,
                            AssemblyLocation = assembly.Location
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to create an instance of macro group '{type.Name}': {ex.Message}");
                        continue;
                    }

                    var macros = manager.LoadMacrosFromGroup(group);
                    foreach (var macro in macros)
                    {
                        AddMacro(macro);
                    }
                }
                else if (type.IsAssignableTo(macroType))
                {
                    if (type.GetCustomAttribute<AutoLoadAttribute>() is null)
                    {
                        continue;
                    }

                    try
                    {
                        var macro = (MacroBase)Activator.CreateInstance(type)!;
                        AddMacro(macro);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to create an instance of macro '{type.Name}': {ex.Message}");
                        continue;
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
