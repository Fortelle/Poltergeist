using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;
using Windows.Storage.Pickers;

namespace Poltergeist.UI.Controls.Options;

// bug: https://github.com/files-community/Files/issues/11440
// warn: FileSavePicker must have at least one file type choice and does not support wildcard.
[ObservableObject]
public sealed partial class PickerOptionControl : UserControl
{
    [ObservableProperty]
    public partial string? Filepath { get; set; }

    [ObservableProperty]
    public partial string? Filename { get; set; }

    private ObservableParameterItem Item { get; }

    public PickerOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is not PathOption)
        {
            throw new NotSupportedException();
        }

        Item = item;

        if (item.Value is string path)
        {
            Filepath = path;
            Filename = Path.GetFileName(path);
        }

        InitializeComponent();
    }

    private async void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var pathOption = (PathOption)Item.Definition;
        switch (pathOption.Mode)
        {
            case PathOptionMode.FileOpen:
                {
                    var openPicker = new FileOpenPicker
                    {
                        ViewMode = PickerViewMode.Thumbnail
                    };
                    if(pathOption.Filters?.Count > 0)
                    {
                        foreach(var value in pathOption.Filters.Values)
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
                    if (pathOption.Filters?.Count > 0)
                    {
                        foreach (var (key, value) in pathOption.Filters)
                        {
                            savePicker.FileTypeChoices.Add(key, value);
                        }
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
        var hWnd = App.Current.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);
    }

}
