using System;
using System.Collections.Generic;
using System.IO;
using Poltergeist.Automations.Configs;

namespace Poltergeist.Automations.Macros;

public abstract class MacroGroup
{
    public string Name { get; init; }
    public string Description { get; init; }
    public MacroOptions Options { get; init; } = new();

    public string GroupFolder { get; set; }

    public List<IMacroBase> Macros { get; } = new();

    protected MacroGroup(string name)
    {
        Name = name;
    }

    public virtual void SetGlobalOptions(MacroOptions options)
    {

    }

    public void LoadOptions()
    {
        if (string.IsNullOrEmpty(GroupFolder)) return;
        var filepath = Path.Combine(GroupFolder, "config.json");
        Options.Load(filepath, true);
    }

    public void SaveOptions()
    {
        if (string.IsNullOrEmpty(GroupFolder)) return;
        var filepath = Path.Combine(GroupFolder, "config.json");
        Options.Save(filepath);
    }

}
