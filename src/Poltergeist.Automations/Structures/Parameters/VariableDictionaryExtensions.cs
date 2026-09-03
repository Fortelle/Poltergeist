namespace Poltergeist.Automations.Structures.Parameters;

public static class VariableDictionaryExtensions
{
    extension(Dictionary<string, object?> dict)
    {
        public void Add<T>(KeyValuePair<string, T?> pair)
        {
            dict.Add(pair.Key, pair.Value);
        }
    }
}
