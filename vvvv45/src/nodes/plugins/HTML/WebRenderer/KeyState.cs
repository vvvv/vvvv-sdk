using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace VVVV.Nodes.HTML
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

        private readonly int FKeyValue;
        private readonly char? FKeyChar;

        public KeyState(int keyValue = 0)
        {
            FKeyValue = keyValue;
            FKeyChar = keyValue > 0 ? FromKeys((Keys)keyValue) : null;
        }

        public int KeyValue { get { return FKeyValue; } }
        public Keys KeyCode { get { return (Keys)KeyValue; } }
        public bool IsKeyDown { get { return KeyValue != 0; } }
        public bool IsKeyPress { get { return FKeyChar.HasValue; } }
        public char Key { get { return FKeyChar.HasValue ? FKeyChar.Value : (char)0; } }

        [Node(Name = "KeyState", Category = "Join")]
        public static KeyState Join(ISpread<int> keys)
        {
            switch (keys.SliceCount)
            {
                case 0:
                    return new KeyState();
                case 1:
                    return new KeyState(keys[0]);
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
                    return new KeyState((int)keyCode);
            }
        }

        [Node(Name = "KeyState", Category = "Split")]
        public static void Split(KeyState keyEvent, out ISpread<int> keys)
        {
            keys = new Spread<int>();
            Keys keyCode = (Keys)keyEvent.KeyValue;
            if ((keyCode & Keys.Shift) > 0) keys.Add((int)Keys.ShiftKey);
            if ((keyCode & Keys.Control) > 0) keys.Add((int)Keys.ControlKey);
            if ((keyCode & Keys.Alt) > 0) keys.Add((int)Keys.Menu);
            if ((keyCode & ~Keys.Modifiers) != Keys.None) keys.Add((int)(keyCode & ~Keys.Modifiers));
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
            return this.KeyValue == other.KeyValue;
        }

        public override int GetHashCode()
        {
            // combine the hash codes of all members here (e.g. with XOR operator ^)
            return KeyValue.GetHashCode();
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
