using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Components;

public class FileStorageService : MacroService
{
    private const string Foldername = "FileStorage";

    public FileStorageService(MacroProcessor processor) : base(processor)
    {
    }

    public T? Get<T>(string filename, bool isGlobal = false) where T : class
    {
        var filepath = GetPath(filename, isGlobal);

        if (!File.Exists(filepath))
        {
            Logger.Debug($"File \"{filepath}\" does not exist.");
            return null;
        }

        try
        {
            SerializationUtil.JsonLoad<T>(filepath, out var item);
            Logger.Debug($"Loaded json file \"{filepath}\".");
            return item;
        }
        catch
        {
            Logger.Warn($"Can not load json file \"{filepath}\".");
        }

        return null;
    }

    public T? Get<T>(string filename, Func<string, T> load, bool isGlobal = false) where T : class
    {
        var filepath = GetPath(filename, isGlobal);

        if (!File.Exists(filepath))
        {
            Logger.Debug($"File \"{filepath}\" does not exist.");
            return null;
        }

        try
        {
            var item = load(filepath);
            Logger.Debug($"Loaded file \"{filepath}\".");
            return item;
        }
        catch
        {
            Logger.Warn($"Can not load file \"{filepath}\".");
        }

        return null;
    }

    public async Task<T?> GetAsync<T>(string filename, Func<string, Task<T>> load, bool isGlobal = false) where T : class
    {
        var filepath = GetPath(filename, isGlobal);

        if (!File.Exists(filepath))
        {
            Logger.Debug($"File \"{filepath}\" does not exist.");
            return null;
        }

        try
        {
            var item = await load(filepath);
            Logger.Debug($"Loaded file \"{filepath}\".");
            return item;
        }
        catch
        {
            Logger.Warn($"Can not load file \"{filepath}\".");
        }

        return null;
    }

    public void Set<T>(string filename, T item, bool isGlobal = false)
    {
        var filepath = GetPath(filename, isGlobal);

        SerializationUtil.JsonSave(filepath, item);
        Logger.Debug($"File is saved to \"{filepath}\".");
    }

    public void Set<T>(string filename, Action<string> save, bool isGlobal = false)
    {
        var filepath = GetPath(filename, isGlobal);

        save(filename);
        Logger.Debug($"File is saved to \"{filepath}\".");
    }

    public async Task SetAsync<T>(string filename, Func<string, Task> save, bool isGlobal = false)
    {
        var filepath = GetPath(filename, isGlobal);

        await save(filename);
        Logger.Debug($"File is saved to \"{filepath}\".");
    }

    private string GetPath(string filename, bool isGlobal)
    {
        if (isGlobal)
        {
            var dataFolder = Processor.Environments.Get<string>("document_data_folder") ?? throw new Exception($"Global folder is not set.");
            return Path.Combine(dataFolder, Foldername, filename);
        }
        else
        {
            var privateFolder = Processor.Environments.Get<string>("private_folder") ?? throw new Exception($"Private folder is not set.");
            return Path.Combine(privateFolder, Foldername, filename);
        }
    }
}
