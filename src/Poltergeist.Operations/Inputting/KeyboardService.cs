using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Inputting;

public abstract class KeyboardService : MacroService
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

    public KeyboardService(
        MacroProcessor processor,
        TimerService timerService,
        IOptions<KeyboardInputOptions> options
        ) : base(processor)
    {
        TimerService = timerService;
        DefaultOptions = options.Value;
    }

    protected readonly TimerService TimerService;
    protected readonly KeyboardInputOptions DefaultOptions;

    protected abstract void DoKeyPress(VirtualKey key, KeyboardInputOptions? options);
    protected abstract void DoKeyDown(VirtualKey key, KeyboardInputOptions? options);
    protected abstract void DoKeyUp(VirtualKey key, KeyboardInputOptions? options);
    protected abstract void DoCombine(VirtualKey[] keys, KeyboardInputOptions? options);

    protected void DoInput(string text, KeyboardInputOptions? options)
    {
        var pressInterval = options?.PressInterval ?? DefaultOptions?.PressInterval ?? default;
        var beginCapsDown = SendInputHelper.IsCapsLockToggled;
        var currentCapsDown = beginCapsDown;
        var inputedText = "";

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
                throw new Exception($"Unsupported character: \"{c}\".");
            }

            if (requireCapsDown == !currentCapsDown)
            {
                DoKeyPress(VirtualKey.Capital, options);
                currentCapsDown = !currentCapsDown;
            }

            if (requireShift)
            {
                DoCombine([VirtualKey.Shift, vk], options);
            }
            else
            {
                DoKeyPress(vk, options);
            }

            inputedText += c;

            Delay(pressInterval);
        }
    }

    protected void Delay(int timeout)
    {
        if (timeout == 0)
        {
            return;
        }
        Thread.Sleep(timeout);
        Logger.Trace($"Delayed for {timeout}ms.");
    }

    protected void Delay(TimeSpanRange range) => Delay(TimerService.GetTimeout(new RangeDelay(range)));
}
