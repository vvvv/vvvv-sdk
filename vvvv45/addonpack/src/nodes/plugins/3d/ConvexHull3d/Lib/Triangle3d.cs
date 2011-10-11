using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.Lib
{
    public class Triangle3d : Object3d
    {
        public Point3d[] tri;
        private HalfSpace h;
        protected Point3d centre;
        internal List<Point3d> pts = new List<Point3d>();


        public bool add(Point3d p)
        {
            if (inside(p))
            {
                pts.Add(p);
                return true;
            }
            else
            {
                return false;
            }
        }


        /** Create a triangle with given colour
         */
        public Triangle3d(Point3d[] tri)
        {
            this.tri = tri;
            computeHalfSpace();
        }

        public Triangle3d(Point3d a, Point3d b, Point3d c)
        {
            tri = new Point3d[3];
            tri[0] = a; tri[1] = b; tri[2] = c;
            computeHalfSpace();
        }

        
        private void computeHalfSpace()
        {
            h = new HalfSpace(tri[0], tri[1], tri[2]);
            centre = tri[0].add(tri[1]).add(tri[2]).scale(1.0 / 3.0);
        }


        public bool inside(Point3d x)
        {
            return h.inside(x);
        }

        public Point3d extreme()
        {
            Point3d res = null;
            double maxd = Double.MinValue;
            for (int i = 0; i < pts.Count; i++)
            {
                double d = h.normal.dot(pts[i]);
                if (d > maxd)
                {
                    res = pts[i];
                    maxd = d;
                }
            }
            return res;
        }


    }
}
