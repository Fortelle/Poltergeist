using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Controls.Options;

[ObservableObject]
public sealed partial class SliderOptionControl : UserControl
{
    [ObservableProperty]
    public partial string? Text { get; set; }

    private ObservableParameterItem Item { get; }

    private double Minimum { get; } = double.MinValue;

    private double Maximum { get; } = double.MaxValue;

    private double StepFrequency { get; } = 1;

    private string? ValueFormat { get; }

    private double Value
    {
        get => Convert.ToDouble(Item.Value);
        set
        {
            Item.Value = Item.Value switch
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

            UpdateText();
        }
    }

    public SliderOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is INumberOption numberOption)
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
                StepFrequency = numberOption.StepFrequency.Value;
                if (StepFrequency <= 0)
                {
                    StepFrequency = 1;
                }
            }
            ValueFormat = numberOption.ValueFormat;
        }

        Item = item;

        InitializeComponent();

        UpdateText();
    }

    private void UpdateText()
    {
        if (ValueFormat is not null)
        {
            Text = Value.ToString(ValueFormat);
        }
        else if (Item.Value is not null)
        {
            Text = Item.Value.ToString();
        }
        else
        {
            Text = "";
        }
    }

}
