using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Cryptology;

namespace Poltergeist.Automations.Components;

public class StorageService : MacroService
{
    public StorageService(MacroProcessor processor) : base(processor)
    {
        Processor.GetService<HookService>().Register<ProcessorEndedHook>(e =>
        {
            ReleaseStorage(SessionStorage);
        });
    }

    private static ParameterValueCollection? _runtimeStorage;
#pragma warning disable CA1822 // Mark members as static
    public ParameterValueCollection RuntimeStorage => _runtimeStorage ??= new();
#pragma warning restore CA1822 // Mark members as static

    public ParameterValueCollection SessionStorage => Processor.SessionStorage;

    private  ParameterValueCollection? _localStorage;
    public ParameterValueCollection LocalStorage => _localStorage ??= CreateStorage("private_folder");

    private static ParameterValueCollection? _globalStorage;
    public ParameterValueCollection GlobalStorage => _globalStorage ??= CreateStorage("document_data_folder");

    private ParameterValueCollection CreateStorage(string folderKey)
    {
        if (!Processor.Environments.TryGetValue<string>(folderKey, out var folder))
        {
            throw new Exception($"{folderKey} is not set.");
        }

        var storage = new ParameterValueCollection();

        var filename = Path.Combine(folder, "LocalStorage.json");
        if (File.Exists(filename))
        {
            SerializationUtil.JsonLoad<Dictionary<string, object?>>(filename, out var dict);
            foreach (var (key, value) in dict!)
            {
                if (value is not null)
                {
                    storage.TryAdd(key, value);
                }
            }
            storage.HasChanged = false;
            Logger.Debug($"Loaded json file \"{filename}\".");
        }

        Processor.GetService<HookService>().Register<ProcessorEndedHook>(e =>
        {
            if (storage.HasChanged)
            {
                SerializationUtil.JsonSave(filename, storage.ToDictionary());
            }
            ReleaseStorage(storage);
        });

        return storage;
    }

    private void ReleaseStorage(ParameterValueCollection storage)
    {
        foreach (var value in storage.Values)
        {
            if (value is IDisposable idis)
            {
                try
                {
                    idis.Dispose();
                }
                catch (Exception exception)
                {
                    Logger?.Error(exception);
                }
            }
        }
        storage.Clear();
    }
}
