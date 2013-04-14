using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "KeyboardState", Category = "System", Version = "Join")]
    public class KeyStateJoinNode : IPluginEvaluate
    {
        [Input("Key Code")]
        public IDiffSpread<ISpread<int>> FKeyIn;

        [Input("Caps Lock", IsSingle = true)]
        public IDiffSpread<bool> FCapsIn;

        [Input("Time")]
        public IDiffSpread<int> FTimeIn;
        
        [Output("Keyboard")]
        public ISpread<KeyboardState> FOutput;

        public void Evaluate(int spreadMax)
        {
            if (!FKeyIn.IsChanged && !FCapsIn.IsChanged && !FTimeIn.IsChanged) return;
            FOutput.SliceCount = FKeyIn.CombineWith(FTimeIn);
            for (int i = 0; i < FOutput.SliceCount; i++)
            {
            	FOutput[i] = KeyboardStateNodes.Join(FKeyIn[i], FCapsIn[0], FTimeIn[i]);
            }
        }
    }
}
