using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Collections.Sync;
using VVVV.Core.Commands;
using VVVV.Core.Menu;
using VVVV.Core.View;
using VVVV.Core.Viewer;

using VVVV.HDE.GraphicalEditing;
using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Core.View.GraphicalEditor;

namespace VVVV.HDE.Viewer.WinFormsViewer
{
    public partial class HierarchyViewer : Viewer, ISelectionProvider, ICamera
    {
        private ICanvas FCanvas;
        private ModelMapper FRootMapper;
        private MapperHierarchyNode FRootNode;
        private int FUpdateLockCount;
        private bool FNeedsUpdate;
        private float FDPIFactor;
        public Dictionary<int, float> DepthOffsets = new Dictionary<int, float>();
        
        public new event ClickHandler MouseClick;
        protected void OnMouseClick(ModelMapper sender, MouseEventArgs e)
        {
            if (MouseClick != null)
                MouseClick(sender, e);
        }
        public new event ClickHandler MouseDoubleClick;
        protected void OnMouseDoubleClick(ModelMapper sender, MouseEventArgs e)
        {
            if (MouseDoubleClick != null)
                MouseDoubleClick(sender, e);
        }
        public new event ClickHandler MouseDown;
        protected virtual void OnMouseDown(ModelMapper sender, MouseEventArgs e)
        {
            if (MouseDown != null) {
                MouseDown(sender, e);
            }
        }
        
        public IDot Background {get; private set;}
        public IDot Foreground {get; private set;}
        public bool ShowLinks {get; set;}
        public bool ShowRoot {get; set;}
        internal MapperHierarchyNode RootNode
        {
            get
            {
                return FRootNode;
            }
        }

        #region initialization
        public HierarchyViewer()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            FCanvas = FGraphEditor as ICanvas;
            Debug.Assert(FCanvas != null);

            using (var g = this.CreateGraphics())
            {
                FDPIFactor = (g.DpiY / 96.0f);
            }

            FCanvas.Root = FCanvas.CreateDot(null);
            Background = FCanvas.CreateDot(null);
            Foreground = FCanvas.CreateDot(null);

            FGraphEditor.MouseDown += FGraphEditorMouseDown;
            FGraphEditor.MouseClick += FGraphEditorMouseClick;
            FGraphEditor.Resize += FGraphEditorResize;
        }

        public int DIP(float value)
        {
            return (int)(value * FDPIFactor);
        }

        private void ToolTipPopupHandler(object sender, PopupEventArgs e)
        {
            e.ToolTipSize = new Size(Math.Min(e.ToolTipSize.Width, 300), e.ToolTipSize.Height);
        }
        
        #endregion initialization
        
        protected override void InitializeMappingRegistry(MappingRegistry registry)
        {
            registry.RegisterDefaultInstance<ICamera>(this);
        }
        
        #region tree building
        public override void Reload()
        {
            //clean up
            FCanvas.Clear();
            Foreground.Clear();
            Background.Clear();
            DepthOffsets.Clear();
            
            if (FRootMapper != null)
                FRootMapper.Dispose();
            
            FRootMapper = new ModelMapper(Input, Registry);
            
            FCanvas.Root.Add(Background);
            FCanvas.Root.Add(Foreground);
            
            //start building the tree from the root
            if (FRootNode != null)
                FRootNode.Dispose();
            
            FRootNode = new MapperHierarchyNode(FRootMapper, FCanvas, Background, this);

            UpdateView();
            ViewAll();
        }
        
        public void UpdateView()
        {
            if (FUpdateLockCount > 0 || FRootNode == null)
            {
                FNeedsUpdate = true;
                return;
            }
            
            FNeedsUpdate = false;
            
            //after tree is built...
            //compute sibling offsets
            FRootNode.UpdateBounds();
            /*
            //recompute depthoffsets from given view
           // if (FRootNode.ContentSize.Width / FRootNode.ContentSize.Height < 0.8f)
            {
                var fullDepth = 0f;
                foreach (var d in DepthOffsets)
                    fullDepth += d.Value;
                
                // prepare the temp list
                List<KeyValuePair<int, float>> list = new List<KeyValuePair<int, float>>(DepthOffsets);

                // iterate through the list and then change the dictionary object
                foreach (var kvp in list)
                    DepthOffsets[kvp.Key] = (kvp.Value / fullDepth) * FCanvas.ViewSize.Width;
                
                //now recompute sibling offsets
                FRootNode.UpdateBounds();
            }*/
            
            ViewAll();
        }

        public void BeginUpdate()
        {
            FUpdateLockCount++;
        }
        
        public void EndUpdate()
        {
            FUpdateLockCount--;
            if (FUpdateLockCount == 0 && FNeedsUpdate)
                UpdateView();
        }
        #endregion tree building
        
        public void ShowToolTip(MapperHierarchyNode node)
        {
            var point = this.PointToClient(Control.MousePosition);
            point = new Point(point.X, point.Y + 20);
            FToolTip.Show(node.Description, this, point);
        }
        
        public void HideToolTip()
        {
            FToolTip.Hide(this);
        }

        public void MouseClickHandler(ModelMapper sender, MouseEventArgs e)
        {
            OnMouseClick(sender, e);
        }
        
        public void MouseDoubleClickHandler(ModelMapper sender, MouseEventArgs e)
        {
            OnMouseDoubleClick(sender, e);
        }
        
        #region functionality
        public void View(object node)
        {
            var mhn = FRootNode.FindNode(node);
            FCanvas.ViewCenter = mhn.ContentCenter;
            FCanvas.ViewSize = new SizeF(mhn.ContentSize.Width + 50, mhn.ContentSize.Height + 50);
        }
        
        public void ViewAll()
        {
            FCanvas.ViewCenter = FCanvas.ContentCenter;
            FCanvas.ViewSize = new SizeF(FCanvas.ContentSize.Width + 50, FCanvas.ContentSize.Height + 50);
        }
        #endregion functionality
        
        #region ISelectionProvider Members
        
        public ISelection CurrentSelection
        {
            get
            {
                return Selection.Empty;
            }
        }
        
        public event SelectionChangedEventHandler SelectionChanged;
        
        protected virtual void OnSelectionChanged(ISelection selection)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new SelectionChangedEventArgs(selection));
            }
        }
        #endregion
        
        void FGraphEditorResize(object sender, EventArgs e)
        {
            ViewAll();
        }
        
        void FGraphEditorKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }
        
        void FGraphEditorKeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }
        
        void FGraphEditorKeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }
        
        void FGraphEditorMouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(null, e);
        }
        
        void FGraphEditorMouseClick(object sender, MouseEventArgs e)
        {
            OnMouseClick(null, e);
        }
    }
    
    public interface ICamera
    {
        void View(object node);
        void ViewAll();
    }
}
