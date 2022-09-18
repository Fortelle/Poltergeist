using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Poltergeist.Input.Windows;

public partial class SendInputHelper
{
    private static readonly int Size = Marshal.SizeOf(typeof(NativeMethods.INPUT));

    private readonly List<NativeMethods.INPUT> InputList;

    public SendInputHelper()
    {
        InputList = new();
    }

    private void AddInput(NativeMethods.INPUT input)
    {
        InputList.Add(input);
    }

    public bool Execute()
    {
        if (InputList.Count == 0)
        {
            return true;
        }

        var nInputs = (uint)InputList.Count;
        var pInputs = InputList.ToArray();
        var result = NativeMethods.SendInput(nInputs, pInputs, Size);
        InputList.Clear();

        return result == pInputs.Length;
    }

}
