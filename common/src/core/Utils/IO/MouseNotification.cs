using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Utils.IO
{
    public enum MouseNotificationKind
    {
        MouseDown,
        MouseUp,
        MouseMove,
        MouseWheel,
        MouseHorizontalWheel,
        MouseClick
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

        public bool IsMouseDown { get { return Kind == MouseNotificationKind.MouseDown; } }
        public bool IsMouseUp { get { return Kind == MouseNotificationKind.MouseUp; } }
        public bool IsMouseMove { get { return Kind == MouseNotificationKind.MouseMove; } }
        public bool IsMouseWheel { get { return Kind == MouseNotificationKind.MouseWheel; } }
        public bool IsMouseClick { get { return Kind == MouseNotificationKind.MouseClick; } }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;
            var n = obj as MouseNotification;
            if (n != null)
                return n.Kind == Kind && 
                       n.Position == Position && 
                       n.ClientArea == ClientArea;
            return false;
        }

        public override int GetHashCode()
        {
            return Kind.GetHashCode() ^ Position.GetHashCode() ^ ClientArea.GetHashCode();
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

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                var n = obj as MouseButtonNotification;
                if (n != null)
                    return n.Buttons == Buttons;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Buttons.GetHashCode();
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

    public class MouseClickNotification : MouseButtonNotification
    {
        public readonly int ClickCount;
        public MouseClickNotification(Point position, Size clientArea, MouseButtons buttons, int clickCount)
            : base(MouseNotificationKind.MouseClick, position, clientArea, buttons)
        {
            ClickCount = clickCount;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                var n = obj as MouseClickNotification;
                if (n != null)
                    return n.ClickCount == ClickCount;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ ClickCount.GetHashCode();
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

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                var n = obj as MouseWheelNotification;
                if (n != null)
                    return n.WheelDelta == WheelDelta;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ WheelDelta.GetHashCode();
        }
    }
    public class MouseHorizontalWheelNotification : MouseNotification
    {
        public readonly int WheelDelta;

        public MouseHorizontalWheelNotification(Point position, Size clientArea, int wheelDelta)
            : base(MouseNotificationKind.MouseHorizontalWheel, position, clientArea)
        {
            WheelDelta = wheelDelta;
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                var n = obj as MouseHorizontalWheelNotification;
                if (n != null)
                    return n.WheelDelta == WheelDelta;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ WheelDelta.GetHashCode();
        }
    }
}
