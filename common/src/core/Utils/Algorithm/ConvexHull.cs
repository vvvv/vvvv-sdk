using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Drawing;
using System;

namespace VVVV.Utils.Algorithm
{
    public static class ConvexHull
    {
        /// <summary>
        /// Three points are a counter-clockwise turn if ccw > 0, clockwise if
        /// ccw < 0, and collinear if ccw = 0 because ccw is a determinant that
        /// gives the signed area of the triangle formed by p1, p2 and p3.
        /// </summary>
        private static double ConterClockWise(PointF p1, PointF p2, PointF p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
        }

        private static PointF SortByPolarAngleAndReturnP0(List<PointF> source)
        {
            PointF point0;
            // determine the point with min Y
            var minY = source.Min(pointCoordY => pointCoordY.Y);
            var leftPoints = source.Where(point => point.Y == minY);
            if (leftPoints.Count() > 1)
            {
                // if there are more than 1 point, get the point with min X
                double minX = leftPoints.Min(pointCoordX => pointCoordX.X);
                point0 = leftPoints.First(point => point.X == minX);
            }
            else
            {
                point0 = leftPoints.First();
            }
            source.Remove(point0);
            source.Sort(new PolarAngleComparer(point0));
            return point0;
        }

        /// <summary>
        /// Generates list of convex hull points from the given list of points using Graham's scan
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<PointF> CreateConvexHull(List<PointF> source)
        {
            // HACK: Invert y
            for (int i = 0; i < source.Count; i++)
            {
                source[i] = new PointF(source[i].X, -source[i].Y);
            }
            //1. create a stack of points
            Stack<PointF> result = new Stack<PointF>();
            //2. sort the incoming points
            var point0 = SortByPolarAngleAndReturnP0(source);
            //3. init stack with 3 first points
            result.Push(point0);
            result.Push(source[0]);
            result.Push(source[1]);
            //result.Push(source[2]);
            //4. perform test for every other point
            for (int i = 2; i < source.Count; i++)
            {
                //5. the angle between NEXT_TO_TOP[S], TOP[S], and p(i) makes a nonleft turn
                while (ConterClockWise(result.ElementAt(result.Count - 2), result.Peek(), source[i]) > 0)
                {
                    result.Pop();
                }
                result.Push(source[i]);
            }
            // HACK: Invert y
            var resultList = new List<PointF>(result);
            for (int i = 0; i < resultList.Count; i++)
            {
                resultList[i] = new PointF(resultList[i].X, -resultList[i].Y);
            }
            return resultList;
        }

        /// <summary>
        /// Compares points by polar angle to the 0 point.
        /// </summary>
        class PolarAngleComparer : IComparer<PointF>
        {
            private PointF point0;

            /// <summary>
            /// Creates an instance of PolarAngleComparer
            /// </summary>
            /// <param name="point0">the zero (top left) point</param>
            public PolarAngleComparer(PointF point0)
            {
                this.point0 = point0;
            }

            /// <summary>
            /// Compares 2 point values in order to determine the one with minimal polar angle to given zero point
            /// </summary>
            /// <param name="a">first point</param>
            /// <param name="b">second point</param>
            /// <returns>a<b => value < 0; a==b => value == 0; a>b => value > 0</returns>
            public int Compare(PointF a, PointF b)
            {
                var angleA = (point0.X - a.X) / (a.Y - point0.Y);
                var angleB = (point0.X - b.X) / (b.Y - point0.Y);

                int result = (-angleA).CompareTo(-angleB);
                if (result == 0)
                {
                    var distanceA = Length(Minus(a, point0));
                    var distanceB = Length(Minus(b, point0));
                    result = distanceA.CompareTo(distanceB);
                }
                return result;
            }

            static PointF Minus(PointF a, PointF b)
            {
                return new PointF(a.X - b.X, a.Y - b.Y);
            }

            static double Length(PointF a)
            {
                return Math.Sqrt(a.X * a.X + a.Y * a.Y);
            }
        }
    }

}
