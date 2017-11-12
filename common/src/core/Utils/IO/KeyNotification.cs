using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.Utils.Win32;

namespace VVVV.Utils.IO
{
    public enum KeyNotificationKind
    {
        KeyDown,
        KeyPress,
        KeyUp,
        DeviceLost
    }

    public abstract class KeyNotification
    {
        public readonly KeyNotificationKind Kind;
        public readonly object Sender;
        protected KeyNotification(KeyNotificationKind kind, object sender = null)
        {
            Kind = kind;
            Sender = sender;
        }

        public bool IsKeyDown { get { return Kind == KeyNotificationKind.KeyDown; } }
        public bool IsKeyUp { get { return Kind == KeyNotificationKind.KeyUp; } }
        public bool IsKeyPress { get { return Kind == KeyNotificationKind.KeyPress; } }
        public bool IsDeviceLost { get { return Kind == KeyNotificationKind.DeviceLost; } }
    }

    public abstract class KeyCodeNotification : KeyNotification
    {
        public readonly Keys KeyCode;
        public KeyCodeNotification(KeyNotificationKind kind, Keys keyCode, object sender = null)
            : base(kind, sender)
        {
            KeyCode = keyCode;
        }
    }

    public class KeyDownNotification : KeyCodeNotification
    {
        public KeyDownNotification(Keys keyCode, object sender = null) : this(new KeyEventArgs(keyCode), sender) { }
        public KeyDownNotification(KeyEventArgs args, object sender = null)
            : base(KeyNotificationKind.KeyDown, args.KeyCode, sender)
        {
            origArgs = args;
        }
        KeyEventArgs origArgs;

        public bool Handled { get { return origArgs.Handled; } set { origArgs.Handled = value; } } 
    }

    public class KeyPressNotification : KeyNotification
    {
        public readonly char KeyChar;
        public KeyPressNotification(char keyChar, object sender = null)
            : base(KeyNotificationKind.KeyPress, sender)
        {
            KeyChar = keyChar;
        }
    }

    public class KeyUpNotification : KeyCodeNotification
    {
        public KeyUpNotification(Keys keyCode, object sender = null)
            : base(KeyNotificationKind.KeyUp, keyCode, sender)
        {
        }
    }

    public class KeyboardLostNotification : KeyNotification
    {
        public KeyboardLostNotification(object sender)
            : base(KeyNotificationKind.DeviceLost, sender)
        {
        }
    }
}
