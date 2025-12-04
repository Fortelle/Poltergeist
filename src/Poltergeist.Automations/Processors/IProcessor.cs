using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IProcessor
{
    string ProcessorId { get; }

    ParameterValueCollection Options { get; }
    ParameterValueCollection Environments { get; }
    ParameterValueCollection SessionStorage { get; }
    ParameterValueCollection OutputStorage { get; }
    ParameterValueCollection Report { get; }

    T GetVariable<T>(string key);
    T? GetVariableOrDefault<T>(string key);
    T? GetVariableOrDefault<T>(string key, T? defaultValue);
    bool TryGetVariable<T>(string key, [MaybeNullWhen(false)] out T value);
}
