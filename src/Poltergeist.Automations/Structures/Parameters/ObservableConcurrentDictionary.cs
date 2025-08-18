using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Structures.Parameters;

/// <summary>
/// Represents a thread-safe dictionary that supports change detection.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
public class ObservableConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    protected readonly ConcurrentDictionary<TKey, TValue> Collection = new();

    private int _hasChanged = 0;
    /// <summary>
    /// Gets or sets a value indicating whether the dictionary has changes.
    /// </summary>
    /// <remarks>
    /// When the <see cref="ObservableConcurrentDictionary"/> is initialized with a collection initializer, this property is set to <see langword="true"/> as well.
    /// </remarks>
    public bool HasChanged
    {
        get => Interlocked.CompareExchange(ref _hasChanged, 0, 0) == 1;
        set => Interlocked.Exchange(ref _hasChanged, value ? 1 : 0);
    }

    /// <summary>
    /// Gets the number of key-value pairs contained in the dictionary.
    /// </summary>
    public int Count => Collection.Count;

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">The specified key does not exist in the dictionary.</exception>
    public TValue this[TKey key]
    {
        get => Get(key);
        set => AddOrUpdate(key, value);
    }

    /// <summary>
    /// Adds the specified key-value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
    // This method is necessary for the collection initializer.
    public void Add(TKey key, TValue value)
    {
        if (!TryAdd(key, value))
        {
            throw new ArgumentException("An element with the same key already exists.");
        }
    }

    /// <summary>
    /// Adds a new key-value pair to the dictionary if the key does not exist, or updates the value associated with the specified key if the key already exists.
    /// </summary>
    /// <param name="key">The key to be added or whose value should be updated.</param>
    /// <param name="value">The value to be added or updated.</param>
    /// <returns>The value of <paramref name="value"/>.</returns>
    public TValue AddOrUpdate(TKey key, TValue value)
    {
        if (TryAdd(key, value))
        {
            return value;
        }

        if (TryUpdate(key, value))
        {
            return value;
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Adds a new key-value pair to the dictionary if the key does not exist, or updates the value associated with the specified key by using the specified function if the key already exists.
    /// </summary>
    /// <param name="key">The key to be added or whose value should be updated.</param>
    /// <param name="addValue">The value to add if the key does not exist.</param>
    /// <param name="updateValueFactory">The function that generates the updated value for the key based on its current value.</param>
    /// <returns>The value that was added or updated.</returns>
    public TValue AddOrUpdate(TKey key, TValue addValue, Func<TValue, TValue> updateValueFactory)
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
    /// <param name="key">The key to be added or whose value should be updated.</param>
    /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
    /// <param name="updateValueFactory">The function used to generate a new value for the existing key based on its current value.</param>
    /// <returns>The value that was added or updated.</returns>
    public TValue AddOrUpdate(TKey key, Func<TValue> addValueFactory, Func<TValue, TValue> updateValueFactory)
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
    /// Gets the value associated with the specified key in the dictionary.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">The specified key does not exist in the dictionary.</exception>
    public TValue Get(TKey key)
    {
        if (!TryGetValue(key, out var value))
        {
            throw new KeyNotFoundException($"The key '{key}' was not found in the dictionary.");
        }

        return value;
    }

    /// <summary>
    /// Gets the value associated with the specified key, or adds the specified key-value pair to the dictionary if the key does not exists.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <param name="addValue">The value to add to the dictionary if the key does not exist.</param>
    /// <returns>The value associated with the specified key if it exists; otherwise, <paramref name="addValue"/>.</returns>
    public TValue GetOrAdd(TKey key, TValue addValue)
    {
        if (TryGetValue(key, out var oldValue))
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
    /// Gets the value associated with the specified key, or adds a new value generated by the specified function if the key does not exist.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <param name="addValueFactory">The function used to generate a value for the key. The function is invoked only if the key is not found.</param>
    /// <returns>The value associated with the specified key if it exists; otherwise, the newly added value.</returns>
    public TValue GetOrAdd(TKey key, Func<TValue> addValueFactory)
    {
        if (TryGetValue(key, out var oldValue))
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
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key, or <see langword="null"/> if the key is not found.</returns>
    public TValue? GetValueOrDefault(TKey key)
    {
        return Collection.GetValueOrDefault(key);
    }

    /// <summary>
    /// Attempts to add the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <returns><see langword="true" /> if the key-value pair was added successfully; <see langword="false" /> if the key already exists.</returns>
    /// <remarks>If the value is added successfully, the <see cref="HasChanged"/> property is set to <see langword="true"/>.</remarks>
    public bool TryAdd(TKey key, TValue value)
    {
        if (Collection.TryAdd(key, value))
        {
            HasChanged = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to add a key-value pair to the dictionary by using the specified function.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="addFactory">The function used to generate a new value for the key.</param>
    /// <returns><see langword="true"/> if the value was added successfully; <see langword="false"/> if the key already exists.</returns>
    public bool TryAdd(TKey key, Func<TValue> addFactory)
    {
        return TryAdd(key, addFactory, out _);
    }

    /// <summary>
    /// Attempts to add a key-value pair to the dictionary by using the specified function.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="addFactory">The function used to generate a new value for the key.</param>
    /// <param name="newValue">When this method returns, contains the new value generated by the add function if the value was added successfully; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the value was added successfully; <see langword="false"/> if the key already exists.</returns>
    public bool TryAdd(TKey key, Func<TValue> addFactory, [MaybeNullWhen(false)] out TValue newValue)
    {
        if (Collection.ContainsKey(key))
        {
            newValue = default;
            return false;
        }

        var addValue = addFactory();
        if (Collection.TryAdd(key, addValue))
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
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key if the key is found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the key was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return Collection.TryGetValue(key, out value);
    }

    /// <summary>
    /// Attempts to update the value associated with the specified key in the dictionary.
    /// </summary>
    /// <param name="key">The key of the value to update.</param>
    /// <param name="value">The new value that replaces the existing value associated with the specified key.</param>
    /// <returns><see langword="true"/> if the value was successfully updated; <see langword="false"/> if the key does not exist.</returns>
    /// <remarks>If the value is updated successfully, the <see cref="HasChanged"/> property is set to <see langword="true"/>.</remarks>
    public bool TryUpdate(TKey key, TValue value)
    {
        if (!Collection.TryGetValue(key, out var oldValue))
        {
            return false;
        }

        if (Collection.TryUpdate(key, value, oldValue))
        {
            HasChanged = true;
            return true;
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Attempts to update the value associated with the specified key in the dictionary.
    /// </summary>
    /// <param name="key">The key of the value to update.</param>
    /// <param name="updateValueFactory">The function used to generate a new value for the key based on its current value.</param>
    /// <returns><see langword="true"/> if the value was successfully updated; <see langword="false"/> if the key does not exist.</returns>
    public bool TryUpdate(TKey key, Func<TValue, TValue> updateValueFactory)
    {
        return TryUpdate(key, updateValueFactory, out _);
    }

    /// <summary>
    /// Attempts to update the value associated with the specified key in the dictionary.
    /// </summary>
    /// <param name="key">The key of the value to update.</param>
    /// <param name="updateValueFactory">The function used to generate a new value for the key based on its current value.</param>
    /// <param name="newValue">When this method returns, contains the new value generated by the update function if the key is found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the value was successfully updated; <see langword="false"/> if the key does not exist.</returns>
    /// <remarks>If the value is updated successfully, the <see cref="HasChanged"/> property is set to <see langword="true"/>.</remarks>
    public bool TryUpdate(TKey key, Func<TValue, TValue> updateValueFactory, [MaybeNullWhen(false)] out TValue newValue)
    {
        if (!Collection.TryGetValue(key, out var oldValue))
        {
            newValue = default;
            return false;
        }

        var updateValue = updateValueFactory(oldValue);
        if (Collection.TryUpdate(key, updateValue, oldValue))
        {
            newValue = updateValue;
            HasChanged = true;
            return true;
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Attempts to remove the value associated with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns><see langword="true"/> if the value was successfully removed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>If the element is successfully removed, the <see cref="HasChanged"/> property is set to <see langword="true"/>.</remarks>
    public bool TryRemove(TKey key)
    {
        if (Collection.TryRemove(key, out _))
        {
            HasChanged = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to remove the value associated with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key if it was found and removed; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the value was successfully removed; otherwise, <see langword="false"/>.</returns>
    /// <remarks>If the element is successfully removed, the <see cref="HasChanged"/> property is set to <see langword="true"/>.</remarks>
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (Collection.TryRemove(key, out value))
        {
            HasChanged = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes all elements from the dictionary.
    /// </summary>
    /// <remarks>
    /// If any elements are removed, the <see cref="HasChanged"/> property is set to <see langword="true"/>.
    /// </remarks>
    public void Clear()
    {
        if (Collection.IsEmpty)
        {
            return;
        }

        Collection.Clear();

        HasChanged = true;
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
    public bool ContainsKey(TKey key)
    {
        return Collection.ContainsKey(key);
    }

    /// <summary>
    /// Gets a collection that contains the keys in the dictionary.
    /// </summary>
    public ICollection<TKey> Keys => Collection.Keys;

    /// <summary>
    /// Gets a collection that contains the values in the dictionary.
    /// </summary>
    public ICollection<TValue> Values => Collection.Values;

    /// <summary>
    /// Creates a new dictionary representation of the <see cref="ObservableConcurrentDictionary"/>.
    /// </summary>
    public Dictionary<TKey, TValue> ToDictionary()
    {
        return Collection.ToDictionary();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Collection.GetEnumerator();
    bool IDictionary<TKey, TValue>.Remove(TKey key) => TryRemove(key);
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)Collection).CopyTo(array, arrayIndex);
    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => TryRemove(item.Key);
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
