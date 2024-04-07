using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Views.Options;

public sealed partial class TimeOnlyOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private TimeOnly Value
    {
        get => Item.Value is TimeOnly x ? x : default;
        set => Item.Value = value;
    }

    private double Hour
    {
        get => Value.Hour;
        set => Value = new((int)value, Value.Minute, Value.Second);
    }

    private double Minute
    {
        get => Value.Minute;
        set => Value = new(Value.Hour, (int)value, Value.Second);
    }

    private double Second
    {
        get => Value.Second;
        set => Value = new(Value.Hour, Value.Minute, (int)value);
    }

    public TimeOnlyOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is not OptionDefinition<TimeOnly>)
        {
            throw new NotSupportedException();
        }

        Item = item;

        InitializeComponent();
    }

}
