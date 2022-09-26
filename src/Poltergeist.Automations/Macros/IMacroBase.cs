using System.Collections.Generic;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Macros;

public interface IMacroBase
{
    public string Name { get; }

    public string Title { get; }
    public string Category { get; }
    public string Description { get; }
    public string[] Tags { get; }
    public MacroOptions UserOptions { get; }
    public VariableCollection Statistics { get; }

    public List<MacroMaintenance> Maintenances { get; }
    public List<MacroModule> Modules { get; }
    public MacroStorage Storage { get; }

    public MacroGroup Group { get; set; }
    public string PrivateFolder { get; set; }
    public string SharedFolder { get; set; }

    public bool RequireAdmin { get; }
    public bool Available { get; }

    public void Initialize();
    public void Load();
    public void LoadOptions();
    public void SaveOptions();
    public void ConfigureServices(MacroServiceCollection services);
    public void Process(MacroProcessor processor);

    public T As<T>() where T : MacroBase;

}
