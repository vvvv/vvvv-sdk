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
using VVVV.Hosting.Graph;

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
        [Output("Mouse", IsSingle = true)]
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
                    FDeltaMouse.Initialize(Control.MousePosition);
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

                var buttons = Control.MouseButtons;
                FMouseOut[0] = new MouseState(x, y, buttons, 0);
            }
            FLastFrameCycle = doCycle;
        }
    }

    [PluginInfo(Name = "Mouse", Category = "System", Version = "Window New")]
    public class WindowMouseNode : UserInputNode, IPluginEvaluate
    {
#pragma warning disable 0649
        [Output("Mouse", IsSingle = true)]
        ISpread<MouseState> FMouseOut;
#pragma warning restore 0649

        protected override void HandleSubclassWindowMessage(object sender, WMEventArgs e)
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
                    case WM.LBUTTONUP:
                    case WM.MBUTTONDOWN:
                    case WM.MBUTTONDBLCLK:
                    case WM.MBUTTONUP:
                    case WM.RBUTTONDOWN:
                    case WM.RBUTTONDBLCLK:
                    case WM.RBUTTONUP:
                    case WM.XBUTTONDOWN:
                    case WM.XBUTTONDBLCLK:
                    case WM.XBUTTONUP:
                        FMouseState.Buttons = Control.MouseButtons;
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
