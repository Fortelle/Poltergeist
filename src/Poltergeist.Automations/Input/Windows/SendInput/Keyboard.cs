
namespace Poltergeist.Input.Windows;

public partial class SendInputHelper
{
    public SendInputHelper AddScancodeUp(VirtualKey key)
    {
        var dwFlags = NativeMethods.KeyEventFlags.ScanCode;
        if (key.IsExtended())
        {
            dwFlags |= NativeMethods.KeyEventFlags.ExtendedKey;
        }
        dwFlags |= NativeMethods.KeyEventFlags.KeyUp;

        AddInput(new()
        {
            type = NativeMethods.InputType.Keyboard,
            inputUnion =
            {
                ki =
                {
                    dwFlags = dwFlags,
                    wScan = (short)NativeMethods.MapVirtualKey((uint)key, NativeMethods.MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC),
                }
            }
        });
        return this;
    }

    public SendInputHelper AddScancodeDown(VirtualKey key)
    {
        var dwFlags = NativeMethods.KeyEventFlags.ScanCode;
        if (key.IsExtended())
        {
            dwFlags |= NativeMethods.KeyEventFlags.ExtendedKey;
        }

        AddInput(new()
        {
            type = NativeMethods.InputType.Keyboard,
            inputUnion =
            {
                ki =
                {
                    dwFlags = dwFlags,
                    wScan = (short)NativeMethods.MapVirtualKey((uint)key, NativeMethods.MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC),
                }
            }
        });
        return this;
    }

    public SendInputHelper AddVkCodeUp(VirtualKey key, bool isUp)
    {
        var dwFlags = NativeMethods.KeyEventFlags.None;
        if (key.IsExtended())
        {
            dwFlags |= NativeMethods.KeyEventFlags.ExtendedKey;
        }
        dwFlags |= NativeMethods.KeyEventFlags.KeyUp;

        AddInput(new()
        {
            type = NativeMethods.InputType.Keyboard,
            inputUnion = {
                ki = {
                    dwFlags = dwFlags,
                    wVk = (short)key,
                },
            },
        });
        return this;
    }

    public SendInputHelper AddVkCodeDown(VirtualKey key, bool isUp)
    {
        var dwFlags = NativeMethods.KeyEventFlags.None;
        if (key.IsExtended())
        {
            dwFlags |= NativeMethods.KeyEventFlags.ExtendedKey;
        }

        AddInput(new()
        {
            type = NativeMethods.InputType.Keyboard,
            inputUnion = {
                ki = {
                    dwFlags = dwFlags,
                    wVk = (short)key,
                },
            },
        });
        return this;
    }

    public SendInputHelper AddUnicodeUp(char c)
    {
        var dwFlags = NativeMethods.KeyEventFlags.Unicode;
        dwFlags |= NativeMethods.KeyEventFlags.KeyUp;

        AddInput(new()
        {
            type = NativeMethods.InputType.Keyboard,
            inputUnion = {
                ki = {
                    dwFlags = dwFlags,
                    wScan = (short)c,
                },
            },
        });
        return this;
    }

    public SendInputHelper AddUnicodeDown(char c)
    {
        var dwFlags = NativeMethods.KeyEventFlags.Unicode;

        AddInput(new()
        {
            type = NativeMethods.InputType.Keyboard,
            inputUnion = {
                ki = {
                    dwFlags = dwFlags,
                    wScan = (short)c,
                },
            },
        });
        return this;
    }
}