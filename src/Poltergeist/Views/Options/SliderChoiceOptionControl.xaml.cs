using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Poltergeist.Automations.Configs;

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class SliderChoiceOptionControl : UserControl
{
    private IChoiceOptionItem Item { get; }

    private ChoiceEntry[] Choices { get; set; }

    [ObservableProperty]
    private double _selectedIndex;

    [ObservableProperty]
    private string _selectedValue;

    private double Minimum { get; } = 0;
    private double Maximum { get; } = double.MaxValue;

    public SliderChoiceOptionControl(IChoiceOptionItem item)
    {
        InitializeComponent();

        Item = item;

        Choices = item.GetChoices();
        Maximum = Choices.Length - 1;

        if (Item is IIndexChoiceOptionItem)
        {
            var value = (int)item.Value!;
            SelectedIndex = value;
            SelectedValue = Choices[value].Value?.ToString() ?? "";
        }
        else if (Item is IChoiceOptionItem)
        {
            var text = item.Value!.ToString();
            SelectedIndex = Array.FindIndex(Choices, x => x.Value!.ToString() == text);
            SelectedValue = text ?? "";
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var index = (int)e.NewValue;
        if(Item is IIndexChoiceOptionItem)
        {
            if (index.ToString() == Item.Value!.ToString())
            {
                return;
            }

            Item.Value = index;
        }
        else if (Item is IChoiceOptionItem)
        {
            if (Choices[index].Value!.ToString() == Item.Value!.ToString())
            {
                return;
            }

            Item.Value = Choices[index].Value;
        }
        else
        {
            throw new NotSupportedException();
        }

        SelectedValue = Choices[index].Value?.ToString() ?? "";
    }
}
