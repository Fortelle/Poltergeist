using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Interactions;

namespace Poltergeist.UI.Controls.Options;

[ObservableObject]
public sealed partial class MultilineTextOptionControl : UserControl
{
    private const int MaxLength = 100;

    private ObservableParameterItem Item { get; }

    [ObservableProperty]
    private string? _text;

    public MultilineTextOptionControl(ObservableParameterItem item)
    {
        Text = Truncate(item.Value as string);
        Item = item;

        InitializeComponent();
    }

    private static string Truncate(string? value)
    {
        if (value is null)
        {
            return "";
        }

        value = value.Length < MaxLength ? value : value[..MaxLength];
        value = value.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");

        return value;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        var textbox = new TextBox()
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Text = (Item.Value as string) ?? "",
            Height = 200,
            Width = 600,
        };

        var stackPanel = new StackPanel();
        stackPanel.Children.Add(new TextBlock()
        {
            Text = App.Localize($"Poltergeist/Resources/Options_MultilineTextOption_Dialog_Text"),
        });
        stackPanel.Children.Add(textbox);

        var contentDialog = new ContentDialogModel()
        {
            Title = App.Localize($"Poltergeist/Resources/Options_MultilineTextOption_Dialog_Title"),
            Content = stackPanel,
        };

        await DialogService.ShowAsync(contentDialog);

        if (contentDialog.Result == DialogResult.Cancel)
        {
            return;
        }

        if (string.IsNullOrEmpty(textbox.Text))
        {
            Item.Value = null;
        }
        else
        {
            Item.Value = textbox.Text.Replace("\n\r", "\n").Replace("\r", "").TrimEnd('\n');
        }

        Text = Truncate(Item.Value as string);
    }
}
