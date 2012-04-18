using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.HTML
{
    [PluginInfo(Name = "MouseEvent", Category = "HTML", Version = "Join")]
    public class MouseEventJoinNode : IPluginEvaluate
    {
        [Input("X")]
        public ISpread<double> FXIn;
        [Input("Y")]
        public ISpread<double> FYIn;
        [Input("Left Button")]
        public ISpread<bool> FLeftButtonIn;
        [Input("Middle Button")]
        public ISpread<bool> FMiddleButtonIn;
        [Input("Right Button")]
        public ISpread<bool> FRightButtonIn;
        [Input("Mouse Up")]
        public ISpread<bool> FMouseUpIn;
        [Input("Click Count")]
        public ISpread<int> FClickCountIn;
        [Output("Output")]
        public ISpread<MouseEvent> FOutput;

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = spreadMax;
            for (int i = 0; i < spreadMax; i++)
            {
                FOutput[i] = MouseEvent.Join(FXIn[i], FYIn[i], FLeftButtonIn[i], FMiddleButtonIn[i], FRightButtonIn[i], FMouseUpIn[i], FClickCountIn[i]);
            }
        }
    }
}
