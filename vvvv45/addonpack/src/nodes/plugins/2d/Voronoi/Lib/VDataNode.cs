using System;
using System.Collections.Generic;
using System.Text;

namespace BenTools.Mathematics
{
    internal class VDataNode : VNode
    {
        public VDataNode(Vector DP)
        {
            this.DataPoint = DP;
        }
        public Vector DataPoint;

        public static VNode ProcessDataEvent(VDataEvent e, VNode Root, VoronoiGraph VG, double ys, out VDataNode[] CircleCheckList)
        {
            if (Root == null)
            {
                Root = new VDataNode(e.DataPoint);
                CircleCheckList = new VDataNode[] { (VDataNode)Root };
                return Root;
            }
            //1. Find the node to be replaced
            VNode C = VNode.FindDataNode(Root, ys, e.DataPoint[0]);
            //2. Create the subtree (ONE Edge, but two VEdgeNodes)
            VoronoiEdge VE = new VoronoiEdge();
            VE.LeftData = ((VDataNode)C).DataPoint;
            VE.RightData = e.DataPoint;
            VE.VVertexA = Fortune.VVUnkown;
            VE.VVertexB = Fortune.VVUnkown;
            VG.Edges.Add(VE);

            VNode SubRoot;
            if (Math.Abs(VE.LeftData[1] - VE.RightData[1]) < 1e-10)
            {
                if (VE.LeftData[0] < VE.RightData[0])
                {
                    SubRoot = new VEdgeNode(VE, false);
                    SubRoot.Left = new VDataNode(VE.LeftData);
                    SubRoot.Right = new VDataNode(VE.RightData);
                }
                else
                {
                    SubRoot = new VEdgeNode(VE, true);
                    SubRoot.Left = new VDataNode(VE.RightData);
                    SubRoot.Right = new VDataNode(VE.LeftData);
                }
                CircleCheckList = new VDataNode[] { (VDataNode)SubRoot.Left, (VDataNode)SubRoot.Right };
            }
            else
            {
                SubRoot = new VEdgeNode(VE, false);
                SubRoot.Left = new VDataNode(VE.LeftData);
                SubRoot.Right = new VEdgeNode(VE, true);
                SubRoot.Right.Left = new VDataNode(VE.RightData);
                SubRoot.Right.Right = new VDataNode(VE.LeftData);
                CircleCheckList = new VDataNode[] { (VDataNode)SubRoot.Left, (VDataNode)SubRoot.Right.Left, (VDataNode)SubRoot.Right.Right };
            }

            //3. Apply subtree
            if (C.Parent == null)
                return SubRoot;
            C.Parent.Replace(C, SubRoot);
            return Root;
        }
    }
}
