using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Util;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Event;

using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Core.View.GraphicalEditor;
using VVVV.Core;

namespace VVVV.HDE.GraphicalEditing
{
    public abstract class Solid : GraphElement, ISolid
    {
        public Solid(IGraphElementHost host)
            : base(host)
        {
            if (PPath != null)
            {
                Pen = null;
            }
        }

        protected override PNode CreatePNode()
        {
            return new PPath();
        }

        public IGraphElementHost Host
        {
            get { return FHost; }
        }

        /// <summary>
        /// The position of the coordinate system of this solid
        /// </summary>
        public PointF Position
        {
            get
            {
                return PNode.Offset;
            }
            set
            {
                FMoving = true;
                try
                {
                    PNode.Offset = value;
                }
                finally
                {
                    FMoving = false;
                }
            }
        }

        public PointF GlobalMiddle
        {
            get
            {
                return PNode.GlobalBounds.GetCenter();
            }
        }

        public virtual SizeF Size
        {
            get
            {
                return PNode.Bounds.Size;
            }
            set
            {
                FMoving = true;
                try
                {
                    PNode.Width = value.Width;
                    PNode.Height = value.Height;

                    //move topleft if in center mode
                    if (FPositionMode == PositionMode.Center)
                    {
                        PNode.X = value.Width * -0.5f;
                        PNode.Y = value.Height * -0.5f;
                    }
                    BoundsChanged(PNode, null);
                    //PNode.SignalBoundsChanged();
                }
                finally
                {
                    FMoving = false;
                }
            }
        }

        public RectangleF GlobalBounds
        {
            get
            {
                return PNode.GlobalBounds;
            }
        }

        protected PositionMode FPositionMode;
        public virtual PositionMode PositionMode
        {
            get
            {
                return FPositionMode;
            }
            set
            {
                if (FPositionMode != value)
                {
                    FPositionMode = value;

                    //set new position if mode changed
                    if (FPositionMode == PositionMode.TopLeft)
                    {
                        PNode.X = 0;
                        PNode.Y = 0;
                    }
                    else
                    {
                        PNode.X = Size.Width * -0.5f;
                        PNode.Y = Size.Height * -0.5f;
                    }
                    //BoundsChanged(PNode, null);
                }
            }
        }
    }
}
