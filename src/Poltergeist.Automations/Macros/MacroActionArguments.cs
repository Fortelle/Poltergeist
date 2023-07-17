namespace Poltergeist.Automations.Macros;

public class MacroActionArguments
{
    public IMacroBase Macro { get; set; }

    //public nint Hwnd { get; set; }

    public MacroActionArguments(IMacroBase macro)
    {
        Macro = macro;
    }
}
