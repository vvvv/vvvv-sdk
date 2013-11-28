using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Utils.IO
{
    public enum MouseNotificationKind
    {
        MouseDown,
        MouseUp,
        MouseMove,
        MouseWheel
    }

    public abstract class MouseNotification
    {
        public readonly MouseNotificationKind Kind;
        public readonly Point Position;
        public readonly Size ClientArea;
        public MouseNotification(MouseNotificationKind kind, Point position, Size clientArea)
        {
            Kind = kind;
            Position = position;
            ClientArea = clientArea;
        }
    }

    public class MouseMoveNotification : MouseNotification
    {
        public MouseMoveNotification(Point position, Size clientArea)
            : base(MouseNotificationKind.MouseMove, position, clientArea)
        {
        }
    }

    public abstract class MouseButtonNotification : MouseNotification
    {
        public readonly MouseButtons Buttons;
        public MouseButtonNotification(MouseNotificationKind kind, Point position, Size clientArea, MouseButtons buttons)
            : base(kind, position, clientArea)
        {
            Buttons = buttons;
        }
    }

    public class MouseDownNotification : MouseButtonNotification
    {
        public MouseDownNotification(Point position, Size clientArea, MouseButtons buttons)
            : base(MouseNotificationKind.MouseDown, position, clientArea, buttons)
        {
        }
    }

    public class MouseUpNotification : MouseButtonNotification
    {
        public MouseUpNotification(Point position, Size clientArea, MouseButtons buttons)
            : base(MouseNotificationKind.MouseUp, position, clientArea, buttons)
        {
        }
    }

    public class MouseWheelNotification : MouseNotification
    {
        public readonly int WheelDelta;

        public MouseWheelNotification(Point position, Size clientArea, int wheelDelta)
            : base(MouseNotificationKind.MouseWheel, position, clientArea)
        {
            WheelDelta = wheelDelta;
        }
    }
}
