using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.IO
{
    [PluginInfo(Name = "KeyState", Category = "System", Version = "Join")]
    public class KeyStateJoinNode : IPluginEvaluate
    {
        [Input("Key")]
        public ISpread<ISpread<int>> FKeyIn;
        [Input("Time")]
        public ISpread<int> FTimeIn;
        [Output("Output")]
        public ISpread<KeyState> FOutput;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = FKeyIn.CombineWith(FTimeIn);
            for (int i = 0; i < FOutput.SliceCount; i++)
            {
                FOutput[i] = KeyState.Join(FKeyIn[i], FTimeIn[i]);
            }
        }
    }
}
