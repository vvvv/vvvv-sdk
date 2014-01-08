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
        KeyUp
    }

    public abstract class KeyNotification
    {
        public readonly KeyNotificationKind Kind;
        protected KeyNotification(KeyNotificationKind kind)
        {
            Kind = kind;
        }
    }

    public abstract class KeyCodeNotification : KeyNotification
    {
        public readonly Keys KeyCode;
        public KeyCodeNotification(KeyNotificationKind kind, Keys keyCode)
            : base(kind)
        {
            KeyCode = keyCode;
        }
    }

    public class KeyDownNotification : KeyCodeNotification
    {
        public KeyDownNotification(Keys keyCode)
            : base(KeyNotificationKind.KeyDown, keyCode)
        {
        }
    }

    public class KeyPressNotification : KeyNotification
    {
        public readonly char KeyChar;
        public KeyPressNotification(char keyChar)
            : base(KeyNotificationKind.KeyPress)
        {
            KeyChar = keyChar;
        }
    }

    public class KeyUpNotification : KeyCodeNotification
    {
        public KeyUpNotification(Keys keyCode)
            : base(KeyNotificationKind.KeyUp, keyCode)
        {
        }
    }
}
