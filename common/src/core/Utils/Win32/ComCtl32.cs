using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VVVV.Utils.Win32
{
    public static class ComCtl32
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr SubClassProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, IntPtr dwRefData);

        [DllImport("ComCtl32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowSubclass(IntPtr hWnd, SubClassProc newProc, UIntPtr uIdSubclass, IntPtr dwRefData);

        [DllImport("ComCtl32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveWindowSubclass(IntPtr hWnd, SubClassProc newProc, UIntPtr uIdSubclass);

        [DllImport("ComCtl32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr DefSubclassProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam);
    }
}
