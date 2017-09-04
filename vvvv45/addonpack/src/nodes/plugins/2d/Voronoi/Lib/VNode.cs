using System;
using System.Collections.Generic;
using System.Text;

namespace BenTools.Mathematics
{
    internal abstract class VNode
    {
        private VNode _Parent = null;
        private VNode _Left = null, _Right = null;
        public VNode Left
        {
            get { return _Left; }
            set
            {
                _Left = value;
                value.Parent = this;
            }
        }
        public VNode Right
        {
            get { return _Right; }
            set
            {
                _Right = value;
                value.Parent = this;
            }
        }
        public VNode Parent
        {
            get { return _Parent; }
            set { _Parent = value; }
        }


        public void Replace(VNode ChildOld, VNode ChildNew)
        {
            if (Left == ChildOld)
                Left = ChildNew;
            else if (Right == ChildOld)
                Right = ChildNew;
            else throw new Exception("Child not found!");
            ChildOld.Parent = null;
        }

        public static VDataNode FirstDataNode(VNode Root)
        {
            VNode C = Root;
            while (C.Left != null)
                C = C.Left;
            return (VDataNode)C;
        }
        public static VDataNode LeftDataNode(VDataNode Current)
        {
            VNode C = Current;
            //1. Up
            do
            {
                if (C.Parent == null)
                    return null;
                if (C.Parent.Left == C)
                {
                    C = C.Parent;
                    continue;
                }
                else
                {
                    C = C.Parent;
                    break;
                }
            } while (true);
            //2. One Left
            C = C.Left;
            //3. Down
            while (C.Right != null)
                C = C.Right;
            return (VDataNode)C; // Cast statt 'as' damit eine Exception kommt
        }
        public static VDataNode RightDataNode(VDataNode Current)
        {
            VNode C = Current;
            //1. Up
            do
            {
                if (C.Parent == null)
                    return null;
                if (C.Parent.Right == C)
                {
                    C = C.Parent;
                    continue;
                }
                else
                {
                    C = C.Parent;
                    break;
                }
            } while (true);
            //2. One Right
            C = C.Right;
            //3. Down
            while (C.Left != null)
                C = C.Left;
            return (VDataNode)C; // Cast statt 'as' damit eine Exception kommt
        }

        public static VEdgeNode EdgeToRightDataNode(VDataNode Current)
        {
            VNode C = Current;
            //1. Up
            do
            {
                if (C.Parent == null)
                    throw new Exception("No Left Leaf found!");
                if (C.Parent.Right == C)
                {
                    C = C.Parent;
                    continue;
                }
                else
                {
                    C = C.Parent;
                    break;
                }
            } while (true);
            return (VEdgeNode)C;
        }

        public static VDataNode FindDataNode(VNode Root, double ys, double x)
        {
            VNode C = Root;
            do
            {
                if (C is VDataNode)
                    return (VDataNode)C;
                if (((VEdgeNode)C).Cut(ys, x) < 0)
                    C = C.Left;
                else
                    C = C.Right;
            } while (true);
        }

        /// <summary>
        /// Will return the new root (unchanged except in start-up)
        /// </summary>

