using System;
using System.Windows.Media;
using Windows.UI;

namespace Poltergeist.Automations.Instruments;

public class ListInstrumentItem
{
    public string Key { get; set; }

    public ProgressStatus Status { get; set; }
    public string Text { get; set; }
    public string Subtext { get; set; }

    public ListInstrumentItem()
    {
    }

    public ListInstrumentItem(string key, ProgressStatus status, string text, string subtext = null)
    {
        Key = key;
        Status = status;
        Text = text;
        Subtext = subtext;
    }

    public ListInstrumentItem(ProgressStatus status, string text, string subtext = null)
    {
        Status = status;
        Text = text;
        Subtext = subtext;
    }

    // 	Puzzle
    // Ringer

    public override string ToString()
    {
        var s = Text;
        if (!string.IsNullOrEmpty(Subtext))
        {
            s += "(" + Subtext + ")";
        }
        return s;
    }
}



public class ListInstrumentItemViewModel
{
    public string Key { get; set; }

    public ProgressStatus Status { get; set; }
    public string Text { get; set; }
    public string Subtext { get; set; }

    public SolidColorBrush Background { get; set; }
    public SolidColorBrush Foreground { get; set; }
    public string Glyph { get; set; }

    public ListInstrumentItemViewModel(ListInstrumentItem item)
    {
        Key = item.Key;
        Status = item.Status;
        Text = item.Text;
        Subtext = item.Subtext;
    }
}
