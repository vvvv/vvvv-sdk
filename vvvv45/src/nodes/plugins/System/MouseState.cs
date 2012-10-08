using System;
using VVVV.Core;

namespace VVVV.Nodes.IO
{
    public enum MouseButton
    {
        None,
        Left,
        Middle,
        Right
    }

    public struct MouseState : IEquatable<MouseState>
    {
        public double X;
        public double Y;
        public MouseButton Button;
        public int MouseWheelDelta;
        
        public MouseState(double x, double y, MouseButton button, int mouseWheelDelta)
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
        
        #region Equals and GetHashCode implementation
        // The code in this region is useful if you want to use this structure in collections.
        // If you don't need it, you can just remove the region and the ": IEquatable<MouseEvent>" declaration.
        
        public override bool Equals(object obj)
        {
            if (obj is MouseState)
                return Equals((MouseState)obj); // use Equals method below
            else
                return false;
        }
        
        public bool Equals(MouseState other)
        {
            // add comparisions for all members here
            return this.X == other.X && this.Y == other.Y && this.Button == other.Button && this.MouseWheelDelta == other.MouseWheelDelta;
        }
        
        public override int GetHashCode()
        {
            // combine the hash codes of all members here (e.g. with XOR operator ^)
            return X.GetHashCode() ^ Y.GetHashCode() ^ Button.GetHashCode() ^ MouseWheelDelta.GetHashCode();
        }
        
        public static bool operator ==(MouseState left, MouseState right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(MouseState left, MouseState right)
        {
            return !left.Equals(right);
        }
        #endregion
    }
}
