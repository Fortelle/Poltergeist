using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Services;

namespace Poltergeist.Pages.Macros.Instruments;

public class ListInstrumentItemViewModel : IDisposable
{
    private const int MaxButtonLimit = 3;

    public string? Key { get; set; }

    public string? Text { get; set; }

    public string? Subtext { get; set; }

    public SolidColorBrush? Background { get; set; }

    public SolidColorBrush? Foreground { get; set; }

    public string? Glyph { get; set; }

    public string? Emoji { get; set; }

    public GridLength ProgressWidth { get; set; }

    public GridLength ProgressWidth2 { get; set; }

    public ButtonViewModel[]? Buttons { get; set; }

    public bool HasIcon => !string.IsNullOrEmpty(Glyph) || !string.IsNullOrEmpty(Emoji);

    private DispatcherTimer? DispatcherTimer { get; set; }

    public ListInstrumentItemViewModel(ListInstrumentItem item)
    {
        Key = item.Key;
        Text = item.Text;
        Subtext = item.Subtext;
        Glyph = item.Glyph;
        Emoji = item.Emoji;

        var progress = item.Progress.HasValue ? Math.Clamp(item.Progress.Value, 0, 1) : 1;
        ProgressWidth = new(progress, GridUnitType.Star);
        ProgressWidth2 = new(1 - progress, GridUnitType.Star);

        if (item.Buttons?.Length > 0)
        {
            Buttons = item.Buttons
                .Take(MaxButtonLimit)
                .Select((x, i) => new ButtonViewModel()
                {
                    Key = x.Key,
                    BaseText = x.Text,
                    Text = x.Text + (x.CountdownSeconds > 0 ? $"({x.CountdownSeconds})" : ""),
                    Argument = x.Argument,
                    Countdown = x.CountdownSeconds,
                })
                .ToArray();

            var countdownButtonIndex = Array.FindIndex(item.Buttons, x => x.CountdownSeconds > 0);
            if (countdownButtonIndex > -1)
            {
                var button = Buttons[countdownButtonIndex];
                var countdown = button.Countdown;
                DispatcherTimer = new()
                {
                    Interval = TimeSpan.FromSeconds(1),
                };
                DispatcherTimer.Tick += (s, e) =>
                {
                    countdown--;
                    button.Text = $"{button.BaseText}({countdown})";
                    if (countdown <= 0)
                    {
                        DispatcherTimer.Stop();
                        var msg = button.Argument is not null ? new InteractionMessage(button.Argument) : new InteractionMessage();
                        PoltergeistApplication.GetService<MacroManager>().SendMessage(msg);
                    }
                };
                DispatcherTimer.Start();
            }
        }
    }

    public void Dispose()
    {
        DispatcherTimer?.Stop();
    }
}
