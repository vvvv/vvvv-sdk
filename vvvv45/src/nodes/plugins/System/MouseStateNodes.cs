using System;
using VVVV.Core;
using VVVV.Utils.IO;

namespace VVVV.Nodes.IO
{
    public struct MouseStateNodes
    {
        public double X;
        public double Y;
        public MouseButton Button;
        public int MouseWheelDelta;
        
        public MouseStateNodes(double x, double y, MouseButton button, int mouseWheelDelta)
        {
            X = x;
            Y = y;
            Button = button;
            MouseWheelDelta = mouseWheelDelta;
        }
        
        [Node(Name = "MouseState", Category = "Join")]
        public static MouseState Join(
            double x,
            double y,
            bool leftButton,
            bool middleButton,
            bool rightButton,
            int mouseWheelDelta)
        {
            var button = MouseButton.None;
            if (leftButton)
                button = MouseButton.Left;
            else if (middleButton)
                button = MouseButton.Middle;
            else if (rightButton)
                button = MouseButton.Right;
            return new MouseState(x, y, button, mouseWheelDelta);
        }

        [Node(Name = "MouseState", Category = "Split")]
        public static void Split(
            MouseState mouseEvent,
            out double x,
            out double y,
            out bool leftButton,
            out bool middleButton,
            out bool rightButton,
            out int mouseWheelDelta
           )
        {
            x = mouseEvent.X;
            y = mouseEvent.Y;
            leftButton = mouseEvent.Button == MouseButton.Left;
            middleButton = mouseEvent.Button == MouseButton.Middle;
            rightButton = mouseEvent.Button == MouseButton.Right;
            mouseWheelDelta = mouseEvent.MouseWheelDelta;
        }
    }
}
