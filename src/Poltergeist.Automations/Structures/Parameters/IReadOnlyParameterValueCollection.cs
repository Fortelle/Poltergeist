using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Structures.Parameters;

public interface IReadOnlyParameterValueCollection : IEnumerable<KeyValuePair<string, object?>>
{
    object? this[string key] { get; }
    T Get<T>(string key);
    T? GetValueOrDefault<T>(string key);
    T? GetValueOrDefault<T>(string key, T? defaultValue);
    object? GetValueOrDefault(IParameterDefinition definition);
    T? GetValueOrDefault<T>(ParameterDefinition<T> definition);
    T? GetValueOrDefault<T>(ParameterDefinition<T> definition, T? defaultValue);
    bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value);
    bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value);
    int Count { get; }
    bool ContainsKey(string key);
}
