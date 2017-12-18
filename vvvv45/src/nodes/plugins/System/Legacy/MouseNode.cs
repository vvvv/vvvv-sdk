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
                var mouseState = new MouseState(x, y, buttons, 0, 0);
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
                        FMouseNotificationSubject.OnNext(new MouseMoveNotification(position, virtualScreenSize, this));

                    if (mouseState.IsLeft && !FLastMouseState.IsLeft)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.Left, this));
                    else if (!mouseState.IsLeft && FLastMouseState.IsLeft)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.Left, this));
                    if (mouseState.IsMiddle && !FLastMouseState.IsMiddle)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.Middle, this));
                    else if (!mouseState.IsMiddle && FLastMouseState.IsMiddle)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.Middle, this));
                    if (mouseState.IsRight && !FLastMouseState.IsRight)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.Right, this));
                    else if (!mouseState.IsRight && FLastMouseState.IsRight)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.Right, this));
                    if (mouseState.IsXButton1 && !FLastMouseState.IsXButton1)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.XButton1, this));
                    else if (!mouseState.IsXButton1 && FLastMouseState.IsXButton1)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.XButton1, this));
                    if (mouseState.IsXButton2 && !FLastMouseState.IsXButton2)
                        FMouseNotificationSubject.OnNext(new MouseDownNotification(position, virtualScreenSize, MouseButtons.XButton2, this));
                    else if (!mouseState.IsXButton2 && FLastMouseState.IsXButton2)
                        FMouseNotificationSubject.OnNext(new MouseUpNotification(position, virtualScreenSize, MouseButtons.XButton2, this));

                    if (mouseState.MouseWheel != FLastMouseState.MouseWheel)
                    {
                        var delta = mouseState.MouseWheel - FLastMouseState.MouseWheel;
                        FMouseNotificationSubject.OnNext(new MouseWheelNotification(position, virtualScreenSize, delta * Const.WHEEL_DELTA, this));
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

        protected override void Initialize(IObservable<EventPattern<WMEventArgs>> windowMessages, IObservable<bool> disabled)
        {
            var mouseNotifications = windowMessages
                .Where(e => e.EventArgs.Message >= WM.MOUSEFIRST && e.EventArgs.Message <= WM.MOUSELAST)
                .Select<EventPattern<WMEventArgs>, MouseNotification>(e =>
                {
                    RECT cr;
                    var a = e.EventArgs;
                    if (User32.GetClientRect(a.HWnd, out cr))
                    {
                        var pos = a.LParam.ToInt32();
                        var lParam = a.LParam;
                        var wParam = a.WParam;
                        var position = new Point(lParam.LoWord(), lParam.HiWord());
                        var clientArea = new Size(cr.Width, cr.Height);

                        switch (a.Message)
                        {
                            case WM.MOUSEWHEEL:
                                unchecked
                                {
                                    var wheel = wParam.HiWord();
                                    return new MouseWheelNotification(position, clientArea, wheel, this);
                                }
                            case WM.MOUSEMOVE:
                                return new MouseMoveNotification(position, clientArea, this);
                            case WM.LBUTTONDOWN:
                            case WM.LBUTTONDBLCLK:
                                return new MouseDownNotification(position, clientArea, MouseButtons.Left, this);
                            case WM.LBUTTONUP:
                                return new MouseUpNotification(position, clientArea, MouseButtons.Left, this);
                            case WM.MBUTTONDOWN:
                            case WM.MBUTTONDBLCLK:
                                return new MouseDownNotification(position, clientArea, MouseButtons.Middle, this);
                            case WM.MBUTTONUP:
                                return new MouseUpNotification(position, clientArea, MouseButtons.Middle, this);
                            case WM.RBUTTONDOWN:
                            case WM.RBUTTONDBLCLK:
                                return new MouseDownNotification(position, clientArea, MouseButtons.Right, this);
                            case WM.RBUTTONUP:
                                return new MouseUpNotification(position, clientArea, MouseButtons.Right, this);
                            case WM.XBUTTONDOWN:
                            case WM.XBUTTONDBLCLK:
                                if ((wParam.HiWord() & 0x0001) > 0)
                                    return new MouseDownNotification(position, clientArea, MouseButtons.XButton1, this);
                                else
                                    return new MouseDownNotification(position, clientArea, MouseButtons.XButton2, this);
                            case WM.XBUTTONUP:
                                if ((wParam.HiWord() & 0x0001) > 0)
                                    return new MouseUpNotification(position, clientArea, MouseButtons.XButton1, this);
                                else
                                    return new MouseUpNotification(position, clientArea, MouseButtons.XButton2, this);
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
