using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Poltergeist.UI.Controls;

public class OptionTemplateSelector : DataTemplateSelector
{
    public DataTemplate? CardTemplate { get; set; }
    public DataTemplate? ExpanderTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        if (item is not OptionItem optionItem)
        {
            return null;
        }

        if (optionItem.HasSubItems)
        {
            return ExpanderTemplate!;
        }
        else
        {
            return CardTemplate!;
        }
    }
}
