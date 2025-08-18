using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IConfigurableProcessor
{
    ParameterValueCollection Options { get; }
    ParameterValueCollection Environments { get; }

    ServiceCollection Services { get; }
}
