using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Utils.Win32
{
    public class WMEventArgs : EventArgs
    {
        public WMEventArgs(IntPtr hWnd, WM msg, IntPtr wparam, IntPtr lparam)
        {
            HWnd = hWnd;
            Message = msg;
            WParam = wparam;
            LParam = lparam;
        }

        public IntPtr HWnd { get; private set; }
        public WM Message { get; private set; }
        public IntPtr WParam { get; private set; }
        public IntPtr LParam { get; private set; }
    }
}
