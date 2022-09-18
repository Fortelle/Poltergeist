using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Poltergeist.Plugins;

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
            var macros = group.GetMacros().ToList();

            manager.Groups.Add(new()
            {
                Key = "group_" + group.Name.ToLower(),
                Name = group.Name,
                Description = group.Description,
                Macros = macros,
            });
            foreach(var macro in macros)
            {
                manager.AddMacro(macro);
            }
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


    private static IEnumerable<IMacroGroup> GetGroups(Assembly assembly)
    {
        var groupType = typeof(IMacroGroup);
        foreach (var type in assembly.GetTypes())
        {
            if (groupType.IsAssignableFrom(type))
            {
                if (Activator.CreateInstance(type) is IMacroGroup result)
                {
                    yield return result;
                }
            }
        }
    }
}
