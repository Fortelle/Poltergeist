using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Modules.Interactions;

namespace Poltergeist.UI.Controls.Options;

[ObservableObject]
public sealed partial class StringArrayOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    [ObservableProperty]
    public partial string? Text { get; set; }

    [ObservableProperty]
    public partial string? Tooltip { get; set; }

    public StringArrayOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is not OptionDefinition<string[]>)
        {
            throw new NotSupportedException();
        }

        Item = item;

        InitializeComponent();

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
            Text = Item.Value is IEnumerable<string> s ? string.Join("\n", s) : "",
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
            Item.Value = Split(textbox.Text);
        }
        UpdateText();
    }

    private static string[] Split(string text)
    {
        var lines = text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

        if (lines[^1] == "")
        {
            // Remove the last empty line if it exists
            // "A\nB\nC\n" -> ["A", "B", "C"]
            // "\n" -> []
            // "\n\n" -> [""]
            Array.Resize(ref lines, lines.Length - 1);
        }

        return lines;
    }
}
