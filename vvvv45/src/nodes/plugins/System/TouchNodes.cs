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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VVVV.Core.Logging;
using VVVV.Hosting;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "Touch",
                Category = "Devices", 
                Version = "Window",
                Help = "Returns the touch device of the current render window.")]
    public class WindowTouchNode : WindowMessageNode, IPluginEvaluate
    {
        [Input("Mode", IsSingle = true)]
        public IDiffSpread<User32.TouchWindowFlags> ModeIn;

        [Output("Device", IsSingle = true)]
        public ISpread<TouchDevice> TouchDeviceOut;

        [Import]
        protected ILogger FLogger;

        private PluginContainer FTouchStatesSplitNode;

        protected override void SubclassCreated(Subclass subclass)
        {
            var mode = ModeIn.SliceCount > 0
                ? ModeIn[0]
                : User32.TouchWindowFlags.None;
            TryEnabledTouchMessage(subclass, mode);
            base.SubclassCreated(subclass);
        }

        private void TryEnabledTouchMessage(Subclass subclass, User32.TouchWindowFlags mode)
        {
            if (!User32.RegisterTouchWindow(subclass.HWnd, mode))
                FLogger.Log(LogType.Error, "Failed to enabled touch messages for window {0}.", subclass.HWnd);
        }

        protected override void Initialize(IObservable<WMEventArgs> windowMessages)
        {
            var notifications = windowMessages
                .Where(e => e.Message == WM.TOUCH)
                .SelectMany<WMEventArgs, TouchNotification>(e => GenerateTouchNotifications(e));
            TouchDeviceOut[0] = new TouchDevice(notifications);

            // Create a touch states split node for us and connect our touch device out to its touch device in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "TouchStates" && n.Category == "Touch" && n.Version == "Split");
            FTouchStatesSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Touch Device", c => TouchDeviceOut);

            ModeIn.Changed += ModeIn_Changed;
        }

        private IEnumerable<TouchNotification> GenerateTouchNotifications(WMEventArgs e)
        {
            var touchPointCount = e.WParam.LoWord();
            if (touchPointCount > 0)
            {
                var touchPoints = new TOUCHINPUT[touchPointCount];
                if (User32.GetTouchInputInfo(e.LParam, touchPointCount, touchPoints, Marshal.SizeOf(typeof(TOUCHINPUT))))
                {
                    e.Handled = true;

                    try
                    {
                        RECT cr;
                        if (User32.GetClientRect(e.HWnd, out cr))
                        {
                            foreach (var touchPoint in touchPoints)
                            {
                                var position = new Point(touchPoint.x / 100, touchPoint.y / 100);
                                User32.ScreenToClient(e.HWnd, ref position);
                                var clientArea = new Size(cr.Width, cr.Height);
                                var id = touchPoint.dwID;
                                var contactArea = (touchPoint.dwMask & Const.TOUCHINPUTMASKF_CONTACTAREA) > 0
                                    ? new Size(touchPoint.cxContact / 100, touchPoint.cyContact / 100)
                                    : Size.Empty;

                                if ((touchPoint.dwFlags & Const.TOUCHEVENTF_DOWN) > 0)
                                    yield return new TouchDownNotification(position, clientArea, id, contactArea);
                                if ((touchPoint.dwFlags & Const.TOUCHEVENTF_MOVE) > 0)
                                    yield return new TouchMoveNotification(position, clientArea, id, contactArea);
                                if ((touchPoint.dwFlags & Const.TOUCHEVENTF_UP) > 0)
                                    yield return new TouchUpNotification(position, clientArea, id, contactArea);
                            }
                        }
                    }
                    finally
                    {
                        User32.CloseTouchInputHandle(e.LParam);
                    }
                }
            }
            yield break;
        }

        void ModeIn_Changed(IDiffSpread<User32.TouchWindowFlags> spread)
        {
            var mode = ModeIn.SliceCount > 0
                ? ModeIn[0]
                : User32.TouchWindowFlags.None;
            foreach (var subclass in Subclasses)
                TryEnabledTouchMessage(subclass, mode);
        }

        public override void Dispose()
        {
            FTouchStatesSplitNode.Dispose();
            base.Dispose();
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
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;
        [Output("Id")]
        public ISpread<int> IdOut;
        [Output("Contact Area")]
        public ISpread<Vector2D> ContactAreaOut;

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
            PositionOut.SliceCount = notifications.Count;
            IdOut.SliceCount = notifications.Count;
            ContactAreaOut.SliceCount = notifications.Count;

            for (int i = 0; i < notifications.Count; i++)
            {
                var n = notifications[i];
                FEventTypeOut[i] = n.Kind;
                var position = new Vector2D(n.Position.X, n.Position.Y);
                var clientArea = new Vector2D(n.ClientArea.Width, n.ClientArea.Height);
                PositionOut[i] = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                IdOut[i] = n.Id;
                ContactAreaOut[i] = new Vector2D(n.ContactArea.Width, n.ContactArea.Height);
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

        [Output("Position")]
        public ISpread<Vector2D> PositionOut;
        [Output("Id")]
        public ISpread<int> IdOut;
        [Output("Contact Area")]
        public ISpread<Vector2D> ContactAreaOut;
        [Output("Is New")]
        public ISpread<bool> IsNewOut;

        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();
        private Subscription<TouchDevice, TouchNotification> FSubscription;

        public void OnImportsSatisfied()
        {
            PositionOut.SliceCount = 0;
            IdOut.SliceCount = 0;
            ContactAreaOut.SliceCount = 0;
            IsNewOut.SliceCount = 0;
            FSubscription = new Subscription<TouchDevice, TouchNotification>(
                touchDevice =>
                {
                    return touchDevice.Notifications;
                },
                (touchDevice, n) =>
                {
                    var position = new Vector2D(n.Position.X, n.Position.Y);
                    var clientArea = new Vector2D(n.ClientArea.Width, n.ClientArea.Height);
                    var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                    var contactArea = new Vector2D(n.ContactArea.Width, n.ContactArea.Height);
                    var index = IdOut.IndexOf(n.Id);
                    switch (n.Kind)
                    {
                        case TouchNotificationKind.TouchDown:
                            if (index < 0)
                            {
                                IdOut.Add(n.Id);
                                PositionOut.Add(normalizedPosition);
                                ContactAreaOut.Add(contactArea);
                                IsNewOut.Add(true);
                            }
                            break;
                        case TouchNotificationKind.TouchUp:
                            if (index >= 0)
                            {
                                IdOut.RemoveAt(index);
                                PositionOut.RemoveAt(index);
                                ContactAreaOut.RemoveAt(index);
                                IsNewOut.RemoveAt(index);
                            }
                            break;
                        case TouchNotificationKind.TouchMove:
                            if (index >= 0)
                            {
                                PositionOut[index] = normalizedPosition;
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
