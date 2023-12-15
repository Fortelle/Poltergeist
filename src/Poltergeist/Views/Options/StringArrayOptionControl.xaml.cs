using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;
using Poltergeist.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class StringArrayOptionControl : UserControl
{
    private OptionItem<string[]> Item { get; }

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private string? _tooltip;

    public StringArrayOptionControl(OptionItem<string[]> item)
    {
        Item = item;

        this.InitializeComponent();

        UpdateText();
    }

    private void UpdateText()
    {
        if (Item.Value is null)
        {
            Text = "(null)";
            Tooltip = null;
        }
        else
        {
            //Text = $"System.String[{Item.Value.Length}]";
            Text = string.Join(", ", Item.Value);
            Tooltip = string.Join("\n", Item.Value);
        }
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        var textbox = new TextBox()
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Text = Item.Value is not null ? string.Join("\n", Item.Value) : "",
            Height = 200,
            Width = 600,
        };

        var stackPanel = new StackPanel();
        stackPanel.Children.Add(new TextBlock()
        {
            Text = App.Localize($"Poltergeist/Resources/Options_StringArrayOption_Dialog_Text"),
        });
        stackPanel.Children.Add(textbox);

        var contentDialog = new ContentDialog()
        {
            Title = App.Localize($"Poltergeist/Resources/Options_StringArrayOption_Dialog_Title"),
            Content = stackPanel,
            PrimaryButtonText = App.Localize($"Poltergeist.Automations/Resources/DialogButton_Ok"),
            CloseButtonText = App.Localize($"Poltergeist.Automations/Resources/DialogButton_Cancel"),
        };
        
        var dialogResult = await DialogService.ShowAsync(contentDialog);

        if(dialogResult == ContentDialogResult.None)
        {
            return;
        }

        if (string.IsNullOrEmpty(textbox.Text))
        {
            Item.Value = null;
        }
        else
        {
            Item.Value = textbox.Text.Replace("\n\r", "\n").Replace("\r", "").TrimEnd('\n').Split("\r");
        }
        UpdateText();
    }
}
