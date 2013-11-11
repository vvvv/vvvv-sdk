using System;
using System.Collections.Generic;
using System.Text;

namespace NTrees.Lib.Quad
{
    public class Edge2d
    {
        private Point2d pt1;
        private Point2d pt2;

        public Edge2d(Point2d pt1, Point2d pt2)
        {
            this.pt1 = pt1;
            this.pt2 = pt2;
        }

        public Edge2d(double x1, double y1,double x2,double y2)
        {
            this.pt1 = new Point2d(x1, y1);
            this.pt2 = new Point2d(x2, y2);
        }

        public Point2d Point1
        {
            get { return pt1; }
        }

        public Point2d Point2
        {
            get { return pt2; }
        }



    }
}
