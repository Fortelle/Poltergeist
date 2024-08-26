using System.Reflection;
using Poltergeist.Automations.Attributes;

namespace Poltergeist.Automations.Macros;

public abstract class MacroGroup
{
    public string Key { get; init; }
    public string? Description { get; init; }

    private string? _title;
    public string Title { get => _title ?? Key; set => _title = value; }

    private bool IsLoaded { get; set; }

    protected MacroGroup(string key)
    {
        Key = key;
    }

    public void Load()
    {
        if (IsLoaded)
        {
            return;
        }

        IsLoaded = true;
    }

    public IEnumerable<MacroBase> ReadMacroFields()
    {
        var fields = GetType()
            .GetFields()
            .Where(field => field.GetCustomAttribute<AutoLoadAttribute>() is not null)
            .Where(field => field.FieldType.IsAssignableTo(typeof(MacroBase)))
            ;

        foreach (var field in fields)
        {
            if (field.GetValue(this) is MacroBase macro)
            {
                yield return macro;
            }
        }
    }

    public IEnumerable<MacroBase> ReadMacroFunctions()
    {
        var methods = GetType()
            .GetMethods()
            .Where(method => method.GetCustomAttribute<AutoLoadAttribute>() is not null)
            .Where(method => method.ReturnType.IsAssignableTo(typeof(MacroBase)))
            ;

        foreach (var method in methods)
        {
            if (method.Invoke(this, null) is MacroBase macro)
            {
                yield return macro;
            }
        }
    }

    public IEnumerable<MacroBase> ReadMacroClasses()
    {
        var types = GetType()
            .GetNestedTypes()
            .Where(type => type.IsAssignableTo(typeof(MacroBase)))
            .Where(type => type.GetCustomAttribute<AutoLoadAttribute>() is not null)
            ;

        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is MacroBase macro)
            {
                yield return macro;
            }
        }
    }
}
