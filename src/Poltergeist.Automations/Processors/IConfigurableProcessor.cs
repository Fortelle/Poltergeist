using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IConfigurableProcessor
{
    public ParameterValueCollection Options { get; }
    public ParameterValueCollection Environments { get; }
    public ParameterValueCollection Statistics { get; }

    public ServiceCollection Services { get; }
}
