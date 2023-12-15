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

    public T? Get<T>(string filename, FileStorageSource source = FileStorageSource.Macro) where T : class
    {
        var filepath = GetPath(filename, source);

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

    public T? Get<T>(string filename, Func<string, T> load, FileStorageSource source = FileStorageSource.Macro) where T : class
    {
        var filepath = GetPath(filename, source);

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

    public async Task<T?> GetAsync<T>(string filename, Func<string, Task<T>> load, FileStorageSource source = FileStorageSource.Macro) where T : class
    {
        var filepath = GetPath(filename, source);

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

    public void Set<T>(string filename, T item, FileStorageSource source = FileStorageSource.Macro)
    {
        var filepath = GetPath(filename, source);

        SerializationUtil.JsonSave(filepath, item);
        Logger.Debug($"File is saved to \"{filepath}\".");
    }

    public void Set<T>(string filename, Action<string> save, FileStorageSource source = FileStorageSource.Macro)
    {
        var filepath = GetPath(filename, source);

        save(filename);
        Logger.Debug($"File is saved to \"{filepath}\".");
    }

    public async Task SetAsync<T>(string filename, Func<string, Task> save, FileStorageSource source = FileStorageSource.Macro)
    {
        var filepath = GetPath(filename, source);

        await save(filename);
        Logger.Debug($"File is saved to \"{filepath}\".");
    }

    public string GetPath(string filename, FileStorageSource source = FileStorageSource.Macro)
    {
        string? root;
        switch (source)
        {
            case FileStorageSource.Macro:
                if (Processor.Macro.PrivateFolder is null)
                {
                    throw new Exception($"The property \"{nameof(Processor.Macro)}.{nameof(Processor.Macro.PrivateFolder)}\" is not set.");
                }
                root = Processor.Macro.PrivateFolder;
                break;
            case FileStorageSource.Group:
                if (Processor.Macro.Group is null)
                {
                    throw new Exception("The macro does not belong to any group.");
                }
                if (Processor.Macro.Group.GroupFolder is null)
                {
                    throw new Exception($"The property \"{nameof(Processor.Macro)}.{nameof(Processor.Macro.Group)}.{nameof(Processor.Macro.Group.GroupFolder)}\" is not set.");
                }
                root = Processor.Macro.Group.GroupFolder;
                break;
            case FileStorageSource.Global:
                if (!Processor.Environments.Contains("document_data_folder"))
                {
                    throw new Exception($"The environment variable \"document_data_folder\" does not exist.");
                }
                root = Processor.Environments.Get<string>("document_data_folder");
                break;
            default:
                throw new NotSupportedException();
        }

        var folder = Path.Combine(root, Foldername);
        Directory.CreateDirectory(folder);

        var filepath = Path.Combine(folder, filename);
        return filepath;
    }

}
