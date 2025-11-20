using System.Text.RegularExpressions;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI.Text;

namespace Poltergeist.Helpers;

// just for fun
// should be improved later
public static class MarkdownRenderer
{
    private static readonly Dictionary<Regex, Func<Match, FrameworkElement>> LineHandlers = new();
    private static readonly Regex TextPattern = new Regex(@"(\*\*\*.+?\*\*\*|___.+?___|\*\*.+?\*\*|__.+?__|\*.+?\*|_.+?_|`.+?`|~~.+~~|~.+~|<sub>.+</sub>|<sup>.+</sup>|<ins>.+</ins>)");

    static MarkdownRenderer()
    {
        LineHandlers.Add(new(@"^[\*\-_]{3,}$"), RenderHorizontalRule);
        LineHandlers.Add(new("^(#+) (.+)$"), RenderHeader);
        LineHandlers.Add(new(@"^((?:\*|\-|\+|\>|\d+.)+) (.+)$"), RenderList);
    }

    public static FrameworkElement RenderLine(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return new TextBlock();
        }

        foreach (var handler in LineHandlers)
        {
            var match = handler.Key.Match(line);
            if (match.Success)
            {
                return handler.Value(match);
            }
        }

        return RenderTextLine(line);
    }

    private static FrameworkElement RenderTextLine(string text)
    {
        var textBlock = new TextBlock()
        {
            LineHeight = 20,
        };
        RenderText(textBlock, text);

        return new Grid()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(0, 2, 0, 2),
            ColumnDefinitions =
            {
                new() { Width = new GridLength(1, GridUnitType.Star) },
            },
            Children =
            {
                textBlock,
            },
        };
    }

    private static void RenderText(TextBlock textBlock, string text)
    {
        var phrases = TextPattern.Split(text);

        foreach (var phrase in phrases)
        {
            if (phrase.StartsWith("***") && phrase.EndsWith("***") || phrase.StartsWith("___") && phrase.EndsWith("___"))
            {
                textBlock.Inlines.Add(new Run()
                {
                    Text = phrase[3..^3],
                    FontWeight = FontWeights.Bold,
                    FontStyle = FontStyle.Italic,
                });
            }
            else if (phrase.StartsWith("**") && phrase.EndsWith("**") || phrase.StartsWith("__") && phrase.EndsWith("__"))
            {
                textBlock.Inlines.Add(new Run()
                {
                    Text = phrase[2..^2],
                    FontWeight = FontWeights.Bold,
                });
            }
            else if (phrase.StartsWith('*') && phrase.EndsWith('*') || phrase.StartsWith('_') && phrase.EndsWith('_'))
            {
                textBlock.Inlines.Add(new Run()
                {
                    Text = phrase[1..^1],
                    FontStyle = FontStyle.Italic,
                });
            }
            else if (phrase.StartsWith('~') && phrase.EndsWith('~'))
            {
                textBlock.Inlines.Add(new Run()
                {
                    Text = phrase.Trim('~'),
                    TextDecorations = TextDecorations.Strikethrough,
                });
            }
            else if (phrase.StartsWith('`') && phrase.EndsWith('`'))
            {
                var selStart = textBlock.Text.Length;
                var selLength = phrase.Length - 2;
                textBlock.Inlines.Add(new Run()
                {
                    Text = phrase[1..^1],
                    FontFamily = new FontFamily("Consolas"),
                });
                textBlock.TextHighlighters.Add(new TextHighlighter()
                {
                    Background = (Brush)Application.Current.Resources["SystemControlBackgroundChromeMediumBrush"],
                    Ranges = { new TextRange(selStart, selLength) }
                });
            }
            else if (phrase.StartsWith("<ins>") && phrase.EndsWith("</ins>"))
            {
                textBlock.Inlines.Add(new Run()
                {
                    Text = phrase[5..^6],
                    TextDecorations = TextDecorations.Underline,
                });
            }
            else if (phrase.StartsWith("<sub>") && phrase.EndsWith("</sub>"))
            {
                var run = new Run()
                {
                    Text = phrase[5..^6],
                };
                Typography.SetVariants(run, FontVariants.Subscript);
                textBlock.Inlines.Add(run);
            }
            else if (phrase.StartsWith("<sup>") && phrase.EndsWith("</sup>"))
            {
                var run = new Run()
                {
                    Text = phrase[5..^6],
                };
                Typography.SetVariants(run, FontVariants.Superscript);
                textBlock.Inlines.Add(run);
            }
            else
            {
                textBlock.Inlines.Add(new Run()
                {
                    Text = phrase
                });
            }
        }
    }

    private static FrameworkElement RenderHeader(Match match)
    {
        var level = match.Groups[1].Value.Length;

        var textBlock = new TextBlock
        {
            FontSize = level switch
            {
                1 => 24,
                2 => 20,
                3 => 18,
                _ => 16
            },
            FontWeight = level switch
            {
                1 => FontWeights.Bold,
                _ => FontWeights.SemiBold
            },
            VerticalAlignment = VerticalAlignment.Center,
        };
        RenderText(textBlock, match.Groups[2].Value);

        var grid = new Grid()
        {
            Margin = level switch
            {
                1 => new Thickness(0, 24, 0, 12),
                2 => new Thickness(0, 16, 0, 8),
                3 => new Thickness(0, 8, 0, 4),
                _ => new Thickness(0)
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            RowDefinitions =
            {
                new()
                {
                    Height = new GridLength(0, GridUnitType.Auto),
                    MinHeight = 24,
                },
                new() 
                {
                    Height = new GridLength(0, GridUnitType.Auto)
                },
            },
            Children =
            {
                textBlock,
            },
        };

        if (level <= 2)
        {
            var hr = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = (Brush)Application.Current.Resources["SystemControlForegroundBaseLowBrush"],
                Margin = new Thickness(0, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Grid.SetRow(hr, 1);
            grid.Children.Add(hr);
        }

        return grid;

    }

    private static FrameworkElement RenderList(Match match)
    {
        var left = match.Groups[1].Value;
        var level = left.Length;

        var marker = left[^1] == '>' ? CreateQuoteMarker() : CreateListMarker();
        var textBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            HorizontalTextAlignment = TextAlignment.Left,
            LineHeight = 20,
            Padding = new Thickness(0, 4, 0, 4),
        };
        RenderText(textBlock, match.Groups[2].Value);
        Grid.SetColumn(textBlock, 1);

        return new Grid()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnDefinitions =
            {
                new() { Width = new GridLength(32 * level, GridUnitType.Pixel) },
                new() { Width = new GridLength(1, GridUnitType.Star) },
            },
            Children =
            {
                marker,
                textBlock,
            },
        };
    }

    private static FrameworkElement RenderHorizontalRule(Match _)
    {
        return new Border
        {
            BorderThickness = new Thickness(0, 0, 0, 1),
            BorderBrush = (Brush)Application.Current.Resources["SystemControlForegroundBaseLowBrush"],
            Margin = new Thickness(0, 8, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
    }

    private static Border CreateListMarker()
    {
        return new Border()
        {
            Width = 24,
            Height = 28,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Child = new Ellipse()
            {
                Width = 8,
                Height = 8,
                Fill = (Brush)Application.Current.Resources["SystemControlForegroundBaseMediumBrush"],
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            }
        };
    }

    private static Border CreateQuoteMarker()
    {
        return new Border()
        {
            Width = 24,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = new Rectangle()
            {
                Width = 8,
                Fill = (Brush)Application.Current.Resources["SystemControlForegroundBaseLowBrush"],
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Center,
            }
        };
    }
}
