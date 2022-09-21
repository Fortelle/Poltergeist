using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Macros;

[JsonObject(MemberSerialization.OptIn)]
public abstract class MacroBase : IMacroInitializer
{
    public string Name { get; set; }
    public string Title { get => string.IsNullOrEmpty(_title) ? Name : _title; set => _title = value; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string[] Tags { get; set; }

    [JsonProperty]
    public MacroOptions UserOptions { get; set; } = new();
    [JsonProperty]
    public VariableCollection Environments { get; set; } = new();

    public List<MacroMaintenance> Maintenances { get; } = new();
    public List<MacroModule> Modules { get; } = new();
    public MacroStorage Storage { get; } = new();

    public Action<MacroServiceCollection> Configure { get; set; }
    public Action<MacroProcessor> Ready { get; set; }
    public MacroGroup Group { get; set; }

    public string PrivateFolder { get; set; }
    public string SharedFolder { get; set; }
    private bool IsInitialized { get; set; }

    private string _title;

    private bool _requireAdmin;
    public bool RequireAdmin { get => _requireAdmin; set => _requireAdmin |= value; }

    public bool UseFile => !string.IsNullOrEmpty(PrivateFolder);

    protected internal virtual void InitProc() { }
    protected internal virtual void ConfigureProc(MacroServiceCollection services) { }
    protected internal virtual void ReadyProc(MacroProcessor processor) { }

    public MacroBase(string name)
    {
        Name = name;
    }

    public T As<T>() where T : MacroBase
    {
        return (T)this;
    }

    public void Initialize()
    {
        if (IsInitialized) return;

        if (UseFile)
        {
            Maintenances.Add(OpenLocalFolder);
        }

        Environments.Add(new("LastRunTime", default(DateTime)));
        Environments.Add(new("TotalRunCount", 0));
        Environments.Add(new("TotalRunTime", default(TimeSpan)));

        InitProc();

        foreach (var module in Modules)
        {
            module.OnMacroInitializing(this);
        }

        LoadOptions();

        IsInitialized = true;
    }

    public void LoadOptions()
    {
        if (IsInitialized) return;
        if (!UseFile) return;

        var path = Path.Combine(PrivateFolder, "config.json");
        if (File.Exists(path))
        {
            SerializationUtil.JsonPopulate(path, this);
        }
    }

    public void SaveOptions()
    {
        if (!IsInitialized) return;
        if (!UseFile) return;

        if (!UserOptions.HasChanged && !Environments.HasChanged) return;

        var path = Path.Combine(PrivateFolder, "config.json");
        SerializationUtil.JsonSave(path, this);
    }

    public void SetThumbnail(Bitmap image)
    {
        if (!UseFile) return;
        var path = Path.Combine(PrivateFolder, "thumbnail.png");
        image.Save(path);
    }

    private static readonly MacroMaintenance OpenLocalFolder = new()
    {
        Text = "Open macro folder",
        Execute = macro =>
        {
            if (!Directory.Exists(macro.PrivateFolder)) return;
            System.Diagnostics.Process.Start("explorer.exe", macro.PrivateFolder);
        },
    };

}

