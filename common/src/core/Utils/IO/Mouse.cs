using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Utils.IO
{
    public class Mouse
    {
        public static readonly Mouse Empty = new Mouse(Observable.Never<MouseNotification>(), false);

        public readonly IObservable<MouseNotification> MouseNotifications;
        private MouseButtons FPressedButtons;

        public Mouse(IObservable<MouseNotification> mouseNotifications, bool injectMouseClicks = true)
        {
            mouseNotifications = mouseNotifications
                .Do(n =>
                    {
                        switch (n.Kind)
                        {
                            case MouseNotificationKind.MouseDown:
                                var mouseDown = n as MouseDownNotification;
                                FPressedButtons |= mouseDown.Buttons;
                                break;
                            case MouseNotificationKind.MouseUp:
                                var mouseUp = n as MouseUpNotification;
                                FPressedButtons &= ~mouseUp.Buttons;
                                break;
                            default:
                                break;
                        }
                    });
            if (injectMouseClicks)
                mouseNotifications = InjectMouseClicks(mouseNotifications);
            MouseNotifications = mouseNotifications.Publish().RefCount();
        }

        /// <summary>
        /// The currently pressed mouse buttons.
        /// </summary>
        public MouseButtons PressedButtons
        {
            get { return FPressedButtons; }
        }

        private static IObservable<MouseNotification> InjectMouseClicks(IObservable<MouseNotification> notifications)
        {
            // Keep only a single subscription
            notifications = notifications.Publish().RefCount();
            // A mouse down followed by a mouse up in the same area with the same button is considered a single click.
            // Each subsequnt mouse down in the same area in a specific time interval with the same button produces
            // another click with an increased click count.
            var lastSeen = new Dictionary<int, int>();
            var mouseClicks = notifications.OfType<MouseDownNotification>()
                .SelectMany((down, i) =>
                {
                    var singleClick = notifications.OfType<MouseUpNotification>()
                        .Take(1)
                        .Where(b => b.Buttons == down.Buttons &&
                                    Math.Abs(b.Position.X - down.Position.X) <= SystemInformation.DoubleClickSize.Width &&
                                    Math.Abs(b.Position.Y - down.Position.Y) <= SystemInformation.DoubleClickSize.Height)
                        .Select(x => Tuple.Create(new MouseClickNotification(down.Position, down.ClientArea, down.Buttons, 1), i));
                    var subsequentClicks =
                            notifications.OfType<MouseDownNotification>()
                                .TakeWhile(n => n.Buttons == down.Buttons &&
                                            Math.Abs(n.Position.X - down.Position.X) <= SystemInformation.DoubleClickSize.Width &&
                                            Math.Abs(n.Position.Y - down.Position.Y) <= SystemInformation.DoubleClickSize.Height)
                                .TimeInterval()
                                .TakeWhile(x => x.Interval.TotalMilliseconds <= SystemInformation.DoubleClickTime)
                                .Select((x, j) => Tuple.Create(new MouseClickNotification(down.Position, down.ClientArea, down.Buttons, j + 2), i));
                    return singleClick.Concat(subsequentClicks);
                }
                )
                // These clicks are now filtered so that three single clicks won't produce two double clicks
                .Where(x =>
                {
                    var clickCount = x.Item1.ClickCount;
                    if (clickCount == 1)
                        return true;
                    var timestamp = x.Item2;
                    int lastTime;
                    if (!lastSeen.TryGetValue(clickCount, out lastTime))
                    {
                        lastSeen.Add(clickCount, timestamp);
                        return true;
                    }
                    if (timestamp - lastTime > (clickCount - 1))
                    {
                        lastSeen[clickCount] = timestamp;
                        return true;
                    }
                    return false;
                }
                )
                .Select(x => x.Item1);
            return notifications.Merge(mouseClicks);
        }
    }
}
