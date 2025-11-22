using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Locating;

namespace Poltergeist.Android.Adb;

public class AdbLocatingService(MacroProcessor processor) : LocatingProvider(processor)
{
    public void SetSize(Size clientSize)
    {
        ClientSize = clientSize;
        Success = true;

        Logger.Debug($"Set client size to {ClientSize}.");
    }
}
