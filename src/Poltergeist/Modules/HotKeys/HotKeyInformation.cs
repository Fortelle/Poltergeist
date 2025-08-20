using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Structures.Parameters;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Modules.HotKeys;

public class HotKeyInformation
{
    public string Name { get; }

    public required Action<HotKey> Callback { get; init; }

    public OptionDefinition<HotKey>? SettingDefinition { get; init; }

    public HotKey? HotKey { get; set; }

    public HotKeyInformation(string name)
    {
        Name = name;
    }

    [SetsRequiredMembers]
    public HotKeyInformation(string name, HotKey hotkey, Action<HotKey> callback) : this(name)
    {
        HotKey = hotkey;
        Callback = callback;
    }

    public HotKeyInformation(string name, OptionDefinition<HotKey> definition) : this(name)
    {
        SettingDefinition = definition;
    }
}
