#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Linq;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.View;
using VVVV.Nodes.Finder;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Utils.Linq;

#endregion usings

//the vvvv node namespace
namespace VVVV.Nodes.WindowSwitcher
{
    [PluginInfo(Name = "WindowSwitcher",
                Category = "VVVV",
                Ignore = true,
                Author = "vvvv group",
                Help = "The Window Switcher")]
    public class WindowSwitcherNode: UserControl, IWindowSwitcher
    {
        #region field declaration
        [Import]
        protected IWindowSwitcherHost FWindowSwitcherHost;
        
        private IHDEHost FHDEHost;
        private NodeView FNodeView;
        private IWindow2 FActiveWindow;
        private NodeView FSelectedNodeView;
        // Track whether Dispose has been called.
        private bool FDisposed = false;
        
        private List<IWindow2> FWindowLIFO = new List<IWindow2>();
        private int FSelectedWindowIndex = 0;
        private readonly NodeFilter FNodeFilter = new NodeFilter();
        #endregion field declaration
        
        #region constructor/destructor
        [ImportingConstructor]
        public WindowSwitcherNode(IHDEHost host)
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FHDEHost = host;
            FHDEHost.WindowAdded += FHDEHost_WindowAdded;
            FHDEHost.WindowRemoved += FHDEHost_WindowRemoved;
            FHDEHost.WindowSelectionChanged += FHDEHost_WindowSelectionChanged;
            FActiveWindow = FHDEHost.ActivePatchWindow;
            
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            FHierarchyViewer.Registry = mappingRegistry;
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!FDisposed)
            {
                if(disposing)
                {
                    // Dispose managed resources.
                    FHDEHost.WindowAdded -= FHDEHost_WindowAdded;
                    FHDEHost.WindowRemoved -= FHDEHost_WindowRemoved;
                    FHDEHost.WindowSelectionChanged -= FHDEHost_WindowSelectionChanged;
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.
                
                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }
        
        private void InitializeComponent()
        {
            this.FDummyTextBox = new System.Windows.Forms.TextBox();
            this.FHierarchyViewer = new VVVV.HDE.Viewer.WinFormsViewer.HierarchyViewer();
            this.SuspendLayout();
            // 
            // FDummyTextBox
            // 
            this.FDummyTextBox.Location = new System.Drawing.Point(3, 3);
            this.FDummyTextBox.Name = "FDummyTextBox";
            this.FDummyTextBox.Size = new System.Drawing.Size(100, 20);
            this.FDummyTextBox.TabIndex = 0;
            this.FDummyTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FDummyTextBoxKeyUp);
            // 
            // FHierarchyViewer
            // 
            this.FHierarchyViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FHierarchyViewer.Location = new System.Drawing.Point(0, 0);
            this.FHierarchyViewer.Name = "FHierarchyViewer";
            this.FHierarchyViewer.Size = new System.Drawing.Size(402, 278);
            this.FHierarchyViewer.TabIndex = 1;
            this.FHierarchyViewer.MouseClick += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FHierarchyViewerClick);
            this.FHierarchyViewer.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FHierarchyViewerKeyUp);
            // 
            // WindowSwitcherNode
            // 
            this.BackColor = System.Drawing.Color.Silver;
            this.Controls.Add(this.FHierarchyViewer);
            this.Controls.Add(this.FDummyTextBox);
            this.DoubleBuffered = true;
            this.Name = "WindowSwitcherNode";
            this.Size = new System.Drawing.Size(402, 278);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        private VVVV.HDE.Viewer.WinFormsViewer.HierarchyViewer FHierarchyViewer;
        private System.Windows.Forms.TextBox FDummyTextBox;
        #endregion constructor/destructor
        
        #region IWindowSwitcher
        public void Initialize()
        {
            //make a tree that only contains nodes that have a window
            //mark nodes with hidden windows as inactive
            if (FNodeView != null)
                FNodeView.Dispose();
            
            FNodeView = FNodeFilter.UpdateFilter("< w t ", FHDEHost.RootNode, false);
            FNodeView.SetActiveWindow(FActiveWindow);
            
            //mark the incoming window as selected
            FHierarchyViewer.Input = FNodeView;
            
            //always on open the second window from the LIFO is selected
            FSelectedWindowIndex = 0;
        }
        
        public void AfterShow()
        {
            //the dummy textbox gets the focus to trigger on CTRL key up
            FDummyTextBox.Focus();
            //focus the viewer to be able to zoom instantly
            FHierarchyViewer.Focus();
        }
        
        private bool IsReservedCaption(string caption)
        {
            return FWindowLIFO[FSelectedWindowIndex].Caption == "VL" || FWindowLIFO[FSelectedWindowIndex].Caption == "Kommunikator";
        }

        public void Up()
        {
            FSelectedWindowIndex = (FWindowLIFO.Count + (FSelectedWindowIndex - 1)) % FWindowLIFO.Count;
            //special treatment for Kommunikator and FiftyEditor: leave em always out for now
            if (IsReservedCaption(FWindowLIFO[FSelectedWindowIndex].Caption))
                Up();
            else
                SelectNode(FWindowLIFO[FSelectedWindowIndex].Node);
        }
        
        public void Down()
        {
            FSelectedWindowIndex = (FSelectedWindowIndex + 1) % FWindowLIFO.Count;
            //special treatment for Kommunikator and FiftyEditor: leave em always out for now
            if (IsReservedCaption(FWindowLIFO[FSelectedWindowIndex].Caption))
                Down();
            else
                SelectNode(FWindowLIFO[FSelectedWindowIndex].Node);
        }
        #endregion IWindowSwitcher
        
        private void SelectNode(INode2 node)
        {
            if (FSelectedNodeView != null)
            {
                FSelectedNodeView.Selected = false;
            }
            
            var flatNodeViewTree = FNodeView.AsDepthFirstEnumerable((nv) => nv.Children);
            FSelectedNodeView = flatNodeViewTree.FirstOrDefault((nv) => nv.Node == node);
            
            if (FSelectedNodeView != null)
            {
                FSelectedNodeView.Selected = true;
            }
        }
        
        void FHDEHost_WindowAdded(object sender, WindowEventArgs args)
        {
            FWindowLIFO.Add(args.Window);
        }
        
        void FHDEHost_WindowRemoved(object sender, WindowEventArgs args)
        {
            FWindowLIFO.Remove(args.Window);
        }
        
        void FHDEHost_WindowSelectionChanged(object sender, WindowEventArgs args)
        {
            FActiveWindow = args.Window;
            
            //remove it from the index it is now
            FWindowLIFO.Remove(FActiveWindow);
            
            //insert it at index 0
            FWindowLIFO.Insert(0, FActiveWindow);
        }
        
        void FHierarchyViewerClick(ModelMapper sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (sender != null)
            {
                var nodeView = sender.Model as NodeView;
                
                if (nodeView != null)
                {
                    FHDEHost.SetComponentMode(nodeView.Node, ComponentMode.InAWindow);
                    FHierarchyViewer.HideToolTip();
                    FWindowSwitcherHost.HideMe();
                }
            }
        }
        
        void FDummyTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (!e.Control)
            {
                FHierarchyViewer.HideToolTip();
                FWindowSwitcherHost.HideMe();
                FHDEHost.SetComponentMode(FWindowLIFO[FSelectedWindowIndex].Node, ComponentMode.InAWindow);
            }
        }
        
        void FHierarchyViewerKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.ControlKey) || (e.KeyData == Keys.Control))
            {
                FHierarchyViewer.HideToolTip();
                FWindowSwitcherHost.HideMe();
                FHDEHost.SetComponentMode(FWindowLIFO[FSelectedWindowIndex].Node, ComponentMode.InAWindow);
            }
        }
    }
}
