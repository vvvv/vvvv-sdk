using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Piccolo.NET;
using Piccolo.NET.Util;
using Piccolo.NET.Nodes;
using Piccolo.NET.Event;

using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Core.View.GraphicalEditor;

namespace VVVV.HDE.GraphicalEditing
{
	
    public class TempPath : GraphElement, ITempPath
    {
        protected override PNode CreatePNode()
        {
            return new PPath();
        }

        #region ITempPath Members     

        public ISolid Start
        {
            get;
            private set;
        }

        public Solid StartSolid
        {
            get { return Start as Solid; }
        }

        public List<PointF> Points
        {
            get;
            protected set;
        }

        public ArrowType ArrowType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public override Pen Pen
        {
            get
            {
                return PPath.Pen;
            }
            set
            {
                PPath.Pen = value;
            }
        }

        public TempPath(IGraphElementHost host, ISolid start)
            : base(host)
        {
 			Points = new List<PointF>();
            Start = start;

            PNode.Brush = null;
			
			//start point
			Points.Add(Start.GlobalMiddle);

			//end point
            Points.Add(Start.GlobalMiddle);
       }
		
		public void AddPoint(PointF Position)
		{
			Points.Add(Position);
		}

        protected void BuildLine(PointF endpoint)
        {
            PPath.Reset();

            Points[0] = Start.GlobalMiddle;
            Points[Points.Count - 1] = endpoint;

            for (int i = 0; i < Points.Count - 1; i++)
                PPath.AddLine(Points[i].X, Points[i].Y, Points[i + 1].X, Points[i + 1].Y);
            	
        }

        internal void Revert(Solid newStart, out Solid newEnd)
        {
            newEnd = Start as Solid;
            Start = newStart;

            Points.Reverse();
            BuildLine(newEnd.GlobalMiddle);
        }

        internal void SetEndPoint(PointF endPoint)
        {
            BuildLine(endPoint);
        }
    	
        CurveType FCurveType;
		public CurveType CurveType 
		{
			get 
			{
				return FCurveType;
			}
			set 
			{
				if(FCurveType != value)
				{
					FCurveType = value;
					BuildLine(Points[Points.Count -1]);
				}
			}
		}
    	
        CurveAlignment FCurveAlignment;
		public CurveAlignment CurveAlignment 
		{
			get 
			{
				return FCurveAlignment;
			}
			set 
			{
				if(FCurveAlignment != value)
				{
					FCurveAlignment = value;
					BuildLine(Points[Points.Count -1]);
				}
			}
		}
    }

    public class LinkPath : TempPath, IPath, IDisposable
    {
        #region IPath Members

        public ISolid End
        {
            get;
            private set;
        }

        public Solid EndSolid
        {
            get { return End as Solid; }
        }

        public IPathHost Host
        {
            get { return FHost as IPathHost; }
        }

        #endregion

        protected void PathConstruction(ISolid end)
        {
            End = end;
            BuildLine();

            PNode.TransformChanged += PathChanged;

            //react to position-changes of the pins node
            //which apparently is Parent.Parent for nodes and the PNode itself for Inlets/Outlets
            if (StartSolid is Rectangle)
            	StartSolid.PNode.TransformChanged += new PPropertyEventHandler(PinBoundsChanged);
            else
            	StartSolid.PNode.Parent.Parent.TransformChanged += new PPropertyEventHandler(PinBoundsChanged);
            
            if (EndSolid is Rectangle)
            	EndSolid.PNode.TransformChanged += new PPropertyEventHandler(PinBoundsChanged);
            else
            	EndSolid.PNode.Parent.Parent.TransformChanged += new PPropertyEventHandler(PinBoundsChanged);
        }

        public LinkPath(IGraphElementHost host, ISolid start, ISolid end)
            : base(host, start)
        {
            PathConstruction(end);
        }

        public LinkPath(IGraphElementHost host, ITempPath temppath, ISolid end)
            : base(host, temppath.Start)
        {
            Points = temppath.Points;
            PathConstruction(end);
        }

        protected void BuildLine()
        {
            base.BuildLine(End.GlobalMiddle);
        }

        private void PinBoundsChanged(object sender, PPropertyEventArgs e)
        {
            BuildLine();
        }

        bool FReactingOnTransform = false;

        private void PathChanged(object sender, PPropertyEventArgs e)
        {
            if (!FReactingOnTransform)
            {
                FReactingOnTransform = true;
                for (int i = 0; i < Points.Count; i++)
			    {
                    Points[i] = PNode.LocalToGlobal(Points[i]); 
    			}

                PNode.Matrix = new PMatrix();

                BuildLine();
                FReactingOnTransform = false;
            }
        }
        
        public void Dispose()
        {
            PNode.TransformChanged -= PathChanged;

            if (StartSolid is Rectangle)
            	StartSolid.PNode.TransformChanged -= PinBoundsChanged;
            else
            	StartSolid.PNode.Parent.Parent.TransformChanged -= PinBoundsChanged;
            
            if (EndSolid is Rectangle)
            	EndSolid.PNode.TransformChanged -= PinBoundsChanged;
            else
            	EndSolid.PNode.Parent.Parent.TransformChanged -= PinBoundsChanged;
        }
    }
}
