using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Controls;

public class OptionItem
{
    public ObservableParameterItem? Item { get; set; }
    public ObservableParameterItem[]? SubItems { get; set; }
    public bool HasSubItems => SubItems?.Length > 0;
}
