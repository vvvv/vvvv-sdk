using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace VVVV.Utils.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        private int _Left;
        private int _Top;
        private int _Right;
        private int _Bottom;

        public RECT(RECT Rectangle)
            : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
        {
        }
        public RECT(int Left, int Top, int Right, int Bottom)
        {
            _Left = Left;
            _Top = Top;
            _Right = Right;
            _Bottom = Bottom;
        }

        public int X
        {
            get { return _Left; }
            set
            {
                _Right -= (_Left - value);
                _Left = value;
            }
        }
        public int Y
        {
            get { return _Top; }
            set
            {
                _Bottom -= (_Top - value);
                _Top = value;
            }
        }
        public int Left
        {
            get { return _Left; }
            set { _Left = value; }
        }
        public int Top
        {
            get { return _Top; }
            set { _Top = value; }
        }
        public int Right
        {
            get { return _Right; }
            set { _Right = value; }
        }
        public int Bottom
        {
            get { return _Bottom; }
            set { _Bottom = value; }
        }
        public int Height
        {
            get { return _Bottom - _Top; }
            set { _Bottom = value + _Top; }
        }
        public int Width
        {
            get { return _Right - _Left; }
            set { _Right = value + _Left; }
        }
        public Point Location
        {
            get { return new Point(Left, Top); }
            set
            {
                _Right -= (_Left - value.X);
                _Bottom -= (_Top - value.Y);
                _Left = value.X;
                _Top = value.Y;
            }
        }
        public Size Size
        {
            get { return new Size(Width, Height); }
            set
            {
                _Right = value.Width + _Left;
                _Bottom = value.Height + _Top;
            }
        }

        public static implicit operator Rectangle(RECT Rectangle)
        {
            return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
        }
        public static implicit operator RECT(Rectangle Rectangle)
        {
            return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
        }
        public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
        {
            return Rectangle1.Equals(Rectangle2);
        }
        public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
        {
            return !Rectangle1.Equals(Rectangle2);
        }

        public override string ToString()
        {
            return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public bool Equals(RECT Rectangle)
        {
            return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
        }

        public override bool Equals(object Object)
        {
            if (Object is RECT)
            {
                return Equals((RECT)Object);
            }
            else if (Object is Rectangle)
            {
                return Equals(new RECT((Rectangle)Object));
            }

            return false;
        }
    }

    public struct KeyInfo
    {
        IntPtr LParam;

        public KeyInfo(IntPtr lParam)
        {
            this.LParam = lParam;
        }

        public short RepeatCount
        {
            get { return (short)(LParam.ToInt32() & 0xFFFF); }
        }

        public byte ScanCode
        {
            get { return (byte)((LParam.ToInt32() & 0xFF0000) >> 16);}
        }

        public bool IsExtendedKey
        {
            get { return ((LParam.ToInt32() & 0x1000000) >> 24) > 0; }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TOUCHINPUT
    {
        public int x;
        public int y;
        public System.IntPtr hSource;
        public int dwID;
        public int dwFlags;
        public int dwMask;
        public int dwTime;
        public System.IntPtr dwExtraInfo;
        public int cxContact;
        public int cyContact;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GESTURECONFIG //https://msdn.microsoft.com/en-us/library/windows/desktop/dd353231%28v=vs.85%29.aspx
    {
        public int dwID;    // gesture ID
        public int dwWant;  // settings related to gesture ID that are to be turned on
        public int dwBlock; // settings related to gesture ID that are to be turned off
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GESTUREINFO
    {
        public int cbSize;          // The size of the structure, in bytes. The caller must set this to sizeof(GESTUREINFO).
        public int dwFlags;         // The state of the gesture
        public int dwID;            // The identifier of the gesture command
        public IntPtr hwndTarget;   // handle to window targeted by this gesture

        public short x;             // current location of this gesture in physical screen coordinates
        public short y;
        public int dwInstanceID;    // internally used
        public int dwSequenceID;    // internally used
        public Int64 ullArguments;  // arguments for gestures whose arguments fit in 8 BYTES
        public int cbExtraArgs;     // size, in bytes, of extra arguments, if any, that accompany this gesture
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GESTURENOTIFYSTRUCT
    {
        public int cbSize;          // The size of the structure
        public int dwFlags;         // Reserved for future use
        public IntPtr hwndTarget;   // The target window for the gesture notification
        public short x;             //The location of the gesture in physical screen coordinates
        public short y;             
        public int dwInstanceID;    // A specific gesture instance with gesture messages starting with GID_START and ending with GID_END
    }
}
