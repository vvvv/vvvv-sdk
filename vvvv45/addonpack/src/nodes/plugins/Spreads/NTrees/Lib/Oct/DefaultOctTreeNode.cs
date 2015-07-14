using System;
using System.Collections.Generic;
using System.Text;

namespace NTrees.Lib.Oct
{
    public class DefaultOctTreeNode : NTreeNode<DefaultOctTreeNode, Box, Point3d>
    {
        public DefaultOctTreeNode(Box bounds, int maxElements)
            : base(bounds,maxElements) {}

        protected override int ChildCount
        {
            get { return 8; }
        }

        protected override DefaultOctTreeNode CreateChild(Box bounds, int maxElements)
        {
            return new DefaultOctTreeNode(bounds, maxElements);
        }
    }
}
