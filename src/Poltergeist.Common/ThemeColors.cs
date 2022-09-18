using System.Windows.Media;

namespace Poltergeist.Common;

public static class ThemeColors
{
    public static ThemeColor Failed = new()
    {
        Background = (Color)ColorConverter.ConvertFromString("#ffcccc"),
        Foreground = (Color)ColorConverter.ConvertFromString("#441111"),
        Icon = "\uEB90",
    };

    public static ThemeColor Succeeded = new()
    {
        Background = (Color)ColorConverter.ConvertFromString("#ccffcc"),
        Foreground = (Color)ColorConverter.ConvertFromString("#114411"),
        Icon = "\uE930",
    };
    public static ThemeColor Warning = new()
    {
        Background = (Color)ColorConverter.ConvertFromString("#ffffcc"),
        Foreground = (Color)ColorConverter.ConvertFromString("#664411"),
        Icon = "\uE7BA",
    };

    public static ThemeColor Info = new()
    {
        Background = (Color)ColorConverter.ConvertFromString("#ccddff"),
        Foreground = (Color)ColorConverter.ConvertFromString("#114477"),
        Icon = "\uE946",
    };

    public static ThemeColor Disabled = new()
    {
        Background = (Color)ColorConverter.ConvertFromString("#eeeeee"),
        Foreground = (Color)ColorConverter.ConvertFromString("#2f2f2f"),
        Icon = "\uF140",
    };

    public static ThemeColor Important = new()
    {
        Background = (Color)ColorConverter.ConvertFromString("#ddccff"),
        Foreground = (Color)ColorConverter.ConvertFromString("#441177"),
        Icon = "\uE73E",
    };

    public static ThemeColor Busy = new()
    {
        Background = (Color)ColorConverter.ConvertFromString("#ffeeaa"),
        Foreground = (Color)ColorConverter.ConvertFromString("#774411"),
        Icon = "\uE823",
    };
}

public class ThemeColor
{
    public string Name { get; set; }
    public Color Background { get; set; }
    public Color Foreground { get; set; }
    public string Icon { get; set; }
}
