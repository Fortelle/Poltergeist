using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Input.Windows;

namespace Poltergeist.Operations.Foreground;

public class ForegroundKeyboardService : MacroService
{
    private KeyboardInputOptions DefaultOptions { get; }
    private RandomEx Random { get; }

    public KeyboardInputMode Mode { get; set; } // todo: not supported yet

    public ForegroundKeyboardService(
        MacroProcessor processor,
        RandomEx random,
        IOptions<KeyboardInputOptions> options
        )
        : base(processor)
    {
        DefaultOptions = options.Value;
        Random = random;
        Mode = KeyboardInputMode.Scancode;

        Logger.Debug($"Initialized <{nameof(ForegroundKeyboardService)}>.", DefaultOptions);
    }

    public void KeyPress(VirtualKey key, KeyboardInputOptions? options = null)
    {
        //Logger.Debug($"Simulating key press: {{{key}}}.", options);

        var (min, max) = options?.PressTime ?? DefaultOptions?.PressTime ?? (0, 0);

        if (min == 0 || max == 0)
        {
            new SendInputHelper()
                .AddScancodeDown(key)
                .AddScancodeUp(key)
                .Execute();

            Logger.Debug($"Simulated key press: {{{key}}}.");
        }
        else
        {
            var interval = Random.Next(min, max);
            new SendInputHelper().AddScancodeDown(key).Execute();
            DoDelay(interval);
            new SendInputHelper().AddScancodeUp(key).Execute();

            Logger.Debug($"Simulated key press: {{{key}}}.", new { min, max, interval });
        }

    }

    public void KeyDown(VirtualKey key)
    {
        //Logger.Debug($"Simulating key down: {{{key}}}.");

        new SendInputHelper().AddScancodeDown(key).Execute();

        Logger.Debug($"Simulated key down: {{{key}}}.");
    }

    public void KeyUp(VirtualKey key)
    {
        //Logger.Debug($"Simulating key up: {{{key}}}.");

        new SendInputHelper().AddScancodeUp(key).Execute();

        Logger.Debug($"Simulated key up: {{{key}}}.");
    }

    public void Combine(params VirtualKey[] keys)
    {
        var text = '{' + string.Join("} + {", keys) + '}';
        //Logger.Debug($"Simulating key combination: {text}.");

        var (min, max) = DefaultOptions?.PressTime ?? (0, 0);

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
                new SendInputHelper().AddScancodeDown(key).Execute();
                DoDelay(Random.Next(min, max));
            }
            foreach (var key in keys.Reverse())
            {
                new SendInputHelper().AddScancodeUp(key).Execute();
                DoDelay(Random.Next(min, max));
            }

            Logger.Debug($"Simulated key combination: {text}.", new { min, max });
        }

    }

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
        if (timeout == 0) return;
        System.Threading.Thread.Sleep(timeout);
    }

}
