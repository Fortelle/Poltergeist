using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Controls;

[DependencyProperty<IEnumerable<ObservableParameterItem>>("Options")]
[DependencyProperty<bool>("IsLocked")]
[DependencyProperty<OptionGroup[]>("Groups")]
public sealed partial class OptionListView : UserControl
{
    private static readonly string UncategorizedGroupLabel = App.Localize("Poltergeist/Resources/Options_Uncategorized");
    
    public OptionListView()
    {
        InitializeComponent();
    }

    partial void OnOptionsChanged(IEnumerable<ObservableParameterItem>? newValue)
    {
        if (newValue is null)
        {
            return;
        }

        var childGroups = newValue
            .Where(x => x.Definition.Status != ParameterStatus.Hidden)
            .Where(x => x.Definition is IOptionDefinition od && od.Parent is not null)
            .GroupBy(x => ((IOptionDefinition)x.Definition).Parent)
            .ToDictionary(x => x.Key!, x => x.ToArray())
            ;

        Groups = newValue
            .Where(x => x.Definition.Status != ParameterStatus.Hidden)
            .Where(x => x.Definition is not IOptionDefinition od || od.Parent is null)
            .GroupBy(x => x.Definition.Category)
            .OrderBy(x => x.Key is null ? 0 : 1)
            .Select(x => new OptionGroup
            {
                Title = x.Key ?? UncategorizedGroupLabel,
                Options = x.Select(y => new OptionItem()
                {
                    Item = y,
                    SubItems = childGroups.TryGetValue(y.Definition.Key, out var children) ? children : null,
                }),
            })
            .ToArray();
    }
}
