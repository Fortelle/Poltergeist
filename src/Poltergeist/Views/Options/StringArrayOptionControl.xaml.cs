using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Parameters;
using Poltergeist.Services;

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class StringArrayOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private string? _tooltip;

    public StringArrayOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is not OptionDefinition<string[]>)
        {
            throw new NotSupportedException();
        }

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

        var contentDialog = new ContentDialogModel()
        {
            Title = App.Localize($"Poltergeist/Resources/Options_StringArrayOption_Dialog_Title"),
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
            Item.Value = textbox.Text.Replace("\n\r", "\n").Replace("\r", "").TrimEnd('\n').Split("\r");
        }
        UpdateText();
    }
}
