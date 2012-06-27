using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.IO
{
    [PluginInfo(Name = "KeyState", Category = "System", Version = "Split")]
    public class KeyStateSplitNode : IPluginEvaluate
    {
        [Input("Input")]
        public ISpread<KeyState> FInput;

        [Output("Key")]
        public ISpread<ISpread<int>> FKeyOut;

        [Output("Time")]
        public ISpread<int> FTimeOut;

        public void Evaluate(int spreadMax)
        {
            FKeyOut.SliceCount = spreadMax;
            FTimeOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var keyEvent = FInput[i];
                ISpread<int> key;
                int time;
                KeyState.Split(keyEvent, out key, out time);
                FKeyOut[i] = key;
                FTimeOut[i] = time;
            }
        }
    }
}
