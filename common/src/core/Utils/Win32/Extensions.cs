using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Utils.Win32
{
    public static class Extensions
    {
        /// <summary>
        /// Extracts lower 16-bit word.
        /// See: http://stackoverflow.com/questions/7913325/win-api-in-c-get-hi-and-low-word-from-intptr
        /// </summary>
        public static short LoWord(this UIntPtr xy)
        {
            return unchecked((short)(long)xy);
        }

        /// <summary>
        /// Extracts higher 16-bit word.
        /// See: http://stackoverflow.com/questions/7913325/win-api-in-c-get-hi-and-low-word-from-intptr
        /// </summary>
        public static short HiWord(this UIntPtr xy)
        {
            return unchecked((short)((long)xy >> 16));
        }

        /// <summary>
        /// Extracts lower 16-bit word.
        /// See: http://stackoverflow.com/questions/7913325/win-api-in-c-get-hi-and-low-word-from-intptr
        /// </summary>
        public static short LoWord(this IntPtr xy)
        {
            return unchecked((short)(long)xy);
        }

        /// <summary>
        /// Extracts higher 16-bit word.
        /// See: http://stackoverflow.com/questions/7913325/win-api-in-c-get-hi-and-low-word-from-intptr
        /// </summary>
        public static short HiWord(this IntPtr xy)
        {
            return unchecked((short)((long)xy >> 16));
        }

        /// <summary>
        /// Virtual keycode to character translation.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/6214326/translate-keys-to-char</remarks>
        public static char? ToChar(this Keys keys, bool capsLock, InputLanguage inputLanguage = null, bool firstChance = true)
        {
            inputLanguage = inputLanguage ?? InputLanguage.CurrentInputLanguage;

            byte[] keyStates = new byte[256];

            try
            {
                keyStates[(int)(keys & Keys.KeyCode)] = Const.KEY_PRESSED;
                keyStates[(int)Keys.Capital] = capsLock ? Const.KEY_TOGGLED : (byte)0;
                keyStates[(int)Keys.ShiftKey] = ((keys & Keys.Shift) == Keys.Shift) ? Const.KEY_PRESSED : (byte)0;
                keyStates[(int)Keys.ControlKey] = ((keys & Keys.Control) == Keys.Control) ? Const.KEY_PRESSED : (byte)0;
                keyStates[(int)Keys.Menu] = ((keys & Keys.Alt) == Keys.Alt) ? Const.KEY_PRESSED : (byte)0;
            }
            catch (IndexOutOfRangeException) { }

            return keys.ToChar(keyStates, inputLanguage, firstChance);
        }

        public static char? ToChar(this Keys keys, byte[] keyStates, InputLanguage inputLanguage = null, bool firstChance = true)
        {
            inputLanguage = inputLanguage ?? InputLanguage.CurrentInputLanguage;
            StringBuilder sb = new StringBuilder(10);
            int ret = User32.ToUnicodeEx(keys, 0, keyStates, sb, sb.Capacity, 0, inputLanguage.Handle);
            if (ret == 1) return sb[0];

            if (ret == -1)
            {
                // dead letter
                if (firstChance)
                {
                    return keys.ToChar(keyStates, inputLanguage, false);
                }
                return null;
            }
            return null;
        }

        public static Keys GetModifiers(this IEnumerable<Keys> keys)
        {
            var modifiers = Keys.None;
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.ShiftKey:
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        modifiers |= Keys.Shift;
                        break;
                    case Keys.ControlKey:
                        modifiers |= Keys.Control;
                        break;
                    case Keys.Menu:
                    case Keys.LMenu:
                    case Keys.RMenu:
                        modifiers |= Keys.Alt;
                        break;
                }
            }
            return modifiers;
        }

        public static IEnumerable<Keys> ReplaceModifierKeys(this IEnumerable<Keys> keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.Shift:
                        yield return Keys.ShiftKey;
                        break;
                    case Keys.Control:
                        yield return Keys.ControlKey;
                        break;
                    case Keys.Alt:
                        yield return Keys.Menu;
                        break;
                    default:
                        yield return key;
                        break;
                }
            }
        }
    }
}
