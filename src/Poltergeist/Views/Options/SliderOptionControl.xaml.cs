using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class SliderOptionControl : UserControl
{
    private IOptionItem Item { get; }

    private double Minimum { get; } = double.MinValue;
    private double Maximum { get; } = double.MaxValue;
    private double StepFrequency { get; } = 1;
    private string? ValueFormat { get; }

    [ObservableProperty]
    private string? _text;

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

    public SliderOptionControl(IOptionItem item)
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
                StepFrequency = numberOption.StepFrequency.Value;
                if (StepFrequency <= 0)
                {
                    StepFrequency = 1;
                }
            }
            ValueFormat = numberOption.ValueFormat;
        }

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
