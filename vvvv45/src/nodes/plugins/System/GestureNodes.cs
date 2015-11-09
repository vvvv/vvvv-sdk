using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "Gesture",
                Category = "Devices", 
                Version = "Window",
                Help = "Returns the gesture device of current render windows.", 
                AutoEvaluate = true)]
    public class WindowGestureNode : WindowMessageNode, IPluginEvaluate
    {
        [Output("Device", IsSingle = true)]
        public ISpread<GestureDevice> GestureDeviceOut;

        [Output("Event Type")]
        public ISpread<GestureNotificationKind> GestureTypeOut;
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;
        [Output("Id")]
        public ISpread<int> IdOut;
        [Output("Device ID")]
        public ISpread<long> DeviceIDOut;

        [Import]
        protected ILogger FLogger;

        private int FGestureConfigSize;
        private int FGestureInfoSize;

        private static readonly IList<GestureNotification> FEmptyList = new List<GestureNotification>(0);
        private IEnumerator<IList<GestureNotification>> FEnumerator;

        public override void OnImportsSatisfied()
        {
            FGestureConfigSize = Marshal.SizeOf(new GESTURECONFIG());
            FGestureInfoSize = Marshal.SizeOf(new GESTUREINFO());

            GestureTypeOut.SliceCount = 0;
            PositionOut.SliceCount = 0;
            IdOut.SliceCount = 0;
            DeviceIDOut.SliceCount = 0;

            base.OnImportsSatisfied();
        }

        protected override void SubclassCreated(Subclass subclass)
        {
            GESTURECONFIG gc = new GESTURECONFIG();
            gc.dwID = 0;                // gesture ID
            gc.dwWant = Const.GC_ALLGESTURES; // settings related to gesture ID that are to be turned on
            gc.dwBlock = 0; // settings related to gesture ID that are to be     

            // We must p/invoke into user32 [winuser.h]
            bool bResult = User32.SetGestureConfig(
                subclass.HWnd, // window for which configuration is specified
                0,      // reserved, must be 0
                1,      // count of GESTURECONFIG structures
                ref gc, // array of GESTURECONFIG structures, dwIDs will be processed in the order specified 
                        // and repeated occurances will overwrite previous ones
                FGestureConfigSize // sizeof(GESTURECONFIG)
            );
            base.SubclassCreated(subclass);
        }

        protected override void Initialize(IObservable<WMEventArgs> windowMessages, IObservable<bool> disabled)
        {
            var notifications = windowMessages
                .Where(e => (e.Message == WM.GESTURENOTIFY) || (e.Message == WM.GESTURE))
                .SelectMany<WMEventArgs, GestureNotification>(e => GenerateGestureNotifications(e));
            GestureDeviceOut[0] = new GestureDevice(notifications);

            FEnumerator = GestureDeviceOut[0].Notifications
                    .Chunkify()
                    .GetEnumerator();
        }

        private IEnumerable<GestureNotification> GenerateGestureNotifications(WMEventArgs e)
        {
            if (e.Message == WM.GESTURENOTIFY)
            {
                e.Handled = FEnabledIn[0];
                yield break;
            }
            else
            {
                var gestureInfo = new GESTUREINFO();
                gestureInfo.cbSize = FGestureInfoSize;
                if (User32.GetGestureInfo(e.LParam, ref gestureInfo))
                {
                    try
                    {
                        RECT cr;
                        if (User32.GetClientRect(e.HWnd, out cr))
                        {
                            var position = new Point(gestureInfo.x, gestureInfo.y);
                            User32.ScreenToClient(e.HWnd, ref position);

                            e.Handled = true;
                            yield return new GestureNotification((GestureNotificationKind)gestureInfo.dwID, 
                                                                position,
                                                                new Size(cr.Width, cr.Height),
                                                                gestureInfo.dwInstanceID,
                                                                gestureInfo.dwSequenceID, 
                                                                gestureInfo.hwndTarget.ToInt64(),
                                                                gestureInfo.dwFlags,
                                                                gestureInfo.ullArguments,
                                                                gestureInfo.cbExtraArgs);
                        }
                    }
                    finally
                    {
                        e.Handled = false;
                    }
                }
                yield break;
            }
        }

        public void Evaluate(int spreadMax)
        {
            var notifications = FEnumerator.MoveNext()
                ? FEnumerator.Current
                : FEmptyList;

            GestureTypeOut.SliceCount = notifications.Count;
            PositionOut.SliceCount = notifications.Count;
            IdOut.SliceCount = notifications.Count;
            DeviceIDOut.SliceCount = notifications.Count;
            for (int i=0; i<notifications.Count; i++)
            {
                GestureTypeOut[i] = notifications[i].Kind;
                PositionOut[i] = VMath.Map(new Vector2D(notifications[i].Position.X,notifications[i].Position.Y), 
                                            Vector2D.Zero,
                                            new Vector2D(notifications[i].ClientArea.Width, notifications[i].ClientArea.Width), 
                                            new Vector2D(-1, 1), 
                                            new Vector2D(1, -1), TMapMode.Float);
                IdOut[i] = notifications[i].Id;
                DeviceIDOut[i] = notifications[i].GestureDeviceID;
            }
        }

        public override void Dispose()
        {
            if (FEnumerator != null)
            {
                FEnumerator.Dispose();
                FEnumerator = null;
            }
            base.Dispose();
        }
    }

    public abstract class GestureEventsSplitNode : IPluginEvaluate, IDisposable
    {
        [Input("Gesture Device", IsSingle = true)]
        public ISpread<GestureDevice> FInput;

        [Output("Id")]
        public ISpread<int> IdOut;
        [Output("Device ID")]
        public ISpread<long> DeviceIDOut;

        private static readonly IList<GestureNotification> FEmptyList = new List<GestureNotification>(0);
        private GestureDevice FGestureDevice;
        private IEnumerator<IList<GestureNotification>> FEnumerator;

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Evaluate(int spreadMax)
        {
            var gestureDevice = FInput[0] ?? GestureDevice.Empty;
            if (gestureDevice != FGestureDevice)
            {
                Unsubscribe();
                FGestureDevice = gestureDevice;
                Subscribe();
            }

            var notifications = FEnumerator.MoveNext()
                ? FEnumerator.Current
                : FEmptyList;

            notifications = UseNotifications(notifications, IdOut).ToList();

            IdOut.SliceCount = notifications.Count;
            DeviceIDOut.SliceCount = notifications.Count;
            for (int i = 0; i < notifications.Count; i++)
            {
                IdOut[i] = notifications[i].Id;
                DeviceIDOut[i] = notifications[i].GestureDeviceID;
            }
        }

        public abstract IEnumerable<GestureNotification> UseNotifications(IList<GestureNotification> notifications, ISpread<int> ids);

        private void Subscribe()
        {
            if (FGestureDevice != null)
            {
                FEnumerator = FGestureDevice.Notifications
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

    [PluginInfo(Name = "ZoomEvents",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class ZoomGestureEventsSplitNode : GestureEventsSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;
        [Output("Distance")]
        public ISpread<int> DistanceOut;

        public override IEnumerable<GestureNotification> UseNotifications(IList<GestureNotification> gestures, ISpread<int> ids)
        {
            PositionOut.SliceCount = 0;
            DistanceOut.SliceCount = 0;
            foreach (var gesture in gestures)
            {
                if ((gesture.Kind == GestureNotificationKind.GestureZoom) ||
                    (gesture.Kind == GestureNotificationKind.GestureEnd && ids.Contains(gesture.Id)))
                {
                    var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                    var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                    var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                    PositionOut.Add(normalizedPosition);
                    int distance = (int)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                    DistanceOut.Add(distance);
                    yield return gesture;
                }
            }
        }
    }

    [PluginInfo(Name = "TwoFingerTapEvents",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class TwoFingerTapGestureEventsSplitNode : GestureEventsSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;

        public override IEnumerable<GestureNotification> UseNotifications(IList<GestureNotification> gestures, ISpread<int> ids)
        {
            PositionOut.SliceCount = 0;
            foreach (var gesture in gestures)
            {
                if ((gesture.Kind == GestureNotificationKind.GestureTwoFingerTap) ||
                    (gesture.Kind == GestureNotificationKind.GestureEnd && ids.Contains(gesture.Id)))
                {
                    var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                    var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                    var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                    PositionOut.Add(normalizedPosition);
                    yield return gesture;
                }
            }
        }
    }

    [PluginInfo(Name = "PanEvents",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class PanGestureEventsSplitNode : GestureEventsSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;

        public override IEnumerable<GestureNotification> UseNotifications(IList<GestureNotification> gestures, ISpread<int> ids)
        {
            PositionOut.SliceCount = 0;
            foreach (var gesture in gestures)
            {
                if ((gesture.Kind == GestureNotificationKind.GesturePan) ||
                    (gesture.Kind == GestureNotificationKind.GestureEnd && ids.Contains(gesture.Id)))
                {
                    var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                    var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                    var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                    PositionOut.Add(normalizedPosition);
                    yield return gesture;
                }
            }
        }
    }

    [PluginInfo(Name = "RotateEvents",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class RotateGestureEventsSplitNode : GestureEventsSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;
        [Output("Rotate")]
        public ISpread<double> RotateOut;

        public override IEnumerable<GestureNotification> UseNotifications(IList<GestureNotification> gestures, ISpread<int> ids)
        {
            PositionOut.SliceCount = 0;
            RotateOut.SliceCount = 0;
            foreach (var gesture in gestures)
            {
                if ((gesture.Kind == GestureNotificationKind.GestureRotate) ||
                    (gesture.Kind == GestureNotificationKind.GestureEnd && ids.Contains(gesture.Id)))
                {
                    var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                    var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                    var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                    PositionOut.Add(normalizedPosition);
                    double rotate = (double)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                    rotate = rotate / 65535.0 * 2 - 1;
                    RotateOut.Add(rotate);

                    yield return gesture;
                }
            }
        }
    }

    [PluginInfo(Name = "PressAndTapEvents",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class PressAndTapGestureEventsSplitNode : GestureEventsSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;

        public override IEnumerable<GestureNotification> UseNotifications(IList<GestureNotification> gestures, ISpread<int> ids)
        {
            PositionOut.SliceCount = 0;
            foreach (var gesture in gestures)
            {
                if ((gesture.Kind == GestureNotificationKind.GesturePressAndTap) ||
                    (gesture.Kind == GestureNotificationKind.GestureEnd && ids.Contains(gesture.Id)))
                {
                    var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                    var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                    var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                    PositionOut.Add(normalizedPosition);
                    yield return gesture;
                }
            }
        }
    }

    public abstract class GestureStatesSplitNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Gesture Device", IsSingle = true)]
        public ISpread<GestureDevice> GestureIn;
        [Input("Queue Mode", IsSingle = true, DefaultEnumEntry = "Discard")]
        public ISpread<QueueMode> QueueModeIn;


        [Output("Id")]
        public ISpread<int> IdOut;
        [Output("Device ID")]
        public ISpread<long> DeviceIDOut;

        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();
        private Subscription<GestureDevice, GestureNotification> FSubscription;

        public virtual void OnImportsSatisfied()
        {
            IdOut.SliceCount = 0;
            DeviceIDOut.SliceCount = 0;

            FSubscription = new Subscription<GestureDevice, GestureNotification>(
                gestureDevice =>
                {
                    return gestureDevice.Notifications.Where(g => FilterGesture(g));
                },
                (gestureDevice, n) =>
                {
                    var index = IdOut.IndexOf(n.Id);
                    if (UseGesture(n, index))
                    {
                        if (index < 0)
                        {
                            IdOut.Add(n.Id);
                            DeviceIDOut.Add(n.GestureDeviceID);
                        }
                        else
                        {
                            IdOut[index] = n.Id;
                            DeviceIDOut[index] = n.GestureDeviceID;
                        }
                    }
                    else if (index >= 0)
                    {
                        IdOut.RemoveAt(index);
                        DeviceIDOut.RemoveAt(index);
                    }
                    
                },
                FScheduler
            );
        }

        public abstract bool FilterGesture(GestureNotification gesture);
        public abstract bool UseGesture(GestureNotification gesture, int slice);

        public virtual void Dispose()
        {
            FSubscription.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            //resubsribe if necessary
            FSubscription.Update(GestureIn[0]);
            //process events
            FScheduler.Run(QueueModeIn[0]);
        }
    }

    [PluginInfo(Name = "ZoomState",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class ZoomGestureStateSplitNode : GestureStatesSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;
        [Output("Distance")]
        public ISpread<double> DistanceOut;

        public override void OnImportsSatisfied()
        {
            PositionOut.SliceCount = 0;
            DistanceOut.SliceCount = 0;
            base.OnImportsSatisfied();
        }

        public override bool FilterGesture(GestureNotification gesture)
        {
            return ((gesture.Kind == GestureNotificationKind.GestureZoom) || (gesture.Kind == GestureNotificationKind.GestureEnd));
        }

        public override bool UseGesture(GestureNotification gesture, int slice)
        {
            var use = gesture.Kind == GestureNotificationKind.GestureZoom;
            if (use)
            {
                var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                int distance = (int)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                if (slice < 0)
                {
                    PositionOut.Add(normalizedPosition);
                    DistanceOut.Add(distance/(double)gesture.ClientArea.Height);
                }
                else
                {
                    PositionOut[slice] = normalizedPosition;
                    DistanceOut[slice] = distance / (double)gesture.ClientArea.Height;
                }
            }
            else if (slice >= 0)
            {
                PositionOut.RemoveAt(slice);
                DistanceOut.RemoveAt(slice);
            }

            return use;
        }
    }

    [PluginInfo(Name = "PanState",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class PanGestureStateSplitNode : GestureStatesSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;

        public override void OnImportsSatisfied()
        {
            PositionOut.SliceCount = 0;
            base.OnImportsSatisfied();
        }

        public override bool FilterGesture(GestureNotification gesture)
        {
            return ((gesture.Kind == GestureNotificationKind.GesturePan) || (gesture.Kind == GestureNotificationKind.GestureEnd));
        }

        public override bool UseGesture(GestureNotification gesture, int slice)
        {
            var use = gesture.Kind == GestureNotificationKind.GesturePan;
            if (use)
            {
                var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                if (slice < 0)
                    PositionOut.Add(normalizedPosition);
                else
                    PositionOut[slice] = normalizedPosition;
            }
            else if (slice >= 0)
                PositionOut.RemoveAt(slice);

            return use;
        }
    }

    [PluginInfo(Name = "RotateState",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class RotateGestureStateSplitNode : GestureStatesSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;
        [Output("Rotate")]
        public ISpread<double> RotateOut;

        public override void OnImportsSatisfied()
        {
            PositionOut.SliceCount = 0;
            RotateOut.SliceCount = 0;
            base.OnImportsSatisfied();
        }

        public override bool FilterGesture(GestureNotification gesture)
        {
            return ((gesture.Kind == GestureNotificationKind.GestureRotate) || (gesture.Kind == GestureNotificationKind.GestureEnd));
        }

        public override bool UseGesture(GestureNotification gesture, int slice)
        {
            var use = gesture.Kind == GestureNotificationKind.GestureRotate;
            if (use)
            {
                var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                double rotate = (double)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                rotate = rotate / 65535.0 * 2 - 1;
                
                if (slice < 0)
                {
                    PositionOut.Add(normalizedPosition);
                    RotateOut.Add(rotate);
                }
                else
                {
                    PositionOut[slice] = normalizedPosition;
                    RotateOut[slice] = rotate;
                }
            }
            else if (slice >= 0)
            {
                PositionOut.RemoveAt(slice);
                RotateOut.RemoveAt(slice);
            }

            return use;
        }
    }

    [PluginInfo(Name = "PressAndTapState",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class PressAndTapGestureStateSplitNode : GestureStatesSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;

        public override void OnImportsSatisfied()
        {
            PositionOut.SliceCount = 0;
            base.OnImportsSatisfied();
        }

        public override bool FilterGesture(GestureNotification gesture)
        {
            return ((gesture.Kind == GestureNotificationKind.GesturePressAndTap) || (gesture.Kind == GestureNotificationKind.GestureEnd));
        }

        public override bool UseGesture(GestureNotification gesture, int slice)
        {
            var use = gesture.Kind == GestureNotificationKind.GesturePressAndTap;
            if (use)
            {
                var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                if (slice < 0)
                    PositionOut.Add(normalizedPosition);
                else
                    PositionOut[slice] = normalizedPosition;
            }
            else if (slice >= 0)
                PositionOut.RemoveAt(slice);

            return use;
        }
    }

    [PluginInfo(Name = "TwoFingerTapState",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class TwoFingerTapGestureStateSplitNode : GestureStatesSplitNode
    {
        [Output("Position")]
        public ISpread<Vector2D> PositionOut;

        public override void OnImportsSatisfied()
        {
            PositionOut.SliceCount = 0;
            base.OnImportsSatisfied();
        }

        public override bool FilterGesture(GestureNotification gesture)
        {
            return ((gesture.Kind == GestureNotificationKind.GestureTwoFingerTap) || (gesture.Kind == GestureNotificationKind.GestureEnd));
        }

        public override bool UseGesture(GestureNotification gesture, int slice)
        {
            var use = gesture.Kind == GestureNotificationKind.GestureTwoFingerTap;
            if (use)
            {
                var position = new Vector2D(gesture.Position.X, gesture.Position.Y);
                var clientArea = new Vector2D(gesture.ClientArea.Width, gesture.ClientArea.Height);
                var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);
                if (slice < 0)
                    PositionOut.Add(normalizedPosition);
                else
                    PositionOut[slice] = normalizedPosition;
            }
            else if (slice >= 0)
                PositionOut.RemoveAt(slice);

            return use;
        }
    }
}