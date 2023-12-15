using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class TimeOnlyOptionControl : UserControl
{
    private OptionItem<TimeOnly> Item { get; }

    private double Hour
    {
        get => Item.Value.Hour;
        set => Item.Value = new((int)value, Item.Value.Minute, Item.Value.Second);
    }

    private double Minute
    {
        get => Item.Value.Minute;
        set => Item.Value = new(Item.Value.Hour, (int)value, Item.Value.Second);
    }

    private double Second
    {
        get => Item.Value.Second;
        set => Item.Value = new(Item.Value.Hour, Item.Value.Minute, (int)value);
    }

    public TimeOnlyOptionControl(OptionItem<TimeOnly> item)
    {
        InitializeComponent();

        Item = item;
    }

}
