using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities;
using Poltergeist.Modules.Macros;
using Poltergeist.UI.Controls;
using Poltergeist.UI.Windows;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Poltergeist.Modules.Interactions;

public class DialogService
{

    public static async Task ShowAsync(DialogModel model)
    {
        List<ObservableParameterItem>? parameters = null;
        var stackPanel = new StackPanel();

        if (!string.IsNullOrEmpty(model.Text))
        {
            stackPanel.Children.Add(new TextBlock()
            {
                Text = model.Text,
            });
        }

        switch (model)
        {
            case InputDialogModel inputDialogModel:
                {
                    parameters ??= new();
                    foreach (var parameterDefinition in inputDialogModel.Inputs)
                    {
                        var omi = new ObservableParameterItem(parameterDefinition);
                        parameters.Add(omi);

                        var grid = new Grid()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            ColumnSpacing = 16,
                            RowSpacing = 8,
                            Margin = new Thickness(0, 0, 0, 16),
                        };
                        var optionControl = new OptionControl()
                        {
                            DataContext = omi,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                        };
                        var labelControl = new TextBlock()
                        {
                            Text = parameterDefinition.DisplayLabel ?? "",
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                        };

                        switch (inputDialogModel.LabelLayout)
                        {
                            case InputDialogLabelLayout.Hidden:
                            case InputDialogLabelLayout.Top when labelControl.Text.Length == 0:
                                grid.Children.Add(optionControl);
                                break;
                            case InputDialogLabelLayout.Top:
                                grid.RowDefinitions.Add(new());
                                grid.RowDefinitions.Add(new());
                                Grid.SetRow(labelControl, 0);
                                Grid.SetRow(optionControl, 1);
                                grid.Children.Add(labelControl);
                                grid.Children.Add(optionControl);
                                break;
                            case InputDialogLabelLayout.Left:
                                grid.ColumnDefinitions.Add(new() { Width = new(0.3, GridUnitType.Star) });
                                grid.ColumnDefinitions.Add(new() { Width = new(0.6, GridUnitType.Star) });
                                Grid.SetColumn(optionControl, 1);
                                grid.Children.Add(labelControl);
                                grid.Children.Add(optionControl);
                                break;
                        }

                        stackPanel.Children.Add(grid);
                    }
                }
                break;
            case ContentDialogModel contentDialogModel:
                {
                    stackPanel.Children.Add(contentDialogModel.Content);
                }
                break;
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

        switch (model)
        {
            case InputDialogModel inputDialogModel:
                {
                    if (inputDialogModel.Valid is not null)
                    {
                        contentDialog.Closing += (sender, args) =>
                        {
                            if (args.Result == ContentDialogResult.Primary)
                            {
                                var paramArgs = parameters?.ToDictionary(x => x.Definition.Key, x => x.Value);
                                if (inputDialogModel.Valid(paramArgs) is not null)
                                {
                                    args.Cancel = true;
                                }
                            }
                        };
                    }
                }
                break;
            case ContentDialogModel contentDialogModel:
                {
                    if (contentDialogModel.Valid is not null)
                    {
                        contentDialog.Closing += (sender, args) =>
                        {
                            if (args.Result == ContentDialogResult.Primary)
                            {
                                var x = contentDialogModel.Valid();

                                if (contentDialogModel.Valid() is not null)
                                {
                                    args.Cancel = true;
                                }
                            }
                        };
                    }
                }
                break;
        }

        var dialogResult = await ShowAsync(contentDialog);

        model.Result = model.GetDialogResult(dialogResult);

        if (parameters is not null)
        {
            model.Parameters = parameters.ToDictionary(x => x.Definition.Key, x => x.Value);
        }

        if (model.ProcessorId is not null)
        {
            var msg = new InteractionMessage()
            {
                ProcessorId = model.ProcessorId,

                [InteractionService.InteractionIdKey] = model.Id,
                [DialogModel.DialogResultKey] = dialogResult switch
                {
                    ContentDialogResult.None => DialogResult.Close.ToString(),
                    ContentDialogResult.Primary => DialogResult.Primary.ToString(),
                    ContentDialogResult.Secondary => DialogResult.Secondary.ToString(),
                    _ => throw new NotSupportedException()
                },
            };

            if (parameters is not null)
            {
                foreach (var parameter in parameters)
                {
                    msg.Add(string.Format(DialogModel.DialogParameterKey, parameter.Definition.Key), $"{parameter.Value}");
                }
            }

            PoltergeistApplication.GetService<MacroManager>().SendMessage(msg);
        }
    }

    public static async Task<ContentDialogResult> ShowAsync(ContentDialog contentDialog)
    {
        contentDialog.XamlRoot = PoltergeistApplication.GetService<ShellPage>().XamlRoot;

        return await contentDialog.ShowAsync();
    }

    public static async Task ShowFileOpenPickerAsync(FileOpenModel model)
    {
        var picker = new FileOpenPicker()
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };

        if (model.Filters?.Count > 0)
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
            if (file is null)
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
            picker.FileTypeChoices.Add("All files", ["."]);
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
        await PoltergeistApplication.Current.MainWindow.ShowMessageDialogAsync(message, title);
    }

    private static ContentDialog? ProgressDialog { get; set; }

    public static void ShowProgress(ProgressModel model)
    {
        // there is a bug on ProgressRing with initialized IsActive
        // https://github.com/microsoft/microsoft-ui-xaml/issues/5925

        if (model.IsOn == false)
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

        if (model.CancellationTokenSource is not null)
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
        var hWnd = PoltergeistApplication.Current.MainWindow.GetWindowHandle();
        InitializeWithWindow.Initialize(picker, hWnd);
    }

}
