using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Util;
using UMD.HCIL.Piccolo.Nodes;

using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Core.View.GraphicalEditor;


namespace VVVV.HDE.GraphicalEditing
{
    public class Circle : Solid, ICircle
    {
        public Circle(IGraphElementHost host)
            : base(host)
        {
            Radius = 5;
            if (Parent != null)
            {
                PPath.AddEllipse(Position.X, Position.Y, Radius, Radius);         			
            }
            else
            {
                PPath.AddEllipse(0, 0, Radius, Radius);         			
            }
        }
    
        #region ICircle Members

        public float Radius
        {
            get
            {
                return Size.Width / 2;
            }
            set
            {
                Size = new SizeF(value * 2, value * 2);
            }
        }

        #endregion
    }
}
