using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IConfigurableProcessor
{
    public IMacroBase Macro { get; }

    public VariableCollection Options { get; }
    public VariableCollection Environments { get; }
    public VariableCollection Statistics { get; }

    public ServiceCollection Services { get; }
}
