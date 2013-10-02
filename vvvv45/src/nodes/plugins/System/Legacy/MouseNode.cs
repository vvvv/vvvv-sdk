using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using VVVV.Hosting;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "Mouse", Category = "System", Version = "Global Legacy")]
    public class LegacyGlobalMouseNode : GlobalInputNode
    {
#pragma warning disable 0649
        [Input("Cycle Mode", IsSingle = true)]
        ISpread<CycleMode> FCycleModeIn;
        [Output("Mouse", IsSingle = true)]
        ISpread<MouseState> FMouseOut;
#pragma warning restore 0649

        readonly DeltaMouse FDeltaMouse = new DeltaMouse();
        bool FLastFrameCycle;
        PluginContainer FMouseSplitNode;

        public override void OnImportsSatisfied()
        {
            // Create a mouse split node for us and connect our mouse out to its mouse in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "MouseState" && n.Category == "System" && n.Version == "Split");
            FMouseSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Mouse", c => FMouseOut);
            base.OnImportsSatisfied();
        }

        public override void Dispose()
        {
            FMouseSplitNode.Dispose();
            base.Dispose();
        }

        protected override void Evaluate(int spreadMax, bool enabled)
        {
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
                FMouseSplitNode.Evaluate(spreadMax);
            }

            FLastFrameCycle = doCycle;
        }
    }

    [PluginInfo(Name = "Mouse", Category = "System", Version = "Window Legacy")]
    public class LegacyWindowMouseNode : WindowInputNode
    {
#pragma warning disable 0649
        [Output("Mouse", IsSingle = true)]
        ISpread<MouseState> FMouseOut;
#pragma warning restore 0649

        MouseState FMouseState = new MouseState();
        PluginContainer FMouseSplitNode;
        int FMouseWheel;

        public override void OnImportsSatisfied()
        {
            // Create a mouse split node for us and connect our mouse out to its mouse in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "MouseState" && n.Category == "System" && n.Version == "Split");
            FMouseSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Mouse", c => FMouseOut);
            base.OnImportsSatisfied();
        }

        public override void Dispose()
        {
            FMouseSplitNode.Dispose();
            base.Dispose();
        }

        protected override void HandleSubclassWindowMessage(object sender, WMEventArgs e)
        {
            if (e.Message >= WM.MOUSEFIRST && e.Message <= WM.MOUSELAST)
            {
                switch (e.Message)
                {
                    case WM.MOUSEWHEEL:
                        unchecked
                        {
                            var wheel = e.WParam.ToInt32().HiWord();
                            FMouseWheel += wheel;
                            FMouseState.MouseWheel = (int)Math.Round((float)FMouseWheel / Const.WHEEL_DELTA);
                        }
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
        
        protected override void Evaluate(int spreadMax, bool enabled)
        {
            if (enabled)
            {
                FMouseOut[0] = FMouseState;
                FMouseSplitNode.Evaluate(spreadMax);
            }
        }
    }
}
