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

    public class GestureNotification
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

        public bool IsGestureBegin { get { return Kind == GestureNotificationKind.GestureBegin; } }
        public bool IsGestureEnd { get { return Kind == GestureNotificationKind.GestureEnd; } }
    }
}