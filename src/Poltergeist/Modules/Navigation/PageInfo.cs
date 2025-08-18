using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures;
using Poltergeist.UI.Pages;

namespace Poltergeist.Modules.Navigation;

public class PageInfo
{
    public required string Key { get; init; }
    public string? Header { get; set; }
    public IconInfo? Icon { get; set; }
    public MenuItemInfo[]? Menu { get; set; }

    public CreateContentCallback? CreateContent { get; set; }
    public CreateHeaderCallback? CreateHeader { get; set; }
    public CreateIconCallback? CreateIcon { get; set; }
    public CreateMenuCallback? CreateMenu { get; set; }
    public UpdateArgumentCallback? UpdateArgument { get; set; }

    public delegate Page CreateContentCallback(string pageKey, object? argument);
    public delegate object? CreateHeaderCallback(Page page);
    public delegate IconSource? CreateIconCallback(Page page);
    public delegate MenuItemInfo[]? CreateMenuCallback(Page page);
    public delegate void UpdateArgumentCallback(Page page, object? argument);

    [SetsRequiredMembers]
    public PageInfo(string key)
    {
        Key = key;
    }
};
