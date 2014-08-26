using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;
using VVVV.Core.Logging;
using VVVV.Hosting;
using VVVV.Hosting.IO;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.Streams;
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

    [PluginInfo(Name = "Mouse",
                Category = "Devices", 
                Version = "Window",
                Help = "Returns the mouse of the current render window.")]
    public class WindowMouseNode : WindowMessageNode, IPluginEvaluate
    {
        [Output("Device", IsSingle = true)]
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
                        var lParam = e.LParam;
                        var position = new Point(lParam.LoWord(), lParam.HiWord());
                        var clientArea = new Size(cr.Width, cr.Height);
                        var wParam = e.WParam;

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
            MouseOut[0] = new Mouse(mouseNotifications);

            // Create a mouse states split node for us and connect our mouse out to its mouse in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "MouseStates" && n.Category == "Mouse" && n.Version == "Split");
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

    [PluginInfo(Name = "Mouse", 
                Category = "Devices", 
                Version = "Desktop",
                Help = "Returns the systemwide mouse.")]
    public class DesktopMouseNode : DesktopDeviceInputNode<Mouse>
    {
        public enum DataSource
        {
            Cursor,
            Raw
        }

        [Input("Source", Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<DataSource> DataSourceIn;

        [Input("Cycle Mode")]
        public IDiffSpread<CycleMode> CycleModeIn;

        public DesktopMouseNode()
            : base(DeviceType.Mouse, "MouseStates", "Mouse")
        {
        }

        public override void OnImportsSatisfied()
        {
            DataSourceIn.Changed += ModeIn_Changed;
            CycleModeIn.Changed += CycleModeIn_Changed;
            base.OnImportsSatisfied();
        }

        void CycleModeIn_Changed(IDiffSpread<CycleMode> spread)
        {
            SubscribeToDevices();
        }

        void ModeIn_Changed(IDiffSpread<DataSource> spread)
        {
            SubscribeToDevices();
        }

        public override void Dispose()
        {
            DataSourceIn.Changed -= ModeIn_Changed;
            CycleModeIn.Changed -= CycleModeIn_Changed;
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

        protected override int GetMaxSpreadCount()
        {
            return SpreadUtils.SpreadMax(DataSourceIn, CycleModeIn)
                .CombineSpreads(base.GetMaxSpreadCount());
        }

        protected override void SubscribeToDevices()
        {
            var dataSource = DataSourceIn.SliceCount > 0 ? DataSourceIn[0] : DataSource.Cursor;
            if (dataSource == DataSource.Cursor)
            {
                DeviceOut.SliceCount = 1;
                DeviceDescriptionOut.SliceCount = 1;
                DeviceOut[0] = CreateCursorMouse();
                DeviceNameOut[0] = string.Empty;
                DeviceDescriptionOut[0] = "Cursor";
                DeviceCountOut[0] = 0;
            }
            else
            {
                base.SubscribeToDevices();
            }
        }

        protected override Mouse CreateDevice(DeviceInfo deviceInfo, int slice)
        {
            // Ignore cycleMode
            var initialPosition = Control.MousePosition;
            var mouseData = new MouseData() { Position = initialPosition };
            var mouseInput = Observable.FromEventPattern<MouseInputEventArgs>(typeof(Device), "MouseInput")
                    .Where(_ => EnabledIn.SliceCount > 0 && EnabledIn[slice]);
            var notifications = mouseInput
                    .Where(ep => ep.EventArgs.Device == deviceInfo.Handle)
                    .SelectMany<EventPattern<MouseInputEventArgs>, MouseNotification>(ep => GenerateMouseNotifications(mouseData, ep.EventArgs));
            return new Mouse(notifications);
        }

        protected override Mouse CreateMergedDevice(int slice)
        {
            // Ignore cycleMode
            var initialPosition = Control.MousePosition;
            var mouseData = new MouseData() { Position = initialPosition };
            var mouseInput = Observable.FromEventPattern<MouseInputEventArgs>(typeof(Device), "MouseInput")
                    .Where(_ => EnabledIn.SliceCount > 0 && EnabledIn[slice]);
            var notifications = mouseInput
                    .SelectMany<EventPattern<MouseInputEventArgs>, MouseNotification>(ep => GenerateMouseNotifications(mouseData, ep.EventArgs));
            return new Mouse(notifications);
        }

        protected override Mouse CreateDummy()
        {
            return Mouse.Empty;
        }

        private Mouse CreateCursorMouse()
        {
            var initialPosition = Control.MousePosition;
            var cycleMode = CycleModeIn.SliceCount > 0 ? CycleModeIn[0] : CycleMode.NoCycle;
            var mouseData = new MouseData() { Position = initialPosition };
            var mouseInput = Observable.FromEventPattern<MouseInputEventArgs>(typeof(Device), "MouseInput")
                    .Where(_ => EnabledIn.SliceCount > 0 && EnabledIn[0]);
            IObservable<MouseNotification> notifications;
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
    }

    [PluginInfo(Name = "MouseEvents", 
                Category = "Mouse", 
                Version = "Join",
                Help = "Creates a virtual mouse based on the given mouse events.")]
    public class MouseEventsJoinNode : IPluginEvaluate, IDisposable
    {
        public ISpread<ISpread<MouseNotificationKind>> EventTypeIn;
        public ISpread<ISpread<Vector2D>> PositionIn;
        public ISpread<ISpread<int>> MouseWheelIn;
        public ISpread<ISpread<int>> ClickCountIn;
        public ISpread<ISpread<bool>> LeftButtonIn;
        public ISpread<ISpread<bool>> MiddleButtonIn;
        public ISpread<ISpread<bool>> RightButtonIn;
        public ISpread<ISpread<bool>> X1ButtonIn;
        public ISpread<ISpread<bool>> X2ButtonIn;
        public IIOContainer<IInStream<int>> BinSizePin;
        public ISpread<Mouse> MouseOut;

        private readonly Spread<Subject<MouseNotification>> FSubjects = new Spread<Subject<MouseNotification>>();

        [ImportingConstructor]
        public MouseEventsJoinNode(IIOFactory factory)
        {
            BinSizePin = factory.CreateBinSizeInput(new InputAttribute("Bin Size") { DefaultValue = InputAttribute.DefaultBinSize, Order = int.MaxValue });
            EventTypeIn = BinSizePin.CreateBinSizeSpread<MouseNotificationKind>(new InputAttribute("Event Type"));
            PositionIn = BinSizePin.CreateBinSizeSpread<Vector2D>(new InputAttribute("Position"));
            MouseWheelIn = BinSizePin.CreateBinSizeSpread<int>(new InputAttribute("Mouse Wheel Delta"));
            ClickCountIn = BinSizePin.CreateBinSizeSpread<int>(new InputAttribute("Click Count") { DefaultValue = 1, MinValue = 1 });
            LeftButtonIn = BinSizePin.CreateBinSizeSpread<bool>(new InputAttribute("Left Button"));
            MiddleButtonIn = BinSizePin.CreateBinSizeSpread<bool>(new InputAttribute("Middle Button"));
            RightButtonIn = BinSizePin.CreateBinSizeSpread<bool>(new InputAttribute("Right Button"));
            X1ButtonIn = BinSizePin.CreateBinSizeSpread<bool>(new InputAttribute("X1 Button"));
            X2ButtonIn = BinSizePin.CreateBinSizeSpread<bool>(new InputAttribute("X2 Button"));
            MouseOut = factory.CreateSpread<Mouse>(new OutputAttribute("Mouse"));
            MouseOut.SliceCount = 0;
        }

        public void Dispose()
        {
            foreach (var subject in FSubjects)
                subject.Dispose();
            BinSizePin.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            var binCount = BinSizePin.IOObject.Length;
            FSubjects.ResizeAndDispose(binCount);
            MouseOut.ResizeAndDismiss(binCount, slice => new Mouse(FSubjects[slice]));
            for (int bin = 0; bin < binCount; bin++)
            {
                var subject = FSubjects[bin];
                var notificationCount = EventTypeIn[bin].SliceCount;
                for (int i = 0; i < notificationCount; i++)
                {
                    var position = PositionIn[bin][i].ToMousePoint();
                    MouseNotification notification;
                    switch (EventTypeIn[bin][i])
                    {
                        case MouseNotificationKind.MouseDown:
                            notification = new MouseDownNotification(position, MouseExtensions.ClientArea, GetMouseButtons(bin, i));
                            break;
                        case MouseNotificationKind.MouseUp:
                            notification = new MouseUpNotification(position, MouseExtensions.ClientArea, GetMouseButtons(bin, i));
                            break;
                        case MouseNotificationKind.MouseMove:
                            notification = new MouseMoveNotification(position, MouseExtensions.ClientArea);
                            break;
                        case MouseNotificationKind.MouseWheel:
                            notification = new MouseWheelNotification(position, MouseExtensions.ClientArea, MouseWheelIn[bin][i]);
                            break;
                        case MouseNotificationKind.MouseClick:
                            notification = new MouseClickNotification(position, MouseExtensions.ClientArea, GetMouseButtons(bin, i), Math.Max(ClickCountIn[bin][i], 1));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    if (notification != null)
                        subject.OnNext(notification);
                }
            }
        }

        MouseButtons GetMouseButtons(int bin, int slice)
        {
            var result = MouseButtons.None;
            if (LeftButtonIn[bin][slice]) result |= MouseButtons.Left;
            if (MiddleButtonIn[bin][slice]) result |= MouseButtons.Middle;
            if (RightButtonIn[bin][slice]) result |= MouseButtons.Right;
            if (X1ButtonIn[bin][slice]) result |= MouseButtons.XButton1;
            if (X2ButtonIn[bin][slice]) result |= MouseButtons.XButton2;
            return result;
        }
    }

    [PluginInfo(Name = "MouseEvents", 
                Category = "Mouse", 
                Version = "Split",
                Help = "Returns all the mouse events of a given mouse.",
                AutoEvaluate = true)]
    public class MouseEventsSplitNode : IPluginEvaluate, IDisposable
    {
        public ISpread<Mouse> MouseIn;
        public ISpread<ISpread<MouseNotificationKind>> EventTypeOut;
        public ISpread<ISpread<Vector2D>> PositionOut;
        public ISpread<ISpread<int>> MouseWheelDeltaOut;
        public ISpread<ISpread<int>> ClickCountOut;
        public ISpread<ISpread<bool>> LeftButtonOut;
        public ISpread<ISpread<bool>> MiddleButtonOut;
        public ISpread<ISpread<bool>> RightButtonOut;
        public ISpread<ISpread<bool>> X1ButtonOut;
        public ISpread<ISpread<bool>> X2ButtonOut;
        public IIOContainer<IOutStream<int>> BinSizePin;

        private static readonly IList<MouseNotification> FEmptyList = new List<MouseNotification>(0);
        private Spread<Tuple<Mouse, IEnumerator<IList<MouseNotification>>>> FEnumerators = new Spread<Tuple<Mouse, IEnumerator<IList<MouseNotification>>>>();

        [ImportingConstructor]
        public MouseEventsSplitNode(IIOFactory factory)
        {
            MouseIn = factory.CreateSpread<Mouse>(new InputAttribute("Mouse"));
            BinSizePin = factory.CreateBinSizeOutput(new OutputAttribute("Bin Size") { Order = int.MaxValue });
            EventTypeOut = BinSizePin.CreateBinSizeSpread<MouseNotificationKind>(new OutputAttribute("Event Type"));
            PositionOut = BinSizePin.CreateBinSizeSpread<Vector2D>(new OutputAttribute("Position"));
            MouseWheelDeltaOut = BinSizePin.CreateBinSizeSpread<int>(new OutputAttribute("Mouse Wheel Delta"));
            ClickCountOut = BinSizePin.CreateBinSizeSpread<int>(new OutputAttribute("Click Count"));
            LeftButtonOut = BinSizePin.CreateBinSizeSpread<bool>(new OutputAttribute("Left Button"));
            MiddleButtonOut = BinSizePin.CreateBinSizeSpread<bool>(new OutputAttribute("Middle Button"));
            RightButtonOut = BinSizePin.CreateBinSizeSpread<bool>(new OutputAttribute("Right Button"));
            X1ButtonOut = BinSizePin.CreateBinSizeSpread<bool>(new OutputAttribute("X1 Button"));
            X2ButtonOut = BinSizePin.CreateBinSizeSpread<bool>(new OutputAttribute("X2 Button"));
        }

        public void Dispose()
        {
            foreach (var tuple in FEnumerators)
                Unsubscribe(tuple);
            BinSizePin.Dispose();
        }

        static Tuple<Mouse, IEnumerator<IList<MouseNotification>>> Subscribe(Mouse mouse)
        {
            return Tuple.Create(
                mouse,
                mouse.MouseNotifications
                    .Chunkify()
                    .GetEnumerator()
            );
        }

        static void Unsubscribe(Tuple<Mouse, IEnumerator<IList<MouseNotification>>> tuple)
        {
            tuple.Item2.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            FEnumerators.Resize(
                spreadMax,
                slice =>
                {
                    var mouse = MouseIn[slice] ?? Mouse.Empty;
                    return Subscribe(mouse);
                },
                Unsubscribe
            );

            EventTypeOut.SliceCount = spreadMax;
            PositionOut.SliceCount = spreadMax;
            MouseWheelDeltaOut.SliceCount = spreadMax;
            ClickCountOut.SliceCount = spreadMax;
            LeftButtonOut.SliceCount = spreadMax;
            MiddleButtonOut.SliceCount = spreadMax;
            RightButtonOut.SliceCount = spreadMax;
            X1ButtonOut.SliceCount = spreadMax;
            X2ButtonOut.SliceCount = spreadMax;

            for (int bin = 0; bin < spreadMax; bin++)
            {
                var mouse = MouseIn[bin] ?? Mouse.Empty;
                var tuple = FEnumerators[bin];
                if (mouse != tuple.Item1)
                {
                    Unsubscribe(tuple);
                    tuple = Subscribe(mouse);
                }

                var enumerator = tuple.Item2;
                var notifications = enumerator.MoveNext()
                    ? enumerator.Current
                    : FEmptyList;

                EventTypeOut[bin].SliceCount = notifications.Count;
                PositionOut[bin].SliceCount = notifications.Count;
                MouseWheelDeltaOut[bin].SliceCount = notifications.Count;
                ClickCountOut[bin].SliceCount = notifications.Count;
                LeftButtonOut[bin].SliceCount = notifications.Count;
                MiddleButtonOut[bin].SliceCount = notifications.Count;
                RightButtonOut[bin].SliceCount = notifications.Count;
                X1ButtonOut[bin].SliceCount = notifications.Count;
                X2ButtonOut[bin].SliceCount = notifications.Count;

                for (int i = 0; i < notifications.Count; i++)
                {
                    var n = notifications[i];
                    EventTypeOut[bin][i] = n.Kind;
                    PositionOut[bin][i] = n.Position.FromMousePoint(n.ClientArea);
                    switch (n.Kind)
                    {
                        case MouseNotificationKind.MouseDown:
                        case MouseNotificationKind.MouseUp:
                            var mouseButton = n as MouseButtonNotification;
                            MouseWheelDeltaOut[bin][i] = 0;
                            ClickCountOut[bin][i] = 0;
                            LeftButtonOut[bin][i] = (mouseButton.Buttons & MouseButtons.Left) > 0;
                            MiddleButtonOut[bin][i] = (mouseButton.Buttons & MouseButtons.Middle) > 0;
                            RightButtonOut[bin][i] = (mouseButton.Buttons & MouseButtons.Right) > 0;
                            X1ButtonOut[bin][i] = (mouseButton.Buttons & MouseButtons.XButton1) > 0;
                            X2ButtonOut[bin][i] = (mouseButton.Buttons & MouseButtons.XButton2) > 0;
                            break;
                        case MouseNotificationKind.MouseMove:
                            MouseWheelDeltaOut[bin][i] = 0;
                            ClickCountOut[bin][i] = 0;
                            LeftButtonOut[bin][i] = false;
                            MiddleButtonOut[bin][i] = false;
                            RightButtonOut[bin][i] = false;
                            X1ButtonOut[bin][i] = false;
                            X2ButtonOut[bin][i] = false;
                            break;
                        case MouseNotificationKind.MouseWheel:
                            var mouseWheel = n as MouseWheelNotification;
                            MouseWheelDeltaOut[bin][i] = mouseWheel.WheelDelta;
                            ClickCountOut[bin][i] = 0;
                            LeftButtonOut[bin][i] = false;
                            MiddleButtonOut[bin][i] = false;
                            RightButtonOut[bin][i] = false;
                            X1ButtonOut[bin][i] = false;
                            X2ButtonOut[bin][i] = false;
                            break;
                        case MouseNotificationKind.MouseClick:
                            var mouseClick = n as MouseClickNotification;
                            MouseWheelDeltaOut[bin][i] = 0;
                            ClickCountOut[bin][i] = mouseClick.ClickCount;
                            LeftButtonOut[bin][i] = (mouseClick.Buttons & MouseButtons.Left) > 0;
                            MiddleButtonOut[bin][i] = (mouseClick.Buttons & MouseButtons.Middle) > 0;
                            RightButtonOut[bin][i] = (mouseClick.Buttons & MouseButtons.Right) > 0;
                            X1ButtonOut[bin][i] = (mouseClick.Buttons & MouseButtons.XButton1) > 0;
                            X2ButtonOut[bin][i] = (mouseClick.Buttons & MouseButtons.XButton2) > 0;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                FEnumerators[bin] = tuple;
            }
        }
    }

    [PluginInfo(Name = "MouseStates", 
                Category = "Mouse", 
                Version = "Split", 
                Help = "Returns the mouse position and the pressed mouse buttons of a given mouse.",
                AutoEvaluate = true)]
    public class MouseStatesSplitNode : IPluginEvaluate, IDisposable
    {
        [Input("Mouse")]
        public ISpread<Mouse> MouseIn;
        [Input("Queue Mode", DefaultEnumEntry = "Discard")]
        public ISpread<QueueMode> QueueModeIn;

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

        private Spread<Subscription2<Mouse, MouseNotification>> FSubscriptions = new Spread<Subscription2<Mouse, MouseNotification>>();
        private Spread<int> FRawMouseWheel = new Spread<int>(1);

        public void Dispose()
        {
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
            FSubscriptions.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            PositionOut.SliceCount = spreadMax;
            MouseWheelOut.SliceCount = spreadMax;
            FRawMouseWheel.SliceCount = spreadMax;
            LeftButtonOut.SliceCount = spreadMax;
            MiddleButtonOut.SliceCount = spreadMax;
            RightButtonOut.SliceCount = spreadMax;
            X1ButtonOut.SliceCount = spreadMax;
            X2ButtonOut.SliceCount = spreadMax;

            FSubscriptions.ResizeAndDispose(
                spreadMax,
                slice =>
                {
                    // Reset states
                    PositionOut[slice] = Vector2D.Zero;
                    MouseWheelOut[slice] = 0;
                    FRawMouseWheel[slice] = 0;
                    LeftButtonOut[slice] = false;
                    MiddleButtonOut[slice] = false;
                    RightButtonOut[slice] = false;
                    X1ButtonOut[slice] = false;
                    X2ButtonOut[slice] = false;
                    return new Subscription2<Mouse, MouseNotification>(m => m.MouseNotifications);
                }
            );

            for (int i = 0; i < spreadMax; i++)
            {
                var notifications = QueueModeIn[i] == QueueMode.Discard
                    ? FSubscriptions[i].ConsumeAll(MouseIn[i])
                    : FSubscriptions[i].ConsumeNext(MouseIn[i]);
                foreach (var n in notifications)
                {
                    switch (n.Kind)
                    {
                        case MouseNotificationKind.MouseDown:
                        case MouseNotificationKind.MouseUp:
                            var mouseButton = n as MouseButtonNotification;
                            var isDown = n.Kind == MouseNotificationKind.MouseDown;
                            if ((mouseButton.Buttons & MouseButtons.Left) > 0)
                                LeftButtonOut[i] = isDown;
                            if ((mouseButton.Buttons & MouseButtons.Middle) > 0)
                                MiddleButtonOut[i] = isDown;
                            if ((mouseButton.Buttons & MouseButtons.Right) > 0)
                                RightButtonOut[i] = isDown;
                            if ((mouseButton.Buttons & MouseButtons.XButton1) > 0)
                                X1ButtonOut[i] = isDown;
                            if ((mouseButton.Buttons & MouseButtons.XButton2) > 0)
                                X2ButtonOut[i] = isDown;
                            break;
                        case MouseNotificationKind.MouseMove:
                            PositionOut[i] = n.Position.FromMousePoint(n.ClientArea);
                            break;
                        case MouseNotificationKind.MouseWheel:
                            var mouseWheel = n as MouseWheelNotification;
                            FRawMouseWheel[i] += mouseWheel.WheelDelta;
                            MouseWheelOut[i] = (int)Math.Round((float)FRawMouseWheel[i] / Const.WHEEL_DELTA);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    [PluginInfo(Name = "MouseStates",
                Category = "Mouse", 
                Version = "Join", 
                Help = "Creates a virtual mouse based on the mouse position and pressed mouse buttons.",
                AutoEvaluate = true)]
    public class MouseStatesJoinNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        static MouseButtons[] Buttons = new MouseButtons[]
        {
            MouseButtons.Left,
            MouseButtons.Middle,
            MouseButtons.Right,
            MouseButtons.XButton1,
            MouseButtons.XButton2
        };

        class MouseState
        {
            public static readonly MouseState Empty = new MouseState();
            public Point Position;
            public int MouseWheel;
            public MouseButtons IsButtonPressed;
            public List<IObserver<MouseNotification>> Observers;
        }

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

        [Output("Mouse")]
        public ISpread<Mouse> MouseOut;

        private readonly Spread<MouseState> MouseStates = new Spread<MouseState>();

        public void OnImportsSatisfied()
        {
            MouseOut.SliceCount = 0;
        }

        private static IEnumerable<MouseNotification> GetNotifications(MouseState oldState, MouseState newState)
        {
            var wheelDelta = newState.MouseWheel - oldState.MouseWheel;
            if (wheelDelta != 0)
                yield return new MouseWheelNotification(newState.Position, MouseExtensions.ClientArea, wheelDelta * Const.WHEEL_DELTA);
            if (newState.IsButtonPressed != oldState.IsButtonPressed)
            {
                var newButton = newState.IsButtonPressed;
                var oldButton = oldState.IsButtonPressed;
                foreach (var button in Buttons)
                {
                    if (newButton.HasFlag(button) && !oldButton.HasFlag(button))
                        yield return new MouseDownNotification(newState.Position, MouseExtensions.ClientArea, button);
                    if (!newButton.HasFlag(button) && oldButton.HasFlag(button))
                        yield return new MouseUpNotification(newState.Position, MouseExtensions.ClientArea, button);
                }
            }
            // Send the move last should anyone downstream use a mouse state join with queue mode = discard
            if (newState.Position != oldState.Position)
                yield return new MouseMoveNotification(newState.Position, MouseExtensions.ClientArea);
        }

        public void Evaluate(int spreadMax)
        {
            // Save the previous slice count
            var oldSliceCount = MouseStates.SliceCount;
            // Set the new slice count
            MouseStates.SliceCount = spreadMax;
            MouseOut.SliceCount = spreadMax;
            for (int i = 0; i < spreadMax; i++)
            {
                // Get the old mouse state
                var oldState = i < oldSliceCount
                    ? MouseStates[i]
                    : MouseState.Empty;
                // Create the new mouse state
                var newState = new MouseState()
                {
                    Position = PositionIn[i].ToMousePoint(),
                    MouseWheel = MouseWheelIn[i],
                    IsButtonPressed =
                        (LeftButtonIn[i] ? MouseButtons.Left : MouseButtons.None) |
                        (MiddleButtonIn[i] ? MouseButtons.Middle : MouseButtons.None) |
                        (RightButtonIn[i] ? MouseButtons.Right : MouseButtons.None) |
                        (X1ButtonIn[i] ? MouseButtons.XButton1 : MouseButtons.None) |
                        (X2ButtonIn[i] ? MouseButtons.XButton2 : MouseButtons.None),
                    Observers = oldState.Observers ?? new List<IObserver<MouseNotification>>()
                };
                // Notify our observers
                foreach (var observer in newState.Observers)
                    NotifyObserver(oldState, newState, observer);
                // Save the new mouse state
                MouseStates[i] = newState;
                // Create a new mouse for this slice
                if (i >= oldSliceCount)
                {
                    var notifications = Observable.Create<MouseNotification>(observer =>
                    {
                        // Add the new observer to our internal list
                        newState.Observers.Add(observer);
                        // Notify a new observer immediately about the observed state change
                        NotifyObserver(oldState, newState, observer);
                        // And remove it from our internal list once it unsubscribes
                        return () => newState.Observers.Remove(observer);
                    });
                    MouseOut[i] = new Mouse(notifications);
                }
            }
        }

        private static void NotifyObserver(MouseState oldState, MouseState newState, IObserver<MouseNotification> observer)
        {
            foreach (var n in GetNotifications(oldState, newState))
                observer.OnNext(n);
        }
    }

    [PluginInfo(
        Name = "MouseMatch", 
        Category = "Mouse",
        Help = "Detects certain mouse events like down/up/click",
        AutoEvaluate = true, 
        Tags = "doubleclick")]
    public class DetectMouseNode : IPluginEvaluate
    {
        [Input("Mouse")]
        public ISpread<Mouse> MouseIn;

        [Input("Event Type", DefaultEnumEntry = "MouseClick")]
        public ISpread<MouseNotificationKind> EventTypeIn;

        [Input("Button", DefaultEnumEntry = "Left")]
        public ISpread<MouseButtons> ButtonIn;

        [Input("Wheel Delta")]
        public ISpread<int> WheelDeltaIn;

        [Input("Click Count", DefaultValue = 1)]
        public ISpread<int> ClickCountIn;

        [Output("Detected", IsBang = true)]
        public ISpread<bool> DetectedOut;

        [Output("Position")]
        public ISpread<Vector2D> PositionOut;

        [Import]
        public ILogger FLogger;

        private readonly Spread<Subscription2<Mouse, MouseNotification>> Subscriptions = new Spread<Subscription2<Mouse, MouseNotification>>();

        public void Evaluate(int spreadMax)
        {
            Subscriptions.ResizeAndDispose(
                spreadMax,
                () => new Subscription2<Mouse, MouseNotification>(mouse => mouse.MouseNotifications));

            DetectedOut.SliceCount = spreadMax;
            PositionOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var detectedNotification = Subscriptions[i].ConsumeAll(MouseIn[i])
                    .Where(n => n.Kind == EventTypeIn[i])
                    .FirstOrDefault(n =>
                        {
                            switch (EventTypeIn[i])
                            {
                                case MouseNotificationKind.MouseClick:
                                    var mouseClick = n as MouseClickNotification;
                                    return mouseClick.Buttons == ButtonIn[i] && mouseClick.ClickCount == ClickCountIn[i];
                                case MouseNotificationKind.MouseDown:
                                case MouseNotificationKind.MouseUp:
                                    var mouseButton = n as MouseButtonNotification;
                                    return mouseButton.Buttons == ButtonIn[i];
                                case MouseNotificationKind.MouseWheel:
                                    var mouseWheel = n as MouseWheelNotification;
                                    return mouseWheel.WheelDelta == WheelDeltaIn[i];
                            }
                            // Mouse move
                            return true;
                        });
                if (detectedNotification != null)
                {
                    DetectedOut[i] = true;
                    PositionOut[i] = detectedNotification.Position.FromMousePoint(detectedNotification.ClientArea);
                }
                else
                {
                    DetectedOut[i] = false;
                    PositionOut[i] = Vector2D.Zero;
                }
                //For debugging only
                //foreach (var n in Subscriptions[i].ConsumeAll(MouseIn[i]).OfType<MouseClickNotification>())
                //{
                //    var s = string.Empty;
                //    if (n.ClickCount == 2)
                //        s = " double";
                //    else if (n.ClickCount == 3)
                //        s = " triple";
                //    else if (n.ClickCount == 4)
                //        s = " quadtruple";
                //    FLogger.Log(LogType.Message, "{0}{1} click at position {2}.", n.Buttons, s, n.Position);
                //}
            }
        }
    }

    [PluginInfo(
        Name = "Merge",
        Category = "Mouse",
        Help = "Merges different mouse devices into one.",
        AutoEvaluate = true)]
    public class MergeMouseNode : IPluginEvaluate
    {
        [Input("Mouse", IsPinGroup = true)]
        public IDiffSpread<ISpread<Mouse>> MiceIn;

        [Output("Mouse")]
        public ISpread<Mouse> MouseOut;

        public void Evaluate(int spreadMax)
        {
            if (!MiceIn.IsChanged)
                return;

            MouseOut.SliceCount = MiceIn.GetMaxSliceCount();
            for (int i = 0; i < MouseOut.SliceCount; i++)
            {
                var mice = MiceIn.Select((ms, j) => ms.SliceCount > 0 ? ms[j] ?? Mouse.Empty : Mouse.Empty);
                MouseOut[i] = new Mouse(Observable.Merge(mice.Select(m => m.MouseNotifications)), false);
            }
        }
    }

    static class MouseExtensions
    {
        public static readonly Size ClientArea = new Size(short.MaxValue, short.MaxValue);

        public static Point ToMousePoint(this Vector2D normV)
        {
            var clientArea = new Vector2D(ClientArea.Width - 1, ClientArea.Height - 1);
            var v = VMath.Map(normV, new Vector2D(-1, 1), new Vector2D(1, -1), Vector2D.Zero, clientArea, TMapMode.Float);
            return new Point((int)v.x, (int)v.y);
        }

        public static Vector2D FromMousePoint(this Point point, Size clientArea)
        {
            var position = new Vector2D(point.X, point.Y);
            var ca = new Vector2D(clientArea.Width - 1, clientArea.Height - 1);
            return VMath.Map(position, Vector2D.Zero, ca, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
        }
    }
}
