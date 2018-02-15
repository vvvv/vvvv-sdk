using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VVVV.Utils.IO
{
    public enum GestureNotificationKind
    {
        GestureBegin = 1,
        GestureEnd = 2,
        GestureZoom = 3,
        GesturePan = 4,
        GestureRotate = 5,
        GestureTwoFingerTap = 6,
        GesturePressAndTap = 7
    }

    public class GestureNotification : Notification
    {
        public readonly GestureNotificationKind Kind;
        public readonly Point Position;
        public readonly Size ClientArea;
        public readonly int Id;
        public readonly int SequenceId;
        public readonly long GestureDeviceID;
        public readonly int Flags;
        public readonly Int64 Arguments;
        public readonly int ExtraArguments;

        public GestureNotification(GestureNotificationKind kind, Point position, Size clientArea, int id, int sequenceId, long gestureDeviceID, int flags, Int64 ullArguments, int cbExtraArgs)
        {
            Kind = kind;
            Position = position;
            ClientArea = clientArea;
            Id = id;
            SequenceId = sequenceId;
            GestureDeviceID = gestureDeviceID;
            Flags = flags;
            Arguments = ullArguments;
            ExtraArguments = cbExtraArgs;
        }

        public GestureNotification(GestureNotificationKind kind, Point position, Size clientArea, int id, int sequenceId, long gestureDeviceID, int flags, Int64 ullArguments, int cbExtraArgs, object sender)
            : base(sender)
        {
            Kind = kind;
            Position = position;
            ClientArea = clientArea;
            Id = id;
            SequenceId = sequenceId;
            GestureDeviceID = gestureDeviceID;
            Flags = flags;
            Arguments = ullArguments;
            ExtraArguments = cbExtraArgs;
        }

        public bool IsGestureBegin { get { return Kind == GestureNotificationKind.GestureBegin; } }
        public bool IsGestureEnd { get { return Kind == GestureNotificationKind.GestureEnd; } }
        public bool IsGestureZoom { get { return Kind == GestureNotificationKind.GestureZoom; } }
        public bool IsGesturePan { get { return Kind == GestureNotificationKind.GesturePan; } }
        public bool IsGestureRotate { get { return Kind == GestureNotificationKind.GestureRotate; } }
        public bool IsGestureTwoFingerTap { get { return Kind == GestureNotificationKind.GestureTwoFingerTap; } }
        public bool IsGesturePressAndTap { get { return Kind == GestureNotificationKind.GesturePressAndTap; } }
    }

    public class GestureBeginNotification : GestureNotification
    {
        public GestureBeginNotification( Point position, Size clientArea, int id, int sequenceId, long gestureDeviceID, int flags, Int64 ullArguments, int cbExtraArgs, object sender)
            : base(GestureNotificationKind.GestureBegin, position, clientArea, id, sequenceId, gestureDeviceID, flags, ullArguments, cbExtraArgs, sender)
        {
        }
    }

    public class GestureEndNotification : GestureNotification
    {
        public GestureEndNotification(Point position, Size clientArea, int id, int sequenceId, long gestureDeviceID, int flags, Int64 ullArguments, int cbExtraArgs, object sender)
            : base(GestureNotificationKind.GestureEnd, position, clientArea, id, sequenceId, gestureDeviceID, flags, ullArguments, cbExtraArgs, sender)
        {
        }
    }

    public class GestureZoomNotification : GestureNotification
    {
        public GestureZoomNotification(Point position, Size clientArea, int id, int sequenceId, long gestureDeviceID, int flags, Int64 ullArguments, int cbExtraArgs, object sender)
            : base(GestureNotificationKind.GestureZoom, position, clientArea, id, sequenceId, gestureDeviceID, flags, ullArguments, cbExtraArgs, sender)
        {
        }
    }

    public class GesturePanNotification : GestureNotification
    {
        public GesturePanNotification(Point position, Size clientArea, int id, int sequenceId, long gestureDeviceID, int flags, Int64 ullArguments, int cbExtraArgs, object sender)
            : base(GestureNotificationKind.GesturePan, position, clientArea, id, sequenceId, gestureDeviceID, flags, ullArguments, cbExtraArgs, sender)
        {
        }
    }

    public class GestureRotateNotification : GestureNotification
    {
        public GestureRotateNotification(Point position, Size clientArea, int id, int sequenceId, long gestureDeviceID, int flags, Int64 ullArguments, int cbExtraArgs, object sender)
            : base(GestureNotificationKind.GestureRotate, position, clientArea, id, sequenceId, gestureDeviceID, flags, ullArguments, cbExtraArgs, sender)
        {
        }
    }

    public class GestureTwoFingerTapNotification : GestureNotification
    {
        public GestureTwoFingerTapNotification(Point position, Size clientArea, int id, int sequenceId, long gestureDeviceID, int flags, Int64 ullArguments, int cbExtraArgs, object sender)
            : base(GestureNotificationKind.GestureTwoFingerTap, position, clientArea, id, sequenceId, gestureDeviceID, flags, ullArguments, cbExtraArgs, sender)
        {
        }
    }

    public class GesturePressAndTapNotification : GestureNotification
    {
        public GesturePressAndTapNotification(Point position, Size clientArea, int id, int sequenceId, long gestureDeviceID, int flags, Int64 ullArguments, int cbExtraArgs, object sender)
            : base(GestureNotificationKind.GesturePressAndTap, position, clientArea, id, sequenceId, gestureDeviceID, flags, ullArguments, cbExtraArgs, sender)
        {
        }
    }
}