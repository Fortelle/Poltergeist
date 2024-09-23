using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Cryptology;

namespace Poltergeist.Automations.Components;

public class LocalStorageService : MacroService
{
    private const string Filename = "LocalStorage.json";
    private ParameterValueCollection? GlobalStorage;
    private ParameterValueCollection? MacroStorage;

    public LocalStorageService(MacroProcessor processor, HookService hook) : base(processor)
    {
        hook.Register<ProcessorEndingHook>(OnEnding);
    }

    public T? Get<T>(string key, T defaultValue)
    {
        return GetMacroStorage().Get<T>(key, defaultValue);
    }

    public T? Get<T>(string key)
    {
        return GetMacroStorage().Get<T>(key);
    }

    public void Set(string key, object value)
    {
        GetMacroStorage().Set(key, value);
    }

    public void Remove(string key)
    {
        GetMacroStorage().Remove(key);
    }

    public T? GlobalGet<T>(string key, T defaultValue)
    {
        return GetGlobalStorage().Get<T>(key, defaultValue);
    }

    public T? GlobalGet<T>(string key)
    {
        return GetGlobalStorage().Get<T>(key);
    }

    public void GlobalSet(string key, object value)
    {
        GetGlobalStorage().Set(key, value);
    }

    public void GlobalRemove(string key)
    {
        GetGlobalStorage().Remove(key);
    }

    private ParameterValueCollection GetMacroStorage()
    {
        if (MacroStorage is null)
        {
            var privateFolder = Processor.Environments.Get<string>("private_folder") ?? throw new Exception($"Private folder is not set.");
            MacroStorage = new();
            var filepath = Path.Combine(privateFolder, Filename);
            if (File.Exists(filepath))
            {
                try
                {
                    SerializationUtil.JsonLoad<Dictionary<string, object?>>(filepath, out var dict);
                    foreach (var (key, value) in dict!)
                    {
                        MacroStorage.Add(key, value);
                    }
                    Logger.Debug($"Loaded json file \"{filepath}\".");
                }
                catch
                {
                    Logger.Warn($"Can not load json file \"{filepath}\".");
                }
            }
        }
        return MacroStorage;
    }
    private ParameterValueCollection GetGlobalStorage()
    {
        if (GlobalStorage is null)
        {
            var globalFolder = Processor.Environments.Get<string>("document_data_folder") ?? throw new Exception($"Global folder is not set.");
            GlobalStorage = new();
            var filepath = Path.Combine(globalFolder, Filename);
            if (File.Exists(filepath))
            {
                try
                {
                    SerializationUtil.JsonLoad<Dictionary<string, object?>>(filepath, out var dict);
                    foreach (var (key, value) in dict!)
                    {
                        GlobalStorage.Add(key, value);
                    }
                    Logger.Debug($"Loaded json file \"{filepath}\".");
                }
                catch
                {
                    Logger.Warn($"Can not load json file \"{filepath}\".");
                }
            }
        }
        return GlobalStorage;
    }

    private void OnEnding(ProcessorEndingHook hook)
    {
        if (MacroStorage is not null && (MacroStorage.Count == 0 || MacroStorage.Any(x => x.HasChanged)))
        {
            var privateFolder = Processor.Environments.Get<string>("private_folder");
            var filepath = Path.Combine(privateFolder!, Filename);
            var dict = MacroStorage.ToDictionary(x => x.Key, x => x.Value);
            SerializationUtil.JsonSave(filepath, dict);
        }
        if (GlobalStorage is not null && (GlobalStorage.Count == 0 || GlobalStorage.Any(x => x.HasChanged)))
        {
            var dataFolder = Processor.Environments.Get<string>("document_data_folder")!;
            var filepath = Path.Combine(dataFolder, Filename);
            var dict = GlobalStorage.ToDictionary(x => x.Key, x => x.Value);
            SerializationUtil.JsonSave(filepath, dict);
        }
    }
}
