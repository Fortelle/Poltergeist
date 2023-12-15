using System.Reflection;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Automations.Macros;

public abstract class MacroGroup
{
    public string Key { get; init; }
    public string? Description { get; init; }
    public OptionCollection Options { get; } = new();
    public ParameterCollection Statistics { get; } = new();

    public string? GroupFolder { get; set; }
    public List<IMacroBase> Macros { get; } = new();

    private string? _title;
    public string Title { get => _title ?? Key; set => _title = value; }

    private bool IsLoaded { get; set; }

    protected MacroGroup(string key)
    {
        Key = key;

        LoadMacroFields();
        LoadMacroFunctions();
        LoadMacroClasses();
    }

    public void Load()
    {
        if (IsLoaded)
        {
            return;
        }

        if (!string.IsNullOrEmpty(GroupFolder))
        {
            Options.Load(Path.Combine(GroupFolder, "useroptions.json"));

            Statistics.Load(Path.Combine(GroupFolder, "statistics.json"));
        }


        IsLoaded = true;
    }

    private void LoadMacroFields()
    {
        var fields = GetType()
            .GetFields()
            .Where(field => field.GetCustomAttribute<AutoLoadAttribute>() != null)
            .Where(field => field.FieldType.IsAssignableTo(typeof(MacroBase)))
            ;

        foreach (var field in fields)
        {
            var macro = (MacroBase)field.GetValue(this)!;
            Macros.Add(macro);
        }
    }

    private void LoadMacroFunctions()
    {
        var methods = GetType()
            .GetMethods()
            .Where(method => method.GetCustomAttribute<AutoLoadAttribute>() != null)
            .Where(method => method.ReturnType.IsAssignableTo(typeof(MacroBase)))
            ;

        foreach (var method in methods)
        {
            var macro = (MacroBase)method.Invoke(this, null)!;
            Macros.Add(macro);
        }
    }

    private void LoadMacroClasses()
    {
        var types = GetType()
            .GetNestedTypes()
            .Where(type => type.IsAssignableTo(typeof(MacroBase)))
            .Where(type => type.GetCustomAttribute<AutoLoadAttribute>() != null)
            ;

        foreach (var type in types)
        {
            var macro = (MacroBase)Activator.CreateInstance(type)!;
            Macros.Add(macro);
        }
    }
}
