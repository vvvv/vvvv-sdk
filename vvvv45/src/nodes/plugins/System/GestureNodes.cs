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
using VVVV.PluginInterfaces.V2.IO;
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
            PositionPixelOut.SliceCount = 0;
            PositionInProjectionSpaceOut.SliceCount = 0;
            PositionInNormalizedProjectionOut.SliceCount = 0;
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

        protected override void Initialize(IObservable<EventPattern<WMEventArgs>> windowMessages, IObservable<bool> disabled)
        {
            var notifications = windowMessages
                .Where(e => (e.EventArgs.Message == WM.GESTURENOTIFY) || (e.EventArgs.Message == WM.GESTURE))
                .SelectMany(e => GenerateGestureNotifications(e));
            GestureDeviceOut[0] = new GestureDevice(notifications);

            FEnumerator = GestureDeviceOut[0].Notifications
                    .Chunkify()
                    .GetEnumerator();
        }

        private IEnumerable<GestureNotification> GenerateGestureNotifications(EventPattern<WMEventArgs> e)
        {
            var a = e.EventArgs;
            if (a.Message == WM.GESTURENOTIFY)
            {
                a.Handled = FEnabledIn[0];
                yield break;
            }
            else
            {
                var gestureInfo = new GESTUREINFO();
                gestureInfo.cbSize = FGestureInfoSize;
                if (User32.GetGestureInfo(a.LParam, ref gestureInfo))
                {
                    try
                    {
                        RECT cr;
                        if (User32.GetClientRect(a.HWnd, out cr))
                        {
                            var position = new Point(gestureInfo.x, gestureInfo.y);
                            User32.ScreenToClient(a.HWnd, ref position);

                            a.Handled = true;
                            switch ((GestureNotificationKind)gestureInfo.dwID)
                            {
                                case GestureNotificationKind.GestureBegin:
                                    yield return new GestureBeginNotification(position,
                                                                new Size(cr.Width, cr.Height),
                                                                gestureInfo.dwInstanceID,
                                                                gestureInfo.dwSequenceID,
                                                                gestureInfo.hwndTarget.ToInt64(),
                                                                gestureInfo.dwFlags,
                                                                gestureInfo.ullArguments,
                                                                gestureInfo.cbExtraArgs,
                                                                e.Sender);
                                    break;
                                case GestureNotificationKind.GestureEnd:
                                    yield return new GestureEndNotification(position,
                                                                new Size(cr.Width, cr.Height),
                                                                gestureInfo.dwInstanceID,
                                                                gestureInfo.dwSequenceID,
                                                                gestureInfo.hwndTarget.ToInt64(),
                                                                gestureInfo.dwFlags,
                                                                gestureInfo.ullArguments,
                                                                gestureInfo.cbExtraArgs,
                                                                e.Sender);
                                    break;
                                case GestureNotificationKind.GestureZoom:
                                    yield return new GestureZoomNotification(position,
                                                                new Size(cr.Width, cr.Height),
                                                                gestureInfo.dwInstanceID,
                                                                gestureInfo.dwSequenceID,
                                                                gestureInfo.hwndTarget.ToInt64(),
                                                                gestureInfo.dwFlags,
                                                                gestureInfo.ullArguments,
                                                                gestureInfo.cbExtraArgs,
                                                                e.Sender);
                                    break;
                                case GestureNotificationKind.GesturePan:
                                    yield return new GesturePanNotification(position,
                                                                new Size(cr.Width, cr.Height),
                                                                gestureInfo.dwInstanceID,
                                                                gestureInfo.dwSequenceID,
                                                                gestureInfo.hwndTarget.ToInt64(),
                                                                gestureInfo.dwFlags,
                                                                gestureInfo.ullArguments,
                                                                gestureInfo.cbExtraArgs,
                                                                e.Sender);
                                    break;
                                case GestureNotificationKind.GestureRotate:
                                    yield return new GestureRotateNotification(position,
                                                                new Size(cr.Width, cr.Height),
                                                                gestureInfo.dwInstanceID,
                                                                gestureInfo.dwSequenceID,
                                                                gestureInfo.hwndTarget.ToInt64(),
                                                                gestureInfo.dwFlags,
                                                                gestureInfo.ullArguments,
                                                                gestureInfo.cbExtraArgs,
                                                                e.Sender);
                                    break;
                                case GestureNotificationKind.GesturePressAndTap:
                                    yield return new GesturePressAndTapNotification(position,
                                                                new Size(cr.Width, cr.Height),
                                                                gestureInfo.dwInstanceID,
                                                                gestureInfo.dwSequenceID,
                                                                gestureInfo.hwndTarget.ToInt64(),
                                                                gestureInfo.dwFlags,
                                                                gestureInfo.ullArguments,
                                                                gestureInfo.cbExtraArgs,
                                                                e.Sender);
                                    break;
                                case GestureNotificationKind.GestureTwoFingerTap:
                                    yield return new GestureTwoFingerTapNotification(position,
                                                                new Size(cr.Width, cr.Height),
                                                                gestureInfo.dwInstanceID,
                                                                gestureInfo.dwSequenceID,
                                                                gestureInfo.hwndTarget.ToInt64(),
                                                                gestureInfo.dwFlags,
                                                                gestureInfo.ullArguments,
                                                                gestureInfo.cbExtraArgs,
                                                                e.Sender);
                                    break;
                                default:
                                    yield break;
                            }
                        }
                    }
                    finally
                    {
                        a.Handled = false;
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
            PositionInProjectionSpaceOut.SliceCount = notifications.Count;
            PositionInNormalizedProjectionOut.SliceCount = notifications.Count;
            PositionPixelOut.SliceCount = notifications.Count;
            IdOut.SliceCount = notifications.Count;
            DeviceIDOut.SliceCount = notifications.Count;
            for (int i=0; i<notifications.Count; i++)
            {
                var n = notifications[i];
                GestureTypeOut[i] = n.Kind;

                Vector2D inNormalizedProjection, inProjection;

                SpaceHelpers.MapFromPixels(n.Position, n.Sender, n.ClientArea,
                    out inNormalizedProjection, out inProjection);

                PositionOut[i] = MouseExtensions.GetLegacyMousePositon(n.Position, n.ClientArea);
                PositionInProjectionSpaceOut[i] = inProjection;
                PositionInNormalizedProjectionOut[i] = inNormalizedProjection;
                PositionPixelOut[i] = new Vector2D(n.Position.X, n.Position.Y);

                IdOut[i] = n.Id;
                DeviceIDOut[i] = n.GestureDeviceID;
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

    public abstract class GestureEventsSplitNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Gesture Device", IsSingle = true)]
        public ISpread<GestureDevice> FInput;

        [Output("Position (Pixel) ", Order = -4, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionPixelOut;
        [Output("Position (Projection) ", Order = -3)]
        public ISpread<Vector2D> PositionInProjectionSpaceOut;
        [Output("Position (Normalized Projection) ", Order = -2, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionInNormalizedProjectionOut;
        [Output("Position (Normalized Window) ", Order = -1, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionOut;

        [Output("Id")]
        public ISpread<int> IdOut;
        [Output("Device ID")]
        public ISpread<long> DeviceIDOut;

        protected GestureNotificationKind FGestureFilterKind;

        private static readonly IList<GestureNotification> FEmptyList = new List<GestureNotification>(0);
        private GestureDevice FGestureDevice;
        private IEnumerator<IList<GestureNotification>> FEnumerator;

        public virtual void OnImportsSatisfied()
        {
            FGestureFilterKind = SetGestureKindFilter();
        }

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

            var gestures = notifications.Where(g => (g.Kind == FGestureFilterKind) || 
                            ((g.Kind == GestureNotificationKind.GestureEnd) && (IdOut.Contains(g.Id))))
                            .ToList();

            UseGestures(gestures);

            PositionOut.SliceCount = gestures.Count;
            PositionInProjectionSpaceOut.SliceCount = gestures.Count;
            PositionInNormalizedProjectionOut.SliceCount = gestures.Count;
            PositionPixelOut.SliceCount = gestures.Count;
            IdOut.SliceCount = gestures.Count;
            DeviceIDOut.SliceCount = gestures.Count;
            for (int i = 0; i < gestures.Count; i++)
            {
                var g = gestures[i];
                Vector2D inNormalizedProjection, inProjection;

                SpaceHelpers.MapFromPixels(g.Position, g.Sender, g.ClientArea,
                    out inNormalizedProjection, out inProjection);

                PositionOut[i] = MouseExtensions.GetLegacyMousePositon(g.Position, g.ClientArea);
                PositionInProjectionSpaceOut[i] = inProjection;
                PositionInNormalizedProjectionOut[i] = inNormalizedProjection;
                PositionPixelOut[i] = new Vector2D(g.Position.X, g.Position.Y);

                IdOut[i] = g.Id;
                DeviceIDOut[i] = g.GestureDeviceID;
            }
        }

        protected abstract GestureNotificationKind SetGestureKindFilter();
        protected virtual void UseGestures(IList<GestureNotification> notifications) { }

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
        [Output("Distance")]
        public ISpread<double> DistanceOut;

        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GestureZoom;
        }

        protected override void UseGestures(IList<GestureNotification> gestures)
        {
            DistanceOut.SliceCount = 0;
            foreach (var gesture in gestures)
            {
                if (gesture.Kind == FGestureFilterKind)
                {
                    double distance = (double)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                    DistanceOut.Add(distance / (double)gesture.ClientArea.Height * 2);
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
        [Output("Distance")]
        public ISpread<double> DistanceOut;

        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GesturePan;
        }

        protected override void UseGestures(IList<GestureNotification> gestures)
        {
            DistanceOut.SliceCount = 0;
            foreach (var gesture in gestures)
            {
                if (gesture.Kind == FGestureFilterKind)
                {
                    double distance = (double)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                    DistanceOut.Add(distance / (double)gesture.ClientArea.Height * 2);
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
        [Output("Rotate")]
        public ISpread<double> RotateOut;

        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GestureRotate;
        }

        protected override void UseGestures(IList<GestureNotification> gestures)
        {
            RotateOut.SliceCount = 0;
            foreach (var gesture in gestures)
            {
                if (gesture.Kind == FGestureFilterKind)
                {
                    double rotate = (double)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                    rotate = rotate / 65535.0 * 2 - 1;
                    RotateOut.Add(rotate);
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
        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GestureTwoFingerTap;
        }
    }

    [PluginInfo(Name = "PressAndTapEvents",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class PressAndTapGestureEventsSplitNode : GestureEventsSplitNode
    {
        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GesturePressAndTap;
        }
    }

    public abstract class GestureStatesSplitNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Gesture Device", IsSingle = true)]
        public ISpread<GestureDevice> GestureIn;
        [Input("Queue Mode", IsSingle = true, DefaultEnumEntry = "Discard")]
        public ISpread<QueueMode> QueueModeIn;

        [Output("Position (Pixel) ", Order = -4, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionPixelOut;
        [Output("Position (Projection) ", Order = -3)]
        public ISpread<Vector2D> PositionInProjectionSpaceOut;
        [Output("Position (Normalized Projection) ", Order = -2, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionInNormalizedProjectionOut;
        [Output("Position (Normalized Window) ", Order = -1, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<Vector2D> PositionOut;

        [Output("Id")]
        public ISpread<int> IdOut;
        [Output("Device ID")]
        public ISpread<long> DeviceIDOut;

        protected GestureNotificationKind FGestureFilterKind;

        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();
        private Subscription<GestureDevice, GestureNotification> FSubscription;

        public virtual void OnImportsSatisfied()
        {
            FGestureFilterKind = SetGestureKindFilter();

            PositionPixelOut.SliceCount = 0;
            PositionInProjectionSpaceOut.SliceCount = 0;
            PositionInNormalizedProjectionOut.SliceCount = 0;
            PositionOut.SliceCount = 0;
            IdOut.SliceCount = 0;
            DeviceIDOut.SliceCount = 0;

            FSubscription = new Subscription<GestureDevice, GestureNotification>(
                gestureDevice =>
                {
                    return gestureDevice.Notifications.Where(g => (g.Kind == FGestureFilterKind) || (g.Kind == GestureNotificationKind.GestureEnd));
                },
                (gestureDevice, g) =>
                {
                    var index = IdOut.IndexOf(g.Id);
                    var isFilterMatch = g.Kind == FGestureFilterKind;
                    UseGesture(g, isFilterMatch, index);
                    if (isFilterMatch)
                    {
                        Vector2D inNormalizedProjection, inProjection;
                        SpaceHelpers.MapFromPixels(g.Position, g.Sender, g.ClientArea,
                            out inNormalizedProjection, out inProjection);
                        var normalizedPosition = MouseExtensions.GetLegacyMousePositon(g.Position, g.ClientArea);
                        var inPixels = new Vector2D(g.Position.X, g.Position.Y);

                        if (index < 0)
                        {
                            PositionPixelOut.Add(inPixels);
                            PositionInProjectionSpaceOut.Add(inProjection);
                            PositionInNormalizedProjectionOut.Add(inNormalizedProjection);
                            PositionOut.Add(normalizedPosition);

                            IdOut.Add(g.Id);
                            DeviceIDOut.Add(g.GestureDeviceID);
                        }
                        else
                        {
                            PositionPixelOut[index] = inPixels;
                            PositionInProjectionSpaceOut[index] = inProjection;
                            PositionInNormalizedProjectionOut[index] = inNormalizedProjection;
                            PositionOut[index] = normalizedPosition;
                            IdOut[index] = g.Id;
                            DeviceIDOut[index] = g.GestureDeviceID;
                        }
                    }
                    else if (index >= 0)
                    {
                        PositionPixelOut.RemoveAt(index);
                        PositionInProjectionSpaceOut.RemoveAt(index);
                        PositionInNormalizedProjectionOut.RemoveAt(index);
                        PositionOut.RemoveAt(index);
                        IdOut.RemoveAt(index);
                        DeviceIDOut.RemoveAt(index);
                    }
                },
                FScheduler
            );
        }

        protected abstract GestureNotificationKind SetGestureKindFilter();
        protected virtual void UseGesture(GestureNotification gesture, bool isFilterMatch, int index) { }

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
        [Output("Distance")]
        public ISpread<double> DistanceOut;

        public override void OnImportsSatisfied()
        {
            DistanceOut.SliceCount = 0;
            base.OnImportsSatisfied();
        }

        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GestureZoom;
        }

        protected override void UseGesture(GestureNotification gesture, bool isFilterMatch, int index)
        {
            if (isFilterMatch)
            {
                double distance = (double)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                if (index < 0)
                {
                    DistanceOut.Add(distance / (double)gesture.ClientArea.Height * 2);
                }
                else
                {
                    DistanceOut[index] = distance / (double)gesture.ClientArea.Height * 2;
                }
            }
            else if (index >= 0)
            {
                DistanceOut.RemoveAt(index);
            }
        }
    }

    [PluginInfo(Name = "PanState",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class PanGestureStateSplitNode : GestureStatesSplitNode
    {
        [Output("Distance")]
        public ISpread<double> DistanceOut;

        public override void OnImportsSatisfied()
        {
            DistanceOut.SliceCount = 0;
            base.OnImportsSatisfied();
        }

        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GesturePan;
        }

        protected override void UseGesture(GestureNotification gesture, bool isFilterMatch, int index)
        {
            if (isFilterMatch)
            {
                double distance = (double)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                if (index < 0)
                {
                    DistanceOut.Add(distance / (double)gesture.ClientArea.Height * 2);
                }
                else
                {
                    DistanceOut[index] = distance / (double)gesture.ClientArea.Height * 2;
                }
            }
            else if (index >= 0)
            {
                DistanceOut.RemoveAt(index);
            }
        }
    }

    [PluginInfo(Name = "RotateState",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class RotateGestureStateSplitNode : GestureStatesSplitNode
    {
        [Output("Rotate")]
        public ISpread<double> RotateOut;

        public override void OnImportsSatisfied()
        {
            RotateOut.SliceCount = 0;
            base.OnImportsSatisfied();
        }

        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GestureRotate;
        }

        protected override void UseGesture(GestureNotification gesture, bool isFilterMatch, int index)
        {
            if (isFilterMatch)
            {
                double rotate = (double)(gesture.Arguments & Const.ULL_ARGUMENTS_BIT_MASK);
                rotate = rotate / 65535.0 * 2 - 1;
                
                if (index < 0)
                    RotateOut.Add(rotate);
                else
                    RotateOut[index] = rotate;
            }
            else if (index >= 0)
            {
                RotateOut.RemoveAt(index);
            }
        }
    }

    [PluginInfo(Name = "PressAndTapState",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class PressAndTapGestureStateSplitNode : GestureStatesSplitNode
    {
        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GesturePressAndTap;
        }
    }

    [PluginInfo(Name = "TwoFingerTapState",
                Category = "Gesture",
                Version = "Split",
                Help = "Returns all the touch events of a given touch device.",
                AutoEvaluate = true)]
    public class TwoFingerTapGestureStateSplitNode : GestureStatesSplitNode
    {
        protected override GestureNotificationKind SetGestureKindFilter()
        {
            return GestureNotificationKind.GestureTwoFingerTap;
        }
    }
}