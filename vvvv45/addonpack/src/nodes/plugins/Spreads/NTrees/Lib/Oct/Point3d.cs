using System;
using System.Collections.Generic;
using System.Text;
using NTrees.Lib.Base;

namespace NTrees.Lib.Oct
{
    public class Point3d : IElement<Point3d>
    {
        public double x;
        public double y;
        public double z;

        public Point3d(Point3d point)
        {
            this.x = point.x;
            this.y = point.y;
            this.z = point.z;
        }

        public Point3d(double x, double y,double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool IsEquals(Point3d element)
        {
            return this.x == element.x && this.y == element.y && this.z == element.z;
        }

        public Point3d Clone()
        {
            return new Point3d(this);
        }
    }
}
