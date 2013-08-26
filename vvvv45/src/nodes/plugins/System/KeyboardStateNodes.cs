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

namespace VVVV.Nodes.Input
{
    public static class KeyboardStateNodes
    {
        [Node(Name = "KeyboardState", Category = "System", Version = "Join")]
        public static KeyboardState Join(ISpread<int> keys, bool capsLock, int time)
        {
            switch (keys.SliceCount)
            {
                case 0:
                    return KeyboardState.Empty;
                case 1:
                    var key = keys[0];
                    if (key == 0)
                        return new KeyboardState(Enumerable.Empty<Keys>(), capsLock, time);
                    else
                        return new KeyboardState(keys.Cast<Keys>(), capsLock, time);
                default:
                    return new KeyboardState(keys.Cast<Keys>(), capsLock, time);
            }
        }

        [Node(Name = "KeyboardState", Category = "System", Version = "Split")]
        public static void Split(KeyboardState keyboardState, out ISpread<int> keyCodes, out int time, out bool capsLock)
        {
            keyCodes = new Spread<int>();
            keyCodes.AssignFrom(keyboardState.KeyCodes.Select(k => (int)k));
            time = keyboardState.Time;
            capsLock = keyboardState.CapsLock;
        }
    }
}
