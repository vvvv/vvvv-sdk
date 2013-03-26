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
            if ((User32.GetKeyState(Keys.LButton) & Const.KEY_PRESSED) > 0)
                button |= MouseButton.Left;
            if ((User32.GetKeyState(Keys.RButton) & Const.KEY_PRESSED) > 0)
                button |= MouseButton.Right;
            if ((User32.GetKeyState(Keys.MButton) & Const.KEY_PRESSED) > 0)
                button |= MouseButton.Middle;
            return button;
        }
    }

    [PluginInfo(Name = "Mouse", Category = "System", Version = "Window New")]
    public class WindowMouseNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
#pragma warning disable 0649
        [Output("Mouse State", IsSingle = true)]
        ISpread<MouseState> FMouseOut;

        [Import]
        IHDEHost FHost;
#pragma warning restore 0649

        private readonly List<Subclass> FSubclasses = new List<Subclass>();

        public void OnImportsSatisfied()
        {
            FHost.WindowAdded += HandleWindowAdded;
        }

        public void Dispose()
        {
            FHost.WindowAdded -= HandleWindowAdded;
            foreach (var subclass in FSubclasses)
                subclass.Dispose();
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
                    var subclass = Subclass.Create(handle);
                    subclass.WindowMessage += HandleSubclassWindowMessage;
                    subclass.Disposed += HandleSubclassDisposed;
                    FSubclasses.Add(subclass);
                }
            }
        }

        void HandleSubclassDisposed(object sender, EventArgs e)
        {
            var subclass = sender as Subclass;
            subclass.WindowMessage -= HandleSubclassWindowMessage;
            subclass.Disposed -= HandleSubclassDisposed;
            FSubclasses.Remove(subclass);
        }

        void HandleSubclassWindowMessage(object sender, WMEventArgs e)
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

        private MouseState FMouseState = new MouseState();
        public void Evaluate(int SpreadMax)
        {
            FMouseOut[0] = FMouseState;
        }
    }
}
