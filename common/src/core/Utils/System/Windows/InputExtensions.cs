using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.IO;

namespace System.Windows.Input
{
    public static class InputExtensions
    {
        public static MouseMoveNotification ToMouseMoveNotification(this MouseEventArgs args, FrameworkElement relativeTo)
        {
            var position = args.GetDrawingPosition(relativeTo);
            var clientArea = relativeTo.GetDrawingSize();
            return new MouseMoveNotification(position, clientArea, relativeTo);
        }

        public static MouseDownNotification ToMouseDownNotification(this MouseButtonEventArgs args, FrameworkElement relativeTo)
        {
            var position = args.GetDrawingPosition(relativeTo);
            var clientArea = relativeTo.GetDrawingSize();
            var button = args.GetButton();
            return new MouseDownNotification(position, clientArea, button, relativeTo);
        }

        public static MouseUpNotification ToMouseUpNotification(this MouseButtonEventArgs args, FrameworkElement relativeTo)
        {
            var position = args.GetDrawingPosition(relativeTo);
            var clientArea = relativeTo.GetDrawingSize();
            var button = args.GetButton();
            return new MouseUpNotification(position, clientArea, button, relativeTo);
        }

        public static MouseWheelNotification ToMouseWheelNotification(this MouseWheelEventArgs args, FrameworkElement relativeTo)
        {
            var position = args.GetDrawingPosition(relativeTo);
            var clientArea = relativeTo.GetDrawingSize();
            var wheelDelta = args.Delta;
            return new MouseWheelNotification(position, clientArea, wheelDelta, relativeTo);
        }

        public static System.Windows.Forms.MouseButtons ToMouseButtons(this MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return System.Windows.Forms.MouseButtons.Left;
                case MouseButton.Middle:
                    return System.Windows.Forms.MouseButtons.Middle;
                case MouseButton.Right:
                    return System.Windows.Forms.MouseButtons.Right;
                case MouseButton.XButton1:
                    return System.Windows.Forms.MouseButtons.XButton1;
                case MouseButton.XButton2:
                    return System.Windows.Forms.MouseButtons.XButton2;
                default:
                    return System.Windows.Forms.MouseButtons.None;
            }
        }

        private static System.Drawing.Point GetDrawingPosition(this MouseEventArgs args, FrameworkElement relativeTo)
        {
            return args.GetPosition(relativeTo).ToDrawingPoint();
        }

        private static System.Drawing.Size GetDrawingSize(this FrameworkElement element)
        {
            return new System.Drawing.Size((int)element.ActualWidth, (int)element.ActualHeight);
        }

        private static System.Windows.Forms.MouseButtons GetButton(this MouseButtonEventArgs args)
        {
            return args.ChangedButton.ToMouseButtons();
        }

        public static KeyDownNotification ToKeyDownNotification(this KeyEventArgs eventArgs)
        {
            return new KeyDownNotification((Forms.Keys)KeyInterop.VirtualKeyFromKey(eventArgs.Key), null);
        }

        public static KeyUpNotification ToKeyUpNotification(this KeyEventArgs eventArgs)
        {
            return new KeyUpNotification((Forms.Keys)KeyInterop.VirtualKeyFromKey(eventArgs.Key), null);
        }

        public static KeyPressNotification ToKeyPressNotification(this TextCompositionEventArgs eventArgs)
        {
            return new KeyPressNotification(eventArgs.Text.FirstOrDefault(), null);
        }
    }
}
