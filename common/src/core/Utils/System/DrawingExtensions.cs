using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace System.Drawing
{
    public static class DrawingExtensions
    {
        /// <summary>
        /// Returns the bounds of the given point cloud.
        /// </summary>
        public static RectangleF GetBounds(this IEnumerable<PointF> points)
        {
            var top = points.Min(p => p.Y);
            var bottom = points.Max(p => p.Y);
            var left = points.Min(p => p.X);
            var right = points.Max(p => p.X);
            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        /// <summary>
        /// Returns the bounds of the given rectangle cloud.
        /// </summary>
        public static RectangleF GetBounds(this IEnumerable<RectangleF> bounds)
        {
            var top = bounds.Min(b => b.Top);
            var bottom = bounds.Max(b => b.Bottom);
            var left = bounds.Min(b => b.Left);
            var right = bounds.Max(b => b.Right);
            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        /// <summary>
        /// Returns the area.
        /// </summary>
        public static float Area(this SizeF size)
        {
            return size.Width * size.Height;
        }

        /// <summary>
        /// Returns the center of this RectangleF.
        /// </summary>
        public static PointF GetCenter(this RectangleF rect)
        {
            return new PointF(rect.X + 0.5f * rect.Width, rect.Y + 0.5f * rect.Height);
        }

        /// <summary>
        /// Returns a rectangle for given center position and size.
        /// </summary>
        public static RectangleF GetRectangleForCenterAndSize(this PointF centerPosition, SizeF size)
        {
            return new RectangleF(centerPosition.X - size.Width * 0.5f, centerPosition.Y - size.Height * 0.5f, size.Width, size.Height);
        }

        /// <summary>
        /// Translates a given <see cref="PointF">p1</see> by a specified <see cref="PointF">p2</see>.
        /// </summary>
        public static PointF Add(this PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }

        /// <summary>
        /// Translates a given <see cref="PointF">p1</see> by a specified <see cref="PointF">p2</see>.
        /// </summary>
        public static PointF Minus(this PointF p1, PointF p2)
        {
            return new PointF(p1.X - p2.X, p1.Y - p2.Y);
        }

        /// <summary>
        /// Returns the distance to another point.
        /// </summary>
        public static float GetDistanceTo(this PointF from, PointF to)
        {
            float x = from.X - to.X;
            float y = from.Y - to.Y;
            return (float)Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Returns the distance to another point.
        /// </summary>
        public static float GetDistanceTo(this Point from, Point to)
        {
            float x = from.X - to.X;
            float y = from.Y - to.Y;
            return (float)Math.Sqrt(x * x + y * y);
        }

        public static IEnumerable<PointF> GetVertices(this RectangleF rectangle)
        {
            yield return new PointF(rectangle.Left, rectangle.Top);
            yield return new PointF(rectangle.Right, rectangle.Top);
            yield return new PointF(rectangle.Right, rectangle.Bottom);
            yield return new PointF(rectangle.Left, rectangle.Bottom);
        }
    }
}
