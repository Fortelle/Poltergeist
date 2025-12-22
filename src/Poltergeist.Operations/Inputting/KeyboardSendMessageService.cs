using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Locating;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Inputting;

public class KeyboardSendMessageService : KeyboardService
{
    private readonly WindowLocatingService WindowLocatingService;

    public KeyboardSendMessageService(
        MacroProcessor processor,
        WindowLocatingService windowLocatingService,
        TimerService timerService,
        IOptions<KeyboardInputOptions> options
        ) : base(processor, timerService, options)
    {
        WindowLocatingService = windowLocatingService;
    }

    public void KeyPress(VirtualKey key, KeyboardInputOptions? options = null)
    {
        Logger.Trace($"Simulating key press action.", new { key, options });
        Logger.IncreaseIndent();

        DoKeyPress(key, options);

        Logger.Debug($"Simulated a key press action with key {{{key}}} on the client window.");
        Logger.DecreaseIndent();
    }

    public void KeyDown(VirtualKey key, KeyboardInputOptions? options = null)
    {
        Logger.Trace($"Simulating key down action.", new { key });
        Logger.IncreaseIndent();

        KeyDown(key, options);

        Logger.Debug($"Simulated a key down action with key {{{key}}} on the client window.");
        Logger.DecreaseIndent();
    }

    public void KeyUp(VirtualKey key, KeyboardInputOptions? options = null)
    {
        Logger.Trace($"Simulating key up action.", new { key });
        Logger.IncreaseIndent();

        DoKeyUp(key, options);

        Logger.Debug($"Simulated a key up action with key {{{key}}} on the client window.");
        Logger.DecreaseIndent();
    }

    public void Combine(VirtualKey[] keys, KeyboardInputOptions? options = null)
    {
        Logger.Trace($"Simulating key combination.", new { keys, options });
        Logger.IncreaseIndent();

        DoCombine(keys, options);

        Logger.Debug($"Simulated a key combination of {'{' + string.Join("} + {", keys) + '}'} on the client window.");
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

        SendKeyDownMessage(key);
        Delay(interval);
        SendKeyUpMessage(key);
    }

    protected override void DoKeyDown(VirtualKey key, KeyboardInputOptions? options)
    {
        SendKeyDownMessage(key);
    }

    protected override void DoKeyUp(VirtualKey key, KeyboardInputOptions? options)
    {
        SendKeyUpMessage(key);
    }

    protected override void DoCombine(VirtualKey[] keys, KeyboardInputOptions? options)
    {
        var interval = options?.KeyDownUpInterval ?? DefaultOptions?.KeyDownUpInterval ?? default;

        foreach (var key in keys)
        {
            SendKeyDownMessage(key);
            Delay(interval);
        }
        foreach (var key in keys.Reverse())
        {
            SendKeyUpMessage(key);
            Delay(interval);
        }
    }

    private void SendKeyDownMessage(VirtualKey key)
    {
        new SendMessageHelper(WindowLocatingService.Handle, Logger)
            .KeyDown(key);
    }

    private void SendKeyUpMessage(VirtualKey key)
    {
        new SendMessageHelper(WindowLocatingService.Handle, Logger)
            .KeyUp(key);
    }
}
