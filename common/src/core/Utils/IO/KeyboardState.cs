using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using VVVV.Utils.Collections;

namespace VVVV.Utils.IO
{
    /// <summary>
    /// Encapsulates the state of a keyboad. GetHashCode and Equals methods are overwritten
    /// so that two keyboard states can be easily compared.
    /// Use the KeyChars property to retrieve all pressed characters.
    /// 
    /// Example:
    /// Keys = { Shift, a, d }
    /// KeyChars = { A, D }
    /// </summary>
    public class KeyboardState : IEquatable<KeyboardState>
    {
        public static KeyboardState Empty = new KeyboardState(Enumerable.Empty<Keys>());

        #region virtual keycode to character translation

        // From: http://stackoverflow.com/questions/6214326/translate-keys-to-char
        public static char? FromKeys(Keys keys, bool capsLock, InputLanguage inputLanguage = null, bool firstChance = true)
        {
            inputLanguage = inputLanguage ?? InputLanguage.CurrentInputLanguage;

            byte[] keyStates = new byte[256];

            const byte keyPressed = 0x80;
            keyStates[(int)(keys & Keys.KeyCode)] = keyPressed;
            keyStates[(int)Keys.Capital] = capsLock ? (byte)0x01 : (byte)0;
            keyStates[(int)Keys.ShiftKey] = ((keys & Keys.Shift) == Keys.Shift) ? keyPressed : (byte)0;
            keyStates[(int)Keys.ControlKey] = ((keys & Keys.Control) == Keys.Control) ? keyPressed : (byte)0;
            keyStates[(int)Keys.Menu] = ((keys & Keys.Alt) == Keys.Alt) ? keyPressed : (byte)0;

            StringBuilder sb = new StringBuilder(10);
            int ret = ToUnicodeEx(keys, 0, keyStates, sb, sb.Capacity, 0, inputLanguage.Handle);
            if (ret == 1) return sb[0];

            if (ret == -1)
            {
                // dead letter
                if (firstChance)
                {
                    return FromKeys(keys, capsLock, inputLanguage, false);
                }
                return null;
            }
            return null;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ToUnicodeEx(Keys wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        #endregion

        private readonly ReadOnlyCollection<Keys> FKeys;
        private readonly bool FCapsLock;
        private readonly int FTime;
        private ReadOnlyCollection<char> FKeyChars;

        public KeyboardState(IEnumerable<Keys> keys, bool capsLock = false, int time = 0)
        {
            FKeys = keys.ToReadOnlyCollection();
        	FCapsLock = capsLock;
            FTime = time;
        }

        public ReadOnlyCollection<Keys> KeyCodes { get { return FKeys; } }
        public bool CapsLock { get { return FCapsLock; } }
        public ReadOnlyCollection<char> KeyChars
        { 
            get 
            {
                if (FKeyChars == null)
                {
                    InitKeys();
                }
                return FKeyChars;
            } 
        }
        public int Time { get { return FTime; } }

        private Keys? FModifiers;
        public Keys Modifiers
        {
            get
            {
                if (!FModifiers.HasValue)
                {
                    InitKeys();
                }
                return FModifiers.Value;
            }
        }

        private void InitKeys()
        {
            var realKeys = new List<Keys>();
            FModifiers = Keys.None;
            foreach (var key in FKeys)
            {
                switch (key)
                {
                    case Keys.ShiftKey:
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        FModifiers |= Keys.Shift;
                        break;
                    case Keys.ControlKey:
                        FModifiers |= Keys.Control;
                        break;
                    case Keys.Menu:
                    case Keys.LMenu:
                    case Keys.RMenu:
                        FModifiers |= Keys.Alt;
                        break;
                    default:
                        // Do not allow more than one "normal" key
                        realKeys.Add(key);
                        break;
                }
            }
            FKeyChars = realKeys
                .Select(keyCode => FromKeys(keyCode | FModifiers.Value, FCapsLock))
                .Where(c => c != null)
                .Select(c => c.Value)
                .ToReadOnlyCollection();
        }
        
        #region Equals and GetHashCode implementation
        // The code in this region is useful if you want to use this structure in collections.
        // If you don't need it, you can just remove the region and the ": IEquatable<MouseEvent>" declaration.

        public override bool Equals(object obj)
        {
            if (obj is KeyboardState)
                return Equals((KeyboardState)obj); // use Equals method below
            else
                return false;
        }

        public bool Equals(KeyboardState other)
        {
            // add comparisions for all members here
            return
                this.FTime == other.FTime &&
                this.FCapsLock == other.FCapsLock &&
                this.FKeys.SequenceEqual(other.FKeys);
        }

        public override int GetHashCode()
        {
            // combine the hash codes of all members here (e.g. with XOR operator ^)
            var seed = FTime.GetHashCode() ^ FCapsLock.GetHashCode();
            return FKeys.Aggregate(seed, (accum, keyCode) => accum ^ keyCode.GetHashCode());
        }

        public static bool operator ==(KeyboardState left, KeyboardState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(KeyboardState left, KeyboardState right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}
