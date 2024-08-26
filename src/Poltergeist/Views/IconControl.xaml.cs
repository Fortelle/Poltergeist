using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Views;

public sealed partial class IconControl : UserControl
{
    public IconControl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is string text)
        {
            var icon = new IconInfo(text);

            if (icon.Glyph is not null)
            {
                Content = new FontIcon()
                {
                    Glyph = icon.Glyph,
                    FontSize = FontSize,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
            }
            else if (icon.Uri is not null)
            {
                Content = new ImageIcon()
                {
                    Source = new BitmapImage(new Uri(icon.Uri)),
                    MaxWidth = Width,
                    MaxHeight = Height,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
            }
            else if (icon.Emoji is not null)
            {
                Content = new FontIcon()
                {
                    FontFamily = new("Segoe UI Emoji"),
                    Glyph = icon.Emoji,
                    FontSize = FontSize,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
            }
            else if (icon.Text is not null)
            {
                Content = new TextBlock()
                {
                    Text = icon.Text,
                    FontSize = FontSize,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
            }
        }
    }

}
