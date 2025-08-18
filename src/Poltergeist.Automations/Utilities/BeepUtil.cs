using System.Runtime.InteropServices;

namespace Poltergeist.Automations.Utilities;

public static partial class BeepUtil
{
    public static void Beep()
    {
        Beep(800, 200);
    }

    public static async Task BeepAsync()
    {
        await BeepAsync(800, 200);
    }

    public static void Beep(int frequency, int duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(frequency, 37);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(frequency, 32767);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, 0);

        _ = NativeMethods.Beep(frequency, duration);
    }

    public static async Task BeepAsync(int frequency, int duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(frequency, 37);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(frequency, 32767);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, 0);

        Beep(frequency, duration);

        await Task.Delay(duration);
    }

    public static void MessageBeep()
    {
        _ = NativeMethods.MessageBeep(NativeMethods.MB_OK);
    }

    public static void MessageError()
    {
        _ = NativeMethods.MessageBeep(NativeMethods.MB_ICONERROR);
    }

    public static void MessageQuestion()
    {
        _ = NativeMethods.MessageBeep(NativeMethods.MB_ICONQUESTION);
    }

    public static void MessageWarning()
    {
        _ = NativeMethods.MessageBeep(NativeMethods.MB_ICONWARNING);
    }

    public static void MessageInformation()
    {
        _ = NativeMethods.MessageBeep(NativeMethods.MB_ICONINFORMATION);
    }

    private static partial class NativeMethods
    {
        [LibraryImport("kernel32.dll")]
        public static partial int Beep(int dwFreq, int dwDuration);

        public const uint MB_OK = 0x00000000;
        public const uint MB_ICONERROR = 0x00000010;
        public const uint MB_ICONQUESTION = 0x00000020;
        public const uint MB_ICONWARNING = 0x00000030;
        public const uint MB_ICONINFORMATION = 0x00000040;
        public const uint MB_DEFAULT = 0xFFFFFFFF;

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool MessageBeep(uint uType);
    }
}
