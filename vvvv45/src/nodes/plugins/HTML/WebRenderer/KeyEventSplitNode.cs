using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.HTML
{
    [PluginInfo(Name = "KeyEvent", Category = "HTML", Version = "Split")]
    public class KeyEventSplitNode : IPluginEvaluate
    {
        [Input("Input")]
        public ISpread<KeyState> FInput;

        [Output("Key")]
        public ISpread<ISpread<int>> FKeyOut;

        public void Evaluate(int spreadMax)
        {
            FKeyOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var keyEvent = FInput[i];
                ISpread<int> key;
                KeyState.Split(keyEvent, out key);
                FKeyOut[i] = key;
            }
        }
    }
}
