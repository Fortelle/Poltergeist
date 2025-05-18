using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures;
using Poltergeist.UI.Pages;

namespace Poltergeist.Modules.Navigation;

public class NavigationInfo
{
    public required string Key { get; set; }
    public string? Text { get; set; }
    public string? Header { get; set; }
    public IconInfo? Icon { get; set; }
    public MenuItemInfo[]? Menu { get; set; }
    public NavigationItemPosition PositionInSidebar { get; set; }

    public CreateContentCallback? CreateContent { get; set; }
    public CreateHeaderCallback? CreateHeader { get; set; }
    public CreateIconCallback? CreateIcon { get; set; }
    public CreateMenuCallback? CreateMenu { get; set; }
    public UpdateArgumentsCallback? UpdateArguments { get; set; }

    public Action? Action { get; set; }

    public delegate Page CreateContentCallback(string pageKey, object? data);
    public delegate object? CreateHeaderCallback(Page page);
    public delegate IconSource? CreateIconCallback(Page page);
    public delegate MenuItemInfo[]? CreateMenuCallback(Page page);
    public delegate void UpdateArgumentsCallback(Page page, object? arguments);
};
