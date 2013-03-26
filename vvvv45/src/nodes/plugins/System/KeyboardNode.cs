using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using System.ComponentModel.Composition;
using VVVV.Utils.Win32;
using VVVV.Hosting.Graph;
using System.Windows.Forms;
using WindowsInput;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "Keyboard", Category = "System", Version = "Global New", AutoEvaluate = true)]
    public class GlobalKeyboardNode : IPluginEvaluate
    {
        [Input("Keyboard", IsSingle = true)]
        protected ISpread<KeyboardState> FKeyboardIn;

        [Output("Keyboard", IsSingle = true)]
        protected ISpread<KeyboardState> FKeyboardOut;

        [Import]
        protected IHDEHost FHost;

        KeyboardState FLastKeyboardIn = KeyboardState.Empty;

        public void Evaluate(int SpreadMax)
        {
            if (FKeyboardIn.SliceCount > 0)
            {
                var currentKeyboardIn = FKeyboardIn[0] ?? KeyboardState.Empty;
                if (currentKeyboardIn != FLastKeyboardIn)
                {
                    var modifierKeys = currentKeyboardIn.ModifierKeys.Select(k => (VirtualKeyCode)k);
                    var keys = currentKeyboardIn.KeyCodes.Select(k => (VirtualKeyCode)k).Except(modifierKeys);
                    InputSimulator.SimulateModifiedKeyStroke(modifierKeys, keys);
                }
                FLastKeyboardIn = currentKeyboardIn;
            }
            // Works when we call GetKeyState before calling GetKeyboardState ?!
            //if (FHost.IsRunningInBackground)
            //    FKeysOut[0] = KeyboardState.CurrentAsync;
            //else
                FKeyboardOut[0] = KeyboardState.Current;
        }
    }

    [PluginInfo(Name = "Keyboard", Category = "System", Version = "Window New")]
    public class WindowKeyboardNode : UserInputNode, IPluginEvaluate
    {
#pragma warning disable 0649
        [Output("Keyboard", IsSingle = true)]
        ISpread<KeyboardState> FKeyboardOut;
#pragma warning restore 0649

        protected override unsafe void HandleSubclassWindowMessage(object sender, WMEventArgs e)
        {
            if (e.Message >= WM.KEYFIRST && e.Message <= WM.KEYLAST)
            {
                var key = (Keys)e.WParam;
                var capsLock = Control.IsKeyLocked(Keys.CapsLock);
                var time = User32.GetMessageTime();
                var chars = Enumerable.Empty<char>();
                var keys = Enumerable.Empty<Keys>();
                switch (e.Message)
                {
                    case WM.KEYDOWN:
                    case WM.SYSKEYDOWN:
                        keys = FKeyboardState.KeyCodes.Concat(new[] { key });
                        break;
                    case WM.CHAR:
                    case WM.SYSCHAR:
                        chars = new[] { (char)e.WParam };
                        keys = FKeyboardState.KeyCodes;
                        break;
                    case WM.KEYUP:
                    case WM.SYSKEYUP:
                        keys = FKeyboardState.KeyCodes.Except(new[] { key });
                        break;
                }
                FKeyboardState = new KeyboardState(keys, chars, capsLock, time);
            }
        }

        private KeyboardState FKeyboardState = KeyboardState.Empty;

        public void Evaluate(int SpreadMax)
        {
            FKeyboardOut[0] = FKeyboardState;
        }
    }
}
