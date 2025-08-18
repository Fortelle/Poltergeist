using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Poltergeist.Helpers;

// Helper class to workaround custom title bar bugs.
// DISCLAIMER: The resource key names and color values used below are subject to change. Do not depend on them.
// https://github.com/microsoft/TemplateStudio/issues/4516
internal partial class TitleBarHelper
{
    public static void UpdateTitleBar(ElementTheme theme)
    {
        if (PoltergeistApplication.Current.MainWindow.ExtendsContentIntoTitleBar)
        {
            if (theme == ElementTheme.Default)
            {
                var uiSettings = new UISettings();
                var background = uiSettings.GetColorValue(UIColorType.Background);

                theme = background == Colors.White ? ElementTheme.Light : ElementTheme.Dark;
            }

            if (theme == ElementTheme.Default)
            {
                theme = Application.Current.RequestedTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
            }

            Application.Current.Resources["WindowCaptionForeground"] = theme switch
            {
                ElementTheme.Dark => new SolidColorBrush(Colors.White),
                ElementTheme.Light => new SolidColorBrush(Colors.Black),
                _ => new SolidColorBrush(Colors.Transparent)
            };

            Application.Current.Resources["WindowCaptionForegroundDisabled"] = theme switch
            {
                ElementTheme.Dark => new SolidColorBrush(Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF)),
                ElementTheme.Light => new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00)),
                _ => new SolidColorBrush(Colors.Transparent)
            };

            Application.Current.Resources["WindowCaptionButtonBackgroundPointerOver"] = theme switch
            {
                ElementTheme.Dark => new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)),
                ElementTheme.Light => new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0x00, 0x00)),
                _ => new SolidColorBrush(Colors.Transparent)
            };

            Application.Current.Resources["WindowCaptionButtonBackgroundPressed"] = theme switch
            {
                ElementTheme.Dark => new SolidColorBrush(Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF)),
                ElementTheme.Light => new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00)),
                _ => new SolidColorBrush(Colors.Transparent)
            };

            Application.Current.Resources["WindowCaptionButtonStrokePointerOver"] = theme switch
            {
                ElementTheme.Dark => new SolidColorBrush(Colors.White),
                ElementTheme.Light => new SolidColorBrush(Colors.Black),
                _ => new SolidColorBrush(Colors.Transparent)
            };

            Application.Current.Resources["WindowCaptionButtonStrokePressed"] = theme switch
            {
                ElementTheme.Dark => new SolidColorBrush(Colors.White),
                ElementTheme.Light => new SolidColorBrush(Colors.Black),
                _ => new SolidColorBrush(Colors.Transparent)
            };

            Application.Current.Resources["WindowCaptionBackground"] = new SolidColorBrush(Colors.Transparent);
            Application.Current.Resources["WindowCaptionBackgroundDisabled"] = new SolidColorBrush(Colors.Transparent);

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(PoltergeistApplication.Current.MainWindow);
            if (hwnd == NativeMethods.GetActiveWindow())
            {
                NativeMethods.SendMessage(hwnd, NativeMethods.WMACTIVATE, NativeMethods.WAINACTIVE, nint.Zero);
                NativeMethods.SendMessage(hwnd, NativeMethods.WMACTIVATE, NativeMethods.WAACTIVE, nint.Zero);
            }
            else
            {
                NativeMethods.SendMessage(hwnd, NativeMethods.WMACTIVATE, NativeMethods.WAACTIVE, nint.Zero);
                NativeMethods.SendMessage(hwnd, NativeMethods.WMACTIVATE, NativeMethods.WAINACTIVE, nint.Zero);
            }
        }
    }

    public static void ApplySystemThemeToCaptionButtons()
    {
        var res = Application.Current.Resources;
        if (PoltergeistApplication.Current.AppTitlebar is FrameworkElement frame)
        {
            if (frame.ActualTheme == ElementTheme.Dark)
            {
                res["WindowCaptionForeground"] = Colors.White;
            }
            else
            {
                res["WindowCaptionForeground"] = Colors.Black;
            }
            UpdateTitleBar(frame.ActualTheme);
        }
    }

    private static partial class NativeMethods
    {
        public const int WAINACTIVE = 0x00;
        public const int WAACTIVE = 0x01;
        public const int WMACTIVATE = 0x0006;

        [LibraryImport("user32.dll")]
        public static partial nint GetActiveWindow();

        [LibraryImport("user32.dll", EntryPoint = "SendMessageA")]
        public static partial nint SendMessage(nint hWnd, int msg, int wParam, nint lParam);
    }
}
