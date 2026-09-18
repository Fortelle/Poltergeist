using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class IconControlExample : CommonOneshotMacroBase
{
    private static readonly string[] BatteryGlyphs = ["\uE850", "\uE851", "\uE852", "\uE853", "\uE854", "\uE855", "\uE856", "\uE857", "\uE858", "\uE859"];
    private static readonly string[] ClockEmojis = ["🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛"];

    public IconControlExample() : base()
    {
        Title = "Icon Examples";

        Category = "Instruments";

        Category = "Features";

        Description = "This example shows the usage of different IconInfo implementations.";

        ShowStatusBar = false;
    }

    protected override void OnExecute(WorkflowController controller)
    {
        var dashboard = controller.Processor.GetService<DashboardService>();

        {
            var instrument = dashboard.Create<ListInstrument>();
            instrument.Title = nameof(GlyphIcon);

            instrument.Add(new()
            {
                Icon = new GlyphIcon("\uE700"),
                Text = nameof(GlyphIcon),
                Subtext = "new GlyphIcon(\"\\uE700\")",
            });
        }

        {
            var instrument = dashboard.Create<ListInstrument>();
            instrument.Title = nameof(EmojiIcon);

            instrument.Add(new()
            {
                Icon = new EmojiIcon("😀"),
                Text = nameof(EmojiIcon),
                Subtext = "new EmojiIcon(\"😀\")",
            });
        }

        {
            var instrument = dashboard.Create<ListInstrument>();
            instrument.Title = nameof(TextIcon);

            instrument.Add(new()
            {
                Icon = new TextIcon("T"),
                Text = $"{nameof(TextIcon)} (One character)",
                Subtext = "new TextIcon(\"T\")",
            });

            instrument.Add(new()
            {
                Icon = new TextIcon("TI"),
                Text = $"{nameof(TextIcon)} (Two characters)",
                Subtext = "new TextIcon(\"TI\")",
            });

            instrument.Add(new()
            {
                Icon = new TextIcon("T")
                {
                    Font = "Segoe Script",
                },
                Text = $"{nameof(TextIcon)} (Custom font)",
                Subtext = "{ Font = \"Segoe Script\" }",
            });
        }

        {
            var instrument = dashboard.Create<ListInstrument>();
            instrument.Title = nameof(UriIcon);

            instrument.Add(new()
            {
                Icon = new UriIcon(@"ms-appx:///Poltergeist/Assets/macro.ico"),
                Text = nameof(UriIcon),
                Subtext = "new UriIcon(\"ms-appx:///Poltergeist/Assets/macro.ico\")",
            });
        }

        {
            var instrument = dashboard.Create<ListInstrument>();
            instrument.Title = nameof(SpinAnimation);

            instrument.Add(new()
            {
                Icon = new GlyphIcon("\uF16A")
                {
                    Animation = new SpinAnimation(),
                },
                Text = nameof(SpinAnimation),
                Subtext = "new SpinAnimation()",
            });

            instrument.Add(new()
            {
                Icon = new EmojiIcon("🌀")
                {
                    Animation = new SpinAnimation()
                    {
                        Anticlockwise = true,
                    },
                },
                Text = $"{nameof(SpinAnimation)} (Anticlockwise)",
                Subtext = "{ Anticlockwise = true }",
            });

            instrument.Add(new()
            {
                Icon = new GlyphIcon("\uF16A")
                {
                    Animation = new SpinAnimation()
                    {
                        Count = 3,
                    },
                },
                Text = $"{nameof(SpinAnimation)} (Limited count)",
                Subtext = "{ Count = 3 }",
            });
        }
        
        {
            var instrument = dashboard.Create<ListInstrument>();
            instrument.Title = nameof(FrameAnimation);

            instrument.Add(new()
            {
                Icon = new GlyphIcon(BatteryGlyphs[0])
                {
                    Animation = new FrameAnimation(BatteryGlyphs),
                },
                Text = nameof(FrameAnimation),
                Subtext = "new FrameAnimation(\"\\uE850\", \"\\uE851\", ...)",
            });

            instrument.Add(new()
            {
                Icon = new EmojiIcon(ClockEmojis[0])
                {
                    Animation = new FrameAnimation(ClockEmojis),
                },
                Text = nameof(FrameAnimation),
                Subtext = "new FrameAnimation(\"🕐\", \"🕑\", ...)",
            });

            instrument.Add(new()
            {
                Icon = new EmojiIcon(ClockEmojis[0])
                {
                    Animation = new FrameAnimation()
                    {
                        Values = ClockEmojis,
                        Interval = TimeSpan.FromMilliseconds(500),
                    },
                },
                Text = $"{nameof(FrameAnimation)} (Custom interval)",
                Subtext = "{ Interval = 500ms }",
            });

            instrument.Add(new()
            {
                Icon = new EmojiIcon(ClockEmojis[0])
                {
                    Animation = new FrameAnimation(ClockEmojis)
                    {
                        AutoReverse = true,
                    },
                },
                Text = $"{nameof(FrameAnimation)} (Auto reverse)",
                Subtext = "{ AutoReverse = true }",
            });

            instrument.Add(new()
            {
                Icon = new TextIcon("1")
                {
                    Animation = new FrameAnimation("1", "2", "3")
                    {
                        Count = 1,
                        Interval = TimeSpan.FromSeconds(1),
                    },
                },
                Text = $"{nameof(FrameAnimation)} (Limited count)",
                Subtext = "{ Count = 1 }",
            });

            instrument.Add(new()
            {
                Icon = new EmojiIcon("⭐")
                {
                    Animation = new FrameAnimation("⭐", "")
                    {
                        Interval = TimeSpan.FromMilliseconds(500),
                    },
                },
                Text = $"{nameof(FrameAnimation)} (Flashing)",
                Subtext = "new FrameAnimation(\"⭐\", \"\")",
            });
        }

    }
}
