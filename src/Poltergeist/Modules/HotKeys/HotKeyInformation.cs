using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Modules.HotKeys;

public class HotKeyInformation
{
    public string Name { get; }

    public required Action Callback { get; init; }

    public OptionDefinition<HotKey>? SettingDefinition { get; init; }

    public HotKey? HotKey { get; set; }

    public HotKeyInformation(string name)
    {
        Name = name;
    }

    public HotKeyInformation(string name, HotKey hotkey, Action callback) : this(name)
    {
        HotKey = hotkey;
        Callback = callback;
    }

    public HotKeyInformation(string name, OptionDefinition<HotKey> definition) : this(name)
    {
        SettingDefinition = definition;
    }
}
