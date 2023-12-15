using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class PickerOptionControl : UserControl
{
    private PathOption Item { get; }

    [ObservableProperty]
    private string? _filepath;

    [ObservableProperty]
    private string? _filename;

    public PickerOptionControl(PathOption item)
    {
        InitializeComponent();

        Item = item;

        if(item.Value is string path)
        {
            Filepath = path;
            Filename = Path.GetFileName(path);
        }
    }

    private async void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        switch (Item.Mode)
        {
            case PathOptionMode.FileOpen:
                {
                    var openPicker = new FileOpenPicker
                    {
                        ViewMode = PickerViewMode.Thumbnail
                    };
                    if(Item.Filters?.Count > 0)
                    {
                        foreach(var value in Item.Filters.Values)
                        {
                            openPicker.FileTypeFilter.Add(value.First());
                        }
                    }
                    else
                    {
                        openPicker.FileTypeFilter.Add("*");
                    }
                    SetWindow(openPicker);
                    var file = await openPicker.PickSingleFileAsync();
                    if (file is null)
                    {
                        break;
                    }

                    Filepath = file.Path;
                    Filename = file.Name;
                    Item.Value = file.Path;
                }
                break;
            case PathOptionMode.FileSave:
                {
                    var savePicker = new FileSavePicker();
                    if (Item.Filters?.Count > 0)
                    {
                        foreach (var (key, value) in Item.Filters)
                        {
                            savePicker.FileTypeChoices.Add(key, value);
                        }
                    }
                    else
                    {
                        savePicker.FileTypeChoices.Add("All files", new List<string>() { "." });
                    }
                    SetWindow(savePicker);
                    var file = await savePicker.PickSaveFileAsync();
                    if (file is null)
                    {
                        break;
                    }

                    Filepath = file.Path;
                    Filename = file.Name;
                    Item.Value = file.Path;
                }
                break;
            case PathOptionMode.FolderOpen:
                {
                    var openPicker = new FolderPicker();
                    SetWindow(openPicker);
                    var file = await openPicker.PickSingleFolderAsync();
                    if (file is null)
                    {
                        break;
                    }
                    
                    Filepath = file.Path;
                    Filename = file.Name;
                    Item.Value = file.Path;
                }
                break;
            default:
                break;
        }
    }

    private static void SetWindow(object picker)
    {
        var hWnd = App.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);
    }

}
