using System;
using VVVV.Utils.VMath;

namespace VVVV.Utils.IO
{
    [Flags]
    public enum MouseButton
    {
        None = 0,
        Left = 1,
        Middle = 2,
        Right = 4
    }

    /// <summary>
    /// Encapsulates the state of a mouse.
    /// </summary>
    public struct MouseState : IEquatable<MouseState>
    {
        /// <summary>
        /// The x coordinate of the mouse.
        /// </summary>
        public readonly double X;

        /// <summary>
        /// The y coordinate of the mouse.
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// The pressed mouse button.
        /// </summary>
        public readonly MouseButton Button;

        /// <summary>
        /// The position of the mouse wheel.
        /// </summary>
        public readonly int MouseWheel;
        
        public MouseState(double x, double y, MouseButton button, int mouseWheel)
        {
            X = x;
            Y = y;
            Button = button;
            MouseWheel = mouseWheel;
        }
        
        public Vector2D Position
        {
        	get
        	{
        		return new Vector2D(X, Y);
        	}
        }
        
        public bool IsLeft
        {
        	get
        	{
        		return (Button & MouseButton.Left) > 0;
        	}
        }
        
        public bool IsMiddle
        {
        	get
        	{
        		return (Button & MouseButton.Middle) > 0;
        	}
        }
        
        public bool IsRight
        {
        	get
        	{
        		return (Button & MouseButton.Right) > 0;
        	}
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
            return this.X == other.X && this.Y == other.Y && this.Button == other.Button && this.MouseWheel == other.MouseWheel;
        }
        
        public override int GetHashCode()
        {
            // combine the hash codes of all members here (e.g. with XOR operator ^)
            return X.GetHashCode() ^ Y.GetHashCode() ^ Button.GetHashCode() ^ MouseWheel.GetHashCode();
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