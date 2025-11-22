using Poltergeist.Automations.Components.Logging;

namespace Poltergeist.Automations.Utilities.Windows;

public partial class SendMessageHelper
{
    public nint Hwnd { get; }

    private LoggerWrapper? Logger;

    public SendMessageHelper(nint hwnd)
    {
        Hwnd = hwnd;
    }

    public SendMessageHelper(nint hwnd, LoggerWrapper logger)
    {
        Hwnd = hwnd;
        if (logger.IsTraceEnabled)
        {
            Logger = logger;
        }
    }
}
