using System;
using System.Collections.Generic;
using System.Text;

namespace NTrees.Lib.Quad
{
    public class DefaultQuadTree
    {
        private int maxPoints;
        private Rect bounds;

        private DefaultQuadTreeNode root;

        public DefaultQuadTree() : this(10, new Rect(1.0, -1.0, -1.0, 1.0)) { }
        public DefaultQuadTree(int maxPoints) : this(maxPoints, new Rect(1.0, -1.0, -1.0, 1.0)) { }
        public DefaultQuadTree(Rect bounds) : this(10, bounds) { }

        public DefaultQuadTree(int maxPoints, Rect bounds)
        {
            this.maxPoints = maxPoints;
            this.bounds = bounds;
            this.root = new DefaultQuadTreeNode(bounds, maxPoints);
        }


        #region Add items
        public bool Add(Point2d point)
        {
            return root.AddElement(point);
        }

        public List<bool> Add(List<Point2d> points)
        {
            List<bool> result = new List<bool>();
            foreach (Point2d point in points)
            {
                result.Add(root.AddElement(point));
            }
            return result;
        }
        #endregion

        public List<Rect> GetAllBounds()
        {
            List<Rect> result = new List<Rect>();
            this.root.GetAllBounds(result);
            return result;
        }

        public Rect FindNodeBounds(Point2d position)
        {
            return this.root.FindNode(position).Bounds;
        }
    }
}
