using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Background;

public class BackgroundLocatingService : MacroService, ILocatingProvider
{
    private SendMessageHelper? sendMessage;
    public SendMessageHelper SendMessage
    {
        get
        {
            if (sendMessage is null)
            {
                throw new InvalidOperationException($"<{nameof(BackgroundLocatingService)}> is currently not available.");
            }
            return sendMessage;
        }
    }

    public Size ClientSize { get; set; }

    public BackgroundLocatingService(MacroProcessor processor) : base(processor)
    {
    }

    public bool Locate(RegionConfig config)
    {
        var result = TryLocate(config, out var hwnd, out var size);

        if (result == LocateResult.Succeeded)
        {
            sendMessage = new SendMessageHelper(hwnd);
            ClientSize = size;
            Logger.Debug($"Found requested region.",new { hwnd, ClientSize });

            return true;
        }
        else
        {
            sendMessage = null;
            ClientSize = default;
            Logger.Warn($"Failed to find requested region: {result}.");

            return false;
        }
    }

    public static LocateResult TryLocate(RegionConfig config, out IntPtr hwnd, out Size size)
    {
        hwnd = default;
        size = default;

        if (config.Delay > 0)
        {
            Thread.Sleep(config.Delay);
        }

        if (config.ClassName is null && config.ProcessName is null && config.WindowName is null && config.Handle == IntPtr.Zero)
        {
            return LocateResult.EmptyParameters;
        }
        else
        {
            var targetHwnd = config.Handle != IntPtr.Zero
                ? config.Handle
                : WindowsFinder.FindWindow(config.WindowName, config.ClassName, config.ProcessName);
            if (targetHwnd == IntPtr.Zero)
            {
                return LocateResult.NotFound;
            }

            if (!string.IsNullOrEmpty(config.ChildClassName))
            {
                var children = WindowsFinder.FindChildWindows(targetHwnd);
                if (!children.Any(childHwnd =>
                {
                    if (WindowHelper.GetClassName(childHwnd) == config.ChildClassName)
                    {
                        targetHwnd = childHwnd;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }))
                {
                    return LocateResult.NotFound;
                }
            }

            var rect = WindowHelper.GetBounds(targetHwnd);
            if (!rect.HasValue)
            {
                return LocateResult.NotFound;
            }

            if (config.OriginSize != default && rect.Value.Size != config.OriginSize)
            {
                return LocateResult.SizeNotMatch;
            }

            hwnd = targetHwnd;
            size = rect.Value.Size;

            return LocateResult.Succeeded;
        }
    }

}
