using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Locating;

public class WindowLocatingService(MacroProcessor processor) : LocatingProvider(processor)
{
    public nint Handle { get; private set; }

    public bool TryLocate(RegionConfig config, out LocatedWindowInfo? info)
    {
        Logger.Trace("Locating the window.", config);

        var result = Locate(config, out info);

        Success = result == LocateResult.Succeeded;

        if (Success)
        {
            Handle = info!.Handle!;
            ClientSize = info.ClientArea.Size;
            WorkspaceSize = config.WorkspaceSize;
            Processor.SessionStorage.AddOrUpdate(WorkspaceSizeKey, WorkspaceSize ?? ClientSize);

            Logger.Info($"Found the specified window: {info}({info.ClientArea.Width},{info.ClientArea.Height}).");
        }
        else
        {
            Handle = IntPtr.Zero;
            ClientSize = default;
            WorkspaceSize = null;
            Processor.SessionStorage.TryRemove(WorkspaceSizeKey);

            Logger.Warn(result switch
            {
                LocateResult.NotFound => $"The specified window was not found.",
                LocateResult.ChildNotFound => $"The specified window [{info}] was found, but the child window with class name \"{config.ChildClassName}\" was not found.",
                LocateResult.Minimized => $"The specified window [{info}] is minimized.",
                LocateResult.SizeNotMatch => $"The size of the located window({info?.ClientArea.Size}) does not match the expected workspace size({config.WorkspaceSize}).",
                LocateResult.EmptyParameters => "No parameters were provided to locate the window.",
                _ => $"Failed to locate the specified window: {result}.",
            });
        }

        return Success;
    }

    public static LocateResult Locate(RegionConfig config, out LocatedWindowInfo? info)
    {
        info = null;

        if (config.ClassName is null && config.ProcessName is null && config.WindowName is null && config.Handle == nint.Zero)
        {
            return LocateResult.EmptyParameters;
        }

        nint parentHwnd;
        if (config.Handle != nint.Zero)
        {
            parentHwnd = config.Handle;
            if (config.ClassName is not null && WindowUtil.GetClassName(config.Handle) != config.ClassName)
            {
                return LocateResult.NotFound;
            }
            if (config.WindowName is not null && WindowUtil.GetWindowName(config.Handle) != config.WindowName)
            {
                return LocateResult.NotFound;
            }
        }
        else
        {
            parentHwnd = WindowsFinder.FindWindow(config.WindowName, config.ClassName, config.ProcessName);
            if (parentHwnd == nint.Zero)
            {
                return LocateResult.NotFound;
            }
        }
        info = new LocatedWindowInfo()
        {
            ParentHandle = parentHwnd,
            ParentClassName = config.ClassName ?? WindowUtil.GetClassName(parentHwnd),
            ParentWindowName = config.WindowName ?? WindowUtil.GetWindowName(parentHwnd),
        };

        nint? childHwnd = null;
        if (!string.IsNullOrEmpty(config.ChildClassName))
        {
            var childrenHwnd = WindowsFinder.FindChildWindows(parentHwnd);
            if (!childrenHwnd.Any(hwnd =>
            {
                if (WindowUtil.GetClassName(hwnd) == config.ChildClassName)
                {
                    childHwnd = hwnd;
                    return true;
                }
                else
                {
                    return false;
                }
            }))
            {
                return LocateResult.ChildNotFound;
            }
        }
        info.ChildHandle = childHwnd;
        info.ChildClassName = config.ChildClassName;

        var targetHwnd = (childHwnd ?? parentHwnd)!;

        var rect = WindowUtil.GetBounds(targetHwnd);
        if (!rect.HasValue)
        {
            return LocateResult.NotFound;
        }
        info.ClientArea = rect.Value;

        if (config.WorkspaceSize.HasValue && rect.Value.Size != config.WorkspaceSize)
        {
            switch (config.Resizable)
            {
                case ResizeRule.Disallow:
                    return LocateResult.SizeNotMatch;
                case ResizeRule.ConstrainProportion:
                    if ((int)(100d * rect.Value.Width / rect.Value.Height) != (int)(100d * config.WorkspaceSize.Value.Width / config.WorkspaceSize.Value.Height))
                    {
                        return LocateResult.SizeNotMatch;
                    }
                    break;
                case ResizeRule.AnySize:
                    break;
            }
        }

        return LocateResult.Succeeded;
    }
}
