using System;

namespace Poltergeist.Input.Windows;

public struct HotKey : IEquatable<HotKey>, IEquatable<VirtualKey>
{
    public VirtualKey KeyCode
    {
        get; set;
    }

    public KeyModifiers Modifiers
    {
        get; set;
    }

    public HotKey(VirtualKey key)
    {
        KeyCode = key;
        Modifiers = KeyModifiers.None;
    }

    public HotKey(VirtualKey key, KeyModifiers modifiers)
    {
        KeyCode = key;
        Modifiers = modifiers;
    }

    public bool Equals(HotKey other)
    {
        return Modifiers == other.Modifiers && KeyCode == other.KeyCode;
    }

    public bool Equals(VirtualKey keyCode)
    {
        return Modifiers == KeyModifiers.None && KeyCode == keyCode;
    }

    public bool HasModifier(KeyModifiers modifier)
    {
        return (Modifiers & modifier) == modifier;
    }

    public override string ToString()
    {
        var s = "";
        if (HasModifier(KeyModifiers.Win))
        {
            s += "Win+";
        }
        if (HasModifier(KeyModifiers.Alt))
        {
            s += "Alt+";
        }
        if (HasModifier(KeyModifiers.Control))
        {
            s += "Ctrl+";
        }
        if (HasModifier(KeyModifiers.Shift))
        {
            s += "Shift+";
        }
        s += KeyCode.ToString();
        return s;
    }
}
