using System;
using VVVV.Utils.IO;

namespace VVVV.VL.Hosting.IO
{
    public static class VVVVNotificationHelpers
    {
        public static TResult NotificationSwitch<TResult>(object eventArg, TResult defaultResult, 
            Func<MouseNotification, TResult> mouseFunc = null,
            Func<KeyNotification, TResult> keyFunc = null,
            Func<TouchNotification, TResult> touchFunc = null,
            Func<GestureNotification, TResult> gestureFunc = null)
            where TResult : class
        {
            if (mouseFunc != null)
            {
                var mouseNotification = eventArg as MouseNotification;
                if (mouseNotification != null)
                {
                    return mouseFunc(mouseNotification);
                } 
            }

            if (touchFunc != null)
            {
                var touchNotification = eventArg as TouchNotification;
                if (touchNotification != null)
                {
                    return touchFunc(touchNotification);
                } 
            }

            if (keyFunc != null)
            {
                var keyNotification = eventArg as KeyNotification;
                if (keyNotification != null)
                {
                    return keyFunc(keyNotification);
                } 
            }

            if (gestureFunc != null)
            {
                var gestureNotification = eventArg as GestureNotification;
                if (gestureNotification != null)
                {
                    return gestureFunc(gestureNotification);
                } 
            }

            return defaultResult;
        }

        public static TResult MouseNotificationSwitch<TResult>(MouseNotification notification, TResult defaultResult,
            Func<MouseDownNotification, TResult> onDown,
            Func<MouseMoveNotification, TResult> onMove,
            Func<MouseUpNotification, TResult> onUp,
            Func<MouseClickNotification, TResult> onClick,
            Func<MouseWheelNotification, TResult> onWheel,
            Func<MouseHorizontalWheelNotification, TResult> onHorizontalWheel,
            Func<MouseLostNotification, TResult> onDeviceLost)
            where TResult : class
        {
            switch (notification.Kind)
            {
                case MouseNotificationKind.MouseDown:
                    return onDown.Invoke((MouseDownNotification)notification) ?? defaultResult;
                case MouseNotificationKind.MouseUp:
                    return onUp.Invoke((MouseUpNotification)notification) ?? defaultResult;
                case MouseNotificationKind.MouseMove:
                    return onMove.Invoke((MouseMoveNotification)notification) ?? defaultResult;
                case MouseNotificationKind.MouseWheel:
                    return onWheel.Invoke((MouseWheelNotification)notification) ?? defaultResult;
                case MouseNotificationKind.MouseHorizontalWheel:
                    return onHorizontalWheel.Invoke((MouseHorizontalWheelNotification)notification) ?? defaultResult;
                case MouseNotificationKind.MouseClick:
                    return onClick.Invoke((MouseClickNotification)notification) ?? defaultResult;
                case MouseNotificationKind.DeviceLost:
                    return onDeviceLost((MouseLostNotification)notification) ?? defaultResult;
                default:
                    return defaultResult;
            }
        }

        public static TResult KeyNotificationSwitch<TResult>(KeyNotification notification, TResult defaultResult,
            Func<KeyDownNotification, TResult> onDown,
            Func<KeyUpNotification, TResult> onUp,
            Func<KeyPressNotification, TResult> onPress,
            Func<KeyboardLostNotification, TResult> onDeviceLost)
            where TResult : class
        {
            switch (notification.Kind)
            {
                case KeyNotificationKind.KeyDown:
                    return onDown?.Invoke((KeyDownNotification)notification) ?? defaultResult;
                case KeyNotificationKind.KeyPress:
                    return onPress?.Invoke((KeyPressNotification)notification) ?? defaultResult;
                case KeyNotificationKind.KeyUp:
                    return onUp?.Invoke((KeyUpNotification)notification) ?? defaultResult;
                case KeyNotificationKind.DeviceLost:
                    return onDeviceLost?.Invoke((KeyboardLostNotification)notification) ?? defaultResult;
                default:
                    return defaultResult;
            }
        }

        public static TResult TouchNotificationSwitch<TResult>(TouchNotification notification, TResult defaultResult,
            Func<TouchDownNotification, TResult> onDown,
            Func<TouchMoveNotification, TResult> onMove,
            Func<TouchUpNotification, TResult> onUp)
            where TResult : class
        {
            switch (notification.Kind)
            {
                case TouchNotificationKind.TouchDown:
                    return onDown?.Invoke((TouchDownNotification)notification) ?? defaultResult;
                case TouchNotificationKind.TouchUp:
                    return onUp?.Invoke((TouchUpNotification)notification) ?? defaultResult;
                case TouchNotificationKind.TouchMove:
                    return onMove?.Invoke((TouchMoveNotification)notification) ?? defaultResult;
                default:
                    return defaultResult;
            }
        }

        public static TResult GestureNotificationSwitch<TResult>(GestureNotification notification, TResult defaultResult,
            Func<GestureBeginNotification, TResult> onBegin,
            Func<GestureEndNotification, TResult> onEnd,
            Func<GestureZoomNotification, TResult> onZoom,
            Func<GesturePanNotification, TResult> onPan,
            Func<GestureRotateNotification, TResult> onRotate,
            Func<GestureTwoFingerTapNotification, TResult> onTwoFingerTap,
            Func<GesturePressAndTapNotification, TResult> onPressAndTap)
            where TResult : class
        {
            switch (notification.Kind)
            {
                case GestureNotificationKind.GestureBegin:
                    return onBegin?.Invoke((GestureBeginNotification)notification) ?? defaultResult;
                case GestureNotificationKind.GestureEnd:
                    return onEnd?.Invoke((GestureEndNotification)notification) ?? defaultResult;
                case GestureNotificationKind.GestureZoom:
                    return onZoom?.Invoke((GestureZoomNotification)notification) ?? defaultResult;
                case GestureNotificationKind.GesturePan:
                    return onPan?.Invoke((GesturePanNotification)notification) ?? defaultResult;
                case GestureNotificationKind.GestureRotate:
                    return onRotate?.Invoke((GestureRotateNotification)notification) ?? defaultResult;
                case GestureNotificationKind.GestureTwoFingerTap:
                    return onTwoFingerTap?.Invoke((GestureTwoFingerTapNotification)notification) ?? defaultResult;
                case GestureNotificationKind.GesturePressAndTap:
                    return onPressAndTap?.Invoke((GesturePressAndTapNotification)notification) ?? defaultResult;
                default:
                    return defaultResult;
            }
        }
    }
}
