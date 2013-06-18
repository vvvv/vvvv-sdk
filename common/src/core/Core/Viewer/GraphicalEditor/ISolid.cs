using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using VVVV.Core.View.GraphicalEditor;

namespace VVVV.Core.Viewer.GraphicalEditor
{
	public enum PositionMode
	{
		TopLeft,
		Center
	}
	
    public interface ISolid : IGraphElement
    {
        PointF Position
        {
            get;
            set;
        }

        PositionMode PositionMode
        {
            get;
            set;
        }

        PointF GlobalMiddle
        {
            get;
        }

        IGraphElementHost Host
        {
            get;
        }

        Brush Brush
        {
            get;
            set;
        }
        
        Pen Pen
        {
        	get;
        	set;
        }

        SizeF Size
        {
            get;
        }

        RectangleF GlobalBounds
        {
            get;
        }
    }
}
