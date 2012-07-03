using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes.IO
{
    [PluginInfo(Name = "KeyboardState", Category = "System", Version = "Join")]
    public class KeyStateJoinNode : IPluginEvaluate
    {
        [Input("Key Code")]
        public ISpread<ISpread<int>> FKeyIn;
        
        [Input("Caps Lock", IsSingle = true)]
        public ISpread<bool> FCapsIn;
        
        [Input("Time")]
        public ISpread<int> FTimeIn;
        
        [Output("Output")]
        public ISpread<KeyboardState> FOutput;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = FKeyIn.CombineWith(FTimeIn);
            for (int i = 0; i < FOutput.SliceCount; i++)
            {
            	FOutput[i] = KeyboardStateNodes.Join(FKeyIn[i], FCapsIn[0], FTimeIn[i]);
            }
        }
    }
}
