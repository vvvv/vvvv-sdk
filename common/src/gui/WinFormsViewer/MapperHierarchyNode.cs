using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using VVVV.Core;
using VVVV.Core.Collections;
using VVVV.Core.Collections.Sync;
using VVVV.Core.View;
using VVVV.Core.View.GraphicalEditor;
using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Utils.Linq;

namespace VVVV.HDE.Viewer.WinFormsViewer
{
    /// <summary>
    /// Based on IGraphElementHost MapperHierarchyNode provides access to the
    /// ModelMapper used to retrieve information for the model element
    /// stored in this node.
    /// Implements IViewableList so it can easily be synchronized with the root node collection
    /// of the TreeView.
    /// </summary>
    public class MapperHierarchyNode: IGraphElementHost, IHoverable, IClickable, ISelectable, IDisposable
    {
        /// <summary>
        /// The ModelMapper containing all the necessary mappings for the
        /// stored model element.
        /// </summary>
        public ModelMapper Mapper {get; private set;}
        
        public event ClickHandler MouseClick;
        protected virtual void OnClick(MouseEventArgs e)
        {
            if (MouseClick != null) {
                MouseClick(Mapper, e);
            }
        }
        
        public event ClickHandler MouseDoubleClick;
        protected virtual void OnDoubleClick(MouseEventArgs e)
        {
            if (MouseClick != null) {
                MouseDoubleClick(Mapper, e);
            }
        }
        
        ICanvas FCanvas;
        IRectangle FBackground;
        IRectangle FIcon;
        IText FText;
        IPolygon FPoly;
        IRectangle FLinkSourceRectangle;
        IRectangle FLinkSinkRectangle;
        
        Pen FTextColor;
        Pen FTextHoverColor;
        Brush FBackColor;
        Brush FBackHoverColor;
        Pen FOutlineColor;
        
        EditableList<MapperHierarchyNode> FSubTree = new EditableList<MapperHierarchyNode>();
        Synchronizer<MapperHierarchyNode, object> FSynchronizer;

        HierarchyViewer FViewer;
        ISelectable FSelectable;
        INamed FNamed;
        private readonly ILinkSource FLinkSource;
        private readonly ILinkSink FLinkSink;
        IDecoratable FDecoratable;
        const int CHorizontalPatchOffset = 2;
        const int CVerticalPatchOffset = 0;
        const int CElementOffset = 2;
        int FTextOffset = 18;
        int FLinkOffset = 0;
        int FDepth = 0;
        
        public MapperHierarchyNode FormerSibling {get; private set;}
        public IEditableList<MapperHierarchyNode> SubTree {get{return FSubTree;}}
        public object Tag {get; private set;}
        public PointF ContentCenter {get{return FBackground.ContentCenter;}}
        public SizeF ContentSize {get{return FBackground.ContentSize;}}
        
        public string Description
        {
            get
            {
                if (Mapper.CanMap<IDescripted>())
                    return Mapper.Map<IDescripted>().Description;
                else
                    return "";
            }
        }
        
        public int SubTreeWidth
        {
            get
            {
                int width = 0;
                foreach (var n in FSubTree)
                    width += n.SubTreeWidth;
                
                width += FSubTree.Count * CElementOffset;
                
                if (FSubTree.Count > 0)
                    width += CVerticalPatchOffset; //for break after patch
                else
                    width += FViewer.DIP(20);
                return width;
            }
        }
        
        public int ChildrenWithSubtreeCount
        {
            get
            {
                int count = 0;
                foreach (var n in FSubTree)
                    if (n.FSubTree.Count > 0)
                        count++;
                return count;
            }
        }

        public MapperHierarchyNode(ModelMapper mapper, ICanvas canvas, IGraphElement parent, HierarchyViewer viewer)
            :base()
        {
            Mapper = mapper;
            FCanvas = canvas;
            FViewer = viewer;
            
            MouseClick += FViewer.MouseClickHandler;
            MouseDoubleClick += FViewer.MouseDoubleClickHandler;
            Tag = mapper.Model;
            
            //graphelements
            FBackground = canvas.CreateRectangle(this);
            FPoly = canvas.CreatePoly(this);
            FText = canvas.CreateText(null, "");
            FIcon = FCanvas.CreateRectangle(null);
            
            parent.Add(FBackground);
            FBackground.Add(FPoly);
            FBackground.Add(FText);
            FBackground.Add(FIcon);
            
            //compute level of depth
            IGraphElement p = FBackground;
            while (p.Parent != null)
            {
                FDepth++;
                p = p.Parent;
            }
            FDepth -= 1;
            
            //init static properties via Mapper
            if (Mapper.CanMap<ISelectable>())
            {
                FSelectable = Mapper.Map<ISelectable>();
                FSelectable.SelectionChanged += selectable_SelectionChanged;
                Selected = FSelectable.Selected;
            }
            
            if (Mapper.CanMap<IDecoratable>())
            {
                FDecoratable = Mapper.Map<IDecoratable>();
                FDecoratable.DecorationChanged += decorated_DecorationChanged;
            }
            
            if (Mapper.CanMap<ILinkSource>())
            {
                FLinkSource = Mapper.Map<ILinkSource>();
                
                FLinkSourceRectangle = FCanvas.CreateRectangle(null);
                FBackground.Add(FLinkSourceRectangle);
                FLinkOffset = FTextOffset;
            }
            
            if (Mapper.CanMap<ILinkSink>())
            {
                FLinkSink = Mapper.Map<ILinkSink>();
                
                FLinkSinkRectangle = FCanvas.CreateRectangle(null);
                FBackground.Add(FLinkSinkRectangle);
            }
            
            if (Mapper.CanMap<INamed>())
            {
                FNamed = Mapper.Map<INamed>();
                FNamed.Renamed += named_Renamed;
                SetCaption(FNamed.Name);
            }
            
            if (Mapper.CanMap<IParent>())
            {
                var node = Mapper.Map<IParent>();
                if (node.Childs != null)
                {
                    // Keep Nodes and items in sync
                    FSynchronizer = FSubTree.SyncWith(node.Childs, CreateChildNode);
                    FSynchronizer.Synced += synchronizer_Synced;
                }
            }

            //init dynamic properties via Mapper
            UpdateColors();
            UpdateIcon();
            UpdateLinkSink();
            UpdateLinkSource();
        }
        
        #region IDisposable

        protected bool FDisposed;

        public void Dispose()
        {
            if (FDisposed) return;
            
            if (FSynchronizer != null)
            {
                FSynchronizer.Synced -= synchronizer_Synced;
                FSynchronizer.Dispose();
            }
            
            if (FNamed != null)
                FNamed.Renamed -= named_Renamed;
            
            if (FDecoratable != null)
                FDecoratable.DecorationChanged -= decorated_DecorationChanged;
            
            if (FSelectable != null)
                FSelectable.SelectionChanged -= selectable_SelectionChanged;
            
            foreach (var n in FSubTree)
                n.Dispose();
            FSubTree.Dispose();
            
            FPoly.Clear();
            FText.Clear();
            FIcon.Clear();
            FBackground.Clear();
            if (FBackground.Parent != null)
                FBackground.Parent.Remove(FBackground);
            
            MouseClick -= FViewer.MouseClickHandler;
            MouseDoubleClick -= FViewer.MouseDoubleClickHandler;
            
            Mapper.Dispose();

            FDisposed = true;
        }
        
        #endregion IDisposable
        
        private void SetCaption(string caption)
        {
            FText.Caption = caption;
            FBackground.Size = new SizeF(FText.Size.Width + FTextOffset + FLinkOffset, FText.Size.Height);
            
            //TODO: the following only broadens depth, but never shortens in case the former longest name is now shorter...
            //special treatment for root
            if (FBackground.Parent is IDot && !FViewer.ShowRoot)
                FViewer.DepthOffsets[0] = 0;
            else if (!FViewer.DepthOffsets.ContainsKey(FDepth))
                FViewer.DepthOffsets.Add(FDepth, FBackground.Size.Width);
            else
                FViewer.DepthOffsets[FDepth] = Math.Max(FBackground.Size.Width, FViewer.DepthOffsets[FDepth]);
        }
        
        #region MapperStuff
        public MapperHierarchyNode CreateChildNode(object item)
        {
            var mapper = Mapper.CreateChildMapper(item);
            var node = new MapperHierarchyNode(mapper, FCanvas, FBackground, FViewer);
            if (FSubTree.Count > 0)
                node.FormerSibling = FSubTree[FSubTree.Count - 1];

            return node;
        }
        
