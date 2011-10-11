using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.Lib
{
    public class Incremental : HullAlgorithm
    {
        public Incremental(Point3d[] pts) : base(pts) {}

        public override List<Triangle3d> build()
        {
            EdgeStack es = new EdgeStack();
            List<Triangle3d> faces = new List<Triangle3d>();
            if (pts.Length < 2)
            {
                return faces;
            }
            faces.Add(new Triangle3d(pts[0], pts[1], pts[2]));
            faces.Add(new Triangle3d(pts[0], pts[2], pts[1]));
            /* now the main loop -- add vertices one at a time */
            for (int i = 3; i < pts.Length; i++)
            {
                /* delete faces that this vertex can see*/
                bool inside = true; //are we inside the hull?
                for (int j = 0; j < faces.Count; j++)
                {
                    Triangle3d t = faces[j];
                    if (t.inside(pts[i]))
                    {
                        inside = false;
                        /* update boundary of hole */
                        es.putp(t.tri[0], t.tri[1]);
                        es.putp(t.tri[1], t.tri[2]);
                        es.putp(t.tri[2], t.tri[0]);
                    }
                }
                if (inside) continue;

                while (!es.isEmpty())
                {
                    Edge3d e = es.get();
                    faces.Add(new Triangle3d(e.start, e.end, pts[i]));
                }
            }
            return faces;
        }
    }

}
