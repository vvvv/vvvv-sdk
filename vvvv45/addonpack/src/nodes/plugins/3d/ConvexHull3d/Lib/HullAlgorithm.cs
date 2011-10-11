using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.Lib
{
    public abstract class HullAlgorithm
    {
        protected Point3d[] pts;

        public HullAlgorithm(Point3d[] pts)
        {
            this.pts = pts;
        }

        public abstract List<Triangle3d> build();
        //public abstract List<Object3d> build2D();

    }
}
