using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes.IO
{
    [PluginInfo(Name = "MouseState", Category = "System", Version = "Join")]
    public class MouseStateJoinNode: IPluginEvaluate
    {
        [Input("X")]
        public ISpread<double> FXIn;
        [Input("Y")]
        public ISpread<double> FYIn;
        [Input("Mouse Wheel")]
        public ISpread<int> FMouseWheelIn;
        [Input("Left Button")]
        public ISpread<bool> FLeftButtonIn;
        [Input("Middle Button")]
        public ISpread<bool> FMiddleButtonIn;
        [Input("Right Button")]
        public ISpread<bool> FRightButtonIn;
        [Output("Mouse")]
        public ISpread<MouseState> FOutput;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = spreadMax;
            for (int i = 0; i < spreadMax; i++)
            {
                FOutput[i] = MouseStateNodes.Join(
                    FXIn[i], 
                    FYIn[i], 
                    FLeftButtonIn[i], 
                    FMiddleButtonIn[i], 
                    FRightButtonIn[i],  
                    FMouseWheelIn[i]
                   );
            }
        }
    }
}
