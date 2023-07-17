using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Poltergeist.Automations.Configs;

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class SliderChoiceOptionControl : UserControl
{
    private IChoiceOptionItem Item { get; }

    private object[] Choices { get; set; }

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

        Choices = item.Choices.OfType<object>().ToArray();
        Maximum = Choices.Length - 1;

        if (Item is IIndexOptionItem)
        {
            var value = (int)item.Value!;
            SelectedIndex = (double)value;
            SelectedValue = Choices[value].ToString() ?? "";
        }
        else
        {
            var text = item.Value!.ToString();
            SelectedIndex = Array.FindIndex(Choices, x => x.ToString() == text);
            SelectedValue = text ?? "";
        }
    }

    private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        var index = (int)e.NewValue;
        if(Item is IIndexOptionItem)
        {
            if (index.ToString() == Item.Value!.ToString())
            {
                return;
            }

            Item.Value = index;
        }
        else
        {
            if (Choices[index].ToString() == Item.Value!.ToString())
            {
                return;
            }

            Item.Value = Choices[index];
        }
        
        SelectedValue = Choices[index].ToString() ?? "";
    }
}
