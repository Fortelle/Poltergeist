using System.Numerics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.UI.Controls.Options;

namespace Poltergeist.UI.Controls;

public sealed partial class OptionControl : UserControl
{
    public OptionControl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ObservableParameterItem item)
        {
            Content = new TextBlock()
            {
                Text = $"Unsupported DataContext: {DataContext?.GetType().Name ?? "(null)"}"
            };
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
            ParameterDefinition<bool> => new SwitchOptionControl(item),

            ParameterDefinition<string[]> => new StringArrayOptionControl(item),

            INumberOption { Layout: NumberOptionLayout.NumberBox } => new NumberBoxOptionControl(item),
            INumberOption { Layout: NumberOptionLayout.Slider } => new SliderOptionControl(item),
            ParameterDefinition<int> or
            ParameterDefinition<byte> or
            ParameterDefinition<decimal> or
            ParameterDefinition<double> or
            ParameterDefinition<Half> or
            ParameterDefinition<short> or
            ParameterDefinition<int> or
            ParameterDefinition<long> or
            ParameterDefinition<sbyte> or
            ParameterDefinition<float> or
            ParameterDefinition<ushort> or
            ParameterDefinition<uint> or
            ParameterDefinition<ulong> => new NumberBoxOptionControl(item),

            ParameterDefinition<TimeOnly> => new TimeOnlyOptionControl(item),

            ParameterDefinition<HotKey> => new HotKeyOptionControl(item),

            PathOption => new PickerOptionControl(item),

            PasswordOption => new PasswordOptionControl(item),

            TextOption { Multiline: true } => new MultilineTextOptionControl(item),
            TextOption or ParameterDefinition<string> => new TextBoxOptionControl(item),

            _ => new TextBlock()
            {
                Text = item.DisplayValue,
                HorizontalAlignment = HorizontalAlignment.Right,
            },
        };
    }
}
