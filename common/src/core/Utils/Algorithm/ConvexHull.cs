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
        /// Three points are a counter-clockwise turn if ccw &gt; 0, clockwise if
        /// ccw &lt; 0, and collinear if ccw = 0 because ccw is a determinant that
        /// gives the signed area of the triangle formed by p1, p2 and p3.
        /// </summary>
        private static float CounterClockWise(PointF p1, PointF p2, PointF p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
        }

        private static PointF GetRightMostLowestPoint(List<PointF> source)
        {
            var maxY = source.Max(p => p.Y);
            var lowestPoints = source.Where(p => p.Y == maxY);
            var maxX = lowestPoints.Max(p => p.X);
            return lowestPoints.First(p => p.X == maxX);
        }


        class PointFEqualityComparer : IEqualityComparer<PointF>
        {
            public bool Equals(PointF x, PointF y)
            {
                return ((x.X - x.Y) < 0.1) && ((y.X - y.Y) < 0.1);
            }

            public int GetHashCode(PointF obj)
            {
                return obj.GetHashCode();
            }
        }

        /// <summary>
        /// Generates list of convex hull points from the given list of points using Graham's scan
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<PointF> CreateConvexHull(List<PointF> source)
        {
            source = source.Distinct(new PointFEqualityComparer()).ToList();

            var N = source.Count;
            Stack<PointF> result = new Stack<PointF>();
            // Select the rightmost lowest point p0
            var p0 = GetRightMostLowestPoint(source);
            // Sort S radially (ccw) about p0 as a center
            source.Sort(new PolarAngleComparer(p0));
            // Init stack with 3 first points
            result.Push(source[0]);
            result.Push(source[1]);
            result.Push(source[2]);
            // Perform test for every other point
            for (int i = 3; i < source.Count; i++)
            {
                // The angle between NEXT_TO_TOP[S], TOP[S], and p(i) makes a nonleft turn
                while (CounterClockWise(result.ElementAt(1), result.Peek(), source[i]) > 0)
                {
                    result.Pop();
                }
                result.Push(source[i]);
            }
            return result.ToList();
        }

        public static IEnumerable<Tuple<T, T>> Pairwise<T>(IList<T> input)
        {
            for (int i = 0; i < input.Count-2; i++) 
            {
                yield return new Tuple<T, T>(input[i], input[i+1]);
            }
        }
        
        
        public static bool IsPointInConvexHull(PointF p, IEnumerable<PointF> hull)
        {
            //if (hull.Count() > 0)
            var myP = new VMath.Vector2D(p.X, p.Y);
            var myHull = hull.Select(q => new VMath.Vector2D(q.X, q.Y));
            
            var points = myHull.Concat(new VMath.Vector2D[]{myHull.First()}).ToArray(); // closed
            var lines = Pairwise(points);

            foreach (var l in lines)
            {
                var AB = l.Item2 - l.Item1;
                var lineNorm = new VMath.Vector2D(-AB.y, AB.x);
                var AP = myP - l.Item1;

                var scalarProduct = lineNorm | AP;

                if (scalarProduct < 0)
                    return false;

            }
            return true;
        }

        class PolarAngleComparer : IComparer<PointF>
        {
            private readonly PointF point0;

            public PolarAngleComparer(PointF point0)
            {
                this.point0 = point0;
            }

            public int Compare(PointF a, PointF b)
            {
                var ccw = ConvexHull.CounterClockWise(point0, a, b);
                if (ccw > 0)
                    return 1;
                if (ccw < 0)
                    return -1;
                var distanceA = Length(Minus(a, point0));
                var distanceB = Length(Minus(b, point0));
                return distanceA.CompareTo(distanceB);
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
