using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Views.Options;

public sealed partial class NumberBoxOptionControl : UserControl
{
    private IOptionItem Item { get; }
    private double Minimum { get; } = double.MinValue;
    private double Maximum { get; } = double.MaxValue;
    private double SmallChange { get; } = 1;

    private double Value
    {
        get => Convert.ToDouble(Item.Value);
        set => Item.Value = Item.Value switch
        {
            byte => Convert.ToByte(value),
            decimal => Convert.ToDecimal(value),
            double => Convert.ToDouble(value),
            Half => Convert.ToDouble(value),
            short => Convert.ToInt16(value),
            int => Convert.ToInt32(value),
            long => Convert.ToInt64(value),
            sbyte => Convert.ToSByte(value),
            float => Convert.ToSingle(value),
            ushort => Convert.ToUInt16(value),
            uint => Convert.ToUInt32(value),
            ulong => Convert.ToUInt64(value),
            _ => throw new NotSupportedException(),
        };
    }

    public NumberBoxOptionControl(IOptionItem item)
    {
        InitializeComponent();

        Item = item;

        if (item is INumberOptionItem numberOption)
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
                if( SmallChange <= 0)
                {
                    SmallChange = 1;
                }
            }
        }

    }

}
