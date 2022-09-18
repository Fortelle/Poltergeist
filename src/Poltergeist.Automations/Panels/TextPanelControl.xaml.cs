using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Poltergeist.Automations.Panels;

public partial class TextPanelControl : UserControl
{
    public TextPanelControl()
    {
        InitializeComponent();
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if(e.OldValue is TextPanelViewModel oldPanel)
        {
            oldPanel.TextRecived -= TextRecived;
        }

        if (e.NewValue is TextPanelViewModel newPanel)
        {
            TextPanelBox.Document.Blocks.Clear();
            if (newPanel.Background != null) TextPanelBox.Background = newPanel.Background;
            if (newPanel.Foreground != null) TextPanelBox.Foreground = newPanel.Foreground;

            newPanel.TextRecived += TextRecived;
        }
    }

    private void TextRecived(TextLine line)
    {
        var block = new Paragraph();

        var run = new Run();
        run.Text = line.Text;
        if (line.Brush != null) run.Foreground = line.Brush;
        if (line.IsBold) run.FontWeight = FontWeights.Bold;
        if (line.IsItalic) run.FontStyle = FontStyles.Italic;

        block.Inlines.Add(run);

        TextPanelBox.Document.Blocks.Add(block);
        TextPanelBox.ScrollToEnd();
    }

}
