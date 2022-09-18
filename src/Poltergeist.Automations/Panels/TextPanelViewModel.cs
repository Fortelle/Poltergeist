using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Poltergeist.Automations.Panels;

public class TextPanelViewModel
{
    public SolidColorBrush Background { get; set; }
    public SolidColorBrush Foreground { get; set; }
    private readonly Dictionary<string, SolidColorBrush> Brushes = new();

    public event Action<TextLine> TextRecived;

    public TextPanelViewModel(TextPanelModel model)
    {

        if (model.BackgroundColor.HasValue)
        {
            Background = new SolidColorBrush(model.BackgroundColor.Value);
            Background.Freeze();
        }

        if (model.ForegroundColor.HasValue)
        {
            Foreground = new SolidColorBrush(model.ForegroundColor.Value);
            Foreground.Freeze();
        }

        foreach(var (key, color) in model.Colors)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            Brushes.Add(key, brush);
        }

        model.TextRecived += OnTextRecived;
    }

    public void OnTextRecived(params TextLine[] lines)
    {
        foreach(var line in lines)
        {
            if (line.Brush == null)
            {
                if (line.Color.HasValue)
                {
                    line.Brush = new SolidColorBrush(line.Color.Value);
                }
                else if (!string.IsNullOrEmpty(line.Category) && Brushes.TryGetValue(line.Category, out var brush))
                {
                    line.Brush = brush;
                }
            }

            TextRecived?.Invoke(line);
        }
    }
}
