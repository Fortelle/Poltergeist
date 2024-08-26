using Microsoft.UI.Xaml.Controls;
using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Views;

namespace Poltergeist.Services;

public class TipService
{
    public static void Show(TipModel model)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            var teachingTip = new TeachingTip()
            {
                Name = model.Id,
                Title = model.Title,
                Subtitle = model.Text,
            };

            if (!string.IsNullOrEmpty(model.Glyph))
            {
                teachingTip.IconSource = new FontIconSource()
                {
                    Glyph = model.Glyph,
                };
            }
            else if (model.Type != TipType.None)
            {
                teachingTip.IconSource = new FontIconSource()
                {
                    Glyph = model.Type switch
                    {
                        TipType.Information => "\uE946",
                        TipType.Exclamation => "\uE7BA",
                        TipType.Question => "\uE9CE",
                        TipType.Disabled => "\uF140",
                        TipType.Error => "\uEA39",
                        _ => null,
                    }
                };
            }

            Show(teachingTip);

            if (model.Timeout > 0)
            {
                Task.Delay(model.Timeout.Value).ContinueWith(_ =>
                {
                    teachingTip.DispatcherQueue.TryEnqueue(() =>
                    {
                        teachingTip.IsOpen = false;
                    });
                });
            }
        });
    }

    private static void Show(TeachingTip teachingTip)
    {
        teachingTip.PreferredPlacement = TeachingTipPlacementMode.Top;
        teachingTip.IsLightDismissEnabled = true;

        teachingTip.Closed += (s, e) =>
        {
            App.GetService<ShellPage>().Resources.Remove(teachingTip.Name);
        };
        App.GetService<ShellPage>().Resources.Add(teachingTip.Name, teachingTip);

        teachingTip.IsOpen = true;
    }

}
