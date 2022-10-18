using System;
using System.Drawing;
using System.Linq;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Input.Windows;

namespace Poltergeist.Operations.BackgroundWindows;

public class BackgroundLocatingService : MacroService
{
    public SendMessageHelper SendMessage { get; set; }

    public Size ClientSize { get; set; }

    public BackgroundLocatingService(MacroProcessor processor) : base(processor)
    {
    }

    public bool Locate(RegionConfig config)
    {
        var result = TryLocate(config, out var hwnd, out var size);

        if (result == LocateResult.Succeeded)
        {
            SendMessage = new SendMessageHelper(hwnd);
            ClientSize = size;
            Logger.Debug($"Found requested region.",new { hwnd, ClientSize });

            return true;
        }
        else
        {
            Logger.Warn($"Failed to find requested region: {result}.");

            return false;
        }
    }

    public static LocateResult TryLocate(RegionConfig config, out IntPtr hwnd, out Size size)
    {
        hwnd = default;
        size = default;

        if (config.ClassName == null && config.ProcessName == null && config.WindowName == null)
        {
            return LocateResult.EmptyParameters;
        }
        else
        {
            var targetHwnd = WindowsFinder.FindWindow(config.WindowName, config.ClassName, config.ProcessName);
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
