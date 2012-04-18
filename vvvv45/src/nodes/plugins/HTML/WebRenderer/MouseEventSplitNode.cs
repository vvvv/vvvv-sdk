using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.HTML
{
    [PluginInfo(Name = "MouseEvent", Category = "HTML", Version = "Split")]
    public class MouseEventSplitNode : IPluginEvaluate
    {
        [Input("Input")]
        public ISpread<MouseEvent> FInput;

        [Output("X")]
        public ISpread<double> FXOut;
        [Output("Y")]
        public ISpread<double> FYOut;
        [Output("Left Button")]
        public ISpread<bool> FLeftButtonOut;
        [Output("Middle Button")]
        public ISpread<bool> FMiddleButtonOut;
        [Output("Right Button")]
        public ISpread<bool> FRightButtonOut;
        [Output("Mouse Up")]
        public ISpread<bool> FMouseUpOut;
        [Output("Click Count")]
        public ISpread<int> FClickCountOut;

        public void Evaluate(int spreadMax)
        {
            FXOut.SliceCount = spreadMax;
            FYOut.SliceCount = spreadMax;
            FLeftButtonOut.SliceCount = spreadMax;
            FMiddleButtonOut.SliceCount = spreadMax;
            FRightButtonOut.SliceCount = spreadMax;
            FMouseUpOut.SliceCount = spreadMax;
            FClickCountOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var mouseEvent = FInput[i];
                double x, y;
                bool leftButton, middleButton, rightButton, mouseUp;
                int clickCount;
                MouseEvent.Split(mouseEvent, out x, out y, out leftButton, out middleButton, out rightButton, out mouseUp, out clickCount);
                FXOut[i] = x;
                FYOut[i] = y;
                FLeftButtonOut[i] = leftButton;
                FMiddleButtonOut[i] = middleButton;
                FRightButtonOut[i] = rightButton;
                FMouseUpOut[i] = mouseUp;
                FClickCountOut[i] = clickCount;
            }
        }
    }
}
