using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Poltergeist.Automations.Structures;
using Poltergeist.Helpers;

namespace Poltergeist.UI.Controls;

[DependencyProperty<object>("Icon")]
public sealed partial class IconControl : UserControl
{
    public IconControl()
    {
        InitializeComponent();
    }

    partial void OnIconChanged(object? newValue)
    {
        if (newValue is null)
        {
            Content = null;
            return;
        }

        var iconInfo = newValue switch
        {
            IconInfo ii => ii,
            string s => IconInfo.FromString(s),
            _ => throw new NotImplementedException(),
        };

        var iconElement = IconInfoHelper.ConvertToIconElement(iconInfo);
        if (iconElement is FontIcon fontIcon)
        {
            fontIcon.FontSize = FontSize;
            var binding = new Binding()
            {
                Path = new PropertyPath(nameof(FontIcon.FontSize)),
                ElementName = Name,
                Mode = BindingMode.OneWay,
            };
            fontIcon.SetBinding(FontIcon.FontSizeProperty, binding);
        }

        Content = iconElement;
    }
}
