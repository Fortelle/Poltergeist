using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations;

namespace Poltergeist.Android.Adb;

public class EmptyCapturingService(MacroProcessor processor) : CapturingProvider(processor)
{
    protected override Bitmap DoCapture()
    {
        throw new NotSupportedException();
    }

    protected override Bitmap DoCapture(Rectangle area)
    {
        throw new NotSupportedException();
    }
}
