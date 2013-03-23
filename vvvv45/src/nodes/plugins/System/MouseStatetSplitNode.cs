using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "MouseState", Category = "System", Version = "Split")]
    public class MouseStatetSplitNode : IPluginEvaluate
    {
        [Input("Mouse")]
        public ISpread<MouseState> FInput;
        [Output("X")]
        public ISpread<double> FXOut;
        [Output("Y")]
        public ISpread<double> FYOut;
        [Output("Mouse Wheel")]
        public ISpread<int> FMouseWheelOut;
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
            FMouseWheelOut.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var mouseState = FInput[i];
                double x, y;
                bool leftButton, middleButton, rightButton;
                int mouseWheel;

                if (mouseState != null)
                    MouseStateNodes.Split(mouseState, out x, out y, out leftButton, out middleButton, out rightButton, out mouseWheel);
                else
                {
                    x = 0;
                    y = 0;
                    leftButton = false;
                    middleButton = false;
                    rightButton = false;
                    mouseWheel = 0;
                }

                FXOut[i] = x;
                FYOut[i] = y;
                FLeftButtonOut[i] = leftButton;
                FMiddleButtonOut[i] = middleButton;
                FRightButtonOut[i] = rightButton;
                FMouseWheelOut[i] = mouseWheel;
            }
        }
    }
}
