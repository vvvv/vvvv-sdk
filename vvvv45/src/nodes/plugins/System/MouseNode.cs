using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using System.Windows.Forms;
using VVVV.Utils.VMath;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using VVVV.Utils.Win32;
using System.Diagnostics;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Factories;

namespace VVVV.Nodes.Input
{
    public enum CycleMode
    {
        NoCycle,
        Cycle,
        IncrementCycle
    }

    [PluginInfo(Name = "Mouse", Category = "System", Version = "Global New")]
    public class GlobalMouseNode : IPluginEvaluate
    {
#pragma warning disable 0649
        [Input("Enabled", IsSingle = true, DefaultBoolean = true)]
        ISpread<bool> FEnabledIn;
        [Input("Cycle Mode", IsSingle = true)]
        ISpread<CycleMode> FCycleModeIn;
        [Output("Mouse State", IsSingle = true)]
        ISpread<MouseState> FMouseOut; 
#pragma warning restore 0649

        private readonly DeltaMouse FDeltaMouse = new DeltaMouse();
        private bool FLastFrameCycle;

        public void Evaluate(int SpreadMax)
        {
            var enabled = FEnabledIn.SliceCount > 0 && FEnabledIn[0];
            var cycleMode = FCycleModeIn.SliceCount > 0
                ? FCycleModeIn[0]
                : CycleMode.NoCycle;
            var doCycle = cycleMode != CycleMode.NoCycle;
            if (enabled)
            {
                FDeltaMouse.EnableCycles = doCycle;

                if (doCycle != FLastFrameCycle)
                    FDeltaMouse.Initialize(Cursor.Position);
                else
                    FDeltaMouse.Update();

                double x, y;
                switch (cycleMode)
                {
                    case CycleMode.Cycle:
                        x = FDeltaMouse.EndlessFloatX - Math.Floor((FDeltaMouse.EndlessFloatY + 1) / 2) * 2;
                        y = FDeltaMouse.EndlessFloatY - Math.Floor((FDeltaMouse.EndlessFloatY + 1) / 2) * 2;
                        break;
                    default:
                        x = FDeltaMouse.EndlessFloatX;
                        y = FDeltaMouse.EndlessFloatY;
                        break;
                }

                var buttons = GetPressedMouseButtons();
                FMouseOut[0] = new MouseState(x, y, buttons, 0);
            }
            FLastFrameCycle = doCycle;
        }

        static MouseButton GetPressedMouseButtons()
        {
            MouseButton button = MouseButton.None;
            if ((GetKeyState(Keys.LButton) & KEY_PRESSED) > 0)
                button |= MouseButton.Left;
            if ((GetKeyState(Keys.RButton) & KEY_PRESSED) > 0)
                button |= MouseButton.Right;
            if ((GetKeyState(Keys.MButton) & KEY_PRESSED) > 0)
                button |= MouseButton.Middle;
            return button;
        }

        [DllImport("user32.dll")]
        static extern short GetKeyState(System.Windows.Forms.Keys vKey);
        const byte KEY_PRESSED = 0x80;
    }

    [PluginInfo(Name = "Mouse", Category = "System", Version = "Window New")]
    public class WindowMouseNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        class MouseHandler : IDisposable
        {
            private static ulong FNextSubclassID = 1;
            private IntPtr FHwnd;
            private UIntPtr FSubclassID;
            private readonly ComCtl32.SubClassProc FSubclassProcDelegate;

            public MouseHandler(IntPtr hwnd)
            {
                FHwnd = hwnd;
                FSubclassID = new UIntPtr(FNextSubclassID++);
                FSubclassProcDelegate = SubclassProc;
                if (!ComCtl32.SetWindowSubclass(FHwnd, FSubclassProcDelegate, FSubclassID, IntPtr.Zero))
                    throw new Exception("SetWindowSubclass failed!");
            }

            ~MouseHandler()
            {
                // Make sure handler is referenced from a root
                // We'd need to apply some GC.KeepAlive stuff otherwise
                Debug.Assert(false);
            }

            public void Dispose()
            {
                if (FHwnd != IntPtr.Zero)
                {
                    if (!ComCtl32.RemoveWindowSubclass(FHwnd, FSubclassProcDelegate, FSubclassID))
                        throw new Exception("RemoveWindowSubclass failed!");
                    FHwnd = IntPtr.Zero;
                    GC.SuppressFinalize(this);
                }
            }

