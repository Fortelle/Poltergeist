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
        if (DataContext is not ObservableParameterItem item)
        {
            return;
        }

        UpdateContent(item);
    }

    private void UpdateContent(ObservableParameterItem item)
    {
        Content = item.Definition switch
        {
            IIndexChoiceOption { Mode: ChoiceOptionMode.ComboBox } => new ComboBoxOptionControl(item),
            IIndexChoiceOption { Mode: ChoiceOptionMode.Slider } => new SliderChoiceOptionControl(item),
            IChoiceOption { Mode: ChoiceOptionMode.ComboBox } => new ComboBoxOptionControl(item),
            IChoiceOption { Mode: ChoiceOptionMode.Slider } => new SliderChoiceOptionControl(item),
            IChoiceOption { Mode: ChoiceOptionMode.ToggleButtons } => new ToggleButtonOptionControl(item),

            RatingOption => new RatingOptionControl(item),

            { BaseType.IsEnum: true } => new ComboBoxOptionControl(item),

            BoolOption { Mode: BoolOptionMode.ToggleSwitch } => new SwitchOptionControl(item),
            BoolOption { Mode: BoolOptionMode.CheckBox } => new CheckBoxOptionControl(item),
            BoolOption { Mode: BoolOptionMode.ToggleButtons } => new ToggleButtonOptionControl(item),
            BoolOption { Mode: BoolOptionMode.LeftRightSwitch } => new LeftRightSwitchOptionControl(item),
            OptionDefinition<bool> => new SwitchOptionControl(item),

            OptionDefinition<string[]> => new StringArrayOptionControl(item),

            INumberOption { Layout: NumberOptionLayout.NumberBox } => new NumberBoxOptionControl(item),
            INumberOption { Layout: NumberOptionLayout.Slider } => new SliderOptionControl(item),
            OptionDefinition<int> or
            OptionDefinition<byte> or
            OptionDefinition<decimal> or
            OptionDefinition<double> or
            OptionDefinition<Half> or
            OptionDefinition<short> or
            OptionDefinition<int> or
            OptionDefinition<long> or
            OptionDefinition<sbyte> or
            OptionDefinition<float> or
            OptionDefinition<ushort> or
            OptionDefinition<uint> or
            OptionDefinition<ulong> => new NumberBoxOptionControl(item),

            OptionDefinition<TimeOnly> => new TimeOnlyOptionControl(item),

            OptionDefinition<HotKey> => new HotKeyOptionControl(item),

            PathOption => new PickerOptionControl(item),

            PasswordOption => new PasswordOptionControl(item),

            TextOption { Multiline: true } => new MultilineTextOptionControl(item),
            TextOption or OptionDefinition<string> => new TextBoxOptionControl(item),

            _ => new TextBlock()
            {
                Text = item.DisplayValue,
                HorizontalAlignment = HorizontalAlignment.Right,
            },
        };
    }
}
