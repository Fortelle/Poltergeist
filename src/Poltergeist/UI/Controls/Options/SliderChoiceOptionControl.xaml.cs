using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.UI.Controls.Options;

[ObservableObject]
public sealed partial class SliderChoiceOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private ChoiceEntry[] Choices { get; set; }

    [ObservableProperty]
    private double _selectedIndex;

    [ObservableProperty]
    private string _selectedValue;

    private double Minimum { get; } = 0;

    private double Maximum { get; } = double.MaxValue;

    public SliderChoiceOptionControl(ObservableParameterItem item)
    {
        switch (item.Definition)
        {
            case IIndexChoiceOption indexChoiceOption:
                {
                    Choices = indexChoiceOption.GetChoices();
                    Maximum = Choices.Length - 1;

                    var value = (int)item.Value!;
                    SelectedIndex = value;
                    SelectedValue = Choices[value].Value?.ToString() ?? "";
                }
                break;
            case IChoiceOption choiceOption:
                {
                    Choices = choiceOption.GetChoices();
                    Maximum = Choices.Length - 1;

                    var text = item.Value!.ToString();
                    SelectedIndex = Array.FindIndex(Choices, x => x.Value!.ToString() == text);
                    SelectedValue = text ?? "";
                }
                break;
            default:
                throw new NotSupportedException();
        }

        Item = item;

        InitializeComponent();
    }

    private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var index = (int)e.NewValue;
        switch (Item.Definition)
        {
            case IIndexChoiceOption:
                {
                    if (index.ToString() == Item.Value!.ToString())
                    {
                        return;
                    }
                    Item.Value = index;
                }
                break;
            case IChoiceOption:
                {
                    if (Choices[index].Value!.ToString() == Item.Value!.ToString())
                    {
                        return;
                    }
                    Item.Value = Choices[index].Value;
                }
                break;
            default:
                throw new NotSupportedException();
        }

        SelectedValue = Choices[index].Value?.ToString() ?? "";
    }
}
