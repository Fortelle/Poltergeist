using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Parameters;

public class ParameterCollection : KeyedCollection<string, IParameterEntry>
{
    protected override string GetKeyForItem(IParameterEntry item) => item.Key;

    private string? Filepath { get; set; }

    public ParameterCollection()
    {
    }

    public void Set<T>(string key, T value)
    {
        if (this.Contains(key))
        {
            this[key].Value = value;
            this[key].HasChanged = true;
        }
        else
        {
            this.Add(new ParameterEntry<T>(key, value)
            {
                HasChanged = true
            });
        }
    }

    public void Update<T>(string key, T value)
    {
        this[key].Value = value;
        this[key].HasChanged = true;
    }

    public void Load(string path)
    {
        Filepath = path;

        if (!File.Exists(Filepath))
        {
            return;
        }

        using var sr = new StreamReader(Filepath);
        using var reader = new JsonTextReader(sr);
        var dict = JObject.Load(reader);

        foreach (var (key, jtoken) in dict)
        {
            if (jtoken is null)
            {
                continue;
            }

            var existingItem = this.FirstOrDefault(x => x.Key == key);
            if (existingItem is null)
            {
                continue;
            }

            try
            {
                existingItem.Value = jtoken.ToObject(existingItem.BaseType)!;
                existingItem.HasChanged = false;
            }
            catch (Exception)
            {
            }
        }
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(Filepath))
        {
            return;
        }

        if(Items.Any(x => x.HasChanged))
        {
            SaveAs(Filepath);
            foreach(var item in this)
            {
                item.HasChanged = false;
            }
        };
    }

    public void SaveAs(string path)
    {
        var dict = Items.ToDictionary(x => x.Key, x => x.Value);
        SerializationUtil.JsonSave(path, dict);
    }

    public IReadOnlyDictionary<string, object?> ToVariableDictionary()
    {
        return Items.ToDictionary(x => x.Key, x => x.Value);
    }
}
