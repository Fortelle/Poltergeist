using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Poltergeist.Automations.Structures.Parameters;

/// <summary>
/// Represents a thread-safe dictionary of key-value pairs.
/// </summary>
public class ParameterValueCollection : ObservableConcurrentDictionary<string, object?>
{
    /// <summary>
    /// Adds a new key-value pair to the dictionary if the key does not exist, or updates the value associated with the specified key by using the specified function if the key already exists.
    /// </summary>
    /// <typeparam name="T">The type of the value associated with the key.</typeparam>
    /// <param name="key">The key to be added or whose value should be updated.</param>
    /// <param name="addValue">The value to add if the key does not exist.</param>
    /// <param name="updateValueFactory">The function that generates the updated value for the key based on its current value.</param>
    /// <returns>The value that was added or updated.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public T AddOrUpdate<T>(string key, T addValue, Func<T, T> updateValueFactory)
    {
        if (TryAdd(key, addValue))
        {
            return addValue;
        }

        if (TryUpdate(key, updateValueFactory, out var newValue))
        {
            return newValue;
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Uses the specified functions to add a new key-value pair to the dictionary if the key does not exist, or to update the value associated with the specified key if the key already exists.
    /// </summary>
    /// <typeparam name="T">The type of the value associated with the key.</typeparam>
    /// <param name="key">The key to be added or whose value should be updated.</param>
    /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
    /// <param name="updateValueFactory">The function used to generate a new value for the existing key based on its current value.</param>
    /// <returns>The value that was added or updated.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public T AddOrUpdate<T>(string key, Func<T> addValueFactory, Func<T, T> updateValueFactory)
    {
        if (TryAdd(key, addValueFactory, out var addValue))
        {
            return addValue;
        }

        if (TryUpdate(key, updateValueFactory, out var updateValue))
        {
            return updateValue;
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Gets the value associated with the specified key in the dictionary and casts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the value should be cast.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key, cast to the specified type.</returns>
    /// <exception cref="KeyNotFoundException">The specified key does not exist in the dictionary.</exception>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public T Get<T>(string key)
    {
        return (T)Get(key)!;
    }

    /// <summary>
    /// Gets the value associated with the specified key, or adds the specified key-value pair to the dictionary if the key does not exists.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add.</typeparam>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <param name="addValue">The value to add to the dictionary if the key does not exist.</param>
    /// <returns>The value associated with the specified key if it exists; otherwise, <paramref name="addValue"/>.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public T GetOrAdd<T>(string key, T addValue)
    {
        if (TryGetValue<T>(key, out var oldValue))
        {
            return oldValue;
        }

        if (TryAdd(key, addValue))
        {
            return addValue;
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Gets the value associated with the specified key, or adds a new value generated by the specified factory function if the key does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the value to get or add.</typeparam>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <param name="addValueFactory">The function used to generate a value for the key. The function is invoked only if the key is not found.</param>
    /// <returns>The value associated with the specified key if it exists; otherwise, the newly added value.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public T GetOrAdd<T>(string key, Func<T> addValueFactory)
    {
        if (TryGetValue<T>(key, out var oldValue))
        {
            return oldValue;
        }

        if (TryAdd(key, addValueFactory, out var addValue))
        {
            return addValue;
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type to which the value should be cast.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key, or the <see langword="default"/> value for <see langword="T"/> if the key is not found.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public T? GetValueOrDefault<T>(string key)
    {
        if (TryGetValue<T>(key, out var value))
        {
            return value;
        }

        return default;
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type to which the value should be cast.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="defaultValue">The default value to return if the key is not found in the dictionary.</param>
    /// <returns>The value associated with the specified key, or the <see cref="defaultValue"/> if the key is not found.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public T? GetValueOrDefault<T>(string key, T? defaultValue)
    {
        if (TryGetValue<T>(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// Attempts to get the value associated with the specified definition in the dictionary.
    /// </summary>
    /// <param name="definition">The parameter definition containing the key and default value.</param>
    /// <returns>The value associated with <see cref="definition.Key"/>, or <see cref="definition.DefaultValue"/> if the key is not found.</returns>
    public object? GetValueOrDefault(IParameterDefinition definition)
    {
        if (TryGetValue(definition.Key, out var value))
        {
            return value;
        }

        return definition.DefaultValue;
    }

    /// <summary>
    /// Attempts to get the value associated with the specified definition in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type to which the value should be cast.</typeparam>
    /// <param name="definition">The parameter definition containing the key and default value.</param>
    /// <returns>The value associated with <see cref="definition.Key"/>, or <see cref="definition.DefaultValue"/> if the key is not found.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public T? GetValueOrDefault<T>(ParameterDefinition<T> definition)
    {
        if (TryGetValue<T>(definition.Key, out var value))
        {
            return value;
        }

        return (T?)definition.DefaultValue;
    }

    /// <summary>
    /// Attempts to get the value associated with the specified definition in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type to which the value should be cast.</typeparam>
    /// <param name="definition">The parameter definition containing the key.</param>
    /// <returns>The value associated with <see cref="definition.Key"/>, or <paramref name="defaultValue"/> if the key is not found.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public T? GetValueOrDefault<T>(ParameterDefinition<T> definition, T? defaultValue)
    {
        if (TryGetValue<T>(definition.Key, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// Attempts to add a key-value pair to the dictionary by using the specified function.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="addFactory">The function used to generate a new value for the key.</param>
    /// <returns><see langword="true"/> if the value was added successfully; <see langword="false"/> if the key already exists.</returns>
    public bool TryAdd<T>(string key, Func<T> addFactory)
    {
        return TryAdd(key, addFactory, out _);
    }

    /// <summary>
    /// Attempts to add a key-value pair to the dictionary by using the specified function.
    /// </summary>
    /// <typeparam name="T">The type of the value to add.</typeparam>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="addFactory">The function used to generate a new value for the key.</param>
    /// <param name="newValue">When this method returns, contains the new value generated by the add function if the value was added successfully; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the value was added successfully; <see langword="false"/> if the key already exists.</returns>
    public bool TryAdd<T>(string key, Func<T> addFactory, [MaybeNullWhen(false)] out T newValue)
    {
        if (Collection.ContainsKey(key))
        {
            newValue = default;
            return false;
        }

        var addValue = addFactory();
        if (TryAdd(key, addValue))
        {
            newValue = addValue;
            HasChanged = true;
            return true;
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the value to get.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key if the key is found and the value can be cast to type <typeparamref name="T"/>; otherwise, contains the default value for type <typeparamref name="T"/>.</param>
    /// <returns><see langword="true"/> if the key was found and the value can be cast to type <typeparamref name="T"/>; <see langword="false"/> if the key does not exist.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        if (!TryGetValue(key, out var oldValue))
        {
            value = default;
            return false;
        }

        value = (T)oldValue!;
        return true;
    }

    /// <summary>
    /// Attempts to update the value associated with the specified key in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the value to update.</typeparam>
    /// <param name="key">The key of the value to update.</param>
    /// <param name="value">The new value that replaces the existing value associated with the specified key.</param>
    /// <returns><see langword="true"/> if the value was successfully updated; <see langword="false"/> if the key does not exist.</returns>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public bool TryUpdate<T>(string key, Func<T, T> updateValueFactory)
    {
        return TryUpdate(key, updateValueFactory, out _);
    }

    /// <summary>
    /// Attempts to update the value associated with the specified key in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the value to update.</typeparam>
    /// <param name="key">The key of the value to update.</param>
    /// <param name="updateValueFactory">The function used to generate a new value for the key based on its current value.</param>
    /// <param name="newValue">When this method returns, contains the new value generated by the update function if the key is found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the value was successfully updated; <see langword="false"/> if the key does not exist.</returns>
    /// <remarks>If the value is updated successfully, the <see cref="HasChanged"/> property is set to <see langword="true"/>.</remarks>
    /// <exception cref="InvalidCastException">The existing value associated with the specified key cannot be cast to the specified type.</exception>
    /// <exception cref="NullReferenceException">The existing value associated with the specified key is <see langword="null"/> but the specified type is non-nullable.</exception>
    public bool TryUpdate<T>(string key, Func<T, T> updateValueFactory, [MaybeNullWhen(false)] out T newValue)
    {
        if (!TryGetValue(key, out var oldValue))
        {
            newValue = default;
            return false;
        }

        var tOldValue = (T)oldValue!;
        var updateValue = updateValueFactory(tOldValue);
        if (Collection.TryUpdate(key, updateValue, oldValue))
        {
            newValue = updateValue;
            HasChanged = true;
            return true;
        }

        throw new UnreachableException();
    }

}
