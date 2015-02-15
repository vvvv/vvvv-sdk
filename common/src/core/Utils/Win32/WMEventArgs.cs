using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Win32
{
    public class WMEventArgs : EventArgs
    {
        public WMEventArgs(IntPtr hWnd, WM msg, UIntPtr wparam, IntPtr lparam)
        {
            HWnd = hWnd;
            Message = msg;
            WParam = wparam;
            LParam = lparam;
        }

        public readonly IntPtr HWnd;
        public readonly WM Message;
        public readonly UIntPtr WParam;
        public readonly IntPtr LParam;
        // If true DefWindowProc won't be called
        public bool Handled;
    }
}
