using System;
using System.Collections;
using BenTools.Data;
using System.Collections.Generic;
using MrVux.Structures.PriorityQueues;

namespace BenTools.Mathematics
{
	public abstract class Fortune
	{
		public static readonly Vector VVInfinite = new Vector(double.PositiveInfinity, double.PositiveInfinity);
		public static readonly Vector VVUnkown = new Vector(double.NaN, double.NaN);
		internal static double ParabolicCut(double x1, double y1, double x2, double y2, double ys)
		{
//			y1=-y1;
//			y2=-y2;
//			ys=-ys;
//			
			if(Math.Abs(x1-x2)<1e-10 && Math.Abs(y1-y2)<1e-10)
			{
//				if(y1>y2)
//					return double.PositiveInfinity;
//				if(y1<y2)
//					return double.NegativeInfinity;
//				return x;
				throw new Exception("Identical datapoints are not allowed!");
			}

			if(Math.Abs(y1-ys)<1e-10 && Math.Abs(y2-ys)<1e-10)
				return (x1+x2)/2;
			if(Math.Abs(y1-ys)<1e-10)
				return x1;
			if(Math.Abs(y2-ys)<1e-10)
				return x2;
			double a1 = 1/(2*(y1-ys));
			double a2 = 1/(2*(y2-ys));
			if(Math.Abs(a1-a2)<1e-10)
				return (x1+x2)/2;
			double xs1 = 0.5/(2*a1-2*a2)*(4*a1*x1-4*a2*x2+2*Math.Sqrt(-8*a1*x1*a2*x2-2*a1*y1+2*a1*y2+4*a1*a2*x2*x2+2*a2*y1+4*a2*a1*x1*x1-2*a2*y2));
			double xs2 = 0.5/(2*a1-2*a2)*(4*a1*x1-4*a2*x2-2*Math.Sqrt(-8*a1*x1*a2*x2-2*a1*y1+2*a1*y2+4*a1*a2*x2*x2+2*a2*y1+4*a2*a1*x1*x1-2*a2*y2));
			xs1=Math.Round(xs1,10);
			xs2=Math.Round(xs2,10);
			if(xs1>xs2)
			{
				double h = xs1;
				xs1=xs2;
				xs2=h;
			}
			if(y1>=y2)
				return xs2;
			return xs1;
		}
		internal static Vector CircumCircleCenter(Vector A, Vector B, Vector C)
		{
			if(A==B || B==C || A==C)
				throw new Exception("Need three different points!");
			double tx = (A[0] + C[0])/2;
			double ty = (A[1] + C[1])/2;

			double vx = (B[0] + C[0])/2;
			double vy = (B[1] + C[1])/2;

			double ux,uy,wx,wy;
			
			if(A[0] == C[0])
			{
				ux = 1;
				uy = 0;
			}
			else
			{
				ux = (C[1] - A[1])/(A[0] - C[0]);
				uy = 1;
			}

			if(B[0] == C[0])
			{
				wx = -1;
				wy = 0;
			}
			else
			{
				wx = (B[1] - C[1])/(B[0] - C[0]);
				wy = -1;
			}

			double alpha = (wy*(vx-tx)-wx*(vy - ty))/(ux*wy-wx*uy);

			return new Vector(tx+alpha*ux,ty+alpha*uy);
		}	
		public static VoronoiGraph ComputeVoronoiGraph(IEnumerable Datapoints)
		{
            PriorityQueue<VEvent> PQ = new PriorityQueue<VEvent>(ePriorityMode.eLowestFirst);
            Dictionary<VNode, VEvent> CurrentCircles = new Dictionary<VNode, VEvent>();

			VoronoiGraph VG = new VoronoiGraph();
			VNode RootNode = null;
			foreach(Vector V in Datapoints)
			{
				PQ.Push(new VDataEvent(V));
			}
			while(PQ.Count>0)
			{
				VEvent VE = PQ.Pop() as VEvent;
				VDataNode[] CircleCheckList;
				if(VE is VDataEvent)
				{
					RootNode = VDataNode.ProcessDataEvent(VE as VDataEvent,RootNode,VG,VE.Y,out CircleCheckList);
				}
				else if(VE is VCircleEvent)
				{
					CurrentCircles.Remove(((VCircleEvent)VE).NodeN);
					if(!((VCircleEvent)VE).Valid)
						continue;
					RootNode = VNode.ProcessCircleEvent(VE as VCircleEvent,RootNode,VG,VE.Y,out CircleCheckList);
				}
				else throw new Exception("Got event of type "+VE.GetType().ToString()+"!");
				foreach(VDataNode VD in CircleCheckList)
				{
					if(CurrentCircles.ContainsKey(VD))
					{
						((VCircleEvent)CurrentCircles[VD]).Valid=false;
						CurrentCircles.Remove(VD);
					}
					VCircleEvent VCE = VNode.CircleCheckDataNode(VD,VE.Y);
					if(VCE!=null)
					{
						PQ.Push(VCE);
						CurrentCircles[VD]=VCE;
					}
				}
				if(VE is VDataEvent)
				{
					Vector DP = ((VDataEvent)VE).DataPoint;
					foreach(VCircleEvent VCE in CurrentCircles.Values)
					{
						if(MathTools.Dist(DP[0],DP[1],VCE.Center[0],VCE.Center[1])<VCE.Y-VCE.Center[1] && Math.Abs(MathTools.Dist(DP[0],DP[1],VCE.Center[0],VCE.Center[1])-(VCE.Y-VCE.Center[1]))>1e-10)
							VCE.Valid = false;
					}
				}
			}
			VNode.CleanUpTree(RootNode);
			foreach(VoronoiEdge VE in VG.Edges)
			{
				if(VE.Done)
					continue;
				if(VE.VVertexB == Fortune.VVUnkown)
				{
					VE.AddVertex(Fortune.VVInfinite);
					if(Math.Abs(VE.LeftData[1]-VE.RightData[1])<1e-10 && VE.LeftData[0]<VE.RightData[0])
					{
						Vector T = VE.LeftData;
						VE.LeftData = VE.RightData;
						VE.RightData = T;
					}
				}
			}

            List<VoronoiEdge> MinuteEdges = new List<VoronoiEdge>();
			foreach(VoronoiEdge VE in VG.Edges)
			{
				if(!VE.IsPartlyInfinite && VE.VVertexA.Equals(VE.VVertexB))
				{
					MinuteEdges.Add(VE);
					// prevent rounding errors from expanding to holes
					foreach(VoronoiEdge VE2 in VG.Edges)
					{
						if(VE2.VVertexA.Equals(VE.VVertexA))
							VE2.VVertexA = VE.VVertexA;
						if(VE2.VVertexB.Equals(VE.VVertexA))
							VE2.VVertexB = VE.VVertexA;
					}
				}
			}
			foreach(VoronoiEdge VE in MinuteEdges)
				VG.Edges.Remove(VE);

			return VG;
		}
		public static VoronoiGraph FilterVG(VoronoiGraph VG, double minLeftRightDist)
		{
			VoronoiGraph VGErg = new VoronoiGraph();
			foreach(VoronoiEdge VE in VG.Edges)
			{
				if(Math.Sqrt(Vector.Dist(VE.LeftData,VE.RightData))>=minLeftRightDist)
					VGErg.Edges.Add(VE);
			}
			foreach(VoronoiEdge VE in VGErg.Edges)
			{
				VGErg.Vertizes.Add(VE.VVertexA);
				VGErg.Vertizes.Add(VE.VVertexB);
			}
			return VGErg;
		}
	}
}
