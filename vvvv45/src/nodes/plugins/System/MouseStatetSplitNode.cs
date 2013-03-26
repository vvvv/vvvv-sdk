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
        [Output("X Button 1")]
        public ISpread<bool> FXButton1Out;
        [Output("X Button 2")]
        public ISpread<bool> FXButton2Out;

        public void Evaluate(int spreadMax)
        {
            FXOut.SliceCount = spreadMax;
            FYOut.SliceCount = spreadMax;
            FMouseWheelOut.SliceCount = spreadMax;
            FLeftButtonOut.SliceCount = spreadMax;
            FMiddleButtonOut.SliceCount = spreadMax;
            FRightButtonOut.SliceCount = spreadMax;
            FXButton1Out.SliceCount = spreadMax;
            FXButton2Out.SliceCount = spreadMax;

            for (int i = 0; i < spreadMax; i++)
            {
                var mouseState = FInput[i];
                FXOut[i] = mouseState.X;
                FYOut[i] = mouseState.Y;
                FLeftButtonOut[i] = mouseState.IsLeft;
                FMiddleButtonOut[i] = mouseState.IsMiddle;
                FRightButtonOut[i] = mouseState.IsRight;
                FXButton1Out[i] = mouseState.IsXButton1;
                FXButton2Out[i] = mouseState.IsXButton2;
                FMouseWheelOut[i] = mouseState.MouseWheel;
            }
        }
    }
}
