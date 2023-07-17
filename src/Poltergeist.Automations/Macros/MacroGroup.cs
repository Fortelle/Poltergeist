using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Configs;

namespace Poltergeist.Automations.Macros;

public abstract class MacroGroup
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public MacroOptions Options { get; init; } = new();

    public string? GroupFolder { get; set; }
    public List<IMacroBase> Macros { get; } = new();


    protected MacroGroup(string name)
    {
        Name = name;

        LoadMacroFields();
        LoadMacroFunctions();
        LoadMacroClasses();
    }

    public virtual void SetGlobalOptions(MacroOptions options)
    {

    }

    public void LoadOptions()
    {
        if (string.IsNullOrEmpty(GroupFolder))
        {
            return;
        }

        var filepath = Path.Combine(GroupFolder, "config.json");
        Options.Load(filepath);
    }

    public void SaveOptions()
    {
        if (string.IsNullOrEmpty(GroupFolder))
        {
            return;
        }

        var filepath = Path.Combine(GroupFolder, "config.json");
        Options.Save(filepath);
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
