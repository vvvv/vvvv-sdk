using System;
using System.Collections.Generic;
using System.Text;
using NTrees.Lib.Base;

namespace NTrees.Lib.Oct
{
    public class Box : IBounds<Point3d>, ISpliteable<Box>
    {
        #region Fields
        private double top;
        private double bottom;
        private double left;
        private double right;
        private double front;
        private double back;
        #endregion

        #region Constructor
        public Box(double top, double bottom, double left, double right, double front, double back)
        {
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
            this.front = front;
            this.back = back;
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

        public double Front
        {
            get { return front; }
            set { front = value; }
        }

        public double Back
        {
            get { return back; }
            set { back = value; }
        }
        #endregion

        #region Calculated Properties
        public bool IsInside(Point3d point)
        {
            return (point.x >= left && point.x < right && point.y >= bottom && point.y < top && point.z < front && point.z >= back);
        }

        public Point3d Center
        {
            get
            {
                return new Point3d(this.right - (right - left) / 2.0, this.top - (top - bottom) / 2.0,this.front - (front - back) / 2.0 );
            }
        }

        public double Volume
        {
            get { return this.Width * this.Height * this.Depth; }
        }

        public double Width
        {
            get { return this.right - this.left; }
        }

        public double Height
        {
            get { return this.top - this.bottom; }
        }

        public double Depth
        {
            get { return this.front - this.back; }
        }
        #endregion

        #region Split
        public Box[] Split()
        {
            Box[] result = new Box[8];
            

            Point3d center = this.Center;
            result[0] = new Box(top, center.y, left, center.x, front, center.z); //top left front
            result[1] = new Box(top, center.y, center.x, right, front, center.z); //top right front
            result[2] = new Box(top, center.y, left, center.x, center.z, back);  //top left back
            result[3] = new Box(top, center.y, center.x, right, center.z, back);//top right back
            result[4] = new Box(center.y, bottom, left, center.x, front, center.z);//bottom left front
            result[5] = new Box(center.y, bottom, center.x, right, front, center.z);//bottom right front
            result[6] = new Box(center.y, bottom, left, center.x, center.z, back); //bottom left back
            result[7] = new Box(center.y, bottom, center.x, right, center.z, back);//bottom right back
            
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

            if (back > front)
            {
                double swap;
                swap = back;
                back = front;
                front = swap;
            }
            #endregion
        }
        #endregion

        #region Zero (Invalid bounds)
        public bool Zero
        {
            get
            {
                return (this.top == this.bottom) || (this.left == this.right) || (this.front == this.back);
            }
        }
        #endregion
    }
}
