using System;
using VVVV.Core;
using VVVV.Utils.IO;

namespace VVVV.Nodes.Input
{
    public struct MouseStateNodes
    {
        public double X;
        public double Y;
        public MouseButton Button;
        public int MouseWheel;
        
        public MouseStateNodes(double x, double y, MouseButton button, int mouseWheel)
        {
            X = x;
            Y = y;
            Button = button;
            MouseWheel = mouseWheel;
        }
        
        [Node(Name = "MouseState", Category = "Join")]
        public static MouseState Join(
            double x,
            double y,
            bool leftButton,
            bool middleButton,
            bool rightButton,
            int mouseWheel)
        {
            var button = MouseButton.None;
            if (leftButton)
                button |= MouseButton.Left;
            if (middleButton)
                button |= MouseButton.Middle;
            if (rightButton)
                button |= MouseButton.Right;
            return new MouseState(x, y, button, mouseWheel);
        }

        [Node(Name = "MouseState", Category = "Split")]
        public static void Split(
            MouseState mouseState,
            out double x,
            out double y,
            out bool leftButton,
            out bool middleButton,
            out bool rightButton,
            out int mouseWheel
           )
        {
            x = mouseState.X;
            y = mouseState.Y;
            leftButton = (mouseState.Button & MouseButton.Left) > 0;
            middleButton = (mouseState.Button & MouseButton.Middle) > 0;
            rightButton = (mouseState.Button & MouseButton.Right) > 0;
            mouseWheel = mouseState.MouseWheel;
        }
    }
}
