using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Poltergeist.Automations.Macros;
using Poltergeist.Common.Converters;

namespace Poltergeist.Views;

public sealed partial class MacroConfigControl : UserControl
{
    public MacroConfigControl()
    {
        InitializeComponent();
    } 

    private void ContentControl_Loaded(object sender, RoutedEventArgs e)
    {
        var control = (ContentControl)sender;
        var item = (OptionItem)control.DataContext;

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new() { Width = new GridLength(0, GridUnitType.Auto) });
        grid.RowDefinitions.Add(new() { Height = new GridLength(36, GridUnitType.Pixel) });
        grid.RowDefinitions.Add(new());
        grid.HorizontalAlignment = HorizontalAlignment.Stretch;
        
        var label = new TextBlock()
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = item.DisplayLabel ?? item.Key,
        };
        grid.Children.Add(label);

        var width = 128;
        var binding = new Binding()
        {
            Path = new PropertyPath("Value"),
            Mode = BindingMode.TwoWay,
            NotifyOnTargetUpdated = true,
        };
        var isBlock = false;
        FrameworkElement element;
        if (item.IsReadonly)
        {
            element = new TextBlock()
            {
                Text = item.Value.ToString(),
            };
        }
        else
        {
            switch (item.Value)
            {
                case int:
                    {
                        element = new ModernWpf.Controls.NumberBox()
                        {
                            SpinButtonPlacementMode = ModernWpf.Controls.NumberBoxSpinButtonPlacementMode.Compact,
                            Width = width,
                        };
                        binding.Converter = new IntToDoubleconverter();
                        BindingOperations.SetBinding(element, ModernWpf.Controls.NumberBox.ValueProperty, binding);
                    }
                    break;
                case string:
                    {
                        element = new TextBox()
                        {
                            Width = width,
                            SpellCheck = {
                                IsEnabled = false,
                            },
                        };
                        BindingOperations.SetBinding(element, TextBox.TextProperty, binding);
                    }
                    break;
                case bool:
                    {
                        element = new ModernWpf.Controls.ToggleSwitch()
                        {
                            OnContent = "",
                            OffContent = "",
                            Margin = new(0, 0, -110, 0),
                            HorizontalAlignment = HorizontalAlignment.Right,
                        };
                        BindingOperations.SetBinding(element, ModernWpf.Controls.ToggleSwitch.IsOnProperty, binding);
                    }
                    break;
                case Enum x:
                    {
                        var source = Enum.GetValues(x.GetType());
                        element = new ComboBox()
                        {
                            ItemsSource = source,
                            Width = width,
                        };
                        BindingOperations.SetBinding(element, ComboBox.SelectedItemProperty, binding);
                    }
                    break;
                default:
                    {
                        element = new TextBlock()
                        {
                            Text = item.Value.ToString(),
                        };
                    }
                    break;
            }

        }

        element.TargetUpdated += (s, args) =>
        {
            item.HasChanged = true;
        };

        grid.Children.Add(element);
        if (isBlock)
        {
            element.HorizontalAlignment = HorizontalAlignment.Stretch;
            element.Margin = new Thickness(32, 0, 0, 0);
            Grid.SetColumn(element, 0);
            Grid.SetRow(element, 1);
            Grid.SetColumnSpan(element, 2);

        }
        else
        {
            element.HorizontalAlignment = HorizontalAlignment.Right;
            element.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(element, 1);
        }

        control.Content = grid;

    }
}
