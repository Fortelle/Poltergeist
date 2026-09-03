namespace Poltergeist.Automations.Structures.Parameters;

public static class ParameterValueCollection_KeyValuePairExtensions
{
    extension(ParameterValueCollection collection)
    {
        public void Add<T>(KeyValuePair<string, T> keyValuePair)
        {
            collection.Add(keyValuePair.Key, keyValuePair.Value);
        }

        public void AddOrUpdate<T>(KeyValuePair<string, T> keyValuePair)
        {
            collection.AddOrUpdate(keyValuePair.Key, keyValuePair.Value);
        }

        public T GetOrAdd<T>(KeyValuePair<string, T> keyValuePair)
        {
            return collection.GetOrAdd(keyValuePair.Key, keyValuePair.Value);
        }

        public T? GetValueOrDefault<T>(KeyValuePair<string, T> keyValuePair)
        {
            return collection.GetValueOrDefault(keyValuePair.Key, keyValuePair.Value);
        }

        public bool TryAdd<T>(KeyValuePair<string, T> keyValuePair)
        {
            return collection.TryAdd(keyValuePair.Key, keyValuePair.Value);
        }

        public bool TryUpdate<T>(KeyValuePair<string, T> keyValuePair)
        {
            return collection.TryUpdate(keyValuePair.Key, keyValuePair.Value);
        }
    }
}
