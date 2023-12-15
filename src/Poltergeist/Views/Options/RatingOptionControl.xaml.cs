using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class RatingOptionControl : UserControl
{
    private RatingOption Item { get; }

    private double Value
    {
        get => Item.Value <= 0 ? -1 : Convert.ToDouble(Item.Value);
        set => Item.Value = value <= 0 ? 0 : Convert.ToInt32(value);
    }

    public RatingOptionControl(RatingOption item)
    {
        InitializeComponent();

        Item = item;

        RatingControl1.Caption = GetCaption();
    }

    private string GetCaption()
    {
        if (Item.CaptionMethod is not null)
        {
            return Item.CaptionMethod(Item.Value);
        }
        else if (Item.CaptionList is not null)
        {
            return Item.Value >= 0 && Item.Value < Item.CaptionList.Length
                ? Item.CaptionList[Item.Value]
                : "";
        }
        else if (Item.Caption is not null)
        {
            return Item.Caption;
        }
        else if (Item.Value > 0)
        {
            return Item.Value.ToString();
        }
        else
        {
            return "";
        }
    }

    private void RatingControl_ValueChanged(RatingControl sender, object e)
    {
        sender.Caption = GetCaption();
    }
}
