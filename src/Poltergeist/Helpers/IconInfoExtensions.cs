using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Helpers;

public static class IconInfoHelper
{
    public static IconSource? ConvertToIconSource(IconInfo? info)
    {
        switch (info)
        {
            case null:
                return null;
            case GlyphIcon glyphIcon:
                {
                    var fontIcon = new FontIconSource()
                    {
                        Glyph = glyphIcon.Glyph,
                    };
                    return fontIcon;
                }
            case EmojiIcon emojiIcon:
                {
                    var fontIcon = new FontIconSource()
                    {
                        FontFamily = new("Segoe UI Emoji"),
                        Glyph = emojiIcon.Emoji,
                    };
                    return fontIcon;
                }
            case TextIcon textIcon:
                {
                    var fontIcon = new FontIconSource()
                    {
                        FontFamily = new(textIcon.Font ?? "Segoe UI"),
                        Glyph = textIcon.Text,
                    };
                    return fontIcon;
                }
            case UriIcon uriIcon:
                {
                    var imageIcon = new ImageIconSource()
                    {
                        ImageSource = new BitmapImage(new Uri(uriIcon.Uri)),
                    };
                    return imageIcon;
                }
            default:
                throw new NotImplementedException();
        }
    }

    public static IconElement? ConvertToIconElement(IconInfo? info)
    {
        IconElement iconElement;
        switch (info)
        {
            case null:
                return null;
            case GlyphIcon glyphIcon:
                iconElement = new FontIcon()
                {
                    Glyph = glyphIcon.Glyph,
                };
                break;
            case EmojiIcon emojiIcon:
                iconElement = new FontIcon()
                {
                    FontFamily = new("Segoe UI Emoji"),
                    Glyph = emojiIcon.Emoji,
                };
                break;
            case TextIcon textIcon:
                iconElement = new FontIcon()
                {
                    FontFamily = new(textIcon.Font ?? "Segoe UI"),
                    Glyph = textIcon.Text,
                };
                break;
            case UriIcon uriIcon:
                iconElement = new ImageIcon()
                {
                    Source = new BitmapImage(new Uri(uriIcon.Uri)),
                };
                break;
            default:
                throw new NotImplementedException();
        }

        if (info.Animation is not null)
        {
            iconElement.Tag = info.Animation;
            iconElement.Loaded += (iconElement, info.Animation) switch
            {
                (_, SpinAnimation) => OnSpinIconLoaded,
                (FontIcon, FrameAnimation) => OnFrameIconLoaded,
                _ => throw new NotImplementedException(),
            };
        }

        return iconElement;
    }

    private static void OnSpinIconLoaded(object sender, RoutedEventArgs e)
    {
        var iconElement = (IconElement)sender;
        var spinAnimationConfig = (SpinAnimation)iconElement.Tag;
        iconElement.Loaded -= OnSpinIconLoaded;
        iconElement.Tag = null;

        var visual = ElementCompositionPreview.GetElementVisual(iconElement);
        var compositor = visual.Compositor;

        var centerPointExpression = compositor.CreateExpressionAnimation();
        centerPointExpression.Expression = "Vector3(this.Target.Size.X / 2, this.Target.Size.Y / 2, 0f)";
        visual.StartAnimation(nameof(Visual.CenterPoint), centerPointExpression);

        var animation = compositor.CreateScalarKeyFrameAnimation();
        if (spinAnimationConfig.Anticlockwise)
        {
            animation.InsertKeyFrame(0.0f, 360.0f);
            animation.InsertKeyFrame(1.0f, 0.0f, compositor.CreateLinearEasingFunction());
        }
        else
        {
            animation.InsertKeyFrame(0.0f, 0.0f);
            animation.InsertKeyFrame(1.0f, 360.0f, compositor.CreateLinearEasingFunction());
        }
        animation.Duration = spinAnimationConfig.Duration;
        if (spinAnimationConfig.Count > 0)
        {
            animation.IterationCount = spinAnimationConfig.Count;
            animation.IterationBehavior = AnimationIterationBehavior.Count;
        }
        else
        {
            animation.IterationBehavior = AnimationIterationBehavior.Forever;
        }
        visual.StartAnimation(nameof(Visual.RotationAngleInDegrees), animation);
    }

    private static void OnFrameIconLoaded(object sender, RoutedEventArgs e)
    {
        var frameworkElement = (FrameworkElement)sender;
        var frameAnimationConfig = (FrameAnimation)frameworkElement.Tag;
        frameworkElement.Loaded -= OnFrameIconLoaded;
        frameworkElement.Tag = null;

        if (frameAnimationConfig.Values.Length <= 1)
        {
            return;
        }

        var propertyPath = frameworkElement switch
        {
            FontIcon => nameof(FontIcon.Glyph),
            _ => null,
        };
        if (propertyPath is null)
        {
            return;
        }

        var animation = new ObjectAnimationUsingKeyFrames()
        {
            Duration = frameAnimationConfig.Interval * frameAnimationConfig.Values.Length,
        };
        for (var i = 0; i < frameAnimationConfig.Values.Length; i++)
        {
            var value = frameAnimationConfig.Values[i];
            var keyFrame = new DiscreteObjectKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(frameAnimationConfig.Interval * i),
                Value = value,
            };
            animation.KeyFrames.Add(keyFrame);
        }
        Storyboard.SetTarget(animation, frameworkElement);
        Storyboard.SetTargetProperty(animation, propertyPath);

        var storyboard = new Storyboard()
        {
            RepeatBehavior = frameAnimationConfig.Count > 0 ? new RepeatBehavior(frameAnimationConfig.Count) : RepeatBehavior.Forever,
            AutoReverse = frameAnimationConfig.AutoReverse,
            Children =
            {
                animation
            }
        };
        storyboard.Begin();
    }
}
