using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

    public static class MouseNotificationExtensions
    {
        public static IObservable<MouseNotification> InjectMouseClicks(this IObservable<MouseNotification> notifications)
        {
            // A mouse down followed by a mouse up in the same area with the same button is considered a single click.
            // Each subsequnt mouse down in the same area in a specific time interval with the same button produces
            // another click with an increased click count.
            var lastSeen = new Dictionary<int, DateTimeOffset>();
            var mouseClicks = notifications.OfType<MouseDownNotification>()
                .Timestamp()
                .SelectMany(down =>
                    {
                        var singleClick = notifications.OfType<MouseUpNotification>()
                            .Take(1)
                            .Where(b => b.Buttons == down.Value.Buttons &&
                                        Math.Abs(b.Position.X - down.Value.Position.X) <= SystemInformation.DoubleClickSize.Width &&
                                        Math.Abs(b.Position.Y - down.Value.Position.Y) <= SystemInformation.DoubleClickSize.Height)
                            .Select(x => Timestamped.Create(new MouseClickNotification(down.Value.Position, down.Value.ClientArea, down.Value.Buttons, 1), down.Timestamp));
                        var subsequentClicks =
                                notifications.OfType<MouseDownNotification>()
                                    .TakeWhile(n => n.Buttons == down.Value.Buttons &&
                                                Math.Abs(n.Position.X - down.Value.Position.X) <= SystemInformation.DoubleClickSize.Width &&
                                                Math.Abs(n.Position.Y - down.Value.Position.Y) <= SystemInformation.DoubleClickSize.Height)
                                    .TimeInterval()
                                    .TakeWhile(x => x.Interval.TotalMilliseconds <= SystemInformation.DoubleClickTime)
                                    .Select((x, i) => Timestamped.Create(new MouseClickNotification(down.Value.Position, down.Value.ClientArea, down.Value.Buttons, i + 2), down.Timestamp));
                        return singleClick.Concat(subsequentClicks);
                    }
                )
                // These clicks are now filtered so that three single clicks won't produce two double clicks
                .Where(x =>
                    {
                        var clickCount = x.Value.ClickCount;
                        if (clickCount == 1)
                            return true;
                        DateTimeOffset lastTime;
                        if (!lastSeen.TryGetValue(clickCount, out lastTime))
                        {
                            lastSeen.Add(clickCount, x.Timestamp);
                            return true;
                        }
                        else
                            lastSeen[clickCount] = x.Timestamp;
                        return (x.Timestamp - lastTime).TotalMilliseconds > SystemInformation.DoubleClickTime * (clickCount - 1);
                    }
                )
                .Select(x => x.Value);
            return notifications.Merge(mouseClicks);
        }
    }
}
