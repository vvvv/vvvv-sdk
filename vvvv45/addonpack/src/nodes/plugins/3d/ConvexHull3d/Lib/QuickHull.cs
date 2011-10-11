using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.Lib
{
    public class QuickHull : HullAlgorithm
    {
        public QuickHull(Point3d[] pts) : base(pts) { }

        //find point with min x and make it pts[0]
        //find point with max x and make it pts[1]
        private void findmaxmin()
        {
            for (int i = 0; i < pts.Length; i++)
            {
                if (pts[i].x() > pts[0].x())
                {
                    Point3d temp = pts[0];
                    pts[0] = pts[i];
                    pts[i] = temp;
                }
                if (pts[i].x() < pts[1].x())
                {
                    Point3d temp = pts[1];
                    pts[1] = pts[i];
                    pts[i] = temp;
                }
            }
        }


        public override List<Triangle3d> build()
        {
            Triangle3d face1, face2; //first two faces created

            EdgeStack es = new EdgeStack(); //used to find boundary of hole
            List<Triangle3d> faces = new List<Triangle3d>();
            this.findmaxmin();

            //make p[3] the furthest from p[0]p[1]
            HalfSpace h = new HalfSpace(pts[0], pts[1]);
            for (int i = 3; i < pts.Length; i++)
            {
                if (h.normal.dot(pts[i]) > h.normal.dot(pts[2]))
                {
                    Point3d temp = pts[2];
                    pts[2] = pts[i];
                    pts[i] = temp;
                }
            }

            face1 = new Triangle3d(pts[0], pts[1], pts[2]);
            face2 = new Triangle3d(pts[0], pts[2], pts[1]);
            faces.Add(face1);
            faces.Add(face2);

            /* associate remaining points with one of these two faces */
            for (int i = 3; i < pts.Length; i++)
            {
                if (!face1.add(pts[i]))
                {
                    face2.add(pts[i]);
                }
            }

            for (int i = 0; i < faces.Count; i++)
            {
                List<Point3d> ps = new List<Point3d>();
                Triangle3d selected = faces[i];

                Point3d newp = selected.extreme();
                if (newp == null) continue;

                for (int j = 0; j < faces.Count; j++)
                {
                    Triangle3d t = faces[j];
                    if (t.inside(newp))
                    {
                        es.putp(t.tri[0], t.tri[1]);
                        es.putp(t.tri[1], t.tri[2]);
                        es.putp(t.tri[2], t.tri[0]);
                        /*add the points associated with this face to ps */
                        ps.AddRange(t.pts);
                    }
                }

                while (!es.isEmpty())
                {
                    Edge3d e = es.get();
                    Triangle3d t = new Triangle3d(e.start, e.end, newp);
                    List<Point3d> ps2 = new List<Point3d>(ps.Count);

                    for (int j = ps.Count - 1; j >= 0; j--)
                    {
                        Point3d p = ps[j];
                        if ((!p.Equals(newp)) && !t.add(p))
                        {
                            ps2.Add(p);
                        }
                    }
                    ps = ps2;
                    faces.Add(t);
                }
            }

            return faces;
        }
    }
}
