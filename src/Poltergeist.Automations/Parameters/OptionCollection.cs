using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Parameters;

public class OptionCollection : KeyedCollection<string, IOptionItem>
{
    protected override string GetKeyForItem(IOptionItem item) => item.Key;

    public IReadOnlyDictionary<string, object?> ToDictionary() => this.ToDictionary(x => x.Key, x => x.Value);

    private string? Filepath { get; set; }

    public void Add<T>(string key, T value)
    {
        Add(new OptionItem<T>(key, value));
    }

    public T? Get<T>(string key, T? defaultValue = default)
    {
        var item = this.FirstOrDefault(x => x.Key == key);

        if (item is null)
        {
            return defaultValue;
        }
        else if (item is UndefinedOptionItem uoi)
        {
            if (uoi.Value is null)
            {
                return defaultValue;
            }

            try
            {
                return uoi.Value.ToObject<T>();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        else if (item.Value is T t)
        {
            return t;
        }
        else if (item.Default is T d)
        {
            return d;
        }
        else
        {
            return defaultValue;
        }
    }

    public void Set<T>(string key, T value)
    {
        var item = this.FirstOrDefault(x => x.Key == key);
        if (item != null)
        {
            item.Value = value;
        }
    }

    // todo: error handler
    public void Load(string path)
    {
        Filepath = path;

        if (!File.Exists(path))
        {
            return;
        }

        using var sr = new StreamReader(path);
        using var reader = new JsonTextReader(sr);
        var dict = JObject.Load(reader);

        var undefinedItems = new List<UndefinedOptionItem>();
        foreach (var (key, jtoken) in dict)
        {
            if (jtoken is null)
            {
                continue;
            }

            var existingItem = this.FirstOrDefault(x => x.Key == key);
            if(existingItem is not null)
            {
                try
                {
                    existingItem.Value = jtoken.ToObject(existingItem.BaseType);
                    existingItem.HasChanged = false;
                }
                catch (Exception)
                {
                }
            }
            else
            {
                undefinedItems.Add(new(key, jtoken));
            }
        }

        foreach (var item in undefinedItems)
        {
            this.Add(item);
        }
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(Filepath))
        {
            return;
        }

        if (this.Any(x => x.HasChanged))
        {
            SaveAs(Filepath);
        };
    }

    public void SaveAs(string path)
    {
        var dict = new Dictionary<string, object>();
        foreach (var item in this)
        {
            if (!item.IsDefault && item.Value is not null)
            {
                dict.Add(item.Key, item.Value);
            }
            item.HasChanged = false;
        }
        SerializationUtil.JsonSave(path, dict);
    }

}
