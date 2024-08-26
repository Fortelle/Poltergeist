using System.Reflection;
using System.Text;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Services;

public class PluginService
{
    private const string FilenameFormat = "Poltergeist.Plugins.*.dll";

    public StringBuilder ErrorLog { get; } = new();

    public PluginService()
    {
    }

    public void Load()
    {
        var manager = App.GetService<MacroManager>();
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
                    try
                    {
                        var group = (MacroGroup)Activator.CreateInstance(type)!;
                        foreach (var macro in group.ReadMacroFields())
                        {
                            AddMacro(macro);
                        }
                        foreach (var macro in group.ReadMacroFunctions())
                        {
                            AddMacro(macro);
                        }
                        foreach (var macro in group.ReadMacroClasses())
                        {
                            AddMacro(macro);
                        }
                    }
                    catch
                    {
                        ErrorLog.AppendLine($"Failed to load group \"{type.Name}\" from plugin \"{assemblyName}\".");
                        continue;
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
                    catch
                    {
                        ErrorLog.AppendLine($"Failed to load macro \"{type.Name}\" from plugin \"{assemblyName}\".");
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
        if(entry is not null && entry != exe)
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
                }
                catch
                {
                    ErrorLog.AppendLine($"Failed to load plugin \"{file}\"");
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
