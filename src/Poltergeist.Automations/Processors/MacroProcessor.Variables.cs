using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Processors;

public partial class MacroProcessor
{
    public bool TryGetVariable<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        if (SessionStorage.TryGetValue(key, out value))
        {
            return true;
        }

        if (Options.TryGetValue(key, out value))
        {
            return true;
        }

        if (Environments.TryGetValue(key, out value))
        {
            return true;
        }

        return false;
    }

    public T? GetVariableOrDefault<T>(string key)
    {
        if (TryGetVariable<T>(key, out var value))
        {
            return value;
        }

        return default;
    }

    public T? GetVariableOrDefault<T>(string key, T? defaultValue)
    {
        if (TryGetVariable<T>(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    public T GetVariable<T>(string key)
    {
        if (TryGetVariable<T>(key, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"The key '{key}' was not found.");
    }
}
