using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace VVVV.Utils.IO
{
    public enum TouchNotificationKind
    {
        TouchDown,
        TouchUp,
        TouchMove
    }

    public class TouchNotification
    {
        public readonly TouchNotificationKind Kind;
        public readonly Point Position;
        public readonly Size ClientArea;
        public readonly int Id;
        public readonly Size ContactArea;
        public readonly long TouchDeviceID;

        public TouchNotification(TouchNotificationKind kind, Point position, Size clientArea, int id, Size contactArea, long touchDeviceID)
        {
            Kind = kind;
            Position = position;
            ClientArea = clientArea;
            Id = id;
            ContactArea = contactArea;
            TouchDeviceID = touchDeviceID;
        }
    }

    public class TouchDownNotification : TouchNotification
    {
        public TouchDownNotification(Point position, Size clientArea, int id, Size contactArea, long touchDeviceID)
            : base(TouchNotificationKind.TouchDown, position, clientArea, id, contactArea, touchDeviceID)
        {
        }
    }

    public class TouchMoveNotification : TouchNotification
    {
        public TouchMoveNotification(Point position, Size clientArea, int id, Size contactArea, long touchDeviceID)
            : base(TouchNotificationKind.TouchMove, position, clientArea, id, contactArea, touchDeviceID)
        {
        }
    }

    public class TouchUpNotification : TouchNotification
    {
        public TouchUpNotification(Point position, Size clientArea, int id, Size contactArea, long touchDeviceID)
            : base(TouchNotificationKind.TouchUp, position, clientArea, id, contactArea, touchDeviceID)
        {
        }
    }
}