        public static VNode ProcessCircleEvent(VCircleEvent e, VNode Root, VoronoiGraph VG, double ys, out VDataNode[] CircleCheckList)
        {
            VDataNode a, b, c;
            VEdgeNode eu, eo;
            b = e.NodeN;
            a = VNode.LeftDataNode(b);
            c = VNode.RightDataNode(b);
            if (a == null || b.Parent == null || c == null || !a.DataPoint.Equals(e.NodeL.DataPoint) || !c.DataPoint.Equals(e.NodeR.DataPoint))
            {
                CircleCheckList = new VDataNode[] { };
                return Root; // Abbruch da sich der Graph verändert hat
            }
            eu = (VEdgeNode)b.Parent;
            CircleCheckList = new VDataNode[] { a, c };
            //1. Create the new Vertex
            Vector VNew = new Vector(e.Center[0], e.Center[1]);
            //			VNew[0] = Fortune.ParabolicCut(a.DataPoint[0],a.DataPoint[1],c.DataPoint[0],c.DataPoint[1],ys);
            //			VNew[1] = (ys + a.DataPoint[1])/2 - 1/(2*(ys-a.DataPoint[1]))*(VNew[0]-a.DataPoint[0])*(VNew[0]-a.DataPoint[0]);
            VG.Vertizes.Add(VNew);
            //2. Find out if a or c are in a distand part of the tree (the other is then b's sibling) and assign the new vertex
            if (eu.Left == b) // c is sibling
            {
                eo = VNode.EdgeToRightDataNode(a);

                // replace eu by eu's Right
                eu.Parent.Replace(eu, eu.Right);
            }
            else // a is sibling
            {
                eo = VNode.EdgeToRightDataNode(b);

                // replace eu by eu's Left
                eu.Parent.Replace(eu, eu.Left);
            }
            eu.Edge.AddVertex(VNew);
            //			///////////////////// uncertain
            //			if(eo==eu)
            //				return Root;
            //			/////////////////////

            //complete & cleanup eo
            eo.Edge.AddVertex(VNew);
            //while(eo.Edge.VVertexB == Fortune.VVUnkown)
            //{
            //    eo.Flipped = !eo.Flipped;
            //    eo.Edge.AddVertex(Fortune.VVInfinite);
            //}
            //if(eo.Flipped)
            //{
            //    Vector T = eo.Edge.LeftData;
            //    eo.Edge.LeftData = eo.Edge.RightData;
            //    eo.Edge.RightData = T;
            //}


            //2. Replace eo by new Edge
            VoronoiEdge VE = new VoronoiEdge();
            VE.LeftData = a.DataPoint;
            VE.RightData = c.DataPoint;
            VE.AddVertex(VNew);
            VG.Edges.Add(VE);

            VEdgeNode VEN = new VEdgeNode(VE, false);
            VEN.Left = eo.Left;
            VEN.Right = eo.Right;
            if (eo.Parent == null)
                return VEN;
            eo.Parent.Replace(eo, VEN);
            return Root;
        }
        public static VCircleEvent CircleCheckDataNode(VDataNode n, double ys)
        {
            VDataNode l = VNode.LeftDataNode(n);
            VDataNode r = VNode.RightDataNode(n);
            if (l == null || r == null || l.DataPoint == r.DataPoint || l.DataPoint == n.DataPoint || n.DataPoint == r.DataPoint)
                return null;
            if (MathTools.ccw(l.DataPoint[0], l.DataPoint[1], n.DataPoint[0], n.DataPoint[1], r.DataPoint[0], r.DataPoint[1], false) <= 0)
                return null;
            Vector Center = Fortune.CircumCircleCenter(l.DataPoint, n.DataPoint, r.DataPoint);
            VCircleEvent VC = new VCircleEvent();
            VC.NodeN = n;
            VC.NodeL = l;
            VC.NodeR = r;
            VC.Center = Center;
            VC.Valid = true;
            if (VC.Y >= ys)
                return VC;
            return null;
        }

        public static void CleanUpTree(VNode Root)
        {
            if (Root is VDataNode)
                return;
            VEdgeNode VE = Root as VEdgeNode;
            while (VE.Edge.VVertexB == Fortune.VVUnkown)
            {
                VE.Edge.AddVertex(Fortune.VVInfinite);
                //				VE.Flipped = !VE.Flipped;
            }
            if (VE.Flipped)
            {
                Vector T = VE.Edge.LeftData;
                VE.Edge.LeftData = VE.Edge.RightData;
                VE.Edge.RightData = T;
            }
            VE.Edge.Done = true;
            CleanUpTree(Root.Left);
            CleanUpTree(Root.Right);
        }
    }
}
