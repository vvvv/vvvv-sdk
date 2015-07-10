using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.Lib
{
    public class HalfSpace
    {

        /*final*/
        internal Point3d normal; // normal to boundary plane
        /*final*/
        double d; // eqn of half space is normal.x - d > 0

        /** Create a half space
         */
        public HalfSpace(Point3d a, Point3d b, Point3d c)
        {
            normal = b.subtract(a).cross(c.subtract(a)).normalize();
            d = normal.dot(a);
        }

        /** Create a half space parallel to z axis
         */
        public HalfSpace(Point3d a, Point3d b)
        {
            normal = b.subtract(a).cross(Point3d.k).normalize();
            d = normal.dot(a);
        }

        public bool inside(Point3d x)
        {
            return normal.dot(x) > d;
        }

        /** z coordinate of intersection of a vertical line through p and boundary plane */
        public double zint(Point3d p)
        {
            return (d - normal.x() * p.x() - normal.y() * p.y()) / normal.z();
        }

    }

}
