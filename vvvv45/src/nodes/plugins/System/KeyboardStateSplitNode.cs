using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes.IO
{
    [PluginInfo(Name = "KeyboardState", Category = "System", Version = "Split")]
    public class KeyStateSplitNode : IPluginEvaluate
    {
        [Input("Input")]
        public ISpread<KeyboardState> FInput;

        [Output("Key Code")]
        public ISpread<ISpread<int>> FKeyCodeOut;
        
        [Output("Caps Lock")]
        public ISpread<bool> FCapsOut;
        
        [Output("Key")]
        public ISpread<string> FKeyOut;

        [Output("Time")]
        public ISpread<int> FTimeOut;

        public void Evaluate(int spreadMax)
        {
            FKeyOut.SliceCount = spreadMax;
            FTimeOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var keyEvent = FInput[i];
                ISpread<int> keyCode;
                string key;
                int time;
                KeyboardStateNodes.Split(keyEvent, out keyCode, out key, out time);
                FKeyCodeOut[i] = keyCode;
                FCapsOut[i] = FInput[i].CapsLock;
                FKeyOut[i] = key;
                FTimeOut[i] = time;
            }
        }
    }
}
