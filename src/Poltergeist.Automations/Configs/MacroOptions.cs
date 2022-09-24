using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Configs;

[JsonConverter(typeof(MacroOptionsConverter))]
public class MacroOptions : IEnumerable<IOptionItem>
{
    private List<IOptionItem> Items { get; } = new();
    public bool HasChanged => Items.Count > 0 && Items.Any(x => x.HasChanged);

    public IEnumerator<IOptionItem> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public Dictionary<string, object> ToDictionary() => Items.ToDictionary(x => x.Key, x => x.Value);

    public MacroOptions()
    {
    }
    
    public void Add(IOptionItem item)
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

    public void Add<T>(string key, T value)
    {
        Add(new OptionItem<T>(key, value));
    }

    public T Get<T>(string key)
    {
        return (T)Items.First(x => x.Key == key).Value;
    }

    public void Load(string path, bool keepUndefined = false)
    {
        if (!File.Exists(path)) return;

        using var sr = new StreamReader(path);
        using var reader = new JsonTextReader(sr);
        var dict = JObject.Load(reader);

        var undefinedItems = new List<UndefinedOptionItem>();
        foreach (var (key, jtoken) in dict)
        {
            var item = this.FirstOrDefault(x => x.Key == key);
            if(item != null)
            {
                var value = jtoken.ToObject(item.Type);
                item.Value = value;
            }
            else if (keepUndefined)
            {
                undefinedItems ??= new();
                undefinedItems.Add(new(key, jtoken));
            }
        }

        foreach (var item in undefinedItems)
        {
            Items.Add(item);
        }
        undefinedItems.Clear();
    }

    public void Save(string path)
    {
        SerializationUtil.JsonSave(path, this);
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
                if (item is UndefinedOptionItem || (!item.IsDefault && item.Value != null))
                {
                    var token = JToken.FromObject(item.Value);
                    dict.Add(item.Key, token);
                }
                item.HasChanged = false;
            }

            dict.WriteTo(writer);
        }
    }

}
