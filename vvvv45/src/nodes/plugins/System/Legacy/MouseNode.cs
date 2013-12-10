using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
    [PluginInfo(Name = "Mouse", Category = "System", Version = "Global Legacy2")]
    public class LegacyGlobalMouseNode : GlobalInputNode
    {
#pragma warning disable 0649
        [Input("Cycle Mode", IsSingle = true)]
        ISpread<CycleMode> FCycleModeIn;
        [Output("Mouse", IsSingle = true)]
        ISpread<Mouse> FMouseOut;
#pragma warning restore 0649

        readonly DeltaMouse FDeltaMouse = new DeltaMouse();
        bool FLastFrameCycle;
        PluginContainer FMouseSplitNode;
        Subject<MouseNotification> FMouseNotificationSubject = new Subject<MouseNotification>();
        MouseState FLastMouseState;

        public override void OnImportsSatisfied()
        {
            FMouseOut[0] = new Mouse(FMouseNotificationSubject);
            // Create a mouse split node for us and connect our mouse out to its mouse in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "MouseState" && n.Category == "System" && n.Version == "Split Legacy");
            FMouseSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Mouse", c => FMouseOut);
            base.OnImportsSatisfied();
        }

        public override void Dispose()
        {
            FMouseNotificationSubject.Dispose();
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
                var mouseState = new MouseState(x, y, buttons, 0);
                if (mouseState != FLastMouseState)
                {
                    var virtualScreenSize = SystemInformation.VirtualScreen.Size;
                        var v = new Vector2D(x, y);
                        var vInScreenCoordinates = VMath.Map(
                            v,
                            new Vector2D(-1, -1),
                            new Vector2D(1, 1),
                            new Vector2D(0, virtualScreenSize.Height - 1),
                            new Vector2D(virtualScreenSize.Width - 1, 0),
                            TMapMode.Float);
                        var position = new Point((int)vInScreenCoordinates.x, (int)vInScreenCoordinates.y);
                    if (mouseState.X != FLastMouseState.X || mouseState.Y != FLastMouseState.Y)
                        FMouseNotificationSubject.OnNext(new MouseMoveNotification(position, virtualScreenSize));

                    if (mouseState.IsLeft && !FLastMouseState.IsLeft)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.Left));
                    else if (!mouseState.IsLeft && FLastMouseState.IsLeft)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.Left));
                    if (mouseState.IsMiddle && !FLastMouseState.IsMiddle)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.Middle));
                    else if (!mouseState.IsMiddle && FLastMouseState.IsMiddle)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.Middle));
                    if (mouseState.IsRight && !FLastMouseState.IsRight)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.Right));
                    else if (!mouseState.IsRight && FLastMouseState.IsRight)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.Right));
                    if (mouseState.IsXButton1 && !FLastMouseState.IsXButton1)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.XButton1));
                    else if (!mouseState.IsXButton1 && FLastMouseState.IsXButton1)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.XButton1));
                    if (mouseState.IsXButton2 && !FLastMouseState.IsXButton2)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.XButton2));
                    else if (!mouseState.IsXButton2 && FLastMouseState.IsXButton2)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.XButton2));

                    if (mouseState.MouseWheel != FLastMouseState.MouseWheel)
                    {
                        var delta = mouseState.MouseWheel - FLastMouseState.MouseWheel;
                        FMouseNotificationSubject.OnNext(new MouseWheelNotification(position, virtualScreenSize, delta * Const.WHEEL_DELTA));
                    }

                    FLastMouseState = mouseState;
                }
                FMouseSplitNode.Evaluate(spreadMax);
            }

            FLastFrameCycle = doCycle;
        }
    }

    [PluginInfo(Name = "Mouse", Category = "System", Version = "Window Legacy2")]
    public class LegacyWindowMouseNode : WindowMessageNode, IPluginEvaluate
    {
#pragma warning disable 0649
        [Output("Mouse", IsSingle = true)]
        ISpread<Mouse> FMouseOut;
#pragma warning restore 0649

        PluginContainer FMouseSplitNode;

        public override void Dispose()
        {
            FMouseSplitNode.Dispose();
            base.Dispose();
        }

        protected override void Initialize(IObservable<WMEventArgs> windowMessages)
        {
            var mouseNotifications = windowMessages
                .Where(e => e.Message >= WM.MOUSEFIRST && e.Message <= WM.MOUSELAST)
                .Select<WMEventArgs, MouseNotification>(e =>
                {
                    RECT cr;
                    if (User32.GetClientRect(e.HWnd, out cr))
                    {
                        var pos = e.LParam.ToInt32();
                        var lParam = e.LParam;
                        var wParam = e.WParam;
                        var position = new Point(lParam.LoWord(), lParam.HiWord());
                        var clientArea = new Size(cr.Width, cr.Height);

                        switch (e.Message)
                        {
                            case WM.MOUSEWHEEL:
                                unchecked
                                {
                                    var wheel = wParam.HiWord();
                                    return new MouseWheelNotification(position, clientArea, wheel);
                                }
                            case WM.MOUSEMOVE:
                                return new MouseMoveNotification(position, clientArea);
                            case WM.LBUTTONDOWN:
                            case WM.LBUTTONDBLCLK:
                                return new MouseDownNotification(position, clientArea, MouseButtons.Left);
                            case WM.LBUTTONUP:
                                return new MouseUpNotification(position, clientArea, MouseButtons.Left);
                            case WM.MBUTTONDOWN:
                            case WM.MBUTTONDBLCLK:
                                return new MouseDownNotification(position, clientArea, MouseButtons.Middle);
                            case WM.MBUTTONUP:
                                return new MouseUpNotification(position, clientArea, MouseButtons.Middle);
                            case WM.RBUTTONDOWN:
                            case WM.RBUTTONDBLCLK:
                                return new MouseDownNotification(position, clientArea, MouseButtons.Right);
                            case WM.RBUTTONUP:
                                return new MouseUpNotification(position, clientArea, MouseButtons.Right);
                            case WM.XBUTTONDOWN:
                            case WM.XBUTTONDBLCLK:
                                if ((wParam.HiWord() & 0x0001) > 0)
                                    return new MouseDownNotification(position, clientArea, MouseButtons.XButton1);
                                else
                                    return new MouseDownNotification(position, clientArea, MouseButtons.XButton2);
                            case WM.XBUTTONUP:
                                if ((wParam.HiWord() & 0x0001) > 0)
                                    return new MouseUpNotification(position, clientArea, MouseButtons.XButton1);
                                else
                                    return new MouseUpNotification(position, clientArea, MouseButtons.XButton2);
                        }
                    }
                    return null;
                }
                )
                .OfType<MouseNotification>();
            FMouseOut[0] = new Mouse(mouseNotifications);

            // Create a mouse split node for us and connect our mouse out to its mouse in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "MouseState" && n.Category == "System" && n.Version == "Split Legacy");
            FMouseSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Mouse", c => FMouseOut);
        }

        public void Evaluate(int spreadMax)
        {
            // Evaluate our split plugin
            FMouseSplitNode.Evaluate(spreadMax);
        }
    }
}
