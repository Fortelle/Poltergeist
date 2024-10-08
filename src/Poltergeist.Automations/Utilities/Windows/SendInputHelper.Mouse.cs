﻿using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Automations.Utilities.Windows;

public partial class SendInputHelper
{
    public SendInputHelper AddMouseUp(MouseButtons button, int data = 0)
    {
        AddInput(new()
        {
            type = NativeMethods.InputType.Mouse,
            inputUnion = {
                mi = {
                    dwFlags = button switch
                    {
                        MouseButtons.Left => NativeMethods.MouseEventFlags.LeftUp,
                        MouseButtons.Right => NativeMethods.MouseEventFlags.RightUp,
                        MouseButtons.Middle => NativeMethods.MouseEventFlags.MiddleUp,
                        MouseButtons.XButton1 => NativeMethods.MouseEventFlags.XUp,
                        MouseButtons.XButton2 => NativeMethods.MouseEventFlags.XUp,
                        _ => throw new NotImplementedException(),
                    },
                    mouseData = button switch
                    {
                        MouseButtons.XButton1 => 0x0001,
                        MouseButtons.XButton2 => 0x0002,
                        _ => data,
                    },
                },
            },
        });
        return this;
    }

    public SendInputHelper AddMouseDown(MouseButtons button, int data = 0)
    {
        AddInput(new()
        {
            type = NativeMethods.InputType.Mouse,
            inputUnion = {
                mi = {
                    dwFlags = button switch
                    {
                        MouseButtons.Left => NativeMethods.MouseEventFlags.LeftDown,
                        MouseButtons.Right => NativeMethods.MouseEventFlags.RightDown,
                        MouseButtons.Middle => NativeMethods.MouseEventFlags.MiddleDown,
                        MouseButtons.XButton1 => NativeMethods.MouseEventFlags.XDown,
                        MouseButtons.XButton2 => NativeMethods.MouseEventFlags.XDown,
                        _ => throw new NotImplementedException(),
                    },
                    mouseData = button switch
                    {
                        MouseButtons.XButton1 => 0x0001,
                        MouseButtons.XButton2 => 0x0002,
                        _ => data,
                    },
                },
            },
        });
        return this;
    }

    public SendInputHelper AddMouseWheel(int movement)
    {
        AddInput(new()
        {
            type = NativeMethods.InputType.Mouse,
            inputUnion = {
                mi = {
                    dwFlags = NativeMethods.MouseEventFlags.Wheel,
                    mouseData = movement,
                },
            },
        });
        return this;
    }

    public SendInputHelper AddMouseHWheel(int movement)
    {
        AddInput(new()
        {
            type = NativeMethods.InputType.Mouse,
            inputUnion = {
                mi = {
                    dwFlags = NativeMethods.MouseEventFlags.HWheel,
                    mouseData = movement,
                },
            },
        });
        return this;
    }
}