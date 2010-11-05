#region usings
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.View;
using VVVV.PluginInterfaces.V2;
#endregion usings

//the vvvv node namespace
namespace VVVV.Nodes.WindowSwitcher
{
    [PluginInfo(Name = "WindowSwitcher",
                Category = "VVVV",
                Ignore = true,
                Author = "vvvv group",
                Help = "The Window Switcher")]
    public class WindowSwitcherNode: UserControl, IWindowSwitcher, IWindowListener, IWindowSelectionListener
    {
        #region field declaration
        [Import]
        protected IWindowSwitcherHost FWindowSwitcherHost;
        
        private IHDEHost FHDEHost;
        private INode FRoot;
        private PatchNode FFullTree;
        private PatchNode FWindowTree;
        private PatchNode FSelectedPatchNode;
        // Track whether Dispose has been called.
        private bool FDisposed = false;
        
        private List<IWindow> FWindowLIFO = new List<IWindow>();
        private Dictionary<INode, IWindow> FWindowNodes = new Dictionary<INode, IWindow>();
        private int FSelectedWindowIndex = 0;
        
        #endregion field declaration
        
        #region constructor/destructor
        [ImportingConstructor]
        public WindowSwitcherNode(IHDEHost host)
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FHDEHost = host;
            FHDEHost.AddListener(this);
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
                    FHDEHost.RemoveListener(this);
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
            this.FHierarchyViewer.Click += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FHierarchyViewerClick);
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
            if (FRoot == null)
            {
                FHDEHost.GetRoot(out FRoot);
                var mappingRegistry = new MappingRegistry();
                mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
                FHierarchyViewer.Registry = mappingRegistry;
                
                FFullTree = new PatchNode(FRoot);
            }
            
            //make a tree that only contains nodes that have a window
            //mark nodes with hidden windows as inactive
            FWindowTree = new PatchNode(null);
            FWindowTree.Node = FRoot;
            AddWindowNodes(FWindowTree, FFullTree);
            
            //mark the incoming window as selected
            FHierarchyViewer.Input = FWindowTree;
            
            //always on open the second window from the LIFO is selected
            FSelectedWindowIndex = 0;
        }
        
        public void AfterShow()
        {
            //the dummy textbox gets the focus to trigger on CTRL key up
            FDummyTextBox.Focus();
            //focus the viewer to be able to zoom instantly
            //FHierarchyViewer.Focus();
        }
        
        public void Up()
        {
            FSelectedWindowIndex = (FWindowLIFO.Count + (FSelectedWindowIndex - 1)) % FWindowLIFO.Count;
            //special treatment for Kommunikator: leave it always out for now
            if (FWindowLIFO[FSelectedWindowIndex].Caption == "Kommunikator")
                Up();
            else
                SelectNode(FWindowLIFO[FSelectedWindowIndex].GetNode());
        }
        
        public void Down()
        {
            FSelectedWindowIndex = (FSelectedWindowIndex + 1) % FWindowLIFO.Count;
            //special treatment for Kommunikator: leave it always out for now
            if (FWindowLIFO[FSelectedWindowIndex].Caption == "Kommunikator")
                Down();
            else
                SelectNode(FWindowLIFO[FSelectedWindowIndex].GetNode());
        }
        #endregion IWindowSwitcher
        
        private void SelectNode(INode node)
        {
            if (FSelectedPatchNode != null)
                FSelectedPatchNode.Selected = false;
            
            FSelectedPatchNode = SelectNodeOfTree(FWindowTree, node);
            FHierarchyViewer.Redraw();
        }
        
        private PatchNode SelectNodeOfTree(PatchNode patchNode, INode node)
        {
            PatchNode result = null;
            if (patchNode.Node == node)
                result = patchNode;
            else
                foreach (PatchNode pn in patchNode)
            {
                result = SelectNodeOfTree(pn, node);
                if (result != null)
                    break;
            }
            
            if (result != null)
                result.Selected = true;
            return result;
        }
        
        private void AddWindowNodes(PatchNode result, PatchNode sourceTree)
        {
            //go through childnodes of sourceTree recursively and copy nodes that have a window
            foreach (PatchNode pn in sourceTree)
            {
                var temp = new PatchNode(null);
                temp.Node = pn.Node;
                
                var hasPatch = false;
                var hasGUI = temp.Node.HasGUI();
                if (hasGUI)
                {
//                    if (FWindowNodes.ContainsKey(temp.Node))
//                        temp.HasVisibleGUI = true;
                }
                else
                {
                    hasPatch = temp.Node.HasPatch();
                    if (hasPatch)
                    {
//                        if (FWindowNodes.ContainsKey(temp.Node))
//                            temp.HasVisiblePatch = true;
                    }
                }
                AddWindowNodes(temp, pn);
                
                if ((hasPatch && temp.Node.GetNodeInfo().Type != NodeType.Module) || hasGUI)
                    result.Add(temp);
            }
        }
        
        #region IWindowListener
        public void WindowAddedCB(IWindow window)
        {
            FWindowLIFO.Add(window);
            FWindowNodes.Add(window.GetNode(), window);
        }
        
        public void WindowRemovedCB(IWindow window)
        {
            FWindowLIFO.Remove(window);
            FWindowNodes.Remove(window.GetNode());
        }
        #endregion IWindowListener
        
        #region IWindowSelectionListener
        public void WindowSelectionChangeCB(IWindow window)
        {
            //remove it from the index it is now
            FWindowLIFO.Remove(window);
            
            //insert it at index 0
            FWindowLIFO.Insert(0, window);
        }
        #endregion
        
        #region events
        void FHierarchyViewerClick(IModelMapper sender, MouseEventArgs e)
        {
            FWindowSwitcherHost.HideMe();
            FHDEHost.SetComponentMode((sender.Model as PatchNode).Node, ComponentMode.InAWindow);
        }
        
        void FDummyTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.ControlKey) || (e.KeyData == Keys.Control))
            {
                FWindowSwitcherHost.HideMe();
                FHDEHost.SetComponentMode(FWindowLIFO[FSelectedWindowIndex].GetNode(), ComponentMode.InAWindow);
            }
        }
        #endregion events
        
        void FHierarchyViewerKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.ControlKey) || (e.KeyData == Keys.Control))
            {
                FWindowSwitcherHost.HideMe();
                FHDEHost.SetComponentMode(FWindowLIFO[FSelectedWindowIndex].GetNode(), ComponentMode.InAWindow);
            }
        }
    }
}
