using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IWshRuntimeLibrary;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Interactions;
using Poltergeist.Modules.Macros;
using Poltergeist.Modules.Navigation;
using Poltergeist.Modules.Settings;

namespace Poltergeist.UI.Pages.Home;

public partial class MacroBrowserViewModel : ObservableRecipient, IDisposable
{
    public const string LastSortIndexKey = "last_sort_index";

    [ObservableProperty]
    public partial MacroInstance[]? Macros { get; set; }

    private int SortIndex;
    private bool disposedValue;

    public MacroBrowserViewModel()
    {
        SortIndex = App.GetService<AppSettingsService>().InternalSettings.Get<int>(LastSortIndexKey);

        RefreshMacroList();

        App.GetService<AppEventService>().Subscribe<MacroInstanceCollectionChangedEvent>(OnMacroInstanceCollectionChanged);
        App.GetService<AppEventService>().Subscribe<MacroInstancePropertyChangedEvent>(OnMacroInstancePropertyChanged);
    }

    public void RefreshMacroList()
    {
        var instanceManager = App.GetService<MacroInstanceManager>();
        var macros = instanceManager.GetInstances();
        
        macros = SortIndex switch
        {
            1 => macros.OrderBy(x => x.Title),
            -1 => macros.OrderByDescending(x => x.Title),
            3 => macros.OrderBy(x => x.Properties?.RunCount is null).ThenBy(x => x.Properties?.RunCount),
            -3 => macros.OrderBy(x => x.Properties?.RunCount is null).ThenByDescending(x => x.Properties?.RunCount),
            4 => macros.OrderBy(x => x.Properties?.LastRunTime is null).ThenBy(x => x.Properties?.LastRunTime),
            -4 => macros.OrderBy(x => x.Properties?.LastRunTime is null).ThenByDescending(x => x.Properties?.LastRunTime),
            _ => macros,
        };

        macros = macros.OrderByDescending(x => x.Properties?.IsFavorite);

        Macros = macros.ToArray();
    }

    [RelayCommand]
    private static async Task NewInstance()
    {
        var templateManager = App.GetService<MacroTemplateManager>();
        var templates = templateManager.Templates.ToDictionary(x => x.Key, x => x.Title);
        var editor = new MacroEditor(templates);

        var contentDialog = new ContentDialogModel()
        {
            Title = App.Localize($"Poltergeist/Home/NewInstanceDialog_Title"),
            Content = editor,
            Valid = () => editor.SelectedTemplateKey is null ? "" : null
        };

        await DialogService.ShowAsync(contentDialog);

        if (contentDialog.Result == DialogResult.Cancel)
        {
            return;
        }

        if (string.IsNullOrEmpty(editor.SelectedTemplateKey))
        {
            return;
        }

        var properties = new MacroInstanceProperties()
        {
            CreatedTime = DateTime.Now,
        };
        if (await EditInstancePropertiesInternal(properties, null) == false)
        {
            return;
        }

        var macroManager = App.GetService<MacroManager>();
        var template = templateManager.GetTemplate(editor.SelectedTemplateKey)!;
        var newInstance = new MacroInstance(template)
        {
            IsUserCreated = true,
            IsPersistent = true,
            Properties = properties,
        };

        var instanceManager = App.GetService<MacroInstanceManager>();
        instanceManager.AddInstance(newInstance, withChange: true);
    }

