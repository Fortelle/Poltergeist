using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Structures.Parameters;

public interface IReadOnlyParameterValueCollection : IEnumerable<KeyValuePair<string, object?>>
{
    public object? this[string key] { get; }
    public T Get<T>(string key);
    public T? GetValueOrDefault<T>(string key);
    public T? GetValueOrDefault<T>(string key, T? defaultValue);
    public object? GetValueOrDefault(IParameterDefinition definition);
    public T? GetValueOrDefault<T>(ParameterDefinition<T> definition);
    public T? GetValueOrDefault<T>(ParameterDefinition<T> definition, T? defaultValue);
    public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value);
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value);
    public int Count { get; }
    public bool ContainsKey(string key);
}
