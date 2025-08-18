using Poltergeist.Automations.Structures;

namespace Poltergeist.Modules.Navigation;

public class SidebarItemInfo
{
    public required string Text { get; set; }
    public IconInfo? Icon { get; set; }
    public string? Group { get; set; }

    public Action? Action { get; set; }
    public NavigationInfo? Navigation { get; set; }

    public SidebarItemPosition Position { get; set; }
};
