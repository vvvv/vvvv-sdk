using System;
using System.Collections.Generic;
using System.Text;

namespace NTrees.Lib.Quad
{
    public class DefaultQuadTreeNode : NTreeNode<DefaultQuadTreeNode, Rect, Point2d>
    {
        public DefaultQuadTreeNode(Rect bounds, int maxElements)
            :
            base(bounds,maxElements) {}

        protected override int ChildCount
        {
            get { return 4; }
        }

        protected override DefaultQuadTreeNode CreateChild(Rect bounds, int maxElements)
        {
            return new DefaultQuadTreeNode(bounds, maxElements);
        }
    }
}
