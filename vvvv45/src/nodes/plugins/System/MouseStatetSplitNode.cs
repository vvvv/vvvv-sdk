using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.IO
{
    [PluginInfo(Name = "MouseState", Category = "System", Version = "Split")]
    public class MouseStatetSplitNode : IPluginEvaluate
    {
        [Input("Input")]
        public ISpread<MouseState> FInput;
        [Output("X")]
        public ISpread<double> FXOut;
        [Output("Y")]
        public ISpread<double> FYOut;
        [Output("Mouse Wheel Delta")]
        public ISpread<int> FMouseWheelDeltaOut;
        [Output("Left Button")]
        public ISpread<bool> FLeftButtonOut;
        [Output("Middle Button")]
        public ISpread<bool> FMiddleButtonOut;
        [Output("Right Button")]
        public ISpread<bool> FRightButtonOut;

        public void Evaluate(int spreadMax)
        {
            FXOut.SliceCount = spreadMax;
            FYOut.SliceCount = spreadMax;
            FLeftButtonOut.SliceCount = spreadMax;
            FMiddleButtonOut.SliceCount = spreadMax;
            FRightButtonOut.SliceCount = spreadMax;
            FMouseWheelDeltaOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var mouseEvent = FInput[i];
                double x, y;
                bool leftButton, middleButton, rightButton;
                int mouseWheelDelta;
                MouseState.Split(mouseEvent, out x, out y, out leftButton, out middleButton, out rightButton, out mouseWheelDelta);
                FXOut[i] = x;
                FYOut[i] = y;
                FLeftButtonOut[i] = leftButton;
                FMiddleButtonOut[i] = middleButton;
                FRightButtonOut[i] = rightButton;
                FMouseWheelDeltaOut[i] = mouseWheelDelta;
            }
        }
    }
}
