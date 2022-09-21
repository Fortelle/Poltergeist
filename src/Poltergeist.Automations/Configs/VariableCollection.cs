using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poltergeist.Automations.Configs;

[JsonConverter(typeof(VariableCollectionConverter))]
public class VariableCollection : IEnumerable<VariableItem>
{
    private List<VariableItem> Items { get; } = new();
    public bool HasChanged => Items.Count > 0 && Items.Any(x => x.HasChanged);

    public IEnumerator<VariableItem> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public VariableCollection()
    {
    }

    public void Add(VariableItem item)
    {
        var i = Items.FindIndex(x => x.Key == item.Key);
        if (i > -1)
        {
            Items[i] = item;
        }
        else
        {
            Items.Add(item);
        }
    }

    public void Add(string key, object value)
    {
        Add(new(key, value));
    }

    public T Get<T>(string key)
    {
        return (T)Items.First(x => x.Key == key).Value;
    }

    public T Get<T>(string key, T defaultValue)
    {
        var i = Items.FindIndex(x => x.Key == key);
        if (i > -1)
        {
            return (T)Items[i].Value;
        }
        else
        {
            return defaultValue;
        }
    }

    public void Set(string key, object value)
    {
        var item = Items.Find(x => x.Key == key);
        item.Value = value;
        item.HasChanged = true;
    }

    public void Set<T>(string key, Func<T, T> action)
    {
        var item = Items.Find(x => x.Key == key);
        var value = (T)item.Value;
        item.Value = action(value);
        item.HasChanged = true;
    }

    public class VariableCollectionConverter : JsonConverter<VariableCollection>
    {
        public override VariableCollection ReadJson(JsonReader reader, Type objectType, VariableCollection existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dict = JObject.Load(reader);
            var mo = new VariableCollection();
            foreach (var item in existingValue)
            {
                if (dict.ContainsKey(item.Key))
                {
                    var value = dict[item.Key].ToObject(item.Type);
                    item.Value = value;
                }
                mo.Add(item);
            }
            return mo;
        }

        public override void WriteJson(JsonWriter writer, VariableCollection value, JsonSerializer serializer)
        {
            var dict = new JObject();
            foreach (var item in value.Items)
            {
                var token = JToken.FromObject(item.Value);
                dict.Add(item.Key, token);
                item.HasChanged = false;
            }

            dict.WriteTo(writer);
        }
    }

}
