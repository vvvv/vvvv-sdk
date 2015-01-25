using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Util;
using UMD.HCIL.Piccolo.Nodes;

using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Core.View.GraphicalEditor;

namespace VVVV.HDE.GraphicalEditing
{
    public class Rectangle : Solid, IRectangle
    {
        public Rectangle(IGraphElementHost host)
            : base(host)
        {
            Position = new PointF(0, 0);
            Size = new SizeF(40, 20);
            PPath.AddRectangle(Position.X, Position.Y, Size.Width, Size.Height);
        }
    }
}
