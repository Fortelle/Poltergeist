﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Poltergeist.Input.Windows;

public static class WindowsFinder
{
    public static Size GetScreenSize()
    {
        var width = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        var height = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
        return new Size(width, height);
    }

    public static IntPtr FindWindow(string windowName, string className, string processName)
    {
        var hWnd = IntPtr.Zero;
        if (processName != null)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0 && windowName != null)
            {
                foreach (var proc in processes)
                {
                    if (proc.MainWindowHandle != default)
                    {
                        hWnd = proc.MainWindowHandle;
                        break;
                    }
                }
            }
            else if (processes.Length == 1)
            {
                hWnd = processes[0].MainWindowHandle;
            }
        }
        else if (windowName != null || className != null)
        {
            hWnd = NativeMethods.FindWindow(className, windowName);
        }


        return hWnd;
    }

    public static IntPtr[] FindChildWindows(IntPtr hwnd)
    {
        var parentHwnd = hwnd;
        var targetHwnd = IntPtr.Zero;
        var children = new List<IntPtr>();

        NativeMethods.EnumChildWindows(parentHwnd, (h, l) =>
        {
            children.Add(h);
            return true;
        }, IntPtr.Zero);

        return children.ToArray();
    }

    public static IntPtr FindChildWindow(IntPtr parentHwnd, string lpszClass)
    {
        var childHwnd = NativeMethods.FindWindowEx(parentHwnd, IntPtr.Zero, lpszClass, null);
        return childHwnd;
    }

    private static class NativeMethods
    {
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    }
}
