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

        public TouchNotification(TouchNotificationKind kind, Point position, Size clientArea, int id, Size contactArea)
        {
            Kind = kind;
            Position = position;
            ClientArea = clientArea;
            Id = id;
            ContactArea = contactArea;
        }
    }

    public class TouchDownNotification : TouchNotification
    {
        public TouchDownNotification(Point position, Size clientArea, int id, Size contactArea)
            : base(TouchNotificationKind.TouchDown, position, clientArea, id, contactArea)
        {
        }
    }

    public class TouchMoveNotification : TouchNotification
    {
        public TouchMoveNotification(Point position, Size clientArea, int id, Size contactArea)
            : base(TouchNotificationKind.TouchMove, position, clientArea, id, contactArea)
        {
        }
    }

    public class TouchUpNotification : TouchNotification
    {
        public TouchUpNotification(Point position, Size clientArea, int id, Size contactArea)
            : base(TouchNotificationKind.TouchUp, position, clientArea, id, contactArea)
        {
        }
    }
}
