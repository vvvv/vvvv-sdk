using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VVVV.Utils.Win32
{
    public class Subclass : IDisposable
    {
        public static Subclass Create(IntPtr hwnd)
        {
            return new Subclass(hwnd);
        }

        private IntPtr FHwnd;
        public IntPtr HWnd { get { return FHwnd; } }
        private readonly ComCtl32.SubClassProc FSubclassProcDelegate;

        private Subclass(IntPtr hwnd)
        {
            FHwnd = hwnd;
            FSubclassProcDelegate = SubclassProc;
            if (!ComCtl32.SetWindowSubclass(FHwnd, FSubclassProcDelegate, UIntPtr.Zero, IntPtr.Zero))
                throw new Exception("SetWindowSubclass failed!");
        }

        ~Subclass()
        {
            // Make sure this object is referenced from a root
            // We'd need to apply some GC.KeepAlive stuff otherwise
            Debug.Assert(false);
        }

        public void Dispose()
        {
            if (FHwnd != IntPtr.Zero)
            {
                ComCtl32.RemoveWindowSubclass(FHwnd, FSubclassProcDelegate, UIntPtr.Zero);                   
                FHwnd = IntPtr.Zero;
                if (Disposed != null)
                    Disposed(this, EventArgs.Empty);
                WindowMessage = null;
                Disposed = null;
                GC.SuppressFinalize(this);
            }
        }

        public event EventHandler<WMEventArgs> WindowMessage;
        public event EventHandler Disposed;

        private IntPtr SubclassProc(IntPtr hWnd, WM msg, UIntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, IntPtr dwRefData)
        {
            var handled = false;
            if (WindowMessage != null)
            {
                var args = new WMEventArgs(hWnd, msg, wParam, lParam);
                WindowMessage(this, args);
                handled = args.Handled;
            }
            if (msg == WM.NCDESTROY)
                Dispose();
            if (!handled)
                return ComCtl32.DefSubclassProc(hWnd, msg, wParam, lParam);
            else
                return IntPtr.Zero;
        }
    }
}
