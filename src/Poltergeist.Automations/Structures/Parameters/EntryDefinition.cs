namespace Poltergeist.Automations.Structures.Parameters;

public class EntryDefinition<TValue> : IEntryDefinition<TValue>
{
    public string Key { get; set; }

    public EntryDefinition(string key)
    {
        Key = key;
    }

    public KeyValuePair<string, object?> WithValue(TValue? value)
    {
        return new KeyValuePair<string, object?>(Key, value);
    }
}
