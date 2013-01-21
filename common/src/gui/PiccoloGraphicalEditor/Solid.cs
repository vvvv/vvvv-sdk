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
            SubscribeToBoundsChanged();
        }

        public override void Dispose()
        {
            UnsubscribeFromBoundsChanged();
            base.Dispose();
        }

        protected void SubscribeToBoundsChanged()
        {
            PNode.BoundsChanged += HandlePNodeBoundsChanged;
        }

        protected void UnsubscribeFromBoundsChanged()
        {
            PNode.BoundsChanged -= HandlePNodeBoundsChanged;
        }

        protected override PNode CreatePNode()
        {
            return new PPath();
        }

        public IGraphElementHost Host
        {
            get { return FHost; }
        }

        protected void HandlePNodeBoundsChanged(object sender, PPropertyEventArgs e)
        {
            var bounds = PNode.Bounds;
            if (PositionMode == PositionMode.TopLeft)
                Position = Position.Add(bounds.Location);
            else
                Position = Position.Add(bounds.GetCenter());
            Size = bounds.Size;
        }

        protected override void OnMoved(RectangleF pNodeBounds)
        {
            // PNode bounds are different. Send ours (not available in base class GraphElement).
            Movable.UpdateBounds(new RectangleF(Position, Size));
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
                    UnsubscribeFromBoundsChanged();
                    PNode.Width = value.Width;
                    PNode.Height = value.Height;
                    UpdatePNodePosition();
                    SubscribeToBoundsChanged();
                    TransformChanged(PNode, null);
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
                    UnsubscribeFromBoundsChanged();
                    UpdatePNodePosition();
                    SubscribeToBoundsChanged();
                    //BoundsChanged(PNode, null);
                }
            }
        }

        private void UpdatePNodePosition()
        {
            //move topleft if in center mode
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
        }
    }
}
