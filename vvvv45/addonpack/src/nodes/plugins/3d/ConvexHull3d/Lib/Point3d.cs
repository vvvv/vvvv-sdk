using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.Lib
{
    public class Point3d : Object3d
    {
        public double[] v;
        public static Point3d o = new Point3d(0, 0, 0);
        public static Point3d i = new Point3d(1, 0, 0);
        public static Point3d j = new Point3d(0, 1, 0);
        public static Point3d k = new Point3d(0, 0, 1);
        public static Point3d ijk = new Point3d(1, 1, 1);

        public Point3d()
        {
            this.v = new double[3];
        }

        public Point3d(double x, double y, double z)
        {
            this.v = new double[3];
            this.v[0] = x;
            this.v[1] = y;
            this.v[2] = z;
        }

        public double x()
        {
            return v[0];
        }

        public double y()
        {
            return v[1];
        }

        public double z()
        {
            return v[2];
        }

        public double theta()
        {
            return Math.Atan2(v[0], v[2]);
        }

        public double r()
        {
            return Math.Sqrt(v[0] * v[0] + v[2] * v[2]);
        }



        public Point3d add(Point3d x)
        {
            Point3d a = new Point3d();
            for (int i = 0; i < 3; i++)
            {
                a.v[i] = v[i] + x.v[i];
            }
            return a;
        }

        public Point3d subtract(Point3d x)
        {
            Point3d a = new Point3d();
            for (int i = 0; i < 3; i++)
            {
                a.v[i] = v[i] - x.v[i];
            }
            return a;
        }

        public Point3d scale(double x)
        {
            Point3d a = new Point3d();
            for (int i = 0; i < 3; i++)
            {
                a.v[i] = x * v[i];
            }
            return a;
        }

        public Point3d scale(double x, double y, double z)
        {
            return new Point3d(x * v[0], y * v[1], z * v[2]);
        }

        public double dot(Point3d x)
        {
            double d = 0;
            for (int i = 0; i < 3; i++)
            {
                d += v[i] * x.v[i];
            }
            return d;
        }

        public Point3d normalize()
        {
            return scale(1 / length());
        }

        public double length()
        {
            return Math.Sqrt(dot(this));
        }

        public Point3d cross(Point3d x)
        {
            return new Point3d(v[1] * x.v[2] - x.v[1] * v[2],
                       v[2] * x.v[0] - x.v[2] * v[0],
                       v[0] * x.v[1] - x.v[0] * v[1]);
        }

        public override bool Equals(object obj)
        {
            if (obj is Point3d)
            {
                Point3d ptest = (Point3d)obj;
                return ptest.x() == this.x() && ptest.y() == this.y() && ptest.z() == this.z();
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            string res = "X: " + this.x() + "  Y: " + this.y() + "  Z: " + this.z();
            return res;
        }


    }
}
