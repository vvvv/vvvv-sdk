using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using VVVV.Utils.Win32;
using System.Windows.Forms;

namespace VVVV.Utils.IO
{
    public class Keyboard// : IDisposable
    {
        public static readonly Keyboard Empty = new Keyboard(Observable.Never<KeyNotification>());

        public readonly IObservable<KeyNotification> KeyNotifications;
        //private IDisposable FSubscription;
        private readonly byte[] FKeyStates = new byte[256];

        public Keyboard(IObservable<KeyNotification> keyNotifications, bool generateKeyPressNotifications = false)
        {
            keyNotifications = keyNotifications.Do(n =>
                {
                    switch (n.Kind)
                    {
                        case KeyNotificationKind.KeyDown:
                            var keyDown = n as KeyDownNotification;
                            var downKeyCode = keyDown.KeyCode;
                            var i = (int)downKeyCode;
                            FKeyStates[(int)downKeyCode] = downKeyCode == Keys.NumLock || downKeyCode == Keys.CapsLock
                                ? (byte)(FKeyStates[(int)downKeyCode] ^ Const.KEY_TOGGLED)
                                : Const.KEY_PRESSED;
                            break;
                        case KeyNotificationKind.KeyUp:
                            var keyUp = n as KeyUpNotification;
                            var upKeyCode = keyUp.KeyCode;
                            FKeyStates[(int)upKeyCode] = upKeyCode == Keys.NumLock || upKeyCode == Keys.CapsLock
                                ? (byte)FKeyStates[(int)upKeyCode]
                                : (byte)0;
                            break;
                    }
                }
            );
            if (generateKeyPressNotifications)
                keyNotifications = keyNotifications.SelectMany(n => GenerateKeyPressNotifications(n));
            KeyNotifications = keyNotifications.Publish().RefCount();
            //Subscribe();
        }

        public Keys Modifiers
        {
            get
            {
                var modifiers = Keys.None;
                if (FKeyStates[(int)Keys.ControlKey] == Const.KEY_PRESSED)
                    modifiers |= Keys.Control;
                if (FKeyStates[(int)Keys.Menu] == Const.KEY_PRESSED)
                    modifiers |= Keys.Alt;
                if (FKeyStates[(int)Keys.ShiftKey] == Const.KEY_PRESSED)
                    modifiers |= Keys.Shift;
                return modifiers;
            }
        }

        private IEnumerable<KeyNotification> GenerateKeyPressNotifications(KeyNotification n)
        {
            if (n.Kind == KeyNotificationKind.KeyDown)
            {
                var keyDown = n as KeyDownNotification;
                yield return keyDown;
                // Check if it's a character
                var chr = keyDown.KeyCode.ToChar(FKeyStates);
                if (chr.HasValue)
                    yield return new KeyPressNotification(chr.Value);
            }
            else
                yield return n;
        }
    }
}
