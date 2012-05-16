using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.HTML
{
    public struct KeyEvent : IEquatable<KeyEvent>
    {
        public int Key;
        public bool IsKeyDown;

        public KeyEvent(int key, bool isKeyDown)
        {
            Key = key;
            IsKeyDown = isKeyDown;
        }

        [Node(Name = "KeyEvent", Category = "Join")]
        public static KeyEvent Join(ISpread<int> keys, bool isKeyDown)
        {
            switch (keys.SliceCount)
            {
                case 0:
                    return new KeyEvent();
                case 1:
                    return new KeyEvent(keys[0], isKeyDown);
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
                    return new KeyEvent((int)keyCode, isKeyDown);
            }
        }

        [Node(Name = "KeyEvent", Category = "Split")]
        public static void Split(KeyEvent keyEvent, out ISpread<int> keys, out bool isKeyDown)
        {
            keys = new Spread<int>();
            Keys keyCode = (Keys)keyEvent.Key;
            if ((keyCode & Keys.Shift) > 0) keys.Add((int)Keys.ShiftKey);
            if ((keyCode & Keys.Control) > 0) keys.Add((int)Keys.ControlKey);
            if ((keyCode & Keys.Alt) > 0) keys.Add((int)Keys.Menu);
            if ((keyCode & ~Keys.Modifiers) != Keys.None) keys.Add((int)(keyCode & ~Keys.Modifiers));
            isKeyDown = keyEvent.IsKeyDown;
        }

        #region Equals and GetHashCode implementation
        // The code in this region is useful if you want to use this structure in collections.
        // If you don't need it, you can just remove the region and the ": IEquatable<MouseEvent>" declaration.

        public override bool Equals(object obj)
        {
            if (obj is KeyEvent)
                return Equals((KeyEvent)obj); // use Equals method below
            else
                return false;
        }

        public bool Equals(KeyEvent other)
        {
            // add comparisions for all members here
            return this.Key == other.Key && this.IsKeyDown == other.IsKeyDown;
        }

        public override int GetHashCode()
        {
            // combine the hash codes of all members here (e.g. with XOR operator ^)
            return Key.GetHashCode() ^ IsKeyDown.GetHashCode();
        }

        public static bool operator ==(KeyEvent left, KeyEvent right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(KeyEvent left, KeyEvent right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}
