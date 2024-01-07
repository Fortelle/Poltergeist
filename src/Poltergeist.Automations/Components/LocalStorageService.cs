using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Parameters;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Components;

public class LocalStorageService : MacroService
{
    private const string Filename = "LocalStorage.json";

    private VariableCollection? GlobalStorage { get; set; }
    private VariableCollection? GroupStorage { get; set; }
    private VariableCollection? MacroStorage { get; set; }

    public LocalStorageService(MacroProcessor processor, HookService hook) : base(processor)
    {
        hook.Register<ProcessorEndingHook>(OnEnding);
    }

    public T Get<T>(string key, T defaultValue, ParameterSource source = ParameterSource.Macro)
    {
        var storage = GetStorage(source);
        if (storage.Contains(key))
        {
            return storage.Get<T>(key);
        }
        else
        {
            return defaultValue;
        }
    }

    public T Get<T>(string key, ParameterSource source = ParameterSource.Macro)
    {
        return Get<T>(key, default, source);
    }

    public void Set<T>(string key, T value, ParameterSource source = ParameterSource.Macro)
    {
        var storage = GetStorage(source);
        storage.Set(key, value, source);
    }

    public void Remove(string key, ParameterSource source = ParameterSource.Macro)
    {
        var storage = GetStorage(source);
        if (storage.Contains(key))
        {
            storage.Remove(key);
        }
    }

    private VariableCollection GetStorage(ParameterSource scope)
    {
        switch (scope)
        {
            case ParameterSource.Macro:
                if(MacroStorage is null)
                {
                    if (Processor.Macro.PrivateFolder is null)
                    {
                        throw new Exception($"The property \"{nameof(Processor.Macro)}.{nameof(Processor.Macro.PrivateFolder)}\" is not set.");
                    }
                    MacroStorage = new();
                    var filepath = Path.Combine(Processor.Macro.PrivateFolder, Filename);
                    if(File.Exists(filepath))
                    {
                        try
                        {
                            SerializationUtil.JsonLoad<Dictionary<string, object?>>(filepath, out var dict);
                            MacroStorage.AddRange(dict, ParameterSource.Macro);
                            Logger.Debug($"Loaded json file \"{filepath}\".");
                        }
                        catch
                        {
                            Logger.Warn($"Can not load json file \"{filepath}\".");
                        }
                    }
                }
                return MacroStorage;
            case ParameterSource.Group:
                if (GroupStorage is null)
                {
                    if (Processor.Macro.Group is null)
                    {
                        throw new Exception("The macro does not belong to any group.");
                    }
                    if (Processor.Macro.Group.GroupFolder is null)
                    {
                        throw new Exception($"The property \"{nameof(Processor.Macro)}.{nameof(Processor.Macro.Group)}.{nameof(Processor.Macro.Group.GroupFolder)}\" is not set.");
                    }
                    GroupStorage = new();
                    var filepath = Path.Combine(Processor.Macro.Group.GroupFolder, Filename);
                    if (File.Exists(filepath))
                    {
                        try
                        {
                            SerializationUtil.JsonLoad<Dictionary<string, object?>>(filepath, out var dict);
                            GroupStorage.AddRange(dict, ParameterSource.Group);
                            Logger.Debug($"Loaded json file \"{filepath}\".");
                        }
                        catch
                        {
                            Logger.Warn($"Can not load json file \"{filepath}\".");
                        }
                    }
                }
                return GroupStorage;
            case ParameterSource.Global:
                if (GlobalStorage is null)
                {
                    if (!Processor.Environments.Contains("document_data_folder"))
                    {
                        throw new Exception($"The environment variable \"document_data_folder\" does not exist.");
                    }
                    GlobalStorage = new();
                    var filepath = Path.Combine(Processor.Environments.Get<string>("document_data_folder"), Filename);
                    if (File.Exists(filepath))
                    {
                        try
                        {
                            SerializationUtil.JsonLoad<Dictionary<string, object?>>(filepath, out var dict);
                            GlobalStorage.AddRange(dict, ParameterSource.Global);
                            Logger.Debug($"Loaded json file \"{filepath}\".");
                        }
                        catch
                        {
                            Logger.Warn($"Can not load json file \"{filepath}\".");
                        }
                    }
                }
                return GlobalStorage;
            default:
                throw new NotSupportedException();
        }
    }

    private void OnEnding()
    {
        if (MacroStorage is not null && (MacroStorage.Count == 0 || MacroStorage.Any(x => x.HasChanged)))
        {
            var filepath = Path.Combine(Processor.Macro.PrivateFolder!, Filename);
            SerializationUtil.JsonSave(filepath, MacroStorage.ToValueDictionary());
        }
        if (GroupStorage is not null && (GroupStorage.Count == 0 || GroupStorage.Any(x => x.HasChanged)))
        {
            var filepath = Path.Combine(Processor.Macro.Group!.GroupFolder!, Filename);
            SerializationUtil.JsonSave(filepath, GroupStorage.ToValueDictionary());
        }
        if (GlobalStorage is not null && (GlobalStorage.Count == 0 || GlobalStorage.Any(x => x.HasChanged)))
        {
            var filepath = Path.Combine(Processor.Environments.Get<string>("document_data_folder"), Filename);
            SerializationUtil.JsonSave(filepath, GlobalStorage.ToValueDictionary());
        }
    }
}
