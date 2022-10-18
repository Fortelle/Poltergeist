using Poltergeist.Automations.Components.Repeats;
using Poltergeist.Automations.Macros;
using Poltergeist.Components.Loops;
using Poltergeist.Operations.AndroidEmulators;
using Poltergeist.Operations.Macros;

namespace Poltergeist.Test;

public class AdbGroup : MacroGroup
{
    public AdbGroup() : base("AdbTests")
    {
        LoadMacros();
    }

    public void LoadMacros()
    {
        Macros.Add(new AndroidEmulatorMacro("test_emulator_capture")
        {
            Title = "Emulator module",
            Configure = (s, c) =>
            {
                c.SetOption("preview_capture", true);
            },
            Iteration = (l, e) =>
            {
                using var bmp = e.Capturing.Capture();
            },
        });

    }

}
