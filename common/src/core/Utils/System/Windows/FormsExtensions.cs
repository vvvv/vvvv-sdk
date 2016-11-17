using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.IO;

namespace System.Windows.Forms
{
    public static class FormsExtensions
    {
        public static bool IsLeft(this MouseButtons buttons)
        {
            return (buttons & MouseButtons.Left) != 0;
        }

        public static bool IsMiddle(this MouseButtons buttons)
        {
            return (buttons & MouseButtons.Middle) != 0;
        }

        public static bool IsRight(this MouseButtons buttons)
        {
            return (buttons & MouseButtons.Right) != 0;
        }

        public static bool IsXButton1(this MouseButtons buttons)
        {
            return (buttons & MouseButtons.XButton1) != 0;
        }

        public static bool IsXbutton2(this MouseButtons buttons)
        {
            return (buttons & MouseButtons.XButton2) != 0;
        }

        public static MouseMoveNotification ToMouseMoveNotification(this MouseEventArgs args, Control relativeTo)
        {
            return new MouseMoveNotification(args.Location, relativeTo.ClientSize);
        }

        public static MouseDownNotification ToMouseDownNotification(this MouseEventArgs args, Control relativeTo)
        {
            return new MouseDownNotification(args.Location, relativeTo.ClientSize, args.Button);
        }

        public static MouseUpNotification ToMouseUpNotification(this MouseEventArgs args, Control relativeTo)
        {
            return new MouseUpNotification(args.Location, relativeTo.ClientSize, args.Button);
        }

        public static MouseWheelNotification ToMouseWheelNotification(this MouseEventArgs args, Control relativeTo)
        {
            return new MouseWheelNotification(args.Location, relativeTo.ClientSize, args.Delta);
        }

        public static KeyDownNotification ToKeyDownNotification(this KeyEventArgs eventArgs)
        {
            return new KeyDownNotification(eventArgs);
        }

        public static KeyUpNotification ToKeyUpNotification(this KeyEventArgs eventArgs)
        {
            return new KeyUpNotification(eventArgs.KeyCode);
        }

        public static KeyPressNotification ToKeyPressNotification(this KeyPressEventArgs eventArgs)
        {
            return new KeyPressNotification(eventArgs.KeyChar);
        }
    }
}
