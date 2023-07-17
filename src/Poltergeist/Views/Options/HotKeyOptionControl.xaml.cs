using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Configs;
using Poltergeist.Input.Windows;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Poltergeist.Views.Options;

public sealed partial class HotKeyOptionControl : UserControl
{
    private OptionItem<HotKey> Item { get; }
    private VirtualKey[] VirtualKeys { get; }

    private bool Ctrl
    {
        get => Item.Value.HasModifier(KeyModifiers.Control);
        set => Item.Value = new(Item.Value.KeyCode, Item.Value.Modifiers | KeyModifiers.Control ^ (value ? 0 : KeyModifiers.Control));
    }

    private bool Shift
    {
        get => Item.Value.HasModifier(KeyModifiers.Shift);
        set => Item.Value = new(Item.Value.KeyCode, Item.Value.Modifiers | KeyModifiers.Shift ^ (value ? 0 : KeyModifiers.Shift));
    }

    private bool Alt
    {
        get => Item.Value.HasModifier(KeyModifiers.Alt);
        set => Item.Value = new(Item.Value.KeyCode, Item.Value.Modifiers | KeyModifiers.Alt ^ (value ? 0 : KeyModifiers.Alt));
    }

    private bool Win
    {
        get => Item.Value.HasModifier(KeyModifiers.Win);
        set => Item.Value = new(Item.Value.KeyCode, Item.Value.Modifiers | KeyModifiers.Win ^ (value ? 0 : KeyModifiers.Win));
    }

    private VirtualKey KeyCode
    {
        get => Item.Value.KeyCode;
        set
        {
            var newValue = new HotKey(value, Item.Value.Modifiers);
            if(Item.Value.ToString() == newValue.ToString())
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
            if (Item.Value.HasModifier(KeyModifiers.Win))
            {
                s += "Win+";
            }
            if (Item.Value.HasModifier(KeyModifiers.Alt))
            {
                s += "Alt+";
            }
            if (Item.Value.HasModifier(KeyModifiers.Control))
            {
                s += "Ctrl+";
            }
            if (Item.Value.HasModifier(KeyModifiers.Shift))
            {
                s += "Shift+";
            }
            if (s.Length == 0)
            {
                s = "Modifiers";
            }
            else
            {
                s = s.TrimEnd('+');
            }
            return s;
        }
    }

    public HotKeyOptionControl(OptionItem<HotKey> item)
    {
        VirtualKeys = Enum.GetValues<VirtualKey>();

        InitializeComponent();

        Item = item;
    }

    private void MenuFlyout_Closed(object sender, object e)
    {
        ModifierDropDownButton.Content = ModifierText;
    }
}
