using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Helpers;
using Windows.UI;
using Windows.UI.Text;

namespace Poltergeist.UI.Controls.Instruments;

public class TextInstrumentViewModel : IInstrumentViewModel
{
    public SolidColorBrush? Background { get; set; }
    public SolidColorBrush? Foreground { get; set; }
    public string? Title { get; set; }

    private readonly Dictionary<Color, SolidColorBrush> Brushes = new();
    private readonly TextInstrument Model;
    private RichTextBlock? RichTextBlock;

    public TextInstrumentViewModel(TextInstrument model)
    {
        Model = model;

        Title = model.Title;

        if (model.BackgroundColor.HasValue)
        {
            Background = new SolidColorBrush(ColorUtil.ToColor(model.BackgroundColor.Value));
        }

        if (model.ForegroundColor.HasValue)
        {
            Foreground = new SolidColorBrush(ColorUtil.ToColor(model.ForegroundColor.Value));
        }
    }

    public void Bind(RichTextBlock rtb)
    {
        RichTextBlock = rtb;

        RichTextBlock.Blocks.Clear();

        App.TryEnqueue(() =>
        {
            lock (Model)
            {
                foreach (var line in Model.TextCollection)
                {
                    PushLine(line);
                }
            }
        });
        Model.TextCollection.CollectionChanged += TextCollection_CollectionChanged;
    }

    private void TextCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (RichTextBlock is null)
        {
            return;
        }

        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
        {
            App.TryEnqueue(() =>
            {
                foreach (var line in e.NewItems.OfType<TextLine>())
                {
                    PushLine(line);
                }
            });
        }
    }

    private void PushLine(TextLine line)
    {
        if (RichTextBlock is null)
        {
            throw new InvalidOperationException();
        }

        var run = new Run
        {
            Text = line.Text
        };
        if (line.Foreground.HasValue)
        {
            run.Foreground = GetBrush(ColorUtil.ToColor(line.Foreground.Value));
        }
        if (line.IsBold.HasValue && line.IsBold == true)
        {
            run.FontWeight = FontWeights.Bold;
        }
        if (line.IsItalic.HasValue && line.IsItalic == true)
        {
            run.FontStyle = FontStyle.Italic;
        }

        var block = new Paragraph();
        block.Inlines.Add(run);

        if (line.ExtraData is System.Drawing.Bitmap bmp)
        {
            block.Inlines.Add(new InlineUIContainer
            {
                Child = new Image
                {
                    Source = BitmapHelper.ToImageSource(bmp),
                    Stretch = Stretch.None,
                },
            });
        }

        RichTextBlock.Blocks.Add(block);
    }

    private SolidColorBrush GetBrush(Color color)
    {
        if (!Brushes.TryGetValue(color, out var brush))
        {
            brush = new SolidColorBrush(color);
            Brushes.Add(color, brush);
        }

        return brush;
    }
}
