using System;
using System.Collections.Generic;
using System.Text;

namespace NTrees.Lib.Oct
{
    public class DefaultOctTree
    {
        private int maxPoints;
        private Box bounds;

        private DefaultOctTreeNode root;

        public DefaultOctTree() : this(10, new Box(1.0, -1.0, -1.0, 1.0, 1.0, -1.0)) { }
        public DefaultOctTree(int maxPoints) : this(maxPoints, new Box(1.0, -1.0, -1.0, 1.0, 1.0, -1.0)) { }
        public DefaultOctTree(Box bounds) : this(10, bounds) { }

        public DefaultOctTree(int maxPoints, Box bounds)
        {
            this.maxPoints = maxPoints;
            this.bounds = bounds;
            this.root = new DefaultOctTreeNode(bounds, maxPoints);
        }


        #region Add items
        public bool Add(Point3d point)
        {
            return root.AddElement(point);
        }

        public List<bool> Add(List<Point3d> points)
        {
            List<bool> result = new List<bool>();
            foreach (Point3d point in points)
            {
                result.Add(root.AddElement(point));
            }
            return result;
        }
        #endregion

        public List<Box> GetAllBounds()
        {
            List<Box> result = new List<Box>();
            this.root.GetAllBounds(result);
            return result;
        }
    }
}
