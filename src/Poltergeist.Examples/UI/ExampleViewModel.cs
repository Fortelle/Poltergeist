using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Modules.Macros;

namespace Poltergeist.Examples.UI;

public partial class ExampleViewModel : ObservableRecipient
{
    public MacroInstance[] MacroInstances { get; }

    public ExampleViewModel()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var instances = new List<MacroInstance>();
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(MacroBase)))
            {
                continue;
            }

            var exampleAttribute = type.GetCustomAttribute<ExampleMacroAttribute>();
            if (exampleAttribute is null)
            {
                continue;
            }

            var macro = (MacroBase)Activator.CreateInstance(type)!;
            if (macro is ICreateInstance ci)
            {
                var instance = ci.CreateInstance(macro);
                instances.Add(instance);
            }
            else
            {
                var instance = MacroInstance.CreateStaticInstance(macro);
                instance.DefaultStartArguments = new()
                {
                    IncognitoMode = exampleAttribute.IsIncognito,
                    OptionOverrides = new()
                    {
                        [MacroLogger.ToDashboardLevelKey] = LogLevel.All,
                    }
                };
                instances.Add(instance);
            }
        }

        MacroInstances = instances.ToArray();
    }

    public void OpenExample(MacroInstance instance)
    {
        App.TryEnqueue(() =>
        {
            App.GetService<MacroManager>().OpenPage(instance);
        });
    }
}
