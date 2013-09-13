// -----------------------------------------------------------------------
// <copyright file="Utils.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Kinect.Toolkit.FaceTracking
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Indices for Animation Units Coefficients
    /// </summary>
    public enum AnimationUnit
    {
        LipRaiser,
        JawLower,
        LipStretcher,
        BrowLower,
        LipCornerDepressor,
        BrowRaiser
    }

    /// <summary>
    /// Indices for feature points in 3D shape and projected 3D shape with
    /// descriptive names to identify them easily
    /// </summary>
    public enum FeaturePoint
    {
        TopSkull                        = 0,
        TopRightForehead                = 1,
        MiddleTopDipUpperLip            = 7,
        AboveChin                       = 9,
        BottomOfChin                    = 10,
        RightOfRightEyebrow             = 15,
        MiddleTopOfRightEyebrow         = 16,
        LeftOfRightEyebrow              = 17,
        MiddleBottomOfRightEyebrow      = 18,
        AboveMidUpperRightEyelid        = 19,
        OuterCornerOfRightEye           = 20,
        MiddleTopRightEyelid            = 21,
        MiddleBottomRightEyelid         = 22,
        InnerCornerRightEye             = 23,
        UnderMidBottomRightEyelid       = 24,
        RightSideOfChin                 = 30,
        OutsideRightCornerMouth         = 31,
        RightOfChin                     = 32,
        RightTopDipUpperLip             = 33,
        TopLeftForehead                 = 34,
        MiddleTopLowerLip               = 40,
        MiddleBottomLowerLip            = 41,
        LeftOfLeftEyebrow               = 48,
        MiddleTopOfLeftEyebrow          = 49,
        RightOfLeftEyebrow              = 50,
        MiddleBottomOfLeftEyebrow       = 51, 
        AboveMidUpperLeftEyelid         = 52, 
        OuterCornerOfLeftEye            = 53,
        MiddleTopLeftEyelid             = 54,
        MiddleBottomLeftEyelid          = 55,
        InnerCornerLeftEye              = 56,
        UnderMidBottomLeftEyelid        = 57, 
        LeftSideOfCheek                 = 63,
        OutsideLeftCornerMouth          = 64,
        LeftOfChin                      = 65,
        LeftTopDipUpperLip              = 66,
        OuterTopRightPupil              = 67,
        OuterBottomRightPupil           = 68,
        OuterTopLeftPupil               = 69,
        OuterBottomLeftPupil            = 70,
        InnerTopRightPupil              = 71,
        InnerBottomRightPupil           = 72, 
        InnerTopLeftPupil               = 73,
        InnerBottomLeftPupil            = 74,
        RightTopUpperLip                = 79,
        LeftTopUpperLip                 = 80,
        RightBottomUpperLip             = 81,
        LeftBottomUpperLip              = 82,
        RightTopLowerLip                = 83,
        LeftTopLowerLip                 = 84,
        RightBottomLowerLip             = 85, 
        LeftBottomLowerLip              = 86, 
        MiddleBottomUpperLip            = 87,
        LeftCornerMouth                 = 88,
        RightCornerMouth                = 89,
        BottomOfRightCheek              = 90,
        BottomOfLeftCheek               = 91,
        AboveThreeFourthRightEyelid     = 95,
        AboveThreeFourthLeftEyelid      = 96,
        ThreeFourthTopRightEyelid       = 97,
        ThreeFourthTopLeftEyelid        = 98,
        ThreeFourthBottomRightEyelid    = 99,
        ThreeFourthBottomLeftEyelid     = 100,
        BelowThreeFourthRightEyelid     = 101,
        BelowThreeFourthLeftEyelid      = 102,
        AboveOneFourthRightEyelid       = 103,
        AboveOneFourthLeftEyelid        = 104,
        OneFourthTopRightEyelid         = 105,
        OneFourthTopLeftEyelid          = 106,
        OneFourthBottomRightEyelid      = 107,
        OneFourthBottomLeftEyelid       = 108
    }

    /// <summary>
    /// Represents a point in 2D space with floating point x & y co-ordinates
    /// </summary>
    [DebuggerDisplay("({x},{y})")]
    [StructLayout(LayoutKind.Sequential)]
    public struct PointF
    {
        private readonly float x;
        private readonly float y;

        public PointF(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Gets an point with 0,0 co-ordinates
        /// </summary>
        public static PointF Empty
        {
            get { return new PointF(0, 0); }
        }

        /// <summary>
        /// Returns X co-ordinate
        /// </summary>
        public float X
        {
            get { return x; }
        }

        /// <summary>
        /// Returns Y co-ordinate
        /// </summary>
        public float Y
        {
            get { return y; }
        }

        public static bool operator ==(PointF point1, PointF point2)
        {
            return point1.Equals(point2);
        }

        public static bool operator !=(PointF point1, PointF point2)
        {
            return !point1.Equals(point2);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PointF))
            {
                return false;
            }

            return Equals((PointF)obj);
        }

        public bool Equals(PointF other)
        {
            if (x != other.x)
            {
                return false;
            }

            return y == other.y;
        }
    }

    /// <summary>
    /// Represents a point in 2D space with integer x & y co-ordinates
    /// </summary>
    [DebuggerDisplay("({X},{Y})")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public Point(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets an point with 0,0 co-ordinates
        /// </summary>
        public static Point Empty
        {
            get { return new Point(0, 0); }
        }

        /// <summary>
        /// Returns X co-ordinate
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Returns Y co-ordinate
        /// </summary>
        public int Y { get; private set; }

        public static bool operator ==(Point point1, Point point2)
        {
            return point1.Equals(point2);
        }

        public static bool operator !=(Point point1, Point point2)
        {
            return !point1.Equals(point2);
        }   
 
        public override int GetHashCode()
        {
            return X ^ Y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
            {
                return false;
            }

            return Equals((Point)obj);
        }

        public bool Equals(Point other)
        {
            if (X != other.X)
            {
                return false;
            }

            return Y == other.Y;
        }
    }

    /// <summary>
    /// Represents a rectangle in 2D space
    /// </summary>
    [DebuggerDisplay("(l={Left},t={Top},r={Right},b={Bottom})")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public Rect(int left, int top, int right, int bottom) : this()
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Gets an rectangle with 0 length & height
        /// </summary>
        public static Rect Empty
        {
            get { return new Rect(0, 0, 0, 0); }
        }

        /// <summary>
        /// Left bound of the rectangle
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Top bound of the rectangle
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// Right bound of the rectangle
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// Bottom bound of the rectangle
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// Width of the rectangle
        /// </summary>
        public int Width
        {
            get { return Right - Left; }
        }

        /// <summary>
        /// Height of the rectangle
        /// </summary>
        public int Height
        {
            get { return Bottom - Top; }
        }

        public static bool operator ==(Rect point1, Rect point2)
        {
            return point1.Equals(point2);
        }

        public static bool operator !=(Rect point1, Rect point2)
        {
            return !point1.Equals(point2);
        }   

        public override int GetHashCode()
        {
            return Left ^ Right ^ Top ^ Bottom;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Rect))
            {
                return false;
            }

            return Equals((Rect)obj);
        }

        public bool Equals(Rect other)
        {
            if (Left != other.Left)
            {
                return false;
            }

            if (Top != other.Top)
            {
                return false;
            }

            if (Right != other.Right)
            {
                return false;
            }

            return Bottom == other.Bottom;
        }
    }

    /// <summary>
    /// Represents a 3D vector with x,y,z elements
    /// </summary>
    [DebuggerDisplay("({X},{Y},{Z})")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3DF
    {
        public Vector3DF(float x, float y, float z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3DF Empty
        {
            get
            {
                return new Vector3DF(0.0f, 0.0f, 0.0f);
            }
        }    

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }  
  
        public static bool operator ==(Vector3DF vector1, Vector3DF vector2)
        {
            return vector1.Equals(vector2);
        }

        public static bool operator !=(Vector3DF vector1, Vector3DF vector2)
        {
            return !vector1.Equals(vector2);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3DF))
            {
                return false;
            }

            return Equals((Vector3DF)obj);
        }

        public bool Equals(Vector3DF other)
        {
            if (X != other.X)
            {
                return false;
            }

            if (Y != other.Y)
            {
                return false;
            }

            return Z == other.Z;
        }
    }

    /// <summary>
    /// Represents a mesh triangle with i,j,k vertex indexes
    /// </summary>
    [DebuggerDisplay("({first},{second},{third})")]
    [StructLayout(LayoutKind.Sequential)]
    public struct FaceTriangle
    {
        private int first;  // index of first vertex
        private int second;  // index of second vertex
        private int third;  // index of third vertex

        public FaceTriangle(int first, int second, int third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }

        /// <summary>
        /// Index of the first vertex
        /// </summary>
        public int First
        {
            get { return this.first; }
            set { this.first = value; }
        }

        /// <summary>
        /// Index of the second vertex
        /// </summary>
        public int Second
        {
            get { return this.second; }
            set { this.second = value; }
        }

        /// <summary>
        /// Index of the third vertex
        /// </summary>
        public int Third
        {
            get { return this.third; }
            set { this.third = value; }
        }

        public static bool operator ==(FaceTriangle triangle1, FaceTriangle triangle2)
        {
            return triangle1.Equals(triangle2);
        }

        public static bool operator !=(FaceTriangle triangle1, FaceTriangle triangle2)
        {
            return !triangle1.Equals(triangle2);
        }

        public override int GetHashCode()
        {
            return first ^ second ^ third;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FaceTriangle))
            {
                return false;
            }

            return Equals((FaceTriangle)obj);  
        }

        public bool Equals(FaceTriangle other)
        {
            return this.first == other.first && this.second == other.second && this.third == other.third;
        }
    }
}