        void synchronizer_Synced(object sender, SyncEventArgs<MapperHierarchyNode, object> args)
        {
            switch (args.Action)
            {
                case CollectionAction.Added:
                    FViewer.UpdateView();
                    break;
                case CollectionAction.Removed:
                    FViewer.UpdateView();
                    break;
                case CollectionAction.Cleared:
                    FViewer.UpdateView();
                    break;
                case CollectionAction.OrderChanged:
                    FViewer.UpdateView();
                    break;
                case CollectionAction.Updating:
                    FViewer.BeginUpdate();
                    break;
                case CollectionAction.Updated:
                    FViewer.EndUpdate();
                    break;
            }
        }
        
        void named_Renamed(INamed sender, string newName)
        {
            //label, SRChannel or Comment has changed
            if (string.IsNullOrEmpty(newName))
                newName = FNamed.Name;
            SetCaption(newName);
            
            UpdateIcon();
            UpdateLinkSink();
            UpdateLinkSource();
            
            FViewer.UpdateView();
        }

        void decorated_DecorationChanged()
        {
            //active window state has changed
            UpdateColors();
        }
        #endregion MapperStuff
        
        #region ISelectable
        public event SelectionChangedHandler SelectionChanged;

        protected virtual void OnSelectionChanged()
        {
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }

        void selectable_SelectionChanged(ISelectable sender, EventArgs args)
        {
            Selected = FSelectable.Selected;
        }
        
        bool FSelected;
        public bool Selected
        {
            get
            {
                return FSelected;
            }
            set
            {
                if (value != FSelected)
                {
                    FSelected = value;

                    if (FBackground.Pen != null)
                        FBackground.Pen.Dispose();

                    if (FSelected)
                        FBackground.Pen = new Pen(Color.Black, 2);
                    else
                        FBackground.Pen = null;

                    FPoly.Pen = FBackground.Pen;
                    OnSelectionChanged();
                }
            }
        }
        #endregion ISelectable
        
        #region IHoverable
        public void MouseEnter(PointF mousePos)
        {
            SetHover(true);
            FViewer.ShowToolTip(this);
        }
        
        public void MouseLeave(PointF mousePos, TimeSpan timeSinceEnter)
        {
            SetHover(false);
            FViewer.HideToolTip();
        }
        
        public void MouseHover(PointF mousePos)
        {
            //throw new NotImplementedException();
        }
        #endregion IHoverable
        
        #region IClickable
        public void Click(PointF mousePos, Mouse_Buttons mouseButton)
        {
            var args = new MouseEventArgs((MouseButtons) mouseButton, 0, (int) mousePos.X, (int) mousePos.Y, 0);
            OnClick(args);
        }
        
        public void DoubleClick(PointF mousePos, Mouse_Buttons mouseButton)
        {
            var args = new MouseEventArgs((MouseButtons) mouseButton, 0, (int) mousePos.X, (int) mousePos.Y, 0);
            OnDoubleClick(args);
        }
        
        public void MouseDown(PointF mousePos, Mouse_Buttons mouseButton)
        {
            //            throw new NotImplementedException();
        }
        
        public void MouseUp(PointF mousePos, Mouse_Buttons mouseButton)
        {
            //            throw new NotImplementedException();
        }
        #endregion IClickable
        
        private void UpdateIcon()
        {
            //special treatment for root
            if (FBackground.Parent is IDot && !FViewer.ShowRoot)
                return;
            
            FIcon.Clear();
            FIcon.Brush = null;
            FIcon.Position = new PointF(3, 3);
            FIcon.Visible = false;
            
            if (FDecoratable != null)
            {
                if (FDecoratable.Icon > NodeIcon.None)
                {
                    FIcon.Visible = true;
                    
                    if (FDecoratable.Icon == NodeIcon.GUI || FDecoratable.Icon == NodeIcon.GUICode || FDecoratable.Icon == NodeIcon.GUIPatch)
                    {
                        FIcon.Pen = FTextColor;
                        FIcon.Size = new SizeF(16, 12);
                    }
                    if (FDecoratable.Icon == NodeIcon.Patch || FDecoratable.Icon == NodeIcon.GUIPatch)
                    {
                        var n = FCanvas.CreateRectangle(null);
                        n.Pen = FTextColor;
                        n.Size = new SizeF(8, 2);
                        n.Position = new PointF(3, 2);
                        FIcon.Add(n);
                        n = FCanvas.CreateRectangle(null);
                        n.Pen = FTextColor;
                        n.Size = new SizeF(8, 2);
                        n.Position = new PointF(6, 5);
                        FIcon.Add(n);
                        n = FCanvas.CreateRectangle(null);
                        n.Pen = FTextColor;
                        n.Size = new SizeF(8, 2);
                        n.Position = new PointF(3, 8);
                        FIcon.Add(n);
                    }
                    
                    if (FDecoratable.Icon == NodeIcon.Code || FDecoratable.Icon == NodeIcon.GUICode)
                    {
                        IPolygon line;
                        var y = 3;
                        for (int i = 0; i < 4; i++)
                        {
                            line = FCanvas.CreatePoly(null);
                            line.IsClosed = false;
                            line.Pen = FTextColor;
                            line.Brush = null;
                            line.Points.Add(new PointF(3, y));
                            if (i == 0)
                                line.Points.Add(new PointF(7, y));
                            else
                                line.Points.Add(new PointF(13, y));
                            FIcon.Add(line);
                            y += 2;
                        }
                    }
                    
                    if (FDecoratable.Icon == NodeIcon.Comment)
                    {
                        IPolygon line;
                        line = FCanvas.CreatePoly(null);
                        line.IsClosed = false;
                        line.Pen = FTextColor;
                        line.Brush = null;
                        line.Points.Add(new PointF(5, 12));
                        line.Points.Add(new PointF(7, 0));
                        FIcon.Add(line);
                        
                        line = FCanvas.CreatePoly(null);
                        line.IsClosed = false;
                        line.Pen = FTextColor;
                        line.Brush = null;
                        line.Points.Add(new PointF(7, 12));
                        line.Points.Add(new PointF(9, 0));
                        FIcon.Add(line);
                    }
                    
                    if (FDecoratable.Icon == NodeIcon.IONode)
                    {
                        IPolygon line;
                        line = FCanvas.CreatePoly(null);
                        line.IsClosed = false;
                        line.Pen = FTextColor;
                        line.Brush = null;
                        line.Points.Add(new PointF(2, 2));
                        line.Points.Add(new PointF(2, 10));
                        FIcon.Add(line);
                        
                        ICircle circle;
                        circle = FCanvas.CreateCircle(null);
                        circle.PositionMode = PositionMode.Center;
                        circle.Pen = FTextColor;
                        circle.Brush = null;
                        circle.Position = new PointF(8, 6);
                        circle.Radius = 4;
                        FIcon.Add(circle);
                    }
                }
            }
        }
        
        private void UpdateColors()
        {
            if (FDecoratable != null)
            {
                FTextColor = FDecoratable.TextColor;
                FTextHoverColor = FDecoratable.TextHoverColor;
                FBackColor = FDecoratable.BackColor;
                FBackHoverColor = FDecoratable.BackHoverColor;
                FOutlineColor = FDecoratable.OutlineColor;
                if (!string.IsNullOrEmpty(FDecoratable.Text))
                    FText.Caption = FDecoratable.Text;
            }
            
            SetColor(false);
        }
        
        private void UpdateLinkSource()
        {
            if (FLinkSourceRectangle != null)
            {
                FLinkSourceRectangle.Brush = Brushes.Black;
                FLinkSourceRectangle.PositionMode = PositionMode.Center;
                FLinkSourceRectangle.Size = new SizeF(7, 7);
                FLinkSourceRectangle.Position = new PointF(FBackground.Size.Width - FTextOffset / 2, FBackground.Size.Height / 2);
            }
        }
        
        private void UpdateLinkSink()
        {
            if (FLinkSinkRectangle != null)
            {
                FLinkSinkRectangle.Brush = Brushes.Black;
                FLinkSinkRectangle.PositionMode = PositionMode.Center;
                FLinkSinkRectangle.Size = new SizeF(5, 5);
                FLinkSinkRectangle.Position = new PointF(FTextOffset / 2, FBackground.Size.Height / 2);
            }
        }
        
