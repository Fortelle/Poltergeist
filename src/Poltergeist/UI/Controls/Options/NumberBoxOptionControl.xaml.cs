using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Controls.Options;

public sealed partial class NumberBoxOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private double Minimum { get; } = double.MinValue;

    private double Maximum { get; } = double.MaxValue;

    private double SmallChange { get; } = 1;

    private double Value
    {
        get => Item.Value switch
        {
            double => (double)Item.Value,
            Half => (double)Item.Value,
            _ => Convert.ToDouble(Item.Value),
        };
        set => Item.Value = Item.Value switch
        {
            sbyte => Convert.ToSByte(value),
            byte => Convert.ToByte(value),
            short => Convert.ToInt16(value),
            ushort => Convert.ToUInt16(value),
            int => Convert.ToInt32(value),
            uint => Convert.ToUInt32(value),
            long => Convert.ToInt64(value),
            ulong => Convert.ToUInt64(value),
            nint => Convert.ToInt64(value),
            nuint => Convert.ToUInt32(value),
            Half => (Half)value,
            float => Convert.ToSingle(value),
            double => value,
            decimal => Convert.ToDecimal(value),
            _ => throw new NotSupportedException(),
        };
    }

    public NumberBoxOptionControl(ObservableParameterItem item)
    {
        if (item is INumberOption numberOption)
        {
            if (numberOption.Minimum.HasValue)
            {
                Minimum = numberOption.Minimum.Value;
            }
            if (numberOption.Maximum.HasValue)
            {
                Maximum = numberOption.Maximum.Value;
            }
            if (numberOption.StepFrequency.HasValue)
            {
                SmallChange = numberOption.StepFrequency.Value;
                if (SmallChange <= 0)
                {
                    SmallChange = 1;
                }
            }
        }

        Item = item;

        InitializeComponent();
    }

}
