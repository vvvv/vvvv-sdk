using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
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
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var p in points)
            {
                var v = new Vector2(p.X, p.Y);
                min = Vector2.Min(min, v);
                max = Vector2.Max(max, v);
            }
            if (min.X != float.MaxValue)
                return RectangleF.FromLTRB(min.X, min.Y, max.X, max.Y);
            return RectangleF.Empty;
        }

        /// <summary>
        /// Returns the bounds of the given rectangle cloud.
        /// </summary>
        public static RectangleF GetBounds(this IEnumerable<RectangleF> bounds)
        {
            var minTopLeft = new Vector2(float.MaxValue, float.MaxValue);
            var maxBottomRight = new Vector2(float.MinValue, float.MinValue);
            foreach (var r in bounds)
            {
                minTopLeft = Vector2.Min(minTopLeft, new Vector2(r.Left, r.Top));
                maxBottomRight = Vector2.Max(maxBottomRight, new Vector2(r.Right, r.Bottom));
            }
            if (minTopLeft.X != float.MaxValue)
                return RectangleF.FromLTRB(minTopLeft.X, minTopLeft.Y, maxBottomRight.X, maxBottomRight.Y);
            return RectangleF.Empty;
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
        /// Returns a rectangle for given center position and 1 pixel size
        /// </summary>
        public static RectangleF GetOnePixelRectangleForCenter(this PointF centerPosition)
        {
            return GetRectangleForCenterAndSize(centerPosition, new SizeF(1, 1));
        }

        /// <summary>
        /// Returns a rectangle for given center position and size.
        /// </summary>
        public static RectangleF GetRectangleForCenterAndSize(this PointF centerPosition, SizeF size)
        {
            return new RectangleF(centerPosition.X - size.Width * 0.5f, centerPosition.Y - size.Height * 0.5f, size.Width, size.Height);
        }

        /// <summary>
        /// Returns a <see cref="Rectangle">Rectangle</see> by casting the float components to integer.
        /// </summary>
        public static Rectangle ToRectangle(this RectangleF rect)
        {
            return new Rectangle(rect.Location.ToPoint(), rect.Size.ToSize());
        }

        /// <summary>
        /// Returns a <see cref="Point">Point</see> by casting the float components to integer.
        /// </summary>
        public static Point ToPoint(this PointF p1)
        {
            return new Point((int)p1.X, (int)p1.Y);
        }

        /// <summary>
        /// Translates a given <see cref="PointF">p1</see> by a specified <see cref="PointF">p2</see>.
        /// </summary>
        public static PointF Plus(this PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }
        
        public static Point Plus(this Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        /// <summary>
        /// Translates a given <see cref="PointF">p1</see> by a specified <see cref="PointF">p2</see>.
        /// </summary>
        public static PointF Minus(this PointF p1, PointF p2)
        {
            return new PointF(p1.X - p2.X, p1.Y - p2.Y);
        }
        
        public static Point Minus(this Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static PointF Multiply(this PointF point, float factor)
        {
            return new PointF(point.X * factor, point.Y * factor);
        }
        
        public static Point Multiply(this Point point, int factor)
        {
            return new Point(point.X * factor, point.Y * factor);
        }

        public static PointF Lerp(this PointF p1, PointF p2, float x)
        {
            return p1.Plus(p2.Minus(p1).Multiply(x));
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
        /// Returns the squared distance to another point. Good for comparisons.
        /// </summary>
        public static float GetSquaredDistanceTo(this PointF from, PointF to)
        {
            float x = from.X - to.X;
            float y = from.Y - to.Y;
            return x * x + y * y;
        }

        /// <summary>
        /// Normalizes the length
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static PointF Normalize(this PointF point)
        {
            var distance = (float)Math.Sqrt(point.X * point.X + point.Y * point.Y);
            if (distance != 0)
                return new PointF(point.X / distance, point.Y / distance);
            else
                return new PointF();
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

        public static IEnumerable<PointF> GetVertices(this IEnumerable<RectangleF> rectangles)
        {
            foreach (var rectangle in rectangles)
                foreach (var vertex in rectangle.GetVertices())
                    yield return vertex;
        }
        
        /// <summary>
        /// Applies the transformation to a PointF
        /// </summary>
        /// <param name="t">A Matrix</param>
        /// <param name="p">The point to transform by the matrix t</param>
        /// <returns></returns>
        public static PointF TransformPoint(this Matrix t, PointF p)
        {
            var pts = new PointF[] { p };
            t.TransformPoints(pts);
            return pts[0];
        }

        /// <summary>
        /// Applies the geometric transform represented by this Matrix to the
        /// given rectangle.
        /// </summary>
        /// <param name="t">A Matrix</param>
        /// <param name="rect">The rectangle to transform.</param>
        /// <returns>The transformed rectangle.</returns>
        public static Rectangle TransformRectangle(this Matrix t, Rectangle rect)
        {
            var tr = TransformRectangle(t, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));
            return new Rectangle((int)tr.X, (int)tr.Y, (int)tr.Width, (int)tr.Height);
        }

        /// <summary>
        /// Applies the geometric transform represented by this Matrix to the
        /// given rectangle.
        /// </summary>
        /// <param name="t">A Matrix</param>
        /// <param name="rect">The rectangle to transform.</param>
        /// <returns>The transformed rectangle.</returns>
        public static RectangleF TransformRectangle(this Matrix t, RectangleF rect)
        {
            float x = rect.X;
            float y = rect.Y;
            float width = rect.Width;
            float height = rect.Height;
            
            var PTS4 = new PointF[4];
            PTS4[0].X = x;
            PTS4[0].Y = y;
            PTS4[1].X = x + width;
            PTS4[1].Y = y;
            PTS4[2].X = x + width;
            PTS4[2].Y = y + height;
            PTS4[3].X = x;
            PTS4[3].Y = y + height;
            
            t.TransformPoints(PTS4);
            
            float minX = PTS4[0].X;
            float minY = PTS4[0].Y;
            float maxX = PTS4[0].X;
            float maxY = PTS4[0].Y;
            
            for (int i = 1; i < 4; i++)
            {
                x = PTS4[i].X;
                y = PTS4[i].Y;
                
                if (x < minX)
                {
                    minX = x;
                }
                if (y < minY)
                {
                    minY = y;
                }
                if (x > maxX)
                {
                    maxX = x;
                }
                if (y > maxY)
                {
                    maxY = y;
                }
            }
            
            rect.X = minX;
            rect.Y = minY;
            rect.Width = maxX - minX;
            rect.Height = maxY - minY;
            return rect;
        }

        public static Matrix Inverse(this Matrix m)
        {
            var i = m.Clone();
            i.Invert();
            return i;
        }

        // From: http://stackoverflow.com/questions/5514366/how-to-know-if-a-line-intersects-a-rectangle
        public static bool IntersectsLine(this RectangleF r, PointF p1, PointF p2)
        {
            return LineIntersectsLine(p1, p2, new PointF(r.X, r.Y), new PointF(r.X + r.Width, r.Y)) ||
                   LineIntersectsLine(p1, p2, new PointF(r.X + r.Width, r.Y), new PointF(r.X + r.Width, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new PointF(r.X + r.Width, r.Y + r.Height), new PointF(r.X, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new PointF(r.X, r.Y + r.Height), new PointF(r.X, r.Y)) ||
                   (r.Contains(p1) && r.Contains(p2));
        }

        private static bool LineIntersectsLine(PointF l1p1, PointF l1p2, PointF l2p1, PointF l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }

        public static void DrawCenteredCircle(this Graphics g, Pen pen,
                              float centerX, float centerY, float size)
        {
            var radius = size / 2f;
            g.DrawEllipse(pen, centerX - radius, centerY - radius,
                          size, size);
        }

        public static void FillCenteredCircle(this Graphics g, Brush brush,
                                      float centerX, float centerY, float size)
        {
            var radius = size / 2f;
            g.FillEllipse(brush, centerX - radius, centerY - radius,
                          size, size);
        }
    }
}