    private static async Task<bool> EditInstancePropertiesInternal(MacroInstanceProperties properties, MacroInstance? instance)
    {
        var existedKeys = App.GetService<MacroInstanceManager>().GetInstances()
            .Where(x => x != instance)
            .Select(x => x.Properties?.Key)
            .OfType<string>()
            .ToHashSet();

        var dialogModel = new InputDialogModel()
        {
            Title = App.Localize($"Poltergeist/Home/EditInstancePropertiesDialog_Title"),
            Inputs = [
                new TextOption("title", properties.Title ?? "") {
                    DisplayLabel = App.Localize($"Poltergeist/Home/MacroPropertyLabel_Title"),
                    Placeholder = instance?.Template?.Title,
                },
                new TextOption("key", properties.Key ?? "") {
                    DisplayLabel = App.Localize($"Poltergeist/Home/MacroPropertyLabel_Key"),
                    Valid = key => existedKeys?.Contains(key) != true,
                },
                new TextOption("description", properties.Description ?? "") {
                    DisplayLabel = App.Localize($"Poltergeist/Home/MacroPropertyLabel_Description"),
                },
                new TextOption("icon", properties.Icon ?? "") {
                    DisplayLabel = App.Localize($"Poltergeist/Home/MacroPropertyLabel_Icon"),
                    Placeholder = instance?.Template?.Icon,
                },
                new BoolOption("is_favourite", properties.IsFavorite) {
                    DisplayLabel = App.Localize($"Poltergeist/Home/MacroPropertyLabel_IsFavorite"),
                },
            ],
            LabelLayout = InputDialogLabelLayout.Top,
            Valid = (parameters) => existedKeys.Contains((string)parameters!["key"]!) == true ? App.Localize($"Poltergeist/Home/InstanceKeyExistsMessage") : null,
        };

        await DialogService.ShowAsync(dialogModel);
        if (dialogModel.Result != DialogResult.Ok)
        {
            return false;
        }

        if (dialogModel.Parameters is null)
        {
            throw new InvalidOperationException();
        }

        var key = (string)dialogModel.Parameters["key"]!;
        properties.Key = string.IsNullOrEmpty(key) ? null : key;

        var title = (string)dialogModel.Parameters["title"]!;
        properties.Title = string.IsNullOrEmpty(title) ? null : title;

        var description = (string)dialogModel.Parameters["description"]!;
        properties.Description = string.IsNullOrEmpty(description) ? null : description;

        var icon = (string)dialogModel.Parameters["icon"]!;
        properties.Icon = string.IsNullOrEmpty(icon) ? null : icon;

        var isFavourite = (bool)dialogModel.Parameters["is_favourite"]!;
        properties.IsFavorite = isFavourite;

        return true;
    }

    public async Task EditInstanceProperties(MacroInstance instance)
    {
        if (instance.Properties is null)
        {
            throw new NotSupportedException();
        }

        if (await EditInstancePropertiesInternal(instance.Properties, instance) == false)
        {
            return;
        }

        var instanceManager = App.GetService<MacroInstanceManager>();
        instanceManager.UpdateProperties(instance, _ => { });
    }

    public async Task DeleteInstance(MacroInstance instance)
    {
        if (instance.IsLocked)
        {
            throw new NotSupportedException();
        }

        var dialogModel = new DialogModel()
        {
            Title = App.Localize($"Poltergeist/Home/DeleteMacroDialog_Title"),
            Text = App.Localize($"Poltergeist/Home/DeleteMacroDialog_Text", instance.Title),
            Type = DialogType.YesNo,
        };
        await DialogService.ShowAsync(dialogModel);
        if (dialogModel.Result != DialogResult.Yes)
        {
            return;
        }

        if (App.GetService<NavigationService>().TryCloseTab(instance.GetPageKey()) != true)
        {
            return;
        }

        var macroManager = App.GetService<MacroInstanceManager>();
        macroManager.RemoveInstance(instance, removeFiles: true, withChanged: true);
    }

