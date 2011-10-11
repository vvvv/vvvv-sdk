using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.Lib
{
    public class GiftWrap : HullAlgorithm
    {

        public GiftWrap(Point3d[] pts)
            : base(pts)
        {
        }

        int index(Point3d p)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                if (p == pts[i])
                {
                    return i;
                }
            }
            return -1;
        }

        protected Point3d search(Edge3d e)
        {
            int i;
            for (i = 0; pts[i] == e.start || pts[i] == e.end; i++)
            {
                /* nothing */
            }
            Point3d cand = pts[i];
            HalfSpace candh = new HalfSpace(e.start, e.end, cand);
            for (i = i + 1; i < pts.Length; i++)
            {
                if (pts[i] != e.start && pts[i] != e.end && candh.inside(pts[i]))
                {
                    cand = pts[i];
                    candh = new HalfSpace(e.start, e.end, cand);
                }
            }
            return cand;
        }


        public override List<Triangle3d> build()
        {
            /* First find a hull edge -- just connect bottommost to second from bottom */
            Point3d bot, bot2; /* bottom point and adjacent point*/
            bot = this.Bottom();
            bot2 = search2d(bot);

            /* intialize the edge stack */
            EdgeStack es = new EdgeStack();
            es.put(bot, bot2);
            es.put(bot2, bot);
            List<Triangle3d> faces = new List<Triangle3d>(20);
            Edge3d e = new Edge3d(bot, bot2);
            
            //faces.addElement(e);

            /* now the main loop -- keep finding faces till there are no more to be found */
            while (!es.isEmpty())
            {
                e = es.get();
                Point3d cand = search(e);
                faces.Add(new Triangle3d(e.start, e.end, cand));
                es.putp(e.start, cand);
                es.putp(cand, e.end);
            }
            return faces;
        }

        protected Point3d Bottom()
        {
            Point3d bot = pts[0];
            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].y() < bot.y())
                {
                    bot = pts[i];
                }
            }
            return bot;
        }


        protected Point3d search2d(Point3d p)
        {
            int i;
            i = pts[0] == p ? 1 : 0;
            Point3d cand = pts[i];
            HalfSpace candh = new HalfSpace(p, cand);
            for (i = i + 1; i < pts.Length; i++)
            {
                if (pts[i] != p && candh.inside(pts[i]))
                {
                    cand = pts[i];
                    candh = new HalfSpace(p, cand);
                }
            }
            return cand;
        }
    }
}
