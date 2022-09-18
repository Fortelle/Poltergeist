using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Poltergeist.Automations.Macros;

[JsonConverter(typeof(MacroOptionsConverter))]
public class MacroOptions : IEnumerable<OptionItem>
{
    private List<OptionItem> Items { get; } = new();
    public bool HasChanged => Items.Count > 0 && Items.Any(x => x.HasChanged);

    public IEnumerator<OptionItem> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public Dictionary<string, object> ToDictionary() => Items.ToDictionary(x => x.Key, x => x.Value);

    public MacroOptions()
    {
    }

    public void Add(OptionItem item)
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

    public class MacroOptionsConverter : JsonConverter<MacroOptions>
    {
        public override MacroOptions ReadJson(JsonReader reader, Type objectType, MacroOptions existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dict = JObject.Load(reader);
            var mo = new MacroOptions();
            foreach (var item in existingValue)
            {
                if (dict.ContainsKey(item.Key))
                {
                    var value = dict[item.Key].ToObject(item.Type);
                    item.Value = value;
                    item.SavedValueString = value.ToString();
                }
                mo.Add(item);
            }
            return mo;
        }

        public override void WriteJson(JsonWriter writer, MacroOptions value, JsonSerializer serializer)
        {
            var dict = new JObject();
            foreach (var item in value.Items)
            {
                var valueString = item.Value.ToString();
                var isDefault = valueString == item.DefaultValueString;
                var isSaved = valueString == (item.SavedValueString ?? item.DefaultValueString);

                if (!isDefault)
                {
                    var token = JToken.FromObject(item.Value);
                    dict.Add(item.Key, token);
                }
                if (!isSaved)
                {
                    item.SavedValueString = valueString;
                }
                item.HasChanged = false;
            }

            dict.WriteTo(writer);
        }
    }

}
