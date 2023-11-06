using System.Reflection;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Services;

public class PluginService
{
    private const string FilenameFormat = "Poltergeist.Plugins.*.dll";

    public PluginService()
    {
        App.ContentLoading += Load;
    }

    private static void Load()
    {
        var manager = App.GetService<MacroManager>();
        var assemblies = GetAssemblies();
        var groups = assemblies.SelectMany(GetGroups);

        foreach (var group in groups)
        {
            manager.AddGroup(group);
        }
    }

    private static IEnumerable<Assembly> GetAssemblies()
    {
        var exe = Assembly.GetExecutingAssembly();
        yield return exe;

        var entry = Assembly.GetEntryAssembly();
        if(entry != null && entry != exe)
        {
            yield return entry;
        }

        var path = App.GetService<PathService>();
        var pluginFolders = new string[]
        {
            path.AppFolder,
            Path.Combine(path.AppFolder, "Plugins"),
            Path.Combine(path.DocumentDataFolder, "Plugins"),
        };
        foreach (var folder in pluginFolders)
        {
            if (!Directory.Exists(folder)) continue;

            var files = Directory.GetFiles(folder, FilenameFormat, SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var assembly = Assembly.LoadFrom(file);
                yield return assembly;
            }
        }
    }


    private static IEnumerable<MacroGroup> GetGroups(Assembly assembly)
    {
        var groupType = typeof(MacroGroup);
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsAssignableTo(groupType))
            {
                continue;
            }
            if (type.GetCustomAttribute<AutoLoadAttribute>() == null)
            {
                continue;
            }
            if (Activator.CreateInstance(type) is not MacroGroup result)
            {
                continue;
            }

            yield return result;
        }
    }
}
