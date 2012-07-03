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
    public static class KeyboardStateNodes
    {
        [Node(Name = "KeyboardState", Category = "System", Version = "Join")]
        public static KeyboardState Join(ISpread<int> keys, bool capsLock, int time)
        {
            return new KeyboardState(keys.Cast<Keys>(), capsLock, time);
        }

        [Node(Name = "KeyboardState", Category = "System", Version = "Split")]
        public static void Split(KeyboardState keyboardState, out ISpread<int> keyCodes, out string key, out int time)
        {
            keyCodes = new Spread<int>();
            keyCodes.AssignFrom(keyboardState.KeyCodes.Select(k => (int)k));
            key = string.Join(string.Empty, keyboardState.KeyChars);
            time = keyboardState.Time;
        }
    }
}
