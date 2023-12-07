using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Common;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Views;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Poltergeist.Services;

public class DialogService
{

    public static async Task ShowAsync(DialogModel model)
    {
        var stackPanel = new StackPanel();

        if (!string.IsNullOrEmpty(model.Text))
        {
            stackPanel.Children.Add(new TextBlock()
            {
                Text = model.Text,
            });
        }
        if(model.Inputs?.Length > 0)
        {
            foreach(var input in model.Inputs)
            {
                stackPanel.Children.Add(new OptionControl()
                {
                    DataContext = input,
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 16, 0, 0),
                });
            }
        }

        var contentDialog = new ContentDialog()
        {
            Name = model.Id,
            Title = model.Title,
            Content = stackPanel,
        };

        var (primaryText, secondaryText, closeText) = model.GetButtonNames();
        model.PrimaryButtonText ??= primaryText;
        model.SecondaryButtonText ??= secondaryText;
        model.CloseButtonText ??= closeText;

        if (!string.IsNullOrEmpty(model.PrimaryButtonText))
        {
            contentDialog.PrimaryButtonText = model.PrimaryButtonText;
        }
        if (!string.IsNullOrEmpty(model.SecondaryButtonText))
        {
            contentDialog.SecondaryButtonText = model.SecondaryButtonText;
        }
        if (!string.IsNullOrEmpty(model.CloseButtonText))
        {
            contentDialog.CloseButtonText = model.CloseButtonText;
        }

        if (model.ShowsDefaultButton)
        {
            contentDialog.DefaultButton = !string.IsNullOrEmpty(model.PrimaryButtonText) ? ContentDialogButton.Primary
                : !string.IsNullOrEmpty(model.SecondaryButtonText) ? ContentDialogButton.Secondary
                : ContentDialogButton.Close;
        }

        var dialogResult = await ShowAsync(contentDialog);

        model.Result = model.GetDialogResult(dialogResult);

        if (model.Inputs?.Length > 0)
        {
            model.Values = model.Inputs.Select(x => x.Value!).ToArray();
        }

        if(model.MacroKey is not null && model.ProcessId is not null)
        {
            var msg = new InteractionMessage()
            {
                MacroKey = model.MacroKey,
                ProcessId = model.ProcessId,

                [InteractionService.InteractionIdKey] = model.Id,
                [DialogModel.DialogResultKey] = dialogResult switch
                {
                    ContentDialogResult.None => DialogResult.Close.ToString(),
                    ContentDialogResult.Primary => DialogResult.Primary.ToString(),
                    ContentDialogResult.Secondary => DialogResult.Secondary.ToString(),
                    _ => throw new NotSupportedException()
                },
            };

            if (model.Inputs?.Length > 0)
            {
                for (var i = 0; i < model.Inputs.Length; i++)
                {
                    msg.Add(string.Format(DialogModel.DialogValueKey, i), $"{model.Inputs[i].Value}");
                }
            }

            App.GetService<MacroManager>().SendMessage(msg);
        }
    }

    public static async Task<ContentDialogResult> ShowAsync(ContentDialog contentDialog)
    {
        contentDialog.XamlRoot = App.GetService<ShellPage>().XamlRoot;

        return await contentDialog.ShowAsync();
    }

    public static async Task ShowFileOpenPickerAsync(FileOpenModel model)
    {
        var picker = new FileOpenPicker()
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };

        if(model.Filters?.Count > 0)
        {
            foreach (var type in model.Filters)
            {
                picker.FileTypeFilter.Add(type);
            }
        }
        else
        {
            picker.FileTypeFilter.Add("*");
        }

        SetWindow(picker);

        if (model.Multiselect)
        {
            var files = await picker.PickMultipleFilesAsync();
            if (files is null)
            {
                return;
            }
            model.FileNames = files.Select(x => x.Path).ToArray();
        }
        else
        {
            var file = await picker.PickSingleFileAsync();
            if(file is null)
            {
                return;
            }
            model.FileName = file.Path;
        }
    }

    public static async Task ShowFileSavePickerAsync(FileSaveModel model)
    {
        var picker = new FileSavePicker()
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };

        if (!string.IsNullOrEmpty(model.SuggestedFileName))
        {
            picker.SuggestedFileName = model.SuggestedFileName;
        }

        if (model.Filters?.Count > 0)
        {
            foreach (var (key, value) in model.Filters)
            {
                picker.FileTypeChoices.Add(key, value);
            }
        }
        else
        {
            picker.FileTypeChoices.Add("All files", new List<string> { "." });
        }

        SetWindow(picker);

        var file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            return;
        }
        model.FileName = file.Path;
    }

    public static async Task ShowFolderPickerAsync(FolderModel model)
    {
        var picker = new FolderPicker()
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };

        SetWindow(picker);

        var file = await picker.PickSingleFolderAsync();
        if (file is null)
        {
            return;
        }
        model.FolderName = file.Path;
    }

    public static async Task ShowMessage(string message, string title = "")
    {
        await App.MainWindow.ShowMessageDialogAsync(message, title);
    }

    private static ContentDialog? ProgressDialog { get; set; }

    public static void ShowProgress(ProgressModel model)
    {
        // there is a bug on ProgressRing with initialized IsActive
        // https://github.com/microsoft/microsoft-ui-xaml/issues/5925

        if(model.IsOn == false)
        {
            ProgressDialog?.Hide();
            return;
        }

        var progressBar = new ProgressBar()
        {
            IsIndeterminate = true,
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
            VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
        };

        ProgressDialog = new ContentDialog()
        {
            Name = model.Id,
            Title = model.Title,
            Content = progressBar,
        };

        if(model.CancellationTokenSource is not null)
        {
            ProgressDialog.PrimaryButtonText = ResourceHelper.Localize("Poltergeist.Automations/Resources/DialogButton_Cancel");
            ProgressDialog.PrimaryButtonClick += (s, e) =>
            {
                model.CancellationTokenSource.Cancel();
            };
        }
        _ = ShowAsync(ProgressDialog);
    }

    private static void SetWindow(object picker)
    {
        var hWnd = App.MainWindow.GetWindowHandle();
        InitializeWithWindow.Initialize(picker, hWnd);
    }

}
