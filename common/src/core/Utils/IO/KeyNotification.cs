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

    public abstract class KeyNotification : Notification
    {
        public readonly KeyNotificationKind Kind;

        protected KeyNotification(KeyNotificationKind kind)
        {
            Kind = kind;
        }
        protected KeyNotification(KeyNotificationKind kind, object sender)
            : base(sender)
        {
            Kind = kind;
        }
        public bool IsKeyDown { get { return Kind == KeyNotificationKind.KeyDown; } }
        public bool IsKeyUp { get { return Kind == KeyNotificationKind.KeyUp; } }
        public bool IsKeyPress { get { return Kind == KeyNotificationKind.KeyPress; } }
        public bool IsDeviceLost { get { return Kind == KeyNotificationKind.DeviceLost; } }
    }

    public abstract class KeyCodeNotification : KeyNotification
    {
        public readonly Keys KeyCode;

        public KeyCodeNotification(KeyNotificationKind kind, Keys keyCode)
            : base(kind)
        {
            KeyCode = keyCode;
        }
        public KeyCodeNotification(KeyNotificationKind kind, Keys keyCode, object sender)
            : base(kind, sender)
        {
            KeyCode = keyCode;
        }
    }

    public class KeyDownNotification : KeyCodeNotification
    {
        KeyEventArgs origArgs;
        public bool Handled { get { return origArgs.Handled; } set { origArgs.Handled = value; } }

        public KeyDownNotification(Keys keyCode)
            : this(new KeyEventArgs(keyCode)) { }
        public KeyDownNotification(Keys keyCode, object sender)
            : this(new KeyEventArgs(keyCode), sender) { }

        public KeyDownNotification(KeyEventArgs args)
            : base(KeyNotificationKind.KeyDown, args.KeyCode)
        {
            origArgs = args;
        }
        public KeyDownNotification(KeyEventArgs args, object sender)
            : base(KeyNotificationKind.KeyDown, args.KeyCode, sender)
        {
            origArgs = args;
        }

        public override INotification WithSender(object sender)
            => new KeyDownNotification(origArgs, sender);
    }

    public class KeyPressNotification : KeyNotification
    {
        public readonly char KeyChar;

        public KeyPressNotification(char keyChar)
            : base(KeyNotificationKind.KeyPress)
        {
            KeyChar = keyChar;
        }
        public KeyPressNotification(char keyChar, object sender)
            : base(KeyNotificationKind.KeyPress, sender)
        {
            KeyChar = keyChar;
        }

        public override INotification WithSender(object sender)
            => new KeyPressNotification(KeyChar, sender);
    }

    public class KeyUpNotification : KeyCodeNotification
    {
        public KeyUpNotification(Keys keyCode)
            : base(KeyNotificationKind.KeyUp, keyCode)
        {
        }
        public KeyUpNotification(Keys keyCode, object sender)
            : base(KeyNotificationKind.KeyUp, keyCode, sender)
        {
        }

        public override INotification WithSender(object sender)
            => new KeyUpNotification(KeyCode, sender);
    }

    public class KeyboardLostNotification : KeyNotification
    {
        public KeyboardLostNotification()
            : base(KeyNotificationKind.DeviceLost)
        {
        }
        public KeyboardLostNotification(object sender)
            : base(KeyNotificationKind.DeviceLost, sender)
        {
        }

        public override INotification WithSender(object sender)
            => new KeyboardLostNotification(sender);
    }
}
