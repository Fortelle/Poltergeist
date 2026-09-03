using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Components.Storages;

public abstract class FileStorageService : MacroService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IncludeFields = true,
    };

    public SerializableParameterValueCollection Storage { get; set; } = new();

    public string FilePath { get; }

    private int Hash;

    protected FileStorageService(MacroProcessor processor, string folderKey) : base(processor)
    {
        if (!Processor.Environments.TryGetValue<string>(folderKey, out var folder))
        {
            throw new Exception($"{folderKey} is not set.");
        }

        FilePath = GetFilePath(folder);

        Load();

        Processor.GetService<HookService>().Register<ProcessorCompletedHook>((s, e) => Save(), priority: int.MinValue);
    }

    private void Load()
    {
        if (!File.Exists(FilePath))
        {
            return;
        }
        var dict = Load(FilePath, out Hash);
        foreach (var (key, value) in dict)
        {
            Storage.TryAdd(key, value);
        }
        Storage.HasChanged = false;
        Logger.Debug($"Loaded json file \"{FilePath}\".");
    }

    private void Save()
    {
        if (!Storage.HasChanged)
        {
            return;
        }

        var text = JsonSerializer.Serialize(Storage, JsonSerializerOptions);
        var hash = text.GetHashCode();
        if (text.GetHashCode() == Hash)
        {
            return;
        }
        else
        {
            Hash = hash;
        }

        var folder = Path.GetDirectoryName(FilePath);
        if (folder is not null && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(FilePath, text);
        Storage.HasChanged = false;
        Logger.Debug($"Saved json file \"{FilePath}\".");
    }

    public static Dictionary<string, JsonNode> Load(string path, out int hash)
    {
        var text = File.ReadAllText(path);
        hash = text.GetHashCode();
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(text, JsonSerializerOptions);
        return dict!;
    }

    public static string GetFilePath(string folder)
    {
        return Path.Combine(folder, "LocalStorage.json");
    }
}