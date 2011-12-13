using System;
using System.Collections.Generic;
using System.Text;

namespace GridTransform
{
    struct Point2D
    {
        public double x;
        public double y;

        public Point2D(double xv, double yv)
        {
            x = xv;
            y = yv;
        }
    }

    class Triangle
    {
        #region Fields

        private Point2D FP1, FP2, FP3;
        private double FAlpha;
        private double FBeta;
        private double FGamma;
        private double FDelta;
        private double FFactor;

        #endregion Fields

        #region Construction

        public Triangle(Point2D p1, Point2D p2, Point2D p3)
        {
            FP1 = p1;
            FP2 = p2;
            FP3 = p3;

            FAlpha = FP1.x - FP3.x;
            FBeta = FP2.x - FP3.x;
            FGamma = FP1.y - FP3.y;
            FDelta = FP2.y - FP3.y;
            FFactor = 1.0d / (FAlpha * FDelta - FBeta * FGamma);
        }

        #endregion Construction

        #region Methods

        // converts a point to barycentric coordinates
        private void Point2Bary(Point2D p, out double a, out double b)
        {
            double rx = p.x - FP3.x;
            double ry = p.y - FP3.y;

            a = FFactor * (+ FDelta * rx - FBeta * ry);
            b = FFactor * (- FGamma * rx + FAlpha * ry);
        }

        // converts barycentric coordinates to a point
        private Point2D Bary2Point(double a, double b)
        {
            double rx = FAlpha * a + FBeta * b + FP3.x;
            double ry = FGamma * a + FDelta * b + FP3.y;

            return new Point2D(rx, ry);
        }

        // checks if point p is inside the triangle
        public bool Intersect(Point2D p, out double a, out double b)
        {
            Point2Bary(p, out a, out b);
            double c = 1 - a - b;

            return a >= 0 && a <= 1 && b >= 0 && b <= 1 && c >= 0 && c <= 1;
        }

        // transforms a point from one triangle to another
        public bool Transform(Point2D pIn, Triangle other, out Point2D pOut)
        {
            double a, b;

            if (Intersect(pIn, out a, out b))
            {
                pOut = other.Bary2Point(a, b);
                return true;
            }

            pOut = new Point2D(0,0);

            return false;
        }

        #endregion Methods
    }
}
