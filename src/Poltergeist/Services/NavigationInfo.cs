using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Common;

namespace Poltergeist.Services;

public class NavigationInfo
{
    public required string Key { get; set; }
    public string? Header { get; set; }
    public IconInfo? Icon { get; set; }
    public bool IsFooter { get; set; }

    public Func<string[], object?, Page?>? CreateContent { get; set; }
    public Func<object, object>? CreateHeader { get; set; }
    public Func<object, IconSource?>? CreateIcon { get; set; }
};
