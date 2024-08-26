using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Operations.Foreground;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Background;

public class BackgroundKeyboardService : MacroService
{
    private readonly BackgroundLocatingService Locating;
    private readonly RandomEx Random;

    private KeyboardInputOptions DefaultOptions { get; }

    public KeyboardInputMode Mode { get; set; } = KeyboardInputMode.Scancode; // todo: not supported yet

    public BackgroundKeyboardService(
        MacroProcessor processor,
        BackgroundLocatingService locating,
        RandomEx random,
        IOptions<KeyboardInputOptions> options
        ) : base(processor)
    {
        Locating = locating;
        Random = random;
        DefaultOptions = options.Value;
    }

    public void KeyPress(VirtualKey key, KeyboardInputOptions? options = null)
    {
        //Logger.Debug($"Simulating key press: {{{key}}}.", options);

        var (min, max) = options?.PressTime ?? DefaultOptions?.PressTime ?? (0, 0);
        var interval = Random.Next(min, max);

        Locating.SendMessage.KeyDown(key);
        DoDelay(interval);
        Locating.SendMessage.KeyUp(key);

        Logger.Debug($"Simulated key press: {{{key}}}.", new { min, max, interval });
    }

    public void KeyDown(VirtualKey key)
    {
        //Logger.Debug($"Simulating key down: {{{key}}}.");

        Locating.SendMessage.KeyDown(key);

        Logger.Debug($"Simulated key down: {{{key}}}.");
    }

    public void KeyUp(VirtualKey key)
    {
        //Logger.Debug($"Simulating key up: {{{key}}}.");

        Locating.SendMessage.KeyUp(key);

        Logger.Debug($"Simulated key up: {{{key}}}.");
    }

    private static void DoDelay(int timeout)
    {
        if (timeout == 0)
        {
            return;
        }

        Thread.Sleep(timeout);
    }

}
