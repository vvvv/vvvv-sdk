using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace VVVV.Nodes.IO
{
    public struct KeyState : IEquatable<KeyState>
    {
        #region virtual keycode to character translation

        // From: http://stackoverflow.com/questions/6214326/translate-keys-to-char
        private static char? FromKeys(Keys keys, InputLanguage inputLanguage = null, bool firstChance = true)
        {
            inputLanguage = inputLanguage ?? InputLanguage.CurrentInputLanguage;

            byte[] keyStates = new byte[256];

            const byte keyPressed = 0x80;
            keyStates[(int)(keys & Keys.KeyCode)] = keyPressed;
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
                    return FromKeys(keys, inputLanguage, false);
                }
                return null;
            }
            return null;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ToUnicodeEx(Keys wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        #endregion

        private readonly Keys FKeyCode;
        private readonly char? FKeyChar;
        private readonly int FTime;

        public KeyState(Keys keyCode = 0, int time = 0)
        {
            FKeyCode = keyCode;
            FKeyChar = keyCode > 0 ? FromKeys((Keys)keyCode) : null;
            FTime = time;
        }

        public Keys KeyCode { get { return FKeyCode; } }
        public char? Key { get { return FKeyChar; } }
        public int Time { get { return FTime; } }

        [Node(Name = "KeyState", Category = "Join")]
        public static KeyState Join(ISpread<int> keys, int time)
        {
            switch (keys.SliceCount)
            {
                case 0:
                    return new KeyState(Keys.None, time);
                case 1:
                    return new KeyState((Keys)keys[0], time);
                default:
                    Keys keyCode = Keys.None;
                    foreach (Keys key in keys)
                    {
                        switch (key)
                        {
                            case Keys.ShiftKey:
                            case Keys.LShiftKey:
                            case Keys.RShiftKey:
                                keyCode |= Keys.Shift;
                                break;
                            case Keys.ControlKey:
                                keyCode |= Keys.Control;
                                break;
                            case Keys.Menu:
                            case Keys.LMenu:
                            case Keys.RMenu:
                                keyCode |= Keys.Alt;
                                break;
                            default:
                                // Do not allow more than one "normal" key
                                keyCode &= Keys.Modifiers;
                                keyCode |= key;
                                break;
                        }
                    }
                    return new KeyState(keyCode, time);
            }
        }

        [Node(Name = "KeyState", Category = "Split")]
        public static void Split(KeyState keyState, out ISpread<int> keys, out int time)
        {
            keys = new Spread<int>();
            Keys keyCode = keyState.KeyCode;
            if ((keyCode & Keys.Shift) > 0) keys.Add((int)Keys.ShiftKey);
            if ((keyCode & Keys.Control) > 0) keys.Add((int)Keys.ControlKey);
            if ((keyCode & Keys.Alt) > 0) keys.Add((int)Keys.Menu);
            if ((keyCode & ~Keys.Modifiers) != Keys.None) keys.Add((int)(keyCode & ~Keys.Modifiers));
            time = keyState.Time;
        }

        #region Equals and GetHashCode implementation
        // The code in this region is useful if you want to use this structure in collections.
        // If you don't need it, you can just remove the region and the ": IEquatable<MouseEvent>" declaration.

        public override bool Equals(object obj)
        {
            if (obj is KeyState)
                return Equals((KeyState)obj); // use Equals method below
            else
                return false;
        }

        public bool Equals(KeyState other)
        {
            // add comparisions for all members here
            return this.FKeyCode == other.FKeyCode && this.FTime == other.FTime;
        }

        public override int GetHashCode()
        {
            // combine the hash codes of all members here (e.g. with XOR operator ^)
            return FKeyCode.GetHashCode() ^ FTime.GetHashCode();
        }

        public static bool operator ==(KeyState left, KeyState right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(KeyState left, KeyState right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}
