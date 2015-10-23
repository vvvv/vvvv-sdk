using System;
using System.Runtime.InteropServices;

namespace CUnit
{
	/// <summary>Native methods</summary>
    public class NativeMethods
    {
        /// <summary>Windows Message</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public IntPtr hWnd;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet=CharSet.Auto)]
        public static extern bool PeekMessage( out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags );
    }
}