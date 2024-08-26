using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Views.Options;

public sealed partial class HotKeyOptionControl : UserControl
{
    private ObservableParameterItem Item { get; }

    private VirtualKey[] VirtualKeys { get; }

    private HotKey Value
    {
        get => Item.Value is HotKey x ? x : default;
        set => Item.Value = value;
    }

    private bool Ctrl
    {
        get => Value.HasModifier(KeyModifiers.Control);
        set => Value = new(Value.KeyCode, Value.Modifiers ^ KeyModifiers.Control);
    }

    private bool Shift
    {
        get => Value.HasModifier(KeyModifiers.Shift);
        set => Value = new(Value.KeyCode, Value.Modifiers ^ KeyModifiers.Shift);
    }

    private bool Alt
    {
        get => Value.HasModifier(KeyModifiers.Alt);
        set => Value = new(Value.KeyCode, Value.Modifiers ^ KeyModifiers.Alt);
    }

    private bool Win
    {
        get => Value.HasModifier(KeyModifiers.Win);
        set => Value = new(Value.KeyCode, Value.Modifiers ^ KeyModifiers.Win);
    }

    private VirtualKey KeyCode
    {
        get => Value.KeyCode;
        set
        {
            var newValue = new HotKey(value, Value.Modifiers);
            if(Value.ToString() == newValue.ToString())
            {
                return;
            }

            Item.Value = newValue;
        }
    }

    private string ModifierText
    {
        get
        {
            var s = "";
            if (Value.HasModifier(KeyModifiers.Win))
            {
                s += "Win+";
            }
            if (Value.HasModifier(KeyModifiers.Alt))
            {
                s += "Alt+";
            }
            if (Value.HasModifier(KeyModifiers.Control))
            {
                s += "Ctrl+";
            }
            if (Value.HasModifier(KeyModifiers.Shift))
            {
                s += "Shift+";
            }
            if (s.Length == 0)
            {
                s = App.Localize("Poltergeist/Resources/HotKeyOption_Modifiers");
            }
            else
            {
                s = s.TrimEnd('+');
            }
            return s;
        }
    }

    public HotKeyOptionControl(ObservableParameterItem item)
    {
        if (item.Definition is not OptionDefinition<HotKey>)
        {
            throw new NotSupportedException();
        }

        VirtualKeys = Enum.GetValues<VirtualKey>();

        Item = item;

        InitializeComponent();
    }

    private void MenuFlyout_Closed(object sender, object e)
    {
        ModifierDropDownButton.Content = ModifierText;
    }
}
