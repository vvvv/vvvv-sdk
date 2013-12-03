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
    public enum CycleMode
    {
        NoCycle,
        Cycle,
        IncrementCycle
    }

    [PluginInfo(Name = "Mouse", Category = "System")]
    public class WindowMouseNode : WindowMessageNode, IPluginEvaluate
    {
        [Output("Mouse", IsSingle = true)]
        public ISpread<Mouse> MouseOut;

        private PluginContainer FMouseStatesSplitNode;

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
                        var position = new Point(pos.LoWord(), pos.HiWord());
                        var clientArea = new Size(cr.Width, cr.Height);
                        var wParam = e.WParam.ToInt32();

                        switch (e.Message)
                        {
                            case WM.MOUSEWHEEL:
                                unchecked
                                {
                                    var wheel = e.WParam.ToInt32().HiWord();
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
            MouseOut[0] = new Mouse(mouseNotifications);

            // Create a mouse states split node for us and connect our mouse out to its mouse in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "MouseStates" && n.Category == "System" && n.Version == "Split");
            FMouseStatesSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Mouse", c => MouseOut);
        }

        public override void Dispose()
        {
            FMouseStatesSplitNode.Dispose();
            base.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            // Evaluate our split plugin
            FMouseStatesSplitNode.Evaluate(spreadMax);
        }
    }

    [PluginInfo(Name = "Mouse", Category = "System", Version = "Global")]
    public class GlobalMouseNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        public enum DataSource
        {
            Cursor,
            Raw
        }

        [Input("Enabled", DefaultBoolean = true)]
        public ISpread<bool> EnabledIn;

        [Input("Source", IsSingle = true)]
        public IDiffSpread<DataSource> DataSourceIn;

        [Input("Cycle Mode", IsSingle = true)]
        public IDiffSpread<CycleMode> CycleModeIn;

        [Output("Mouse")]
        public ISpread<Mouse> MouseOut;

        [Import]
        protected IOFactory FIOFactory;

        private PluginContainer FMouseStatesSplitNode;

        public void OnImportsSatisfied()
        {
            RawInputService.DevicesChanged += RawKeyboardService_DevicesChanged;
            SubscribeToDevices();

            DataSourceIn.Changed += ModeIn_Changed;
            CycleModeIn.Changed += CycleModeIn_Changed;

            // Create a mouse states split node for us and connect our mouse out to its mouse in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "MouseStates" && n.Category == "System" && n.Version == "Split");
            FMouseStatesSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Mouse", c => MouseOut);
        }

        void CycleModeIn_Changed(IDiffSpread<CycleMode> spread)
        {
            SubscribeToDevices();
        }

        void ModeIn_Changed(IDiffSpread<DataSource> spread)
        {
            SubscribeToDevices();
        }

        void RawKeyboardService_DevicesChanged(object sender, EventArgs e)
        {
            SubscribeToDevices();
        }

        public void Dispose()
        {
            RawInputService.DevicesChanged -= RawKeyboardService_DevicesChanged;
            FMouseStatesSplitNode.Dispose();
        }

        private void SubscribeToDevices()
        {
            var initialPosition = Control.MousePosition;
            var dataSource = DataSourceIn.SliceCount > 0 ? DataSourceIn[0] : DataSource.Cursor;
            var cycleMode = CycleModeIn.SliceCount > 0 ? CycleModeIn[0] : CycleMode.NoCycle;
            var mouseDevices = Device.GetDevices()
                .Where(d => d.DeviceType == DeviceType.Mouse)
                .OrderBy(d => d, new DeviceComparer())
                .ToList();
            MouseOut.SliceCount = dataSource == DataSource.Raw
                ? mouseDevices.Count
                : Math.Min(mouseDevices.Count, 1);
            for (int i = 0; i < MouseOut.SliceCount; i++)
            {
                MouseOut[i] = CreateMouse(mouseDevices[i], i, initialPosition, dataSource, cycleMode);
            }
        }

        // Little helper classes to keep track of individual mouse positions
        class MouseData
        {
            public Point Position;
        }

        class IncrementalMouseData : MouseData
        {
            public Point IncrementalPosition;
        }

        private Mouse CreateMouse(DeviceInfo deviceInfo, int slice, Point initialPosition, DataSource dataSource, CycleMode cycleMode)
        {
            var mouseData = new MouseData() { Position = initialPosition };
            var mouseInput = Observable.FromEventPattern<MouseInputEventArgs>(typeof(Device), "MouseInput")
                    .Where(_ => EnabledIn.SliceCount > 0 && EnabledIn[slice]);
            IObservable<MouseNotification> notifications;
            if (dataSource == DataSource.Cursor)
            {
                switch (cycleMode)
                {
                    case CycleMode.NoCycle:
                        notifications = mouseInput
                            .SelectMany<EventPattern<MouseInputEventArgs>, MouseNotification>(ep => GenerateCursorNotifications(mouseData, ep.EventArgs));
                        break;
                    case CycleMode.Cycle:
                        notifications = mouseInput
                            .SelectMany<EventPattern<MouseInputEventArgs>, MouseNotification>(ep => GenerateCyclicCursorNotifications(mouseData, ep.EventArgs));
                        break;
                    case CycleMode.IncrementCycle:
                        var incrementalMouseData = new IncrementalMouseData()
                        {
                            Position = initialPosition,
                            IncrementalPosition = initialPosition
                        };
                        notifications = mouseInput
                            .SelectMany<EventPattern<MouseInputEventArgs>, MouseNotification>(ep => GenerateIncrementalCyclicCursorNotifications(incrementalMouseData, ep.EventArgs));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
                // Ignore cycleMode
                notifications = mouseInput
                    .Where(ep => ep.EventArgs.Device == deviceInfo.Handle)
                    .SelectMany<EventPattern<MouseInputEventArgs>, MouseNotification>(ep => GenerateMouseNotifications(mouseData, ep.EventArgs));
            return new Mouse(notifications);
        }

        private IEnumerable<MouseNotification> GenerateMouseNotifications(MouseData mouseData, MouseInputEventArgs args)
        {
            var virtualScreenSize = SystemInformation.VirtualScreen.Size;
            var position = mouseData.Position;
            switch (args.Mode)
            {
                case MouseMode.MoveAbsolute:
                    // x,y between 0x0000 and 0xffff
                    position = new Point(args.X / virtualScreenSize.Width, args.Y / virtualScreenSize.Height);
                    break;
                case MouseMode.MoveRelative:
                    position = new Point(VMath.Clamp(args.X + position.X, 0, virtualScreenSize.Width - 1), VMath.Clamp(args.Y + position.Y, 0, virtualScreenSize.Height - 1));
                    break;
                case MouseMode.VirtualDesktop:
                    position = new Point(args.X, args.Y);
                    break;
                case MouseMode.AttributesChanged:
                case MouseMode.MoveNoCoalesce:
                    // Ignore
                    break;
                default:
                    break;
            }
            if (mouseData.Position != position)
            {
                mouseData.Position = position;
                yield return new MouseMoveNotification(position, virtualScreenSize);
            }
            foreach (var n in GenerateMouseButtonNotifications(args, position, virtualScreenSize))
                yield return n;
        }

        private IEnumerable<MouseNotification> GenerateCursorNotifications(MouseData mouseData, MouseInputEventArgs args)
        {
            var virtualScreenSize = SystemInformation.VirtualScreen.Size;
            var position = Control.MousePosition;
            if (mouseData.Position != position)
            {
                mouseData.Position = position;
                yield return new MouseMoveNotification(position, virtualScreenSize);
            }
            foreach (var n in GenerateMouseButtonNotifications(args, position, virtualScreenSize))
                yield return n;
        }

        private IEnumerable<MouseNotification> GenerateCyclicCursorNotifications(MouseData mouseData, MouseInputEventArgs args)
        {
            var virtualScreenSize = SystemInformation.VirtualScreen.Size;
            var position = Control.MousePosition;
            var newPosition = position;
            var leftBounds = GetLeftMostMonitorBounds(position.Y);
            var rightBounds = GetRightMostMonitorBounds(position.Y);
            if (position.X <= leftBounds.Left)
                newPosition.X = rightBounds.Right;
            if (position.X >= rightBounds.Right - 1)
                newPosition.X = leftBounds.Left;
            var topBounds = GetTopMostMonitorBounds(position.X);
            var bottomBounds = GetBottomMostMonitorBounds(position.X);
            if (position.Y <= topBounds.Top)
                newPosition.Y = bottomBounds.Bottom;
            if (position.Y >= bottomBounds.Bottom - 1)
                newPosition.Y = topBounds.Top;
            if (newPosition != position)
            {
                Cursor.Position = newPosition;
                position = newPosition;
            }
            if (mouseData.Position != position)
            {
                mouseData.Position = position;
                yield return new MouseMoveNotification(position, virtualScreenSize);
            }
            foreach (var n in GenerateMouseButtonNotifications(args, position, virtualScreenSize))
                yield return n;
        }

        private IEnumerable<MouseNotification> GenerateIncrementalCyclicCursorNotifications(IncrementalMouseData mouseData, MouseInputEventArgs args)
        {
            var virtualScreenSize = SystemInformation.VirtualScreen.Size;
            var position = Control.MousePosition;
            var delta = new Size(position.X - mouseData.Position.X, position.Y - mouseData.Position.Y);
            var newPosition = position;
            var leftBounds = GetLeftMostMonitorBounds(position.Y);
            var rightBounds = GetRightMostMonitorBounds(position.Y);
            if (position.X <= leftBounds.Left)
            {
                newPosition.X = rightBounds.Right;
                delta = new Size(-1, delta.Height);
            }
            if (position.X >= rightBounds.Right - 1)
            {
                newPosition.X = leftBounds.Left;
                delta = new Size(1, delta.Height);
            }
            var topBounds = GetTopMostMonitorBounds(position.X);
            var bottomBounds = GetBottomMostMonitorBounds(position.X);
            if (position.Y <= topBounds.Top)
            {
                newPosition.Y = bottomBounds.Bottom;
                delta = new Size(delta.Width, -1);
            }
            if (position.Y >= bottomBounds.Bottom - 1)
            {
                newPosition.Y = topBounds.Top;
                delta = new Size(delta.Width, 1);
            }
            if (newPosition != position)
            {
                Cursor.Position = newPosition;
                position = newPosition;
            }
            if (mouseData.Position != position)
            {
                mouseData.Position = position;
                mouseData.IncrementalPosition += delta;
                yield return new MouseMoveNotification(mouseData.IncrementalPosition, virtualScreenSize);
            }
            foreach (var n in GenerateMouseButtonNotifications(args, mouseData.IncrementalPosition, virtualScreenSize))
                yield return n;
        }

        private static IEnumerable<MouseNotification> GenerateMouseButtonNotifications(MouseInputEventArgs args, Point position, Size clientArea)
        {
            var buttonFlags = args.ButtonFlags;
            if ((buttonFlags & MouseButtonFlags.LeftButtonDown) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.Left);
            if ((buttonFlags & MouseButtonFlags.LeftButtonUp) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.Left);
            if ((buttonFlags & MouseButtonFlags.RightButtonDown) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.Right);
            if ((buttonFlags & MouseButtonFlags.RightButtonUp) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.Right);
            if ((buttonFlags & MouseButtonFlags.MiddleButtonDown) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.Middle);
            if ((buttonFlags & MouseButtonFlags.MiddleButtonUp) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.Middle);
            if ((buttonFlags & MouseButtonFlags.Button4Down) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.XButton1);
            if ((buttonFlags & MouseButtonFlags.Button4Up) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.XButton1);
            if ((buttonFlags & MouseButtonFlags.Button5Down) > 0)
                yield return new MouseDownNotification(position, clientArea, MouseButtons.XButton2);
            if ((buttonFlags & MouseButtonFlags.Button5Up) > 0)
                yield return new MouseUpNotification(position, clientArea, MouseButtons.XButton2);
            if ((buttonFlags & MouseButtonFlags.MouseWheel) > 0)
            {
                yield return new MouseWheelNotification(position, clientArea, args.WheelDelta);
            }
        }

        static Rectangle GetBounds(Point pt)
        {
            var screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                var bounds = screens[i].Bounds;
                if (bounds.Contains(pt))
                    return bounds;
            }
            return Rectangle.Empty;
        }

        static Rectangle GetLeftMostMonitorBounds(int y)
        {
            return Screen.AllScreens
                .Select(s => s.Bounds)
                .Where(b => b.Top <= y && y < b.Bottom)
                .OrderBy(b => b.Left)
                .FirstOrDefault();
        }

        static Rectangle GetRightMostMonitorBounds(int y)
        {
            return Screen.AllScreens
                .Select(s => s.Bounds)
                .Where(b => b.Top <= y && y < b.Bottom)
                .OrderByDescending(b => b.Right)
                .FirstOrDefault();
        }

        static Rectangle GetTopMostMonitorBounds(int x)
        {
            return Screen.AllScreens
                .Select(s => s.Bounds)
                .Where(b => b.Left <= x && x < b.Right)
                .OrderBy(b => b.Top)
                .FirstOrDefault();
        }

        static Rectangle GetBottomMostMonitorBounds(int x)
        {
            return Screen.AllScreens
                .Select(s => s.Bounds)
                .Where(b => b.Left <= x && x < b.Right)
                .OrderByDescending(b => b.Bottom)
                .FirstOrDefault();
        }

        public void Evaluate(int spreadMax)
        {
            // Evaluate our split plugin
            FMouseStatesSplitNode.Evaluate(spreadMax);
        }
    }

    [PluginInfo(Name = "MouseEvents", Category = "System", Version = "Split", AutoEvaluate = true)]
    public class MouseEventsSplitNode : IPluginEvaluate, IDisposable
    {
        [Input("Mouse", IsSingle = true)]
        public ISpread<Mouse> FInput;

        [Output("Event Type")]
        public ISpread<MouseNotificationKind> FEventTypeOut;
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;
        [Output("Mouse Wheel Delta")]
        public ISpread<int> MouseWheelDeltaOut;
        [Output("Left Button")]
        public ISpread<bool> LeftButtonOut;
        [Output("Middle Button")]
        public ISpread<bool> MiddleButtonOut;
        [Output("Right Button")]
        public ISpread<bool> RightButtonOut;
        [Output("X1 Button")]
        public ISpread<bool> X1ButtonOut;
        [Output("X2 Button")]
        public ISpread<bool> X2ButtonOut;

        private static readonly IList<MouseNotification> FEmptyList = new List<MouseNotification>(0);
        private Mouse FMouse;
        private IEnumerator<IList<MouseNotification>> FEnumerator;

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Evaluate(int spreadMax)
        {
            var mouse = FInput[0] ?? Mouse.Empty;
            if (mouse != FMouse)
            {
                Unsubscribe();
                FMouse = mouse;
                Subscribe();
            }

            var notifications = FEnumerator.MoveNext()
                ? FEnumerator.Current
                : FEmptyList;
            FEventTypeOut.SliceCount = notifications.Count;
            PositionOut.SliceCount = notifications.Count;
            MouseWheelDeltaOut.SliceCount = notifications.Count;
            LeftButtonOut.SliceCount = notifications.Count;
            MiddleButtonOut.SliceCount = notifications.Count;
            RightButtonOut.SliceCount = notifications.Count;
            X1ButtonOut.SliceCount = notifications.Count;
            X2ButtonOut.SliceCount = notifications.Count;

            for (int i = 0; i < notifications.Count; i++)
            {
                var n = notifications[i];
                FEventTypeOut[i] = n.Kind;
                var position = new Vector2D(n.Position.X, n.Position.Y);
                var clientArea = new Vector2D(n.ClientArea.Width, n.ClientArea.Height);
                PositionOut[i] = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                switch (n.Kind)
                {
                    case MouseNotificationKind.MouseDown:
                    case MouseNotificationKind.MouseUp:
                        var mouseButton = n as MouseButtonNotification;
                        MouseWheelDeltaOut[i] = 0;
                        LeftButtonOut[i] = (mouseButton.Buttons & MouseButtons.Left) > 0;
                        MiddleButtonOut[i] = (mouseButton.Buttons & MouseButtons.Middle) > 0;
                        RightButtonOut[i] = (mouseButton.Buttons & MouseButtons.Right) > 0;
                        X1ButtonOut[i] = (mouseButton.Buttons & MouseButtons.XButton1) > 0;
                        X2ButtonOut[i] = (mouseButton.Buttons & MouseButtons.XButton2) > 0;
                        break;
                    case MouseNotificationKind.MouseMove:
                        MouseWheelDeltaOut[i] = 0;
                        LeftButtonOut[i] = false;
                        MiddleButtonOut[i] = false;
                        RightButtonOut[i] = false;
                        X1ButtonOut[i] = false;
                        X2ButtonOut[i] = false;
                        break;
                    case MouseNotificationKind.MouseWheel:
                        var mouseWheel = n as MouseWheelNotification;
                        MouseWheelDeltaOut[i] = mouseWheel.WheelDelta;
                        LeftButtonOut[i] = false;
                        MiddleButtonOut[i] = false;
                        RightButtonOut[i] = false;
                        X1ButtonOut[i] = false;
                        X2ButtonOut[i] = false;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void Subscribe()
        {
            if (FMouse != null)
            {
                FEnumerator = FMouse.MouseNotifications
                    .Chunkify()
                    .GetEnumerator();
            }
        }

        private void Unsubscribe()
        {
            if (FEnumerator != null)
            {
                FEnumerator.Dispose();
                FEnumerator = null;
            }
        }
    }

    [PluginInfo(Name = "MouseStates", Category = "System", Version = "Split", AutoEvaluate = true)]
    public class MouseStatesSplitNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Mouse", IsSingle = true)]
        public ISpread<Mouse> MouseIn;
        [Input("Schedule Mode", IsSingle = true, DefaultEnumEntry = "Discard")]
        public ISpread<ScheduleMode> ScheduleModeIn;

        [Output("Position")]
        public ISpread<Vector2D> PositionOut;
        [Output("Mouse Wheel")]
        public ISpread<int> MouseWheelOut;
        [Output("Left Button")]
        public ISpread<bool> LeftButtonOut;
        [Output("Middle Button")]
        public ISpread<bool> MiddleButtonOut;
        [Output("Right Button")]
        public ISpread<bool> RightButtonOut;
        [Output("X1 Button")]
        public ISpread<bool> X1ButtonOut;
        [Output("X2 Button")]
        public ISpread<bool> X2ButtonOut;

        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();
        private Subscription<Mouse, MouseNotification> FSubscription;
        private Spread<int> FRawMouseWheel = new Spread<int>(1);

        public void OnImportsSatisfied()
        {
            FSubscription = new Subscription<Mouse, MouseNotification>(
                mouse =>
                {
                    return mouse.MouseNotifications;
                },
                n =>
                {
                    switch (n.Kind)
                    {
                        case MouseNotificationKind.MouseDown:
                        case MouseNotificationKind.MouseUp:
                            var mouseButton = n as MouseButtonNotification;
                            var isDown = n.Kind == MouseNotificationKind.MouseDown;
                            if ((mouseButton.Buttons & MouseButtons.Left) > 0)
                                LeftButtonOut[0] = isDown;
                            if ((mouseButton.Buttons & MouseButtons.Middle) > 0)
                                MiddleButtonOut[0] = isDown;
                            if ((mouseButton.Buttons & MouseButtons.Right) > 0)
                                RightButtonOut[0] = isDown;
                            if ((mouseButton.Buttons & MouseButtons.XButton1) > 0)
                                X1ButtonOut[0] = isDown;
                            if ((mouseButton.Buttons & MouseButtons.XButton2) > 0)
                                X2ButtonOut[0] = isDown;
                            break;
                        case MouseNotificationKind.MouseMove:
                            var position = new Vector2D(n.Position.X, n.Position.Y);
                            var clientArea = new Vector2D(n.ClientArea.Width, n.ClientArea.Height);
                            PositionOut[0] = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                            break;
                        case MouseNotificationKind.MouseWheel:
                            var mouseWheel = n as MouseWheelNotification;
                            FRawMouseWheel[0] += mouseWheel.WheelDelta;
                            MouseWheelOut[0] = (int)Math.Round((float)FRawMouseWheel[0] / Const.WHEEL_DELTA);
                            break;
                        default:
                            break;
                    }
                },
                FScheduler
            );
        }

        public void Dispose()
        {
            FSubscription.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            //resubsribe if necessary
            FSubscription.Update(MouseIn[0]);
            //process events
            FScheduler.Run(ScheduleModeIn[0]);
        }
    }

    [PluginInfo(Name = "MouseStates", Category = "System", Version = "Join", AutoEvaluate = true)]
    public class MouseStatesJoinNode : IPluginBase, IPartImportsSatisfiedNotification
    {
        [Input("Position")]
        public IDiffSpread<Vector2D> PositionIn;
        [Input("Mouse Wheel")]
        public IDiffSpread<int> MouseWheelIn;
        [Input("Left Button")]
        public IDiffSpread<bool> LeftButtonIn;
        [Input("Middle Button")]
        public IDiffSpread<bool> MiddleButtonIn;
        [Input("Right Button")]
        public IDiffSpread<bool> RightButtonIn;
        [Input("X1 Button")]
        public IDiffSpread<bool> X1ButtonIn;
        [Input("X2 Button")]
        public IDiffSpread<bool> X2ButtonIn;

        [Output("Mouse", IsSingle = true)]
        public ISpread<Mouse> MouseOut;

        public void OnImportsSatisfied()
        {
            var mouseMoves = PositionIn.ToObservable(0)
                .Select(v => new MouseMoveNotification(ToMousePoint(v), FClientArea));
            var mouseButtons = Observable.Merge(
                LeftButtonIn.ToObservable(0).Select(x => Tuple.Create(x, MouseButtons.Left)),
                MiddleButtonIn.ToObservable(0).Select(x => Tuple.Create(x, MouseButtons.Middle)),
                RightButtonIn.ToObservable(0).Select(x => Tuple.Create(x, MouseButtons.Right)),
                X1ButtonIn.ToObservable(0).Select(x => Tuple.Create(x, MouseButtons.XButton1)),
                X2ButtonIn.ToObservable(0).Select(x => Tuple.Create(x, MouseButtons.XButton2))
            );
            var mouseDowns = mouseButtons.Where(x => x.Item1)
                .Select(x => x.Item2)
                .Select(x => new MouseDownNotification(ToMousePoint(PositionIn[0]), FClientArea, x));
            var mouseUps = mouseButtons.Where(x => !x.Item1)
                .Select(x => x.Item2)
                .Select(x => new MouseUpNotification(ToMousePoint(PositionIn[0]), FClientArea, x));
            var mouseWheelDeltas = MouseWheelIn.ToObservable(0)
                .StartWith(0)
                .Buffer(2, 1)
                .Select(b => b[1] - b[0])
                .Select(d => new MouseWheelNotification(ToMousePoint(PositionIn[0]), FClientArea, d * Const.WHEEL_DELTA))
                .Cast<MouseNotification>();
            var notifications = Observable.Merge<MouseNotification>(
                mouseMoves, 
                mouseDowns, 
                mouseUps, 
                mouseWheelDeltas);
            MouseOut[0] = new Mouse(notifications);
        }

        static Point ToMousePoint(Vector2D normV)
        {
            var clientArea = new Vector2D(FClientArea.Width, FClientArea.Height);
            var v = VMath.Map(normV, new Vector2D(-1, 1), new Vector2D(1, -1), Vector2D.Zero, clientArea, TMapMode.Clamp);
            return new Point((int)v.x, (int)v.y);
        }

        static Size FClientArea = new Size(short.MaxValue, short.MaxValue);
    }
}
