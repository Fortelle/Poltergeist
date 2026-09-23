using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Components.Storages;

public class LocalStorageService : FileStorageService
{
    public static readonly EntryDefinition<string> FolderDefinition = new("private_folder");

    public LocalStorageService(MacroProcessor processor) : base(processor, FolderDefinition.Key)
    {
    }

    public static SerializableParameterValueCollection Load(IReadOnlyParameterValueCollection environments)
    {
        if (!environments.TryGetValue(FolderDefinition, out var folder))
        {
            throw new Exception($"{FolderDefinition.Key} is not set.");
        }

        var filepath = GetFilePath(folder);
        var dict = Load(filepath, out _);
        var storage = new SerializableParameterValueCollection();
        foreach (var (key, value) in dict)
        {
            storage.TryAdd(key, value);
        }
        storage.HasChanged = false;
        return storage;
    }
}
