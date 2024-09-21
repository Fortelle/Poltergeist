using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Modules.Navigation;

public class NavigationInfo
{
    public required string Key { get; set; }
    public string? Text { get; set; }
    public string? Header { get; set; }
    public IconInfo? Icon { get; set; }
    public NavigationItemPosition PositionInSidebar { get; set; }

    public CreateContentCallback? CreateContent { get; set; }
    public CreateHeaderCallback? CreateHeader { get; set; }
    public CreateIconCallback? CreateIcon { get; set; }

    public Action? Action { get; set; }

    public delegate Page CreateContentCallback(string pageKey, object? data);
    public delegate object? CreateHeaderCallback(Page page);
    public delegate IconSource? CreateIconCallback(Page page);
};
