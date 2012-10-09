using System;
using System.Collections.Generic;
using System.Text;
using NTrees.Lib.Base;

namespace NTrees.Lib.Quad
{
    public class Point2d : IElement<Point2d>
    {
        public double x;
        public double y;

        public Point2d(Point2d point)
        {
            this.x = point.x;
            this.y = point.y;
        }

        public Point2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public bool IsEquals(Point2d element)
        {
            return this.x == element.x && this.y == element.y;
        }

        public Point2d Clone()
        {
            return new Point2d(this);
        }
    }
}
