using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public interface IMacroBase
{
    public string Key { get; }

    public string Title { get; }

    public string? Category { get; }
    public string? Description { get; }
    public string[]? Details { get; }
    public string[]? Tags { get; }
    public OptionCollection UserOptions { get; }
    public ParameterCollection Statistics { get; }
    public ProcessHistoryCollection History { get; }

    public List<MacroAction> Actions { get; }
    public List<MacroModule> Modules { get; }
    public MacroStorage Storage { get; }

    public MacroGroup? Group { get; set; }
    public string? PrivateFolder { get; set; }
    public string? SharedFolder { get; set; }

    public bool RequiresAdmin { get; }
    public bool MinimizeApplication { get; }
    public bool IsAvailable { get; }

    public void Initialize();
    public void Load();
    public void ConfigureServices(ServiceCollection services, IConfigureProcessor processor);
    public void Process(MacroProcessor processor);
    public void ExecuteAction(MacroAction action, IReadOnlyDictionary<string, object?> options, IReadOnlyDictionary<string, object?> environments);

    public T As<T>() where T : MacroBase;

    public VariableCollection GetOptionCollection();
    public VariableCollection GetStatisticCollection();
}
