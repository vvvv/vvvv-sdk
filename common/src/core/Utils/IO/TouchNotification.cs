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
        public readonly bool Primary;
        public readonly Size ContactArea;
        public readonly long TouchDeviceID;

        public TouchNotification(TouchNotificationKind kind, Point position, Size clientArea, int id, bool primary, Size contactArea, long touchDeviceID)
        {
            Kind = kind;
            Position = position;
            ClientArea = clientArea;
            Id = id;
            Primary = primary;
            ContactArea = contactArea;
            TouchDeviceID = touchDeviceID;
        }
        
        public bool IsTouchDown { get { return Kind == TouchNotificationKind.TouchDown; } }
        public bool IsTouchUp { get { return Kind == TouchNotificationKind.TouchUp; } }
        public bool IsTouchMove { get { return Kind == TouchNotificationKind.TouchMove; } }
    }

    public class TouchDownNotification : TouchNotification
    {
        public TouchDownNotification(Point position, Size clientArea, int id, bool primary, Size contactArea, long touchDeviceID)
            : base(TouchNotificationKind.TouchDown, position, clientArea, id, primary, contactArea, touchDeviceID)
        {
        }
    }

    public class TouchMoveNotification : TouchNotification
    {
        public TouchMoveNotification(Point position, Size clientArea, int id, bool primary, Size contactArea, long touchDeviceID)
            : base(TouchNotificationKind.TouchMove, position, clientArea, id, primary, contactArea, touchDeviceID)
        {
        }
    }

    public class TouchUpNotification : TouchNotification
    {
        public TouchUpNotification(Point position, Size clientArea, int id, bool primary, Size contactArea, long touchDeviceID)
            : base(TouchNotificationKind.TouchUp, position, clientArea, id, primary, contactArea, touchDeviceID)
        {
        }
    }
}