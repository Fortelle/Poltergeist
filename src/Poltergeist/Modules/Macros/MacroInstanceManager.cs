using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic.FileIO;
using Poltergeist.Automations.Structures;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroInstanceManager : ServiceBase
{
    public const string DefaultIconUri = @"ms-appx:///Poltergeist/Assets/macro_16px.png";
    private const int SettingsSaveDelaySeconds = 30;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    private readonly List<MacroInstance> MacroInstances = new();

    private readonly MacroTemplateManager TemplateManager;

    public IEnumerable<MacroInstance> GetInstances() => MacroInstances.AsEnumerable();

    public IEnumerable<T> GetInstances<T>() where T : MacroInstance => MacroInstances.OfType<T>();

    private bool HasChanged;

    private Debouncer? SaveDebouncer;

    public MacroInstanceManager(
        AppEventService eventService,
        MacroTemplateManager templateManager
        )
    {
        TemplateManager = templateManager;

        eventService.Subscribe<AppContentLoadingEvent>(OnAppContentLoading, new() { Priority = 100 });
        eventService.Subscribe<AppExitingEvent>(OnAppExiting);
    }

    public MacroInstance? GetInstance(string instanceIdentifier)
    {
        foreach (var instance in MacroInstances)
        {
            if (instance.InstanceId.Equals(instanceIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                return instance;
            }
            if (instance.Properties?.Key is string key && key.Equals(instanceIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                return instance;
            }
        }

        return null;
    }

    public void AddInstance(MacroInstance instance, bool withChange = false)
    {
        if (instance.IsPersistent)
        {
            instance.PrivateFolder ??= Path.Combine(PoltergeistApplication.Paths.MacroFolder, instance.InstanceId);
        }

        MacroInstances.Add(instance);

        if (withChange)
        {
            PoltergeistApplication.GetService<AppEventService>().Publish(new MacroInstanceCollectionChangedEvent()
            {
                NewItems = [instance],
            });

            HasChanged = true;
            SaveProperties();
        }

        Logger.Trace($"Added macro instance '{instance.InstanceId}'.");
    }

    public bool RemoveInstance(MacroInstance instance, bool removeFiles = false, bool withChanged = false)
    {
        if (!MacroInstances.Contains(instance))
        {
            return false;
        }

        MacroInstances.Remove(instance);

        if (removeFiles && !string.IsNullOrEmpty(instance.PrivateFolder) && Directory.Exists(instance.PrivateFolder))
        {
            try
            {
                FileSystem.DeleteDirectory(instance.PrivateFolder, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                Logger.Trace($"Deleted folder '{instance.PrivateFolder}'.");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to delete folder '{instance.PrivateFolder}': {ex.Message}");
            }
        }

        if (withChanged)
        {
            PoltergeistApplication.GetService<AppEventService>().Publish(new MacroInstanceCollectionChangedEvent()
            {
                OldItems = [instance],
            });

            HasChanged = true;
            SaveProperties();
        }

        Logger.Trace($"Removed macro instance '{instance.InstanceId}'.");

        return true;
    }

    public void LoadProperties()
    {
        var savePath = PoltergeistApplication.Paths.MacroInstancesFile;
        if (!File.Exists(savePath))
        {
            Logger.Trace($"Skipped loading macro instances: File does not exist.", new
            {
                Path = savePath,
            });
            return;
        }

        try
        {
            using var fs = File.Open(savePath, FileMode.Open, FileAccess.Read);
            var saveItems = JsonSerializer.Deserialize<MacroInstancePropertiesSaveItem[]>(fs, JsonSerializerOptions)!;
            foreach (var saveItem in saveItems)
            {
                var template = TemplateManager.GetTemplate(saveItem.TemplateKey);
                var instance = new MacroInstance(saveItem.TemplateKey, saveItem.InstanceId)
                {
                    IsUserCreated = true,
                    IsPersistent = true,
                    Template = template,
                    Properties = saveItem.Properties,
                };
                AddInstance(instance, withChange: false);
            }
        }
        catch (Exception ex)
        {
            Logger.Trace($"Failed to load macro instances: {ex.Message}", new
            {
                Path = savePath,
            });
        }
    }

    public void UpdateProperties(MacroInstance instance, Action<MacroInstanceProperties> action)
    {
        if (instance.Properties is null)
        {
            return;
        }

        action.Invoke(instance.Properties);

        PoltergeistApplication.GetService<AppEventService>().Publish(new MacroInstancePropertyChangedEvent(instance));

        HasChanged = true;

        if (instance.IsPersistent)
        {
            SaveProperties();
        }
    }

    public void SaveProperties(bool forceExecute = false)
    {
        if (forceExecute && SaveDebouncer is null)
        {
            SavePropertiesInternal();
            return;
        }
        else
        {
            SaveDebouncer ??= new(SavePropertiesInternal, TimeSpan.FromSeconds(SettingsSaveDelaySeconds));
            SaveDebouncer.Trigger(forceExecute);
        }
    }
    
    private void SavePropertiesInternal()
    {
        if (!HasChanged)
        {
            return;
        }

        var savePath = PoltergeistApplication.Paths.MacroInstancesFile;
        var saveItems = MacroInstances
            .Where(x => x.IsPersistent)
            .Where(x => x.Properties is not null)
            .Select(x => new MacroInstancePropertiesSaveItem()
            {
                TemplateKey = x.TemplateKey,
                InstanceId = x.InstanceId,
                Properties = x.Properties!,
            })
            .ToArray();

        try
        {
            var jsonText = JsonSerializer.Serialize(saveItems, JsonSerializerOptions);

            var folder = Path.GetDirectoryName(savePath);
            if (folder is not null && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText(savePath, jsonText);

            HasChanged = false;
            Logger.Trace($"Saved macro instances file '{savePath}'.");
            if (PoltergeistApplication.Current.IsDevelopment)
            {
                PoltergeistApplication.ShowTeachingTip($"Saved macro instances");
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to save macro instances file '{savePath}': {ex.Message}");
            if (PoltergeistApplication.Current.IsDevelopment)
            {
                PoltergeistApplication.ShowTeachingTip($"Failed to save macro instances");
            }
        }
    }

    private void OnAppContentLoading(AppContentLoadingEvent _)
    {
        LoadProperties();
    }

    private void OnAppExiting(AppExitingEvent _)
    {
        SaveProperties(true);
    }


    private class MacroInstancePropertiesSaveItem
    {
        public required string TemplateKey { get; init; }
        public required string InstanceId { get; init; }
        public required MacroInstanceProperties Properties { get; init; }
    }
}