    public async Task CreateShortcut(MacroInstance instance)
    {
        if (!instance.IsPersistent)
        {
            throw new NotSupportedException();
        }

        var savedialog = new FileSaveModel()
        {
            SuggestedFileName = $"{instance.Title}.lnk",
            Filters = new()
            {
                [App.Localize($"Poltergeist/Home/CreateShortcut_Dialog_Filter")] = [
                    ".lnk"
                ]
            }
        };
        await DialogService.ShowFileSavePickerAsync(savedialog);
        var shortcutPath = savedialog.FileName;

        if (shortcutPath is null)
        {
            return;
        }

        var optiondialog = new InputDialogModel()
        {
            Title = App.Localize($"Poltergeist/Home/CreateShortcut_Dialog_Title"),
            Text = shortcutPath,
            Inputs = [
                new BoolOption("autostart")
                {
                    Mode = BoolOptionMode.CheckBox,
                    Text = App.Localize($"Poltergeist/Home/CreateShortcut_Dialog_AutoStart"),
                },
                new BoolOption("autoclose")
                {
                    Mode = BoolOptionMode.CheckBox,
                    Text = App.Localize("Poltergeist/Home/CreateShortcut_Dialog_AutoClose"),
                },
                new BoolOption("exclusivemode")
                {
                    Mode = BoolOptionMode.CheckBox,
                    Text = App.Localize("Poltergeist/Home/CreateShortcut_Dialog_ExclusiveMode"),
                },
            ],
        };
        await DialogService.ShowAsync(optiondialog);
        if (optiondialog.Result != DialogResult.Ok)
        {
            return;
        }

        var autostart = (bool)optiondialog.Parameters!["autostart"]!;
        var autoclose = (bool)optiondialog.Parameters!["autoclose"]!;
        var exclusivemode = (bool)optiondialog.Parameters!["exclusivemode"]!;

        var arguments = $"--macro={instance.InstanceId}";
        if (autostart)
        {
            arguments += " --autostart";
        }
        if (autoclose)
        {
            arguments += " --autoclose";
        }
        if (exclusivemode)
        {
            arguments += " --exclusivemode";
        }

        var runAsAdmin = instance.Template is MacroBase mb && mb.RequiresAdmin == true;
        var iconLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, new Uri(@"ms-appx:///Poltergeist/Assets/macro.ico").LocalPath.TrimStart('/'));

        var wshShell = new WshShell();
        var shortcut = (IWshShortcut)wshShell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = Environment.ProcessPath;
        shortcut.Arguments = arguments;
        shortcut.WorkingDirectory = Environment.CurrentDirectory;
        shortcut.IconLocation = iconLocation;
        shortcut.Save();
        if (runAsAdmin)
        {
            using var fs = new FileStream(shortcutPath, FileMode.Open, FileAccess.ReadWrite);
            fs.Seek(0x15, SeekOrigin.Begin);
            fs.WriteByte(0x22);
            fs.Flush();
        }

        var successDialog = new DialogModel()
        {
            Title = App.Localize($"Poltergeist/Home/CreateShortcut_Dialog_Title"),
            Text = App.Localize($"Poltergeist/Home/CreateShortcut_Dialog_Message"),
            SecondaryButtonText = App.Localize($"Poltergeist/Home/CreateShortcut_Dialog_OpenFolder"),
        };
        await DialogService.ShowAsync(successDialog);
        if (successDialog.Result == DialogResult.Secondary)
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{shortcutPath}\"");
        }
    }

    public void OpenPrivateFolder(MacroInstance instance)
    {
        if (string.IsNullOrEmpty(instance.PrivateFolder))
        {
            throw new InvalidOperationException();
        }

        if (!Directory.Exists(instance.PrivateFolder))
        {
            return;
        }

        System.Diagnostics.Process.Start("explorer.exe", instance.PrivateFolder);
    }

    public void Sort(int index)
    {
        if (index == 0)
        {
            return;
        }

        if (SortIndex == index)
        {
            SortIndex = -index;
        }
        else
        {
            SortIndex = index;
        }

        RefreshMacroList();

        App.GetService<AppSettingsService>().InternalSettings.Set(LastSortIndexKey, SortIndex);
    }

    private void OnMacroInstancePropertyChanged(MacroInstancePropertyChangedEvent _)
    {
        App.TryEnqueue(() =>
        {
            RefreshMacroList();
        });
    }

    private void OnMacroInstanceCollectionChanged(MacroInstanceCollectionChangedEvent _)
    {
        App.TryEnqueue(() =>
        {
            RefreshMacroList();
        });
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
        {
            return;
        }

        if (disposing)
        {
            App.GetService<AppEventService>().Unsubscribe<MacroInstanceCollectionChangedEvent>(OnMacroInstanceCollectionChanged);
            App.GetService<AppEventService>().Unsubscribe<MacroInstancePropertyChangedEvent>(OnMacroInstancePropertyChanged);
        }

        disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
