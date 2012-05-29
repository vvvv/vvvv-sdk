using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.HTML
{
    [PluginInfo(Name = "KeyEvent", Category = "HTML", Version = "Join")]
    public class KeyEventJoinNode : IPluginEvaluate
    {
        [Input("Key")]
        public ISpread<ISpread<int>> FKeyIn;
        [Output("Output")]
        public ISpread<KeyState> FOutput;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = FKeyIn.SliceCount;
            for (int i = 0; i < FOutput.SliceCount; i++)
            {
                FOutput[i] = KeyState.Join(FKeyIn[i]);
            }
        }
    }
}
