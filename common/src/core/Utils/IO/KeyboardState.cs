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
    /// Encapsulates the state of a keyboard. GetHashCode and Equals methods are overwritten
    /// so that two keyboard states can be easily compared.
    /// Use the KeyChars property to retrieve all pressed characters.
    /// 
    /// Example:
    /// Keys = { Shift, a, d }
    /// KeyChars = { A, D }
    /// </summary>
    public class KeyboardState : IEquatable<KeyboardState>
    {
        public static KeyboardState Empty { get; private set; }
        static KeyboardState()
        {
            Empty = new KeyboardState(Enumerable.Empty<Keys>());
        }

        public static KeyboardState Current
        {
            get
            {
                // Windows bug? Remove this call and GetKeyboardState will only
                // work if process is not in background.
                // See strange comment here: http://www.pinvoke.net/default.aspx/user32.getkeyboardstate
                GetKeyState(Keys.None);
                var lpKeyState = new byte[255];
                if (GetKeyboardState(lpKeyState))
                {
                    var keys = new List<Keys>();
                    for (int i = 0; i < lpKeyState.Length; i++)
                    {
                        if ((lpKeyState[i] & KEY_PRESSED) > 0)
                            keys.Add((Keys)i);
                    }
                    var capsLock = (lpKeyState[(int)Keys.CapsLock] & KEY_TOGGLED) > 0;
                    return new KeyboardState(keys, capsLock);
                }
                else
                    return Empty;
            }
        }

        // Slow
        public static KeyboardState CurrentAsync
        {
            get
            {
                var keys = new List<Keys>();
                for (int i = 0; i < 255; i++)
                {
                    var key = (Keys)i;
                    if ((GetAsyncKeyState(key) & 0x8000) > 0)
                        keys.Add(key);
                }
                var capsLock = (GetKeyState(Keys.CapsLock) & KEY_TOGGLED) > 0;
                return new KeyboardState(keys, capsLock);
            }
        }

        public static KeyboardState FromScanCodes(IEnumerable<uint> scanCodes)
        {
            var virtualKeys = scanCodes.Select(sc => ScanCodeToVirtualKey(sc));
            return new KeyboardState(virtualKeys);
        }

        private static Keys ScanCodeToVirtualKey(uint scanCode)
        {
            return (Keys)MapVirtualKey(scanCode, MAPVK_VSC_TO_VK);
        }

        #region virtual keycode to character translation

        // From: http://stackoverflow.com/questions/6214326/translate-keys-to-char
        private static char? FromKeys(Keys keys, bool capsLock, InputLanguage inputLanguage = null, bool firstChance = true)
        {
            inputLanguage = inputLanguage ?? InputLanguage.CurrentInputLanguage;

            byte[] keyStates = new byte[256];

            keyStates[(int)(keys & Keys.KeyCode)] = KEY_PRESSED;
            keyStates[(int)Keys.Capital] = capsLock ? (byte)0x01 : (byte)0;
            keyStates[(int)Keys.ShiftKey] = ((keys & Keys.Shift) == Keys.Shift) ? KEY_PRESSED : (byte)0;
            keyStates[(int)Keys.ControlKey] = ((keys & Keys.Control) == Keys.Control) ? KEY_PRESSED : (byte)0;
            keyStates[(int)Keys.Menu] = ((keys & Keys.Alt) == Keys.Alt) ? KEY_PRESSED : (byte)0;

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

        #endregion

        #region native methods

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int ToUnicodeEx(Keys wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern int MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        [DllImport("user32.dll")]
        static extern short GetKeyState(System.Windows.Forms.Keys vKey); 

        private const byte KEY_PRESSED = 0x80;
        private const byte KEY_TOGGLED = 0x01;
        private const uint MAPVK_VK_TO_VSC = 0x00;
        private const uint MAPVK_VSC_TO_VK = 0x01;
        private const uint MAPVK_VK_TO_CHAR = 0x02;
        private const uint MAPVK_VSC_TO_VK_EX = 0x03;
        private const uint MAPVK_VK_TO_VSC_EX = 0x04;

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

        /// <summary>
        /// Returns all the pressed keys.
        /// </summary>
        public ReadOnlyCollection<Keys> KeyCodes { get { return FKeys; } }

        /// <summary>
        /// Gets whether or not the caps lock is enabled.
        /// </summary>
        public bool CapsLock { get { return FCapsLock; } }

        /// <summary>
        /// Returns all the pressed characters. 
        /// 
        /// For example if the KeyCodes property contains SHIFT and A, this property will return 'A'
        /// or if the KeyCodes property contains CTRL, ALT and Q, this property will return '@'
        /// on a german keyboard.
        /// </summary>
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

        /// <summary>
        /// Used to distinguish two keyboard states if they contain the same same key codes.
        /// Useful if a key is pressed for a long time which should generate repeated key strokes.
        /// </summary>
        public int Time { get { return FTime; } }

        private Keys? FModifiers;

        /// <summary>
        /// Returns all the pressed modifier keys in one enumeration.
        /// </summary>
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

        /// <summary>
        /// Returns true if the other keyboard state contains the same keys, has the same caps lock state
        /// and time value.
        /// </summary>
        /// <param name="other">The keyboard state to compare with the current keyboard state.</param>
        /// <returns>True if the specified keyboard state is equal to the current keyboard state.</returns>
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

        public static bool operator ==(KeyboardState a, KeyboardState b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(KeyboardState a, KeyboardState b)
        {
            return !(a == b);
        }
        #endregion
    }
}
