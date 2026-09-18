using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Structures;
using Poltergeist.Helpers;

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
            string s => IconInfo.FromString(s),
            _ => null,
        };

        if (iconInfo is null)
        {
            return;
        }

        var control = (UserControl)d;

        var iconElement = IconInfoHelper.ConvertToIconElement(iconInfo);
        if (iconElement is FontIcon fontIcon)
        {
            fontIcon.FontSize = control.FontSize;
            var binding = new Binding()
            {
                Path = new PropertyPath(nameof(FontIcon.FontSize)),
                ElementName = control.Name,
                Mode = BindingMode.OneWay,
            };
            fontIcon.SetBinding(FontIcon.FontSizeProperty, binding);
        }

        control.Content = iconElement;
    }
}