            public event EventHandler<WMEventArgs> WindowMessage;

            private IntPtr SubclassProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, IntPtr dwRefData)
            {
                if (WindowMessage != null)
                    WindowMessage(this, new WMEventArgs(hWnd, msg, wParam, lParam));
                return ComCtl32.DefSubclassProc(hWnd, msg, wParam, lParam);
            }
        }

#pragma warning disable 0649
        [Output("Mouse State", IsSingle = true)]
        ISpread<MouseState> FMouseOut;

        [Import]
        IHDEHost FHost;
#pragma warning restore 0649

        private readonly List<MouseHandler> FHandlers = new List<MouseHandler>();

        public void OnImportsSatisfied()
        {
            FHost.WindowAdded += HandleWindowAdded;
        }

        public void Dispose()
        {
            FHost.WindowAdded -= HandleWindowAdded;
            foreach (var handler in FHandlers)
                handler.Dispose();
            FHandlers.Clear();
        }

        void HandleWindowAdded(object sender, WindowEventArgs args)
        {
            var window = args.Window.InternalCOMInterf;
            var inputWindow = window as IUserInputWindow;
            if (inputWindow == null)
            {
                // Special treatment for plugins
                var pluginHost = window.GetNode() as IInternalPluginHost;
                if (pluginHost != null)
                {
                    inputWindow = pluginHost.Plugin as IUserInputWindow;
                    if (inputWindow == null)
                    {
                        var pluginContainer = pluginHost.Plugin as PluginContainer;
                        if (pluginContainer != null)
                        {
                            inputWindow = pluginContainer.PluginBase as IUserInputWindow;
                        }
                    }
                }
            }
            if (inputWindow != null)
            {
                var handle = inputWindow.InputWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    var handler = new MouseHandler(handle);
                    handler.WindowMessage += HandleWindowMessage;
                    FHandlers.Add(handler);
                }
            }
        }

        unsafe void HandleWindowMessage(object sender, WMEventArgs e)
        {
            var handler = sender as MouseHandler;
            if (e.Message == WM.NCDESTROY)
            {
                handler.WindowMessage -= HandleWindowMessage;
                handler.Dispose();
                FHandlers.Remove(handler);
            }
            else
            {
                if (e.Message >= WM.MOUSEFIRST && e.Message <= WM.MOUSELAST)
                {
                    switch (e.Message)
                    {
                        case WM.MOUSEWHEEL:
                            var wheel = e.WParam.ToInt32().HiWord();
                            FMouseState.MouseWheel = FMouseState.MouseWheel + (wheel / Const.WHEEL_DELTA);
                            break;
                        case WM.MOUSEMOVE:
                            RECT cr;
                            if (User32.GetClientRect(e.HWnd, out cr))
                            {
                                var pos = e.LParam.ToInt32();
                                var x = (int)pos.LoWord();
                                var y = (int)pos.HiWord();
                                var width = cr.Width;
                                var height = cr.Height;
                                FMouseState.X = 2d * x / (width - 1) - 1;
                                FMouseState.Y = 1d - 2d * y / (height - 1);
                            }
                            break;
                        case WM.LBUTTONDOWN:
                        case WM.LBUTTONDBLCLK:
                            FMouseState.Button = FMouseState.Button | MouseButton.Left;
                            break;
                        case WM.MBUTTONDOWN:
                        case WM.MBUTTONDBLCLK:
                            FMouseState.Button = FMouseState.Button | MouseButton.Middle;
                            break;
                        case WM.RBUTTONDOWN:
                        case WM.RBUTTONDBLCLK:
                            FMouseState.Button = FMouseState.Button | MouseButton.Right;
                            break;
                        case WM.LBUTTONUP:
                            FMouseState.Button = FMouseState.Button & ~MouseButton.Left;
                            break;
                        case WM.MBUTTONUP:
                            FMouseState.Button = FMouseState.Button & ~MouseButton.Middle;
                            break;
                        case WM.RBUTTONUP:
                            FMouseState.Button = FMouseState.Button & ~MouseButton.Right;
                            break;
                    }
                }
            }
        }

        private MouseState FMouseState = new MouseState();
        public void Evaluate(int SpreadMax)
        {
            FMouseOut[0] = FMouseState;
        }
    }
}
