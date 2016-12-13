using System;
using VVVV.Utils.VMath;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Utils.IO
{
    /// <summary>
    /// Encapsulates the state of a mouse.
    /// </summary>
    public struct MouseState : IEquatable<MouseState>
    {
        public static MouseState Create(double x, double y, bool left, bool middle, bool right, bool xButton1, bool xButton2, int mouseWheel)
        {
            var buttons = MouseButtons.None;
            if (left)
                buttons |= MouseButtons.Left;
            if (middle)
                buttons |= MouseButtons.Middle;
            if (right)
                buttons |= MouseButtons.Right;
            if (xButton1)
                buttons |= MouseButtons.XButton1;
            if (xButton2)
                buttons |= MouseButtons.XButton2;
            return new MouseState(x, y, buttons, mouseWheel);
        }
        public static MouseState Create(double x, double y, bool left, bool middle, bool right, bool xButton1, bool xButton2, int mouseWheel, int mouseHWheel)
        {
            var buttons = MouseButtons.None;
            if (left)
                buttons |= MouseButtons.Left;
            if (middle)
                buttons |= MouseButtons.Middle;
            if (right)
                buttons |= MouseButtons.Right;
            if (xButton1)
                buttons |= MouseButtons.XButton1;
            if (xButton2)
                buttons |= MouseButtons.XButton2;
            return new MouseState(x, y, buttons, mouseWheel, mouseHWheel);
        }

        /// <summary>
        /// The x coordinate of the mouse.
        /// </summary>
        public double X;

        /// <summary>
        /// The y coordinate of the mouse.
        /// </summary>
        public double Y;

        /// <summary>
        /// The pressed mouse button.
        /// </summary>
        public MouseButtons Buttons;

        /// <summary>
        /// The position of the mouse wheel.
        /// </summary>
        public int MouseWheel;

        /// <summary>
        /// The position of the horizontal mouse wheel.
        /// </summary>
        public int MouseHorizontalWheel;

        public MouseState(double x, double y, MouseButtons buttons, int mouseWheel)
        {
            X = x;
            Y = y;
            Buttons = buttons;
            MouseWheel = mouseWheel;
            MouseHorizontalWheel = 0;
        }
        public MouseState(double x, double y, MouseButtons buttons, int mouseWheel, int mouseHWheel)
        {
            X = x;
            Y = y;
            Buttons = buttons;
            MouseWheel = mouseWheel;
            MouseHorizontalWheel = mouseHWheel;
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
        		return (Buttons & MouseButtons.Left) > 0;
        	}
        }
        
        public bool IsMiddle
        {
        	get
        	{
        		return (Buttons & MouseButtons.Middle) > 0;
        	}
        }
        
        public bool IsRight
        {
        	get
        	{
        		return (Buttons & MouseButtons.Right) > 0;
        	}
        }

        public bool IsXButton1
        {
            get
            {
                return (Buttons & MouseButtons.XButton1) > 0;
            }
        }

        public bool IsXButton2
        {
            get
            {
                return (Buttons & MouseButtons.XButton2) > 0;
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
            return this.X == other.X && this.Y == other.Y &&
                this.Buttons == other.Buttons &&
                this.MouseWheel == other.MouseWheel &&
                this.MouseHorizontalWheel == other.MouseHorizontalWheel;
        }
        
        public override int GetHashCode()
        {
            // combine the hash codes of all members here (e.g. with XOR operator ^)
            return X.GetHashCode() ^ Y.GetHashCode() ^ Buttons.GetHashCode() ^ MouseWheel.GetHashCode() ^ MouseHorizontalWheel.GetHashCode();
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