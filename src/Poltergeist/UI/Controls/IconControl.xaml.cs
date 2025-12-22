using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Poltergeist.Automations.Structures;

namespace Poltergeist.UI.Controls;

public sealed partial class IconControl : UserControl
{
    public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached(nameof(Icon), typeof(object), typeof(IconControl), new PropertyMetadata(null, OnIconChanged));
    public object? Icon
    {
        get => (object?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IconControl()
    {
        InitializeComponent();
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var iconInfo = e.NewValue switch
        {
            IconInfo ii => ii,
            string s => new IconInfo(s),
            _ => null,
        };

        if (iconInfo is null)
        {
            return;
        }

        var control = (UserControl)d;
        control.Content = ToFrameworkElement(control, iconInfo);
    }

    private static FrameworkElement ToFrameworkElement(UserControl control, IconInfo iconInfo)
    {
        if (iconInfo.Glyph is not null)
        {
            var fi = new FontIcon()
            {
                Glyph = iconInfo.Glyph,
                FontSize = control.FontSize,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            var binding = new Binding()
            {
                Path = new PropertyPath("FontSize"),
                ElementName = control.Name,
                Mode = BindingMode.OneWay,
            };
            fi.SetBinding(FontIcon.FontSizeProperty, binding);
            return fi;
        }
        else if (iconInfo.Uri is not null)
        {
            return new ImageIcon()
            {
                Source = new BitmapImage(new Uri(iconInfo.Uri)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
        }
        else if (iconInfo.Emoji is not null)
        {
            var fi = new FontIcon()
            {
                FontFamily = new("Segoe UI Emoji"),
                Glyph = iconInfo.Emoji,
                FontSize = control.FontSize,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            var binding = new Binding()
            {
                Path = new PropertyPath("FontSize"),
                ElementName = control.Name,
                Mode = BindingMode.OneWay,
            };
            fi.SetBinding(FontIcon.FontSizeProperty, binding);
            return fi;
        }
        else if (iconInfo.Text is not null)
        {
            var tb = new TextBlock()
            {
                Text = iconInfo.Text,
                FontSize = control.FontSize,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            var binding = new Binding()
            {
                Path = new PropertyPath("FontSize"),
                ElementName = control.Name,
                Mode = BindingMode.OneWay,
            };
            tb.SetBinding(TextBlock.FontSizeProperty, binding);
            return tb;
        }
        throw new NotSupportedException();
    }

}
