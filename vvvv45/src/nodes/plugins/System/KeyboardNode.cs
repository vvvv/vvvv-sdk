using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using System.ComponentModel.Composition;

namespace VVVV.Nodes
{
    [PluginInfo(Name = "Keyboard", Category = "System", Version = "Global New")]
    public class GlobalKeyboardNode : IPluginEvaluate
    {
        [Output("Keys")]
        protected ISpread<KeyboardState> FKeysOut;

        [Import]
        protected IHDEHost FHost;

        public void Evaluate(int SpreadMax)
        {
            // Works when we call GetKeyState before calling GetKeyboardState ?!
            //if (FHost.IsRunningInBackground)
            //    FKeysOut[0] = KeyboardState.CurrentAsync;
            //else
                FKeysOut[0] = KeyboardState.Current;
        }
    }
}
