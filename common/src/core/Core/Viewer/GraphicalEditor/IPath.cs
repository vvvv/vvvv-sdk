using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using VVVV.Core.View.GraphicalEditor;

namespace VVVV.Core.Viewer.GraphicalEditor
{
    public enum ArrowType
    {
        Arrow,
        Plain
    }
    
    public enum CurveType
    {
    	Line,
    	Edge,
    	Bow,
    	Curve
    }
    
    public enum CurveAlignment
    {
    	Horizontal,
    	Vertical
    }

    public interface ITempPath : IGraphElement
    {
        ISolid Start
        {
            get;
        }

        List<PointF> Points
        {
            get;
        }

        ArrowType ArrowType
        {
            get;
            set;
        }
        
        CurveType CurveType
        {
        	get;
        	set;
        }
        
        CurveAlignment CurveAlignment
        {
        	get;
        	set;
        }

        Pen Pen
        {
            get;
            set;
        }
    }
    
    public interface IPath : ITempPath
    {

        ISolid End
        {
            get;
        }

        IPathHost Host
        {
            get;
        }
    }


    public interface IPathHost : IGraphElementHost
    {
        List<PointF> Points
        {
            get;
        }
    
        void UpdatePoints();
    }
}
