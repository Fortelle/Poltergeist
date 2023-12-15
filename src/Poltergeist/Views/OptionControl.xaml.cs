using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Parameters;
using Poltergeist.Input.Windows;
using Poltergeist.Views.Options;

namespace Poltergeist.Views;

public sealed partial class OptionControl : UserControl
{
    public OptionControl()
    {
        this.InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if(DataContext is not IOptionItem item)
        {
            return;
        }

        UpdateContent(item);
    }

    private void UpdateContent(IOptionItem item)
    {
        Content = item switch
        {
            IIndexChoiceOptionItem { Mode: ChoiceOptionMode.ComboBox } => new ComboBoxOptionControl(item),
            IIndexChoiceOptionItem { Mode: ChoiceOptionMode.Slider } x => new SliderChoiceOptionControl(x),
            IChoiceOptionItem { Mode: ChoiceOptionMode.ComboBox } => new ComboBoxOptionControl(item),
            IChoiceOptionItem { Mode: ChoiceOptionMode.Slider } x => new SliderChoiceOptionControl(x),
            IChoiceOptionItem { Mode: ChoiceOptionMode.ToggleButtons } => new ToggleButtonOptionControl(item),

            { BaseType.IsEnum: true } => new ComboBoxOptionControl(item),

            BoolOption { Mode: BoolOptionMode.ToggleSwitch } => new SwitchOptionControl(item),
            BoolOption { Mode: BoolOptionMode.CheckBox } => new CheckBoxOptionControl(item),
            BoolOption { Mode: BoolOptionMode.ToggleButtons } => new ToggleButtonOptionControl(item),
            BoolOption { Mode: BoolOptionMode.LeftRightSwitch } => new LeftRightSwitchOptionControl(item),
            OptionItem<bool> => new SwitchOptionControl(item),

            OptionItem<string[]> x => new StringArrayOptionControl(x),

            INumberOptionItem { Layout: NumberOptionLayout.NumberBox } => new NumberBoxOptionControl(item),
            INumberOptionItem { Layout: NumberOptionLayout.Slider } => new SliderOptionControl(item),
            OptionItem<int> or
            OptionItem<byte> or
            OptionItem<decimal> or
            OptionItem<double> or
            OptionItem<Half> or
            OptionItem<short> or
            OptionItem<int> or
            OptionItem<long> or
            OptionItem<sbyte> or
            OptionItem<float> or
            OptionItem<ushort> or
            OptionItem<uint> or
            OptionItem<ulong> => new NumberBoxOptionControl(item),

            OptionItem<TimeOnly> x => new TimeOnlyOptionControl(x),

            OptionItem<HotKey> x => new HotKeyOptionControl(x),

            PathOption pathOption => new PickerOptionControl(pathOption),

            PasswordOption passwordOption => new PasswordOptionControl(passwordOption),

            TextOption { Multiline: true } x => new MultilineTextOptionControl(x),
            TextOption or OptionItem<string> => new TextBoxOptionControl(item),

            _ => new TextBlock()
            {
                Text = item.DisplayValue,
                HorizontalAlignment = HorizontalAlignment.Right,
            },
        };
    }
}
