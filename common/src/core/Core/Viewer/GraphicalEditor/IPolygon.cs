using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using VVVV.Core.Collections;

namespace VVVV.Core.Viewer.GraphicalEditor
{
    public interface IPolygon : ISolid
    {
        EditableList<PointF> Points
        {
            get;
        }
        
        bool IsClosed
        {
        	get;
        	set;
        }
    }
}
