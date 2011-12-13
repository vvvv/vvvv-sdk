using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.Lib
{
    public class Edge3d : Object3d
    {
        internal Point3d start, end; //end points
        protected Point3d centre;

        public Edge3d(Point3d start, Point3d end)
        {
            this.start = start;
            this.end = end;
            centre = start;
        }

        public override bool Equals(Object o)
        {
            if (o is Edge3d)
            {
                Edge3d e = (Edge3d)o;
                return (start.Equals(e.end) && end.Equals(e.start) ||
              (end.Equals(e.end) && start.Equals(e.start)));
            }
            else
            {
                return false;
            }
        }

        public bool inside(Point3d x)
        {
            HalfSpace h = new HalfSpace(start, end);
            return h.inside(x);
        }

    }
}
