using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VVVV.Core.Logging;
using VVVV.Hosting;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.IO;
using VVVV.PluginInterfaces.V2.Win32;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "Touch",
                Category = "Devices", 
                Version = "Window",
                Help = "Returns the touch device of the current render window.")]
    public class WindowTouchNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Handle", DefaultValue = -1)]
        public IDiffSpread<int> HandleIn;

        [Input("Enabled", DefaultBoolean = true, Order = int.MinValue)]
        public IDiffSpread<bool> FEnabledIn;

        [Input("Mode", IsSingle = true)]
        public IDiffSpread<User32.TouchWindowFlags> ModeIn;

        [Output("Device", IsSingle = true)]
        public ISpread<TouchDevice> TouchDeviceOut;

        [Import]
        protected ILogger FLogger;

        [Import]
        protected IOFactory FIOFactory;

        [Import]
        IWindowMessageService FWMService;

        HashSet<Subclass> TouchEnabledWindows = new HashSet<Subclass>();

        private PluginContainer FTouchStatesSplitNode;

        public void OnImportsSatisfied()
        {
            HandleIn.Changed += InputPinChanged;
            FEnabledIn.Changed += InputPinChanged;
            ModeIn.Changed += InputPinChanged;

            var notifications = FWMService.MessageNotifications
                .Where(e => e.EventArgs.Message == WM.TOUCH)
                .SelectMany(e => GenerateTouchNotifications(e));
            TouchDeviceOut[0] = new TouchDevice(notifications);

            // Create a touch states split node for us and connect our touch device out to its touch device in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "TouchStates" && n.Category == "Touch" && n.Version == "Split");
            FTouchStatesSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Touch Device", c => TouchDeviceOut);

            FWMService.SubclassCreated += FWMService_SubclassCreated;
            FWMService.SubclassDestroyed += FWMService_SubclassDestroyed;
        }

        private void FWMService_SubclassDestroyed(object sender, EventArgs e)
        {
            TouchEnabledWindows.Remove(sender as Subclass);
        }

        private void FWMService_SubclassCreated(object sender, EventArgs e)
        {
            InputPinChanged(null);
        }

        private void InputPinChanged(object _)
        {
            if (HandleIn != null && FEnabledIn != null && ModeIn != null)
            {
                int spreadMax = HandleIn.CombineWith(FEnabledIn).CombineWith(ModeIn);
                if (spreadMax > 0)
                {
                    if (HandleIn.Any(i => i < 0)) //handle any window
                    {
                        if (FEnabledIn[0])
                        {
                            foreach (var s in FWMService.Subclasses)
                                if (TryRegisterTouch(s, ModeIn[0]))
                                    TouchEnabledWindows.Add(s);
                        }
                        else
                        {
                            var disabled = new List<Subclass>();
                            foreach (var s in TouchEnabledWindows)
                                if (TryUnregisterTouch(s))
                                    disabled.Add(s);
                            foreach (var s in disabled)
                                TouchEnabledWindows.Remove(s);
                        }
                    }
                    else
                    {
                        var former = new HashSet<Subclass>(TouchEnabledWindows);
                        TouchEnabledWindows.Clear();
                        for (int i = 0; i < spreadMax; i++)
                        {
                            var selected = FWMService.Subclasses.Where(s => s.HWnd.ToInt32() == HandleIn[i]).FirstOrDefault();
                            if (selected != null)
                            {
                                if (FEnabledIn[i])
                                {
                                    if (TryRegisterTouch(selected, ModeIn[i]))
                                    {
                                        TouchEnabledWindows.Add(selected);
                                        former.Remove(selected);
                                    }
                                }
                            }
                        }
                        foreach (var s in former)
                            if (!TryUnregisterTouch(s))
                                TouchEnabledWindows.Add(s);
                    }
                }
            }
        }

        private bool TryRegisterTouch(Subclass subclass, User32.TouchWindowFlags mode)
        {
            if (User32.RegisterTouchWindow(subclass.HWnd, mode))
            {
                TouchEnabledWindows.Add(subclass);
                return true;
            }
            else
            {
                FLogger.Log(LogType.Error, "Failed to enable touch messages for window {0}.", subclass.HWnd);
                return false;
            }
        }

        private bool TryUnregisterTouch(Subclass subclass)
        {
            if (!User32.UnregisterTouchWindow(subclass.HWnd))
            {
                FLogger.Log(LogType.Error, "Failed to disable touch messages for window {0}.", subclass.HWnd);
                return false;
            }
            return true;    
        }

        private IEnumerable<TouchNotification> GenerateTouchNotifications(EventPattern<WMEventArgs> e)
        {
            var a = e.EventArgs;
            var touchPointCount = a.WParam.LoWord();
            if (touchPointCount > 0)
            {
                var touchPoints = new TOUCHINPUT[touchPointCount];
                if (User32.GetTouchInputInfo(a.LParam, touchPointCount, touchPoints, Marshal.SizeOf(typeof(TOUCHINPUT))))
                {
                    a.Handled = true;

                    try
                    {
                        RECT cr;
                        if (User32.GetClientRect(a.HWnd, out cr))
                        {
                            foreach (var touchPoint in touchPoints)
                            {
                                var position = new Point(touchPoint.x / 100, touchPoint.y / 100);
                                User32.ScreenToClient(a.HWnd, ref position);
                                var clientArea = new Size(cr.Width, cr.Height);
                                var id = touchPoint.dwID;
                                var primary = (touchPoint.dwFlags & Const.TOUCHEVENTF_PRIMARY) > 0;
                                var contactArea = (touchPoint.dwMask & Const.TOUCHINPUTMASKF_CONTACTAREA) > 0
                                    ? new Size(touchPoint.cxContact / 100, touchPoint.cyContact / 100)
                                    : Size.Empty;

                                if ((touchPoint.dwFlags & Const.TOUCHEVENTF_DOWN) > 0)
                                    yield return new TouchDownNotification(position, clientArea, id, primary, contactArea, touchPoint.hSource.ToInt64(), e.Sender);
                                if ((touchPoint.dwFlags & Const.TOUCHEVENTF_MOVE) > 0)
                                    yield return new TouchMoveNotification(position, clientArea, id, primary, contactArea, touchPoint.hSource.ToInt64(), e.Sender);
                                if ((touchPoint.dwFlags & Const.TOUCHEVENTF_UP) > 0)
                                    yield return new TouchUpNotification(position, clientArea, id, primary, contactArea, touchPoint.hSource.ToInt64(), e.Sender);
                            }
                        }
                    }
                    finally
                    {
                        User32.CloseTouchInputHandle(a.LParam);
                    }
                }
            }
            yield break;
        }

        public void Dispose()
        {
            FWMService.SubclassCreated -= FWMService_SubclassCreated;
            FWMService.SubclassDestroyed -= FWMService_SubclassDestroyed;
            foreach (var s in TouchEnabledWindows)
                TryUnregisterTouch(s);
        }

        public void Evaluate(int spreadMax)
        {
            // Evaluate our split plugin
            FTouchStatesSplitNode.Evaluate(spreadMax);
        }
    }

    [PluginInfo(Name = "TouchEvents", 
                Category = "Touch",
                Version = "Split", 
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true,
                Bugs = "Not spreadable")]
    public class TouchEventsSplitNode : IPluginEvaluate, IDisposable
    {
        [Input("Touch Device", IsSingle = true)]
        public ISpread<TouchDevice> FInput;

        [Output("Event Type")]
        public ISpread<TouchNotificationKind> FEventTypeOut;
        [Output("Position (Pixel) ", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionPixelOut;
        [Output("Position (Projection) ")]
        public ISpread<Vector2D> PositionInProjectionSpaceOut;
        [Output("Position (Normalized Projection) ", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionInNormalizedProjectionOut;
        [Output("Position (Normalized Window) ", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionOut;
        [Output("Id")]
        public ISpread<int> IdOut;
        [Output("Primary")]
        public ISpread<bool> PrimaryOut;
        [Output("Contact Area")]
        public ISpread<Vector2D> ContactAreaOut;

        [Output("Device ID")]
        public ISpread<long> DeviceIDOut;

        private static readonly IList<TouchNotification> FEmptyList = new List<TouchNotification>(0);
        private TouchDevice FTouchDevice;
        private IEnumerator<IList<TouchNotification>> FEnumerator;

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Evaluate(int spreadMax)
        {
            var touchDevice = FInput[0] ?? TouchDevice.Empty;
            if (touchDevice != FTouchDevice)
            {
                Unsubscribe();
                FTouchDevice = touchDevice;
                Subscribe();
            }

            var notifications = FEnumerator.MoveNext()
                ? FEnumerator.Current
                : FEmptyList;
            FEventTypeOut.SliceCount = notifications.Count;
            PositionPixelOut.SliceCount = notifications.Count;
            PositionInProjectionSpaceOut.SliceCount = notifications.Count;
            PositionInNormalizedProjectionOut.SliceCount = notifications.Count;
            PositionOut.SliceCount = notifications.Count;
            IdOut.SliceCount = notifications.Count;
            PrimaryOut.SliceCount = notifications.Count;
            ContactAreaOut.SliceCount = notifications.Count;
            DeviceIDOut.SliceCount = notifications.Count;

            for (int i = 0; i < notifications.Count; i++)
            {
                var n = notifications[i];
                FEventTypeOut[i] = n.Kind;
                Vector2D inNormalizedProjection, inProjection;

                SpaceHelpers.MapFromPixels(n.Position, n.Sender, n.ClientArea,
                    out inNormalizedProjection, out inProjection);

                PositionOut[i] = MouseExtensions.GetLegacyMousePositon(n.Position, n.ClientArea);
                PositionInProjectionSpaceOut[i] = inProjection;
                PositionInNormalizedProjectionOut[i] = inNormalizedProjection;

                PositionPixelOut[i] = new Vector2D(n.Position.X, n.Position.Y);
                IdOut[i] = n.Id;
                PrimaryOut[i] = n.Primary;
                ContactAreaOut[i] = new Vector2D(n.ContactArea.Width, n.ContactArea.Height);
                DeviceIDOut[i] = n.TouchDeviceID;
            }
        }

        private void Subscribe()
        {
            if (FTouchDevice != null)
            {
                FEnumerator = FTouchDevice.Notifications
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

    [PluginInfo(Name = "TouchStates", 
                Category = "Touch",
                Version = "Split", 
                Help = "Returns the touched points of a given touch device.",
                AutoEvaluate = true,
                Bugs = "Not spreadable")]
    public class TouchStatesSplitNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Touch Device", IsSingle = true)]
        public ISpread<TouchDevice> MouseIn;
        [Input("Queue Mode", IsSingle = true, DefaultEnumEntry = "Discard")]
        public ISpread<QueueMode> QueueModeIn;

        [Output("Position (Pixel) ", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionPixelOut;
        [Output("Position (Projection) ")]
        public ISpread<Vector2D> PositionInProjectionSpaceOut;
        [Output("Position (Normalized Projection) ", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionInNormalizedProjectionOut;
        [Output("Position (Normalized Window) ", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionOut;
        [Output("Id")]
        public ISpread<int> IdOut;
        [Output("Primary")]
        public ISpread<bool> PrimaryOut;
        [Output("Contact Area")]
        public ISpread<Vector2D> ContactAreaOut;
        [Output("Is New")]
        public ISpread<bool> IsNewOut;

        [Output("Device ID")]
        public ISpread<long> DeviceIDOut;

        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();
        private Subscription<TouchDevice, TouchNotification> FSubscription;

        public void OnImportsSatisfied()
        {
            PositionPixelOut.SliceCount = 0;
            PositionInProjectionSpaceOut.SliceCount = 0;
            PositionInNormalizedProjectionOut.SliceCount = 0;
            PositionOut.SliceCount = 0;
            IdOut.SliceCount = 0;
            PrimaryOut.SliceCount = 0;
            ContactAreaOut.SliceCount = 0;
            IsNewOut.SliceCount = 0;
            DeviceIDOut.SliceCount = 0;

            FSubscription = new Subscription<TouchDevice, TouchNotification>(
                touchDevice =>
                {
                    return touchDevice.Notifications;
                },
                (touchDevice, n) =>
                {
                    Vector2D inNormalizedProjection, inProjection;
                    SpaceHelpers.MapFromPixels(n.Position, n.Sender, n.ClientArea,
                        out inNormalizedProjection, out inProjection);
                    var normalizedPosition = MouseExtensions.GetLegacyMousePositon(n.Position, n.ClientArea);
                    var inPixels = new Vector2D(n.Position.X, n.Position.Y);

                    var contactArea = new Vector2D(n.ContactArea.Width, n.ContactArea.Height);
                    var index = IdOut.IndexOf(n.Id);
                    var primary = n.Primary;
                    var deviceid = n.TouchDeviceID;
                    switch (n.Kind)
                    {
                        case TouchNotificationKind.TouchDown:
                            if (index < 0)
                            {
                                IdOut.Add(n.Id);
                                PositionPixelOut.Add(inPixels);
                                PositionInProjectionSpaceOut.Add(inProjection);
                                PositionInNormalizedProjectionOut.Add(inNormalizedProjection);
                                PositionOut.Add(normalizedPosition);
                                PrimaryOut.Add(primary);
                                ContactAreaOut.Add(contactArea);
                                IsNewOut.Add(true);
                                DeviceIDOut.Add(deviceid);
                            }
                            break;
                        case TouchNotificationKind.TouchUp:
                            if (index >= 0)
                            {
                                IdOut.RemoveAt(index);
                                PositionPixelOut.RemoveAt(index);
                                PositionInProjectionSpaceOut.RemoveAt(index);
                                PositionInNormalizedProjectionOut.RemoveAt(index);
                                PositionOut.RemoveAt(index);
                                PrimaryOut.RemoveAt(index);
                                ContactAreaOut.RemoveAt(index);
                                IsNewOut.RemoveAt(index);
                                DeviceIDOut.RemoveAt(index);
                            }
                            break;
                        case TouchNotificationKind.TouchMove:
                            if (index >= 0)
                            {
                                PositionPixelOut[index] = inPixels;
                                PositionInProjectionSpaceOut[index] = inProjection;
                                PositionInNormalizedProjectionOut[index] = inNormalizedProjection;
                                PositionOut[index] = normalizedPosition;
                                PrimaryOut[index] = primary;
                                ContactAreaOut[index] = contactArea;
                                IsNewOut[index] = false;
                            }
                            break;
                        default:
                            throw new NotImplementedException();
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
            FScheduler.Run(QueueModeIn[0]);
        }
    }
}
