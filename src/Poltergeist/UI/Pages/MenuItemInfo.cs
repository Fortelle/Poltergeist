using Poltergeist.Automations.Structures;

namespace Poltergeist.UI.Pages;

public class MenuItemInfo
{
    public required string Text { get; set; }
    public required Action Execute { get; set; }
    public Func<bool>? CanExecute { get; set; }
    public IconInfo? Icon { get; set; }
}
