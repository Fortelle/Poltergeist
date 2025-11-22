using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Inputing;

public class KeyboardSendInputService : KeyboardService
{
    public KeyboardSendInputService(
        MacroProcessor processor,
        TimerService timerService,
        IOptions<KeyboardInputOptions> options
        ) : base(processor, timerService, options)
    {
    }

    public void KeyPress(VirtualKey key, KeyboardInputOptions? options = null)
    {
        Logger.Trace($"Simulating key press action.", new { key, options });
        Logger.IncreaseIndent();

        DoKeyPress(key, options);

        Logger.Debug($"Simulated a key press action with key {{{key}}}.");
        Logger.DecreaseIndent();
    }

    public void KeyDown(VirtualKey key, KeyboardInputOptions? options = null)
    {
        Logger.Trace($"Simulating key down action.", new { key, options });
        Logger.IncreaseIndent();

        DoKeyDown(key, options);

        Logger.Debug($"Simulated a key down action with key {{{key}}}.");
        Logger.DecreaseIndent();
    }

    public void KeyUp(VirtualKey key, KeyboardInputOptions? options = null)
    {
        Logger.Trace($"Simulating key up action.", new { key, options });
        Logger.IncreaseIndent();

        DoKeyUp(key, options);

        Logger.Debug($"Simulated a key up action with key {{{key}}}.");
        Logger.DecreaseIndent();
    }

    public void Combine(VirtualKey[] keys, KeyboardInputOptions? options = null)
    {
        Logger.Trace($"Simulating key combination.", new { keys, options });
        Logger.IncreaseIndent();

        DoCombine(keys, options);

        Logger.Debug($"Simulated a key combination of {'{' + string.Join("} + {", keys) + '}'}.");
        Logger.DecreaseIndent();
    }

    public void Input(string text, KeyboardInputOptions? options = null)
    {
        Logger.Trace($"Simulating text input.", new { text, options });
        Logger.IncreaseIndent();

        DoInput(text, options);

        Logger.Debug($"Simulated inputting text: \"{text}\".");
        Logger.DecreaseIndent();
    }

    protected override void DoKeyPress(VirtualKey key, KeyboardInputOptions? options)
    {
        var interval = options?.KeyDownUpInterval ?? DefaultOptions?.KeyDownUpInterval ?? default;
        var mode = options?.Mode ?? DefaultOptions?.Mode ?? KeyboardInputMode.Scancode;

        if (interval == default)
        {
            SendPressInput(key, mode);
        }
        else
        {
            SendKeyDownInput(key, mode);
            Delay(interval);
            SendKeyUpInput(key, mode);
        }
    }

    protected override void DoKeyDown(VirtualKey key, KeyboardInputOptions? options)
    {
        var mode = options?.Mode ?? DefaultOptions?.Mode ?? KeyboardInputMode.Scancode;

        SendKeyDownInput(key, mode);
    }

    protected override void DoKeyUp(VirtualKey key, KeyboardInputOptions? options)
    {
        var mode = options?.Mode ?? DefaultOptions?.Mode ?? KeyboardInputMode.Scancode;

        SendKeyUpInput(key, mode);
    }

    protected override void DoCombine(VirtualKey[] keys, KeyboardInputOptions? options)
    {
        var interval = options?.KeyDownUpInterval ?? DefaultOptions?.KeyDownUpInterval ?? default;
        var mode = options?.Mode ?? DefaultOptions?.Mode ?? KeyboardInputMode.Scancode;

        if (interval == default)
        {
            var input = new SendInputHelper();
            foreach (var key in keys)
            {
                input.AddKey(key, false, mode);
            }
            foreach (var key in keys.Reverse())
            {
                input.AddKey(key, true, mode);
            }
            input.Execute();

            Logger.Trace($"Sent key combination.", new { keys, mode });
        }
        else
        {
            foreach (var key in keys)
            {
                DoKeyUp(key, options);
                Delay(interval);
            }
            foreach (var key in keys.Reverse())
            {
                DoKeyDown(key, options);
                Delay(interval);
            }
        }
    }

    private void SendPressInput(VirtualKey key, KeyboardInputMode mode)
    {
        new SendInputHelper()
            .AddKey(key, false, mode)
            .AddKey(key, true, mode)
            .Execute();

        Logger.Trace($"Sent key down-up input.", new { key, mode });
    }

    private void SendKeyDownInput(VirtualKey key, KeyboardInputMode mode)
    {
        new SendInputHelper()
            .AddKey(key, false, mode)
            .Execute();

        Logger.Trace($"Sent key down input.", new { key, mode });
    }

    private void SendKeyUpInput(VirtualKey key, KeyboardInputMode mode)
    {
        new SendInputHelper()
            .AddKey(key, true, mode)
            .Execute();

        Logger.Trace($"Sent key up input.", new { key, mode });
    }
}
