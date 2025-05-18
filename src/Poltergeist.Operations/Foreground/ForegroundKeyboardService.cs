using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Foreground;

public class ForegroundKeyboardService : MacroService
{
    private static readonly Dictionary<char, VirtualKey> KeyMap = new()
    {
        [' '] = VirtualKey.Space,
        ['1'] = VirtualKey.D1,
        ['2'] = VirtualKey.D2,
        ['3'] = VirtualKey.D3,
        ['4'] = VirtualKey.D4,
        ['5'] = VirtualKey.D5,
        ['6'] = VirtualKey.D6,
        ['7'] = VirtualKey.D7,
        ['8'] = VirtualKey.D8,
        ['9'] = VirtualKey.D9,
        ['0'] = VirtualKey.D0,
        [';'] = VirtualKey.OEM_1,
        ['='] = VirtualKey.OEM_Plus,
        [','] = VirtualKey.OEM_Comma,
        ['-'] = VirtualKey.OEM_Minus,
        ['.'] = VirtualKey.OEM_Period,
        ['/'] = VirtualKey.OEM_2,
        ['`'] = VirtualKey.OEM_3,
        ['['] = VirtualKey.OEM_4,
        ['\\'] = VirtualKey.OEM_5,
        [']'] = VirtualKey.OEM_6,
        ['\''] = VirtualKey.OEM_7,
    };

    private static readonly Dictionary<char, VirtualKey> KeyMapShift = new()
    {
        ['!'] = VirtualKey.D1,
        ['@'] = VirtualKey.D2,
        ['#'] = VirtualKey.D3,
        ['$'] = VirtualKey.D4,
        ['%'] = VirtualKey.D5,
        ['^'] = VirtualKey.D6,
        ['&'] = VirtualKey.D7,
        ['*'] = VirtualKey.D8,
        ['('] = VirtualKey.D9,
        [')'] = VirtualKey.D0,
        [':'] = VirtualKey.OEM_1,
        ['+'] = VirtualKey.OEM_Plus,
        ['<'] = VirtualKey.OEM_Comma,
        ['_'] = VirtualKey.OEM_Minus,
        ['>'] = VirtualKey.OEM_Period,
        ['?'] = VirtualKey.OEM_2,
        ['~'] = VirtualKey.OEM_3,
        ['{'] = VirtualKey.OEM_4,
        ['|'] = VirtualKey.OEM_5,
        ['}'] = VirtualKey.OEM_6,
        ['"'] = VirtualKey.OEM_7,
    };

    private readonly RandomEx Random;
    private readonly KeyboardInputOptions DefaultOptions;

    public ForegroundKeyboardService(
        MacroProcessor processor,
        RandomEx random,
        IOptions<KeyboardInputOptions> options
        ) : base(processor)
    {
        Random = random;
        DefaultOptions = options.Value;
    }

    public void KeyPress(VirtualKey key, KeyboardInputOptions? options = null)
    {
        var (min, max) = options?.PressDuration ?? DefaultOptions?.PressDuration ?? (0, 0);
        var mode = options?.Mode ?? DefaultOptions?.Mode ?? KeyboardInputMode.Scancode;

        if (min == 0 || max == 0)
        {
            new SendInputHelper()
                .AddKey(key, false, mode)
                .AddKey(key, true, mode)
                .Execute();
            Logger.Debug($"Simulated a key press message.", new { key, mode });
        }
        else
        {
            var interval = Random.Next(min, max);
            new SendInputHelper().AddKey(key, false, mode).Execute();
            Logger.Trace($"Simulated a key down message.", new { key, mode, min, max, interval });
            DoDelay(interval);
            new SendInputHelper().AddKey(key, true, mode).Execute();
            Logger.Trace($"Simulated a key up message.", new { key, mode, min, max, interval });

        }

        Logger.Debug($"Simulated a key press action.", new { key, mode });
    }

    public void KeyDown(VirtualKey key, KeyboardInputOptions? options = null)
    {
        var mode = options?.Mode ?? DefaultOptions?.Mode ?? KeyboardInputMode.Scancode;
        new SendInputHelper().AddKey(key, false, mode).Execute();
        Logger.Debug($"Simulated a key down message.", new { key, mode });
    }

    public void KeyUp(VirtualKey key, KeyboardInputOptions? options = null)
    {
        var mode = options?.Mode ?? DefaultOptions?.Mode ?? KeyboardInputMode.Scancode;
        new SendInputHelper().AddKey(key, true, mode).Execute();
        Logger.Debug($"Simulated a key up message.", new { key, mode });
    }

    public void Combine(params VirtualKey[] keys)
    {
        var text = '{' + string.Join("} + {", keys) + '}';
        //Logger.Debug($"Simulating key combination: {text}.");

        var (min, max) = DefaultOptions?.PressDuration ?? (0, 0);
        var mode = DefaultOptions?.Mode ?? KeyboardInputMode.Scancode;

        if (min == 0 || max == 0)
        {
            var input = new SendInputHelper();
            foreach (var key in keys)
            {
                input.AddScancodeDown(key);
            }
            foreach (var key in keys.Reverse())
            {
                input.AddScancodeUp(key);
            }
            input.Execute();

            Logger.Debug($"Simulated key combination: {{{text}}}.");
        }
        else
        {
            foreach (var key in keys)
            {
                var interval = Random.Next(min, max);
                new SendInputHelper().AddScancodeDown(key).Execute();
                DoDelay(interval);
            }
            foreach (var key in keys.Reverse())
            {
                var interval = Random.Next(min, max);
                new SendInputHelper().AddScancodeUp(key).Execute();
                DoDelay(interval);
            }

            Logger.Debug($"Simulated key combination: {text}.", new { min, max });
        }

    }

    public void Input(string text, KeyboardInputOptions? options = null)
    {
        //Logger.Debug($"Simulating inputting text: \"{text}\".", options);

        var (min, max) = options?.PressInterval ?? DefaultOptions?.PressInterval ?? (0, 0);
        var beginCapsDown = SendInputHelper.IsCapsLockToggled;
        var currentCapsDown = beginCapsDown;
        var inputedText = "";

        void press(VirtualKey key)
        {
            KeyPress(key, options);
            if (min > 0 && max > 0)
            {
                var interval = Random.Next(min, max);
                DoDelay(interval);
            }
            if(key == VirtualKey.Capital)
            {
                currentCapsDown = !currentCapsDown;
            }
        }

        foreach (var c in text)
        {
            VirtualKey vk;
            bool? requireCapsDown = null;
            var requireShift = false;
            if (c >= 'a' && c <= 'z')
            {
                vk = VirtualKey.A + (byte)(c - 'a');
                requireCapsDown = false;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                vk = VirtualKey.A + (byte)(c - 'A');
                requireCapsDown = true;
            }
            else if (KeyMap.TryGetValue(c, out vk))
            {
            }
            else if (KeyMapShift.TryGetValue(c, out vk))
            {
                requireShift = true;
            }
            else
            {
                Logger.Warn($"Unsupported character: \"{c}\".");
                continue;
            }

            if (requireCapsDown == !currentCapsDown)
            {
                press(VirtualKey.Capital);
            }

            if (requireShift)
            {
                Combine(VirtualKey.Shift, vk);
                if (min > 0 && max > 0)
                {
                    DoDelay(Random.Next(min, max));
                }
            }
            else
            {
                press(vk);
            }

            inputedText += c;
        }

        Logger.Debug($"Simulated inputting text: \"{text}\".");
    }

    private static void DoDelay(int timeout)
    {
        if (timeout <= 0)
        {
            return;
        }

        Thread.Sleep(timeout);
    }

}
