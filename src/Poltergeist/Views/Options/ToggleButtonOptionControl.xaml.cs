using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Poltergeist.Automations.Configs;

namespace Poltergeist.Views.Options;

[ObservableObject]
public sealed partial class ToggleButtonOptionControl : UserControl
{
    private IOptionItem Item { get; }

    private ChoiceEntry[] Choices { get; set; }
    private double SelectedIndex = 0;

    public ToggleButtonOptionControl(IOptionItem item)
    {
        InitializeComponent();

        switch (item)
        {
            case IIndexChoiceOptionItem { Mode: ChoiceOptionMode.ToggleButtons } icoi:
                {
                    Item = icoi;
                    Choices = icoi.GetChoices();
                    SelectedIndex = (int)item.Value!;
                }
                break;
            case IChoiceOptionItem { Mode: ChoiceOptionMode.ToggleButtons } coi:
                {
                    Item = coi;
                    Choices = coi.GetChoices();
                    var text = item.Value!.ToString();
                    SelectedIndex = Array.FindIndex(Choices, x => x.Value!.ToString() == text);
                }
                break;
            case BoolOption { Mode: BoolOptionMode.ToggleButtons } boi:
                {
                    Item = boi;
                    Choices = new ChoiceEntry[] {
                        new(true, boi.OnText ?? boi.Text ?? "\u2713"),
                        new(false, boi.OffText ?? boi.Text ?? "\u2715"),
                    };
                    var text = item.Value!.ToString();
                    SelectedIndex = Array.FindIndex(Choices, x => x.Value!.ToString() == text);
                }
                break;
            default:
                throw new NotSupportedException();
        }

        for (var i = 0; i < Choices.Length; i++)
        {
            ButtonGroupGrid.ColumnDefinitions.Add(new() { Width = new (1, Microsoft.UI.Xaml.GridUnitType.Star) });
            var child = new ToggleButton()
            {
                IsChecked = i == SelectedIndex,
                Content = Choices[i].Text,
                HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                Tag = i,
            };
            child.Click += Child_Click;
            Grid.SetColumn(child, i);
            ButtonGroupGrid.Children.Add(child);
        }
    }

    private void Child_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var index = (int)((ToggleButton)sender).Tag;

        for (var i = 0; i < ButtonGroupGrid.Children.Count; i++)
        {
            var button = (ToggleButton)ButtonGroupGrid.Children[i];
            button.IsChecked = i == index;
        }

        if(index == SelectedIndex)
        {
            return;
        }
        SelectedIndex = index;

        switch (Item)
        {
            case IIndexChoiceOptionItem:
                {
                    if (index.ToString() == Item.Value!.ToString())
                    {
                        break;
                    }

                    Item.Value = index;
                }
                break;
            case IChoiceOptionItem or BoolOption:
                {
                    if (Choices[index].Value!.ToString() == Item.Value!.ToString())
                    {
                        break;
                    }

                    Item.Value = Choices[index].Value;
                }
                break;
            default:
                throw new NotSupportedException();
        }
    }

}
