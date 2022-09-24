using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Services;

public class PluginService
{
    public PluginService()
    {
    }

    public async Task Load()
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
            Path.Combine(path.DocumentFolder, "Plugins"),
        };
        foreach (var folder in pluginFolders)
        {
            if (!Directory.Exists(folder)) continue;

            var files = Directory.GetFiles(folder, "Poltergeist.Plugins.*.dll", SearchOption.TopDirectoryOnly);
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
            if (groupType.IsAssignableFrom(type))
            {
                if (Activator.CreateInstance(type) is MacroGroup result)
                {
                    yield return result;
                }
            }
        }
    }
}
