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

        public static MouseMoveNotification ToMouseMoveNotification(this MouseEventArgs args, Control relativeTo, object sender = null)
        {
            return new MouseMoveNotification(args.Location, relativeTo.ClientSize, sender);
        }

        public static MouseDownNotification ToMouseDownNotification(this MouseEventArgs args, Control relativeTo, object sender = null)
        {
            return new MouseDownNotification(args.Location, relativeTo.ClientSize, args.Button, sender);
        }

        public static MouseUpNotification ToMouseUpNotification(this MouseEventArgs args, Control relativeTo, object sender = null)
        {
            return new MouseUpNotification(args.Location, relativeTo.ClientSize, args.Button, sender);
        }

        public static MouseWheelNotification ToMouseWheelNotification(this MouseEventArgs args, Control relativeTo, object sender = null)
        {
            return new MouseWheelNotification(args.Location, relativeTo.ClientSize, args.Delta, sender);
        }

        public static KeyDownNotification ToKeyDownNotification(this KeyEventArgs eventArgs, object sender = null)
        {
            return new KeyDownNotification(eventArgs, sender);
        }

        public static KeyUpNotification ToKeyUpNotification(this KeyEventArgs eventArgs, object sender = null)
        {
            return new KeyUpNotification(eventArgs.KeyCode, sender);
        }

        public static KeyPressNotification ToKeyPressNotification(this KeyPressEventArgs eventArgs, object sender = null)
        {
            return new KeyPressNotification(eventArgs.KeyChar, sender);
        }
    }
}
