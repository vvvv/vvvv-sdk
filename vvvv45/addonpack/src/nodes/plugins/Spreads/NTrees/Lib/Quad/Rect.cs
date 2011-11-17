using System;
using System.Collections.Generic;
using System.Text;
using NTrees.Lib.Base;

namespace NTrees.Lib.Quad
{
    public class Rect : IBounds<Point2d>, ISpliteable<Rect>
    {
        #region Fields
        private double top;
        private double bottom;
        private double left;
        private double right;
        #endregion

        #region Constructor
        public Rect(double top, double bottom, double left, double right)
        {
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
            this.SwapBounds();
        }
        #endregion

        #region Properties
        public double Top
        {
            get { return top; }
            set { top = value; }
        }

        public double Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }

        public double Left
        {
            get { return left; }
            set { left = value; }
        }

        public double Right
        {
            get { return right; }
            set { right = value; }
        }
        #endregion

        #region Calculated Properties
        public bool IsInside(Point2d point)
        {
            return (point.x >= left && point.x < right && point.y >= bottom && point.y < top);
        }

        public Point2d Center
        {
            get
            {
                return new Point2d(this.right - (right - left) / 2.0, this.top - (top - bottom) / 2.0);
            }
        }

        public double Area
        {
            get { return this.Width * this.Height; }
        }

        public double Width
        {
            get { return this.right - this.left; }
        }

        public double Height
        {
            get { return this.top - this.bottom; }
        }

        public Edge2d[] Edges
        {
            get
            {
                Edge2d[] result = new Edge2d[4]; // Four edges per quad
                result[0] = new Edge2d(left, top, left, bottom); //left - top to bottom
                result[1] = new Edge2d(right, top, right, bottom); //right - top to bottom
                result[2] = new Edge2d(left, top, right, top); //top - left to right
                result[3] = new Edge2d(left, bottom, right, bottom); //bottom - left to right

                return result;
            }
        }
        #endregion

        #region Split
        public Rect[] Split()
        {
            Rect[] result = new Rect[4];
            //To avoid calculate 4 times, every little counts :)

            Point2d center = this.Center;

            result[0] = new Rect(top, center.y, left, center.x);
            result[1] = new Rect(center.y, bottom, left, center.x);
            result[2] = new Rect(top, center.y, center.x, right);
            result[3] = new Rect(center.y, bottom, center.x, right);

            return result;
        }
        #endregion

        #region Swap Bounds to make sure they fine
        private void SwapBounds()
        {
            #region Swap min/max just in case
            if (bottom > top)
            {
                double swap;
                swap = top;
                top = bottom;
                bottom = swap;
            }

            if (left > right)
            {
                double swap;
                swap = left;
                left = right;
                right = swap;
            }
            #endregion
        }
        #endregion

        #region Zero (Invalid bounds)
        public bool Zero
        {
            get
            {
                return (this.top == this.bottom) || (this.left == this.right);
            }
        }
        #endregion
    }
}
