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
        public ISpread<KeyEvent> FInput;

        [Output("Key")]
        public ISpread<ISpread<int>> FKeyOut;
        [Output("Is Key Down")]
        public ISpread<bool> FIsKeyDownOut;

        public void Evaluate(int spreadMax)
        {
            FKeyOut.SliceCount = spreadMax;
            FIsKeyDownOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var keyEvent = FInput[i];
                ISpread<int> key;
                bool isKeyDown;
                KeyEvent.Split(keyEvent, out key, out isKeyDown);
                FKeyOut[i] = key;
                FIsKeyDownOut[i] = isKeyDown;
            }
        }
    }
}
