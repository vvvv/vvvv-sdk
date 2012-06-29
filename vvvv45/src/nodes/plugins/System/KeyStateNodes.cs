using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Core;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using System.Runtime.InteropServices;
using System.Diagnostics;
using VVVV.Utils.IO;

namespace VVVV.Nodes.IO
{
    public struct KeyStateNodes
    {
        [Node(Name = "KeyState", Category = "Join")]
        public static KeyState Join(ISpread<int> keys, bool capsLock, int time)
        {
            switch (keys.SliceCount)
            {
                case 0:
                    return new KeyState(Keys.None, capsLock, time);
                case 1:
                    return new KeyState((Keys)keys[0], capsLock, time);
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
                    return new KeyState(keyCode, capsLock, time);
            }
        }

        [Node(Name = "KeyState", Category = "Split")]
        public static void Split(KeyState keyState, out ISpread<int> keyCodes, out string key, out int time)
        {
            keyCodes = new Spread<int>();
            Keys keyCode = keyState.KeyCode;
            if ((keyCode & Keys.Shift) > 0) keyCodes.Add((int)Keys.ShiftKey);
            if ((keyCode & Keys.Control) > 0) keyCodes.Add((int)Keys.ControlKey);
            if ((keyCode & Keys.Alt) > 0) keyCodes.Add((int)Keys.Menu);
            if ((keyCode & ~Keys.Modifiers) != Keys.None) keyCodes.Add((int)(keyCode & ~Keys.Modifiers));
            time = keyState.Time;
            var _key = KeyState.FromKeys((Keys)keyCode, keyState.CapsLock);
            if (_key == null)
            	key = "";
            else
            	key = _key.ToString();
        }
    }
}
