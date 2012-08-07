using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.Lib
{
    public class PointStack
    {
        private List<Point3d> starts; // unmatched start points
        private List<Point3d> ends; // unmatched end points

        public PointStack()
        {
            starts = new List<Point3d>();
            ends = new List<Point3d>();
        }

        public bool isEmpty()
        {
            return starts.Count == 0;
        }

        public void put(Point3d start, Point3d end)
        {
            if (!ends.Remove(start))
            {
                starts.Add(start);
            }
            if (start == end || !starts.Remove(end))
            {
                ends.Add(end);
            }
        }

        public Point3d getStart()
        {
            Point3d p = (Point3d)starts[starts.Count -1];
            starts.Clear();
            return p;
        }

        public Point3d getEnd()
        {
            Point3d p = (Point3d)ends[ends.Count -1];
            ends.Clear();
            return p;
        }

    }
}
