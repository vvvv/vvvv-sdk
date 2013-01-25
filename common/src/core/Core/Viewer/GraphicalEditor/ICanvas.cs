using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using VVVV.Core.View.GraphicalEditor;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;

namespace VVVV.Core.Viewer.GraphicalEditor
{
    public interface IContentBounds
    {
        /// <summary>
        /// Gets the bounds of all child elements
        /// </summary>
        RectangleF ContentBounds
        {
            get;
        }
        
        /// <summary>
        /// Gets the center of all child elements
        /// </summary>
        PointF ContentCenter
        {
            get;
        }
        
        /// <summary>
        /// Gets the bounding box of all elements
        /// </summary>
        SizeF ContentSize
        {
            get;
        }
    }
    
    public interface ICanvas : IContentBounds
    {
        ICanvasHost Host
        {
            get;
            set;
        }

        /// <summary>
        /// The root element to add graph elements.
        /// </summary>
        IGraphElement Root
        {
            get;
            set;
        }
        
        /// <summary>
        /// The root element to add links.
        /// </summary>
        IGraphElement LinkRoot
        {
            get;
            set;
        }
        
        /// <summary>
        /// Get/Set the background color of the canvas
        /// </summary>
        Color Color
        {
            get;
            set;
        }
        
        /// <summary>
        /// Get/Set the focus point of the view
        /// </summary>
        PointF ViewCenter
        {
            get;
            set;
        }
        
        /// <summary>
        /// Get/Set the size of the view
        /// </summary>
        SizeF ViewSize
        {
            get;
            set;
        }

        IDot CreateDot(IGraphElementHost host);
        IRectangle CreateRectangle(IGraphElementHost host);
        IPolygon CreatePoly(IGraphElementHost host);
        ICircle CreateCircle(IGraphElementHost host);
        IText CreateText(IGraphElementHost host, string caption);
        
        void RemoveGraphElement(IGraphElement element);

        IPath CreatePath(IPathHost host, ITempPath temppath, ISolid end);
        IPath CreatePath(IPathHost host, ISolid start, ISolid end);
        void RemovePath(IPath path);
        
        /// <summary>
        /// Remove all graph elements from the canvas
        /// </summary>
        void Clear();
        
        /// <summary>
        /// Request a full redraw
        /// </summary>
        void Invalidate();
        
        void ShowToolTip(Point tipPoint, string tip, bool center);
        void HideToolTip();
    }

    public interface ICanvasHost
    {
        ICanvas Canvas
        {
            get;
        }

        bool AcceptsSolid(IIDItem item);
        bool AcceptsSolid(string name);

        void CreateSolid(IIDItem item, PointF pos);
        void CreateSolid(string name, PointF pos);

        void StartPath();
        /// <summary>
        /// typically a language would create an IPathHost, and by that call ICanvas.CreatePath to establish the link
        /// </summary>
        /// <param name="apath"></param>
        void FinishPath(ITempPath apath, IConnectable end);
        
        void FinishPathWithNode(ITempPath apath);
        
        void HighlightElement(IGraphElement element);
        
        void MarqueeSelectionEnded(RectangleF bounds);

        void StartMoveSelected(IEnumerable<IGraphElement> selection);
        void MoveSelected(IEnumerable<IGraphElement> selection);
        void EndMoveSelected(IEnumerable<IGraphElement> selection);
    }
}
