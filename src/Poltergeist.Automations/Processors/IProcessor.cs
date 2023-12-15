using Poltergeist.Automations.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IProcessor
{
    public string? Comment { get; set; }

    public VariableCollection Options { get; }
    public VariableCollection Environments { get; }
    public VariableCollection Statistics { get; }

    public T GetService<T>() where T : class;
}
