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
                if (!ComCtl32.RemoveWindowSubclass(FHwnd, FSubclassProcDelegate, UIntPtr.Zero))
                    throw new Exception("RemoveWindowSubclass failed!");
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

        private IntPtr SubclassProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, IntPtr dwRefData)
        {
            if (WindowMessage != null)
                WindowMessage(this, new WMEventArgs(hWnd, msg, wParam, lParam));
            if (msg == WM.NCDESTROY)
                Dispose();
            return ComCtl32.DefSubclassProc(hWnd, msg, wParam, lParam);
        }
    }
}
