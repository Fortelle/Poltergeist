using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Controls.Options;

public sealed partial class RatingOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private double Value
    {
        get => (Item.Value is int x && x > 0) ? Convert.ToDouble(x) : -1;
        set => Item.Value = value <= 0 ? 0 : Convert.ToInt32(value);
    }

    private bool IsClearEnabled { get; }

    private int MaxRating { get; }

    private string Caption { get; }

    public RatingOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is not RatingOption ratingOption)
        {
            throw new NotSupportedException();
        }

        IsClearEnabled = ratingOption.AllowsEmpty;
        MaxRating = ratingOption.MaxRating;

        Item = item;

        Caption = GetCaption();

        InitializeComponent();
    }

    private string GetCaption()
    {
        var ratingOption = (RatingOption)Item.Definition;
        var value = (int)Item.Value!;

        if (ratingOption.CaptionMethod is not null)
        {
            return ratingOption.CaptionMethod(value);
        }
        else if (ratingOption.CaptionList is not null)
        {
            return value >= 0 && value < ratingOption.CaptionList.Length
                ? ratingOption.CaptionList[value]
                : "";
        }
        else if (ratingOption.Caption is not null)
        {
            return ratingOption.Caption;
        }
        else if (value > 0)
        {
            return value.ToString();
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
