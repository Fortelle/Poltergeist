using System.Runtime.InteropServices;
using System.Text;

namespace Poltergeist.Helpers;

public class RuntimeHelper
{
    public static bool IsMSIX
    {
        get
        {
            var length = 0;

            return NativeMethods.GetCurrentPackageFullName(ref length, null) != 15700L;
        }
    }

    private static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);
    }
}
