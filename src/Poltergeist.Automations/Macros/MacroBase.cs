using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Macros;

[JsonObject(MemberSerialization.OptIn)]
public abstract class MacroBase : IMacroBase, IMacroInitializer
{
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string[] Tags { get; set; }

    [JsonProperty]
    public MacroOptions UserOptions { get; set; } = new();

    [JsonProperty]
    public VariableCollection Statistics { get; set; } = new();

    public List<MacroMaintenance> Maintenances { get; } = new();
    public List<MacroModule> Modules { get; } = new();
    public MacroStorage Storage { get; } = new();

    public Action<MacroServiceCollection, IConfigureProcessor> Configure { get; set; }
    public Action<MacroProcessor> Process { get; set; }

    private bool IsInitialized { get; set; }
    private bool IsLoaded { get; set; }

    MacroGroup IMacroBase.Group { get; set; }

    private string _title;
    public string Title { get => string.IsNullOrEmpty(_title) ? Name : _title; set => _title = value; }

    private bool _requireAdmin;
    public bool RequireAdmin { get => _requireAdmin; set => _requireAdmin |= value; }

    private string _privateFolder;
    public string PrivateFolder => _privateFolder;
    string IMacroBase.PrivateFolder { get => _privateFolder; set => _privateFolder = value; }

    private string _sharedFolder;
    public string SharedFolder => _sharedFolder;
    string IMacroBase.SharedFolder { get => _sharedFolder; set => _sharedFolder = value; }

    private bool UseFile => !string.IsNullOrEmpty(PrivateFolder);

    public bool Available => IsInitialized;

    protected internal virtual void OnInitialize() { }
    protected internal virtual void OnLoad() { }
    protected internal virtual void OnConfigure(MacroServiceCollection services, IConfigureProcessor processor) { }
    protected internal virtual void OnProcess(MacroProcessor processor) { }

    public MacroBase(string name)
    {
        Name = name;
    }

    public T As<T>() where T : MacroBase
    {
        return (T)this;
    }

    void IMacroBase.Initialize()
    {
        if (IsInitialized) return;

        if (UseFile)
        {
            Maintenances.Add(OpenLocalFolder);
        }

        Statistics.Add(new("LastRunTime", default(DateTime)));
        Statistics.Add(new("TotalRunCount", 0));
        Statistics.Add(new("TotalRunTime", default(TimeSpan)));

        OnInitialize();

        foreach (var module in Modules)
        {
            module.OnMacroInitialize(this);
        }

        IsInitialized = true;
    }

    void IMacroBase.Load()
    {
        if (!IsInitialized) throw new InvalidOperationException();
        if (IsLoaded) return;

        if (UseFile)
        {
            ((IMacroBase)this).LoadOptions();
        }

        IsLoaded = true;
    }

    void IMacroBase.LoadOptions()
    {
        if (!IsInitialized) throw new InvalidOperationException();
        if (!UseFile) return;

        var path = Path.Combine(PrivateFolder, "config.json");
        if (File.Exists(path))
        {
            SerializationUtil.JsonPopulate(path, this);
        }
    }

    void IMacroBase.SaveOptions()
    {
        if (!IsInitialized) return;
        if (!UseFile) return;

        if (!UserOptions.HasChanged && !Statistics.HasChanged) return;

        var path = Path.Combine(PrivateFolder, "config.json");
        SerializationUtil.JsonSave(path, this);
    }

    void IMacroBase.ConfigureServices(MacroServiceCollection services, IConfigureProcessor processor)
    {
        Configure?.Invoke(services, processor);
        OnConfigure(services, processor);
    }

    void IMacroBase.Process(MacroProcessor processor)
    {
        Process?.Invoke(processor);
        OnProcess(processor);
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
