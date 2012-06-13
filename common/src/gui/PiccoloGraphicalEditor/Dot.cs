using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;

using Piccolo.NET;
using Piccolo.NET.Util;
using Piccolo.NET.Nodes;

using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Core.View.GraphicalEditor;

namespace VVVV.HDE.GraphicalEditing
{
	/// <summary>
	/// <b>Dot</b> renders a dot
	/// </summary>
    public class Dot : Solid, IDot
    {
        public Dot(IGraphElementHost host)
            : base(host)
        {
            DotSize = 1;
            PNode.Brush = null;
            PPath.AddRectangle(Position.X, Position.Y, DotSize, DotSize);
        }
      
        public float Rotation
        {
            get 
            {
                return PNode.Rotation; 
            }
            set 
            {
                PNode.Rotation = value; 
            }
        }

        public float DotSize
        {
            get 
            { 
                return Size.Width; 
            }
            set
            {
                Size = new SizeF(value, value);
            }
        }
        
    }
}