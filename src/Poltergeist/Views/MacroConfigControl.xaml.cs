using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using Poltergeist.Automations.Configs;
using Poltergeist.Common.Converters;

namespace Poltergeist.Views;

public sealed partial class MacroConfigControl : UserControl
{
    public event EventHandler ItemUpdated;

    public MacroConfigControl()
    {
        InitializeComponent();
    } 

    private void ContentControl_Loaded(object sender, RoutedEventArgs e)
    {
        var control = (ContentControl)sender;
        var item = (IOptionItem)control.DataContext;

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
        if(item is UndefinedOptionItem)
        {
            label.FontStyle = FontStyles.Italic;
            label.ToolTip = "Undefined option";
        }
        else if (!string.IsNullOrEmpty(item.Description))
        {
            label.ToolTip = item.Description;
        }
        grid.Children.Add(label);

        var width = 160;
        var binding = new Binding()
        {
            Path = new PropertyPath("Value"),
            Mode = BindingMode.TwoWay,
            NotifyOnSourceUpdated = true,
            //UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
        };
        var isBlock = false;
        FrameworkElement element;
        switch (item)
        {
            case FileOptionItem foi:
                {
                    element = CreatePathControl();
                    element.Width = width;
                    BindingOperations.SetBinding(element, DataContextProperty, binding);
                }
                break;
            case IChoiceOptionItem coi:
                {
                    element = new ComboBox()
                    {
                        ItemsSource = coi.Choices,
                        Width = width,
                    };
                    BindingOperations.SetBinding(element, ComboBox.SelectedItemProperty, binding);
                }
                break;
            case OptionItem<string>:
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
            case OptionItem<bool>:
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
            case OptionItem<int>:
            case OptionItem<double>:
                {
                    element = new ModernWpf.Controls.NumberBox()
                    {
                        SpinButtonPlacementMode = ModernWpf.Controls.NumberBoxSpinButtonPlacementMode.Compact,
                        Width = width,
                    };
                    BindingOperations.SetBinding(element, ModernWpf.Controls.NumberBox.ValueProperty, binding);
                }
                break;
            case IOptionItem x when x.Type.IsEnum:
                {
                    var source = Enum.GetValues(x.Type);
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
                        Width = width,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        ToolTip = item.Value.ToString(),
                    };
                }
                break;
        }
        if (item.IsReadonly)
        {
            element.IsEnabled = false;
        }

        element.SourceUpdated += (s, args) =>
        {
            item.HasChanged = true;
            ItemUpdated?.Invoke(this, new());
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
    
    private static FrameworkElement CreatePathControl()
    {
        var txtbox = new TextBox()
        {
            IsReadOnly = true,
        };
        var button = new Button()
        {
            Content = new TextBlock()
            {
                Text = "...",
            },
            Margin = new Thickness(4, 0, 0, 0),
        };
        var grid = new Grid()
        {
            ColumnDefinitions =
            {
                new() { Width = new GridLength(1, GridUnitType.Star) },
                new() { Width = new GridLength(0, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new()
            },
            Children =
            {
                txtbox,
                button,
            },
        };
        Grid.SetColumn(txtbox, 0);
        Grid.SetColumn(button, 1);
        button.Click += (_, _) =>
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                var path = ofd.FileName;
                grid.DataContext = path;
            }
        };
        grid.DataContextChanged += (_, e) =>
        {
            var path = (string)e.NewValue;
            if (File.Exists(path))
            {
                txtbox.Text = Path.GetFileName(path);
            }
        };
        return grid;
    }

    private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
    {
        e.Accepted = ((IOptionItem)e.Item).IsBrowsable;
    }
}
