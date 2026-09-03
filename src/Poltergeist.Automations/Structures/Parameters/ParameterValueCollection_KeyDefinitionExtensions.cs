using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Structures.Parameters;

public static class ParameterValueCollection_KeyDefinitionExtensions
{
    extension(IReadOnlyParameterValueCollection collection)
    {
        public bool ContainsKey<T>(IEntryDefinition<T> keyDefinition)
        {
            return collection.ContainsKey(keyDefinition.Key);
        }

        public T Get<T>(IEntryDefinition<T> keyDefinition)
        {
            return collection.Get<T>(keyDefinition.Key);
        }

        public T? GetValueOrDefault<T>(IEntryDefinition<T> keyDefinition)
        {
            return collection.GetValueOrDefault<T>(keyDefinition.Key);
        }

        public T? GetValueOrDefault<T>(IEntryDefinition<T> keyDefinition, T? defaultValue)
        {
            return collection.GetValueOrDefault(keyDefinition.Key, defaultValue);
        }

        public bool TryGetValue<T>(IEntryDefinition<T> keyDefinition, [MaybeNullWhen(false)] out T value)
        {
            return collection.TryGetValue(keyDefinition.Key, out value);
        }
    }

    extension(ParameterValueCollection collection)
    {
        public void Add<T>(IEntryDefinition<T> keyDefinition, T addValue)
        {
            collection.Add(keyDefinition.Key, addValue);
        }

        public void AddOrUpdate<T>(IEntryDefinition<T> keyDefinition, T value)
        {
            collection.AddOrUpdate(keyDefinition.Key, value);
        }

        public T AddOrUpdate<T>(IEntryDefinition<T> keyDefinition, T addValue, Func<T, T> updateValueFactory)
        {
            return collection.AddOrUpdate(keyDefinition.Key, addValue, updateValueFactory);
        }

        public T AddOrUpdate<T>(IEntryDefinition<T> keyDefinition, Func<T> addValueFactory, Func<T, T> updateValueFactory)
        {
            return collection.AddOrUpdate(keyDefinition.Key, addValueFactory, updateValueFactory);
        }

        public T GetOrAdd<T>(IEntryDefinition<T> keyDefinition, T addValue)
        {
            return collection.GetOrAdd<T>(keyDefinition.Key, addValue);
        }

        public T GetOrAdd<T>(IEntryDefinition<T> keyDefinition, Func<T> addValueFactory)
        {
            return collection.GetOrAdd<T>(keyDefinition.Key, addValueFactory);
        }

        public bool TryAdd<T>(IEntryDefinition<T> keyDefinition, T addValue)
        {
            return collection.TryAdd(keyDefinition.Key, addValue);
        }

        public bool TryAdd<T>(IEntryDefinition<T> keyDefinition, Func<T> addFactory)
        {
            return collection.TryAdd(keyDefinition.Key, addFactory);
        }

        public bool TryAdd<T>(IEntryDefinition<T> keyDefinition, Func<T> addFactory, [MaybeNullWhen(false)] out T newValue)
        {
            return collection.TryAdd(keyDefinition.Key, addFactory, out newValue);
        }

        public bool TryUpdate<T>(IEntryDefinition<T> keyDefinition, T updateValue)
        {
            return collection.TryUpdate(keyDefinition.Key, updateValue);
        }

        public bool TryUpdate<T>(IEntryDefinition<T> keyDefinition, Func<T, T> updateValueFactory)
        {
            return collection.TryUpdate(keyDefinition.Key, updateValueFactory, out _);
        }

        public bool TryUpdate<T>(IEntryDefinition<T> keyDefinition, Func<T, T> updateValueFactory, [MaybeNullWhen(false)] out T newValue)
        {
            return collection.TryUpdate(keyDefinition.Key, updateValueFactory, out newValue);
        }

        public bool TryRemove<T>(IEntryDefinition<T> keyDefinition)
        {
            return collection.TryRemove(keyDefinition.Key);
        }

        public bool TryRemove<T>(IEntryDefinition<T> keyDefinition, [MaybeNullWhen(false)] out T value)
        {
            return collection.TryRemove(keyDefinition.Key, out value);
        }
    }
}
