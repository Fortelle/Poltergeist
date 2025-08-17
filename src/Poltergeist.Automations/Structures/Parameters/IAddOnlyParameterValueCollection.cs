using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Structures.Parameters;

public interface IAddOnlyParameterValueCollection : IReadOnlyParameterValueCollection
{
    T GetOrAdd<T>(string key, T addValue);
    T GetOrAdd<T>(string key, Func<T> addValueFactory);
    bool TryAdd<T>(string key, Func<T> addFactory);
    bool TryAdd<T>(string key, Func<T> addFactory, [MaybeNullWhen(false)] out T newValue);
}
