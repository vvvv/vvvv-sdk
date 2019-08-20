using System;
using Xenko.Core.Mathematics;
using VL.Lib.IO.Notifications;
using Drawing = System.Drawing;
using VL.UI.Core;
using VL.Lib.IO;

namespace VVVV.VL.Hosting.IO
{
    public interface INotificationSpaceTransformer
    {
        Vector2 Transform(Drawing.Point point);
        Vector2 Transform(Drawing.Size size);
    }

    public class DummyNotificationSpaceTransformer : INotificationSpaceTransformer
    {
        public static INotificationSpaceTransformer Instance = new DummyNotificationSpaceTransformer();

        public Vector2 Transform(Drawing.Size size)
        {
            return new Vector2(size.Width, size.Height);
        }

        public Vector2 Transform(Drawing.Point point)
        {
            return new Vector2(point.X, point.Y);
        }
    }

    public class DIPNotificationSpaceTransformer : INotificationSpaceTransformer
    {
        public static INotificationSpaceTransformer Instance = new DIPNotificationSpaceTransformer();

        float Scaling;

        public DIPNotificationSpaceTransformer()
        {
            Scaling = 1 / DIPHelpers.DIPFactor();
        }

        public Vector2 Transform(Drawing.Size size)
        {
            return new Vector2(size.Width * Scaling, size.Height * Scaling);
        }

        public Vector2 Transform(Drawing.Point point)
        {
            return new Vector2(point.X * Scaling, point.Y * Scaling);
        }
    }

    public class NotificationSpaceTransformer : INotificationSpaceTransformer
    {
        readonly Vector2 Offset;
        readonly Vector2 Scaling;

        public NotificationSpaceTransformer(Vector2 offset, Vector2 scaling)
        {
            Offset = offset;
            Scaling = scaling;
        }

        public Vector2 Transform(Drawing.Point point)
        {
            return new Vector2((point.X + Offset.X) * Scaling.X, (point.Y + Offset.Y) * Scaling.Y);
        }

        public Vector2 Transform(Drawing.Size size)
        {
            return new Vector2(size.Width * Scaling.X, size.Height * Scaling.Y);
        }
    }

    public static class NotificationConverter
    {
        public class NotificationSenderProxy : IProjectionSpace
        {
            public readonly object OriginalSender;

            public NotificationSenderProxy(object originalSender)
            {
                OriginalSender = originalSender;
            }

            public void MapFromPixels(INotificationWithPosition notification, out Vector2 inNormalizedProjection, out Vector2 inProjection)
            {
                Utils.VMath.Vector2D np, p;
                PluginInterfaces.V2.IO.SpaceHelpers.MapFromPixels(
                    new Drawing.Point((int)notification.Position.X, (int)notification.Position.Y),
                    OriginalSender, new Drawing.Size((int)notification.ClientArea.X, (int)notification.ClientArea.Y), out np, out p);
                inNormalizedProjection = new Vector2((float)np.x, (float)np.y);
                inProjection = new Vector2((float)p.x, (float)p.y);
            }
        }

        public static object ConvertNotification(object notification, INotificationSpaceTransformer transformer)
        {
            var sender = new NotificationSenderProxy((notification as Utils.IO.Notification).Sender);

            return VVVVNotificationHelpers.NotificationSwitch<NotificationBase>(notification, null,

                //mouse
                mn => VVVVNotificationHelpers.MouseNotificationSwitch<MouseNotification>(mn, null,

                    n => new MouseDownNotification(
                        transformer.Transform(n.Position),
                        transformer.Transform(n.ClientArea),
                        n.Buttons, sender),

                    n => new MouseMoveNotification(
                        transformer.Transform(n.Position),
                        transformer.Transform(n.ClientArea),
                        sender),

                    n => new MouseUpNotification(
                        transformer.Transform(n.Position),
                        transformer.Transform(n.ClientArea),
                        n.Buttons, sender),

                    n => new MouseClickNotification(
                        transformer.Transform(n.Position),
                        transformer.Transform(n.ClientArea),
                        n.Buttons, n.ClickCount, sender),

                    n => new MouseWheelNotification(
                        transformer.Transform(n.Position),
                        transformer.Transform(n.ClientArea),
                        n.WheelDelta, sender),

                    n => new MouseHorizontalWheelNotification(
                        transformer.Transform(n.Position),
                        transformer.Transform(n.ClientArea),
                        n.WheelDelta, sender),

                    n => new MouseLostNotification(
                        transformer.Transform(n.Position),
                        transformer.Transform(n.ClientArea),
                        sender)
                ),

                //keyboard
                kn => VVVVNotificationHelpers.KeyNotificationSwitch<KeyNotification>(kn, null,

                    n => new KeyDownNotification(n.KeyCode, sender),
                    n => new KeyUpNotification(n.KeyCode, sender),
                    n => new KeyPressNotification(n.KeyChar, sender),
                    n => new KeyboardLostNotification(sender)

                ),

                //touch
                touch =>
                {
                    var pos = transformer.Transform(touch.Position);
                    var clientArea = transformer.Transform(touch.ClientArea);
                    var contactArea = transformer.Transform(touch.ContactArea);

                    return VVVVNotificationHelpers.TouchNotificationSwitch<TouchNotification>(touch, null,
                        n => new TouchNotification(TouchNotificationKind.TouchDown, pos, clientArea, n.Id, n.Primary, contactArea, n.TouchDeviceID, sender),
                        n => new TouchNotification(TouchNotificationKind.TouchMove, pos, clientArea, n.Id, n.Primary, contactArea, n.TouchDeviceID, sender),
                        n => new TouchNotification(TouchNotificationKind.TouchUp, pos, clientArea, n.Id, n.Primary, contactArea, n.TouchDeviceID, sender)
                    );
                },

                //gesture
                gesture =>
                {
                    var pos = transformer.Transform(gesture.Position);
                    var clientArea = transformer.Transform(gesture.ClientArea);

                    return VVVVNotificationHelpers.GestureNotificationSwitch<GestureNotification>(gesture, null,
                        n => new GestureNotification(GestureNotificationKind.GestureBegin, pos, clientArea, n.Id, n.SequenceId, n.GestureDeviceID, n.Flags, n.Arguments, n.ExtraArguments, sender),
                        n => new GestureNotification(GestureNotificationKind.GestureEnd, pos, clientArea, n.Id, n.SequenceId, n.GestureDeviceID, n.Flags, n.Arguments, n.ExtraArguments, sender),
                        n => new GestureNotification(GestureNotificationKind.GestureZoom, pos, clientArea, n.Id, n.SequenceId, n.GestureDeviceID, n.Flags, n.Arguments, n.ExtraArguments, sender),
                        n => new GestureNotification(GestureNotificationKind.GesturePan, pos, clientArea, n.Id, n.SequenceId, n.GestureDeviceID, n.Flags, n.Arguments, n.ExtraArguments, sender),
                        n => new GestureNotification(GestureNotificationKind.GestureRotate, pos, clientArea, n.Id, n.SequenceId, n.GestureDeviceID, n.Flags, n.Arguments, n.ExtraArguments, sender),
                        n => new GestureNotification(GestureNotificationKind.GestureTwoFingerTap, pos, clientArea, n.Id, n.SequenceId, n.GestureDeviceID, n.Flags, n.Arguments, n.ExtraArguments, sender),
                        n => new GestureNotification(GestureNotificationKind.GesturePressAndTap, pos, clientArea, n.Id, n.SequenceId, n.GestureDeviceID, n.Flags, n.Arguments, n.ExtraArguments, sender)
                        );
                }
            );
        }
    }
}
