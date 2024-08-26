namespace Poltergeist.Automations.Utilities.Windows;

public readonly struct HotKey : IEquatable<HotKey>
{
    public VirtualKey KeyCode { get; init; }

    public KeyModifiers Modifiers { get; init; }

    public HotKey()
    {
        KeyCode = VirtualKey.None;
        Modifiers = KeyModifiers.None;
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

    public bool Equals(HotKey other)
    {
        return Modifiers == other.Modifiers && KeyCode == other.KeyCode;
    }

    public override bool Equals(object? obj)
    {
        return obj is HotKey key && Equals(key);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(KeyCode, Modifiers);
    }

    public static bool operator ==(HotKey left, HotKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HotKey left, HotKey right)
    {
        return !(left == right);
    }

    public static bool operator ==(HotKey left, VirtualKey right)
    {
        return left.Modifiers == KeyModifiers.None && left.KeyCode == right;
    }

    public static bool operator !=(HotKey left, VirtualKey right)
    {
        return !(left == right);
    }


}