        public void UpdateBounds()
        {
            //special treatment for root
            if (FBackground.Parent is IDot && !FViewer.ShowRoot)
            {
                FBackground.Visible = false;
                FPoly.Visible = false;
                FText.Visible = false;
                FIcon.Visible = false;
            }
            else
            {
                float offset = 0;
                
                if (FViewer.DepthOffsets.ContainsKey(FDepth-1))
                    offset = FViewer.DepthOffsets[FDepth-1];
                
                float x = offset + CHorizontalPatchOffset;
                float y = 0;
                if (FormerSibling != null)
                {
                    var stw = FormerSibling.SubTreeWidth;
                    y = FormerSibling.FBackground.Position.Y + stw + CElementOffset;
                }

                var height = Math.Max(FBackground.Size.Height, SubTreeWidth);
                if (FSubTree.Count > 0)
                    height -= (CVerticalPatchOffset + CElementOffset);
                
                float width = 0;
                if (FViewer.DepthOffsets.ContainsKey(FDepth))
                    width = FViewer.DepthOffsets[FDepth];
                FBackground.Size = new SizeF(width, height);
                
                FPoly.Points.Clear();
                FPoly.Points.Add(new PointF(0, height/2-10));
                FPoly.Points.Add(new PointF(width-20, 0));
                FPoly.Points.Add(new PointF(width, 0));
                FPoly.Points.Add(new PointF(width, height));
                FPoly.Points.Add(new PointF(width-20, height));
                FPoly.Points.Add(new PointF(0, height/2+10));
                FPoly.IsClosed = true;
                
                FText.Position = new PointF(FTextOffset, height/2-FViewer.DIP(10));
                FIcon.Position = new PointF(3, 3 + height/2-10);
                
                FBackground.Position = new PointF(x, y);
                
                FBackground.Visible = FSubTree.Count == 0;
                FPoly.Visible = !FBackground.Visible;
            }
            
            MapperHierarchyNode sibling = null;
            foreach (var n in FSubTree)
            {
                n.FormerSibling = sibling;
                n.UpdateBounds();
                sibling = n;
            }
        }
        
        private void SetColor(bool hover)
        {
            if (hover)
            {
                FBackground.Brush = FBackHoverColor;
                FText.Pen = FTextHoverColor;
            }
            else
            {
                FBackground.Brush = FBackColor;
                FText.Pen = FTextHoverColor;
            }
            
            FPoly.Brush = FBackground.Brush;
        }
        
        private readonly List<IPath> FLinkPaths = new List<IPath>();
        private void SetHover(bool hover)
        {
            SetColor(hover);
            
            if (hover)
            {
                // Draw links
                if (FLinkSource != null)
                {
                    // Draw from this source to all sinks
                    foreach (var node in FViewer.RootNode.AsDepthFirstEnumerable((n) => n.SubTree))
                    {
                        var linkSink = node.FLinkSink;
                        if (linkSink != null && linkSink.Accepts(FLinkSource))
                        {
                            var linkPath = FCanvas.CreatePath(null, FLinkSourceRectangle, node.FLinkSinkRectangle);
                            linkPath.Pen = FTextColor;
                            
                            FViewer.Background.Add(linkPath);
                            
                            // Store the line so we can remove it later
                            FLinkPaths.Add(linkPath);
                        }
                    }
                }
                
                if (FLinkSink != null)
                {
                    // Find source for this sink and draw one link
                    foreach (var node in FViewer.RootNode.AsDepthFirstEnumerable((n) => n.SubTree))
                    {
                        var linkSource = node.FLinkSource;
                        if (linkSource != null && FLinkSink.Accepts(linkSource))
                        {
                            var linkPath = FCanvas.CreatePath(null, FLinkSinkRectangle, node.FLinkSourceRectangle);
                            linkPath.Pen = FTextColor;
                            
                            FViewer.Background.Add(linkPath);
                            
                            // Store the line so we can remove it later
                            FLinkPaths.Add(linkPath);
                        }
                    }
                }
            }
            else
            {
                // Hide links
                foreach (var linkLine in FLinkPaths)
                {
                    FViewer.Background.Remove(linkLine);
                }
                FLinkPaths.Clear();
            }
        }
        
        public MapperHierarchyNode FindNode(object node)
        {
            if (Tag == node)
                return this;
            else
                foreach (MapperHierarchyNode n in SubTree)
            {
                var result = n.FindNode(node);
                if (result != null)
                    return result;
            }
            
            return null;
        }
    }
}