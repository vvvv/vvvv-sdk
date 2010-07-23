#region usings
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Microsoft.Practices.Unity;

using VVVV.Core;
using VVVV.Core.Menu;
using VVVV.Core.View;
using VVVV.HDE.Viewer;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
#endregion usings

namespace VVVV.Nodes.GraphViewer
{
    public enum SearchMode {Global, Local, Downstream};
    
    [PluginInfo(Name = "GraphViewer",
                Category = "HDE",
                Shortcut = "Ctrl+F",
                Author = "vvvv group",
                Help = "The Graph Viewer",
                InitialBoxWidth = 200,
                InitialBoxHeight = 100,
                InitialWindowWidth = 340,
                InitialWindowHeight = 460,
                InitialComponentMode = TComponentMode.InAWindow)]
    public class GraphViewerPluginNode: UserControl, IGraphViewer, INodeSelectionListener, IWindowListener, IWindowSelectionListener
    {
        #region field declaration
        
        //the host (mandatory)
        private IHDEHost FHDEHost;
        [Import]
        private IGraphViewerHost FGraphViewerHost;
        private IUnityContainer FChildContainer;
        private IWindow FActiveWindow;
        private PatchNode FActivePatchNode;
        private List<PatchNode> FPlainSearchList = new List<PatchNode>();
        private INode FRoot;
        private bool FAttached = false;
        private SearchMode FSearchMode = SearchMode.Local;
        private int FSearchIndex;
        
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //further fields
        System.Collections.Generic.List<INode> FNodes = new List<INode>();
        
        #endregion field declaration
        
        #region constructor/destructor
        [ImportingConstructor]
        public GraphViewerPluginNode(IHDEHost host)
        {
            FHDEHost = host;
            //this will trigger the initial WindowSelectionChangeCB
            FHDEHost.AddListener(this);
            
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FSearchTextBox.ContextMenu = new ContextMenu();
        }
        
        private void InitializeComponent()
        {
            this.FTreeViewer = new VVVV.HDE.Viewer.WinFormsViewer.TreeViewer();
            this.FSearchTextBox = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.FDownstreamButton = new System.Windows.Forms.Button();
            this.FLocalButton = new System.Windows.Forms.Button();
            this.FGlobalButton = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.FWindowLabel = new System.Windows.Forms.Label();
            this.FAttachButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.FResetButton = new System.Windows.Forms.Button();
            this.FNodeCountLabel = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FTreeViewer
            // 
            this.FTreeViewer.AutoScroll = true;
            this.FTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FTreeViewer.FlatStyle = true;
            this.FTreeViewer.Location = new System.Drawing.Point(0, 64);
            this.FTreeViewer.Name = "FTreeViewer";
            this.FTreeViewer.ShowLines = true;
            this.FTreeViewer.ShowPlusMinus = true;
            this.FTreeViewer.ShowRoot = false;
            this.FTreeViewer.ShowRootLines = true;
            this.FTreeViewer.ShowTooltip = true;
            this.FTreeViewer.Size = new System.Drawing.Size(310, 307);
            this.FTreeViewer.TabIndex = 6;
            this.FTreeViewer.DoubleClick += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FTreeViewerDoubleClick);
            this.FTreeViewer.Click += new VVVV.HDE.Viewer.WinFormsViewer.ClickHandler(this.FTreeViewerClick);
            // 
            // FSearchTextBox
            // 
            this.FSearchTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FSearchTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FSearchTextBox.Location = new System.Drawing.Point(0, 0);
            this.FSearchTextBox.Name = "FSearchTextBox";
            this.FSearchTextBox.Size = new System.Drawing.Size(290, 20);
            this.FSearchTextBox.TabIndex = 0;
            this.FSearchTextBox.TextChanged += new System.EventHandler(this.FFindTextBoxTextChanged);
            this.FSearchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FSearchTextBoxKeyDown);
            this.FSearchTextBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FFindTextBoxMouseClick);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.FDownstreamButton);
            this.panel2.Controls.Add(this.FLocalButton);
            this.panel2.Controls.Add(this.FGlobalButton);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 22);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(310, 22);
            this.panel2.TabIndex = 4;
            // 
            // FDownstreamButton
            // 
            this.FDownstreamButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.FDownstreamButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FDownstreamButton.Location = new System.Drawing.Point(160, 0);
            this.FDownstreamButton.Name = "FDownstreamButton";
            this.FDownstreamButton.Size = new System.Drawing.Size(80, 22);
            this.FDownstreamButton.TabIndex = 4;
            this.FDownstreamButton.Text = "Downstream";
            this.FDownstreamButton.UseVisualStyleBackColor = true;
            this.FDownstreamButton.Click += new System.EventHandler(this.DownstreamButtonClick);
            // 
            // FLocalButton
            // 
            this.FLocalButton.BackColor = System.Drawing.Color.DarkGray;
            this.FLocalButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.FLocalButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FLocalButton.Location = new System.Drawing.Point(80, 0);
            this.FLocalButton.Name = "FLocalButton";
            this.FLocalButton.Size = new System.Drawing.Size(80, 22);
            this.FLocalButton.TabIndex = 3;
            this.FLocalButton.Text = "Local";
            this.FLocalButton.UseVisualStyleBackColor = false;
            this.FLocalButton.Click += new System.EventHandler(this.LocalButtonClick);
            // 
            // FGlobalButton
            // 
            this.FGlobalButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.FGlobalButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FGlobalButton.Location = new System.Drawing.Point(0, 0);
            this.FGlobalButton.Name = "FGlobalButton";
            this.FGlobalButton.Size = new System.Drawing.Size(80, 22);
            this.FGlobalButton.TabIndex = 2;
            this.FGlobalButton.Text = "Global";
            this.FGlobalButton.UseVisualStyleBackColor = true;
            this.FGlobalButton.Click += new System.EventHandler(this.GlobalButtonClick);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.FWindowLabel);
            this.panel3.Controls.Add(this.FAttachButton);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(310, 22);
            this.panel3.TabIndex = 6;
            // 
            // FWindowLabel
            // 
            this.FWindowLabel.AutoEllipsis = true;
            this.FWindowLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FWindowLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FWindowLabel.Location = new System.Drawing.Point(80, 0);
            this.FWindowLabel.Name = "FWindowLabel";
            this.FWindowLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.FWindowLabel.Size = new System.Drawing.Size(230, 22);
            this.FWindowLabel.TabIndex = 7;
            this.FWindowLabel.Text = "label1";
            this.FWindowLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FAttachButton
            // 
            this.FAttachButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.FAttachButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FAttachButton.Location = new System.Drawing.Point(0, 0);
            this.FAttachButton.Name = "FAttachButton";
            this.FAttachButton.Size = new System.Drawing.Size(80, 22);
            this.FAttachButton.TabIndex = 5;
            this.FAttachButton.Text = "Attach";
            this.FAttachButton.UseVisualStyleBackColor = true;
            this.FAttachButton.Click += new System.EventHandler(this.FAttachButtonClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.FSearchTextBox);
            this.panel1.Controls.Add(this.FResetButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 44);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(310, 20);
            this.panel1.TabIndex = 7;
            // 
            // FResetButton
            // 
            this.FResetButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.FResetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FResetButton.Location = new System.Drawing.Point(290, 0);
            this.FResetButton.Name = "FResetButton";
            this.FResetButton.Size = new System.Drawing.Size(20, 20);
            this.FResetButton.TabIndex = 1;
            this.FResetButton.Text = "X";
            this.FResetButton.UseVisualStyleBackColor = true;
            this.FResetButton.Click += new System.EventHandler(this.FResetButtonClick);
            // 
            // FNodeCountLabel
            // 
            this.FNodeCountLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FNodeCountLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.FNodeCountLabel.Location = new System.Drawing.Point(0, 371);
            this.FNodeCountLabel.Name = "FNodeCountLabel";
            this.FNodeCountLabel.Size = new System.Drawing.Size(310, 15);
            this.FNodeCountLabel.TabIndex = 8;
            this.FNodeCountLabel.Text = "Selected Nodes: ";
            // 
            // GraphViewerPluginNode
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Controls.Add(this.FTreeViewer);
            this.Controls.Add(this.FNodeCountLabel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.DoubleBuffered = true;
            this.Name = "GraphViewerPluginNode";
            this.Size = new System.Drawing.Size(310, 386);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
        }
        private System.Windows.Forms.Label FNodeCountLabel;
        private System.Windows.Forms.TextBox FSearchTextBox;
        private System.Windows.Forms.Button FGlobalButton;
        private System.Windows.Forms.Button FLocalButton;
        private System.Windows.Forms.Button FDownstreamButton;
        private System.Windows.Forms.Button FResetButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label FWindowLabel;
        private System.Windows.Forms.Button FAttachButton;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private VVVV.HDE.Viewer.WinFormsViewer.TreeViewer FTreeViewer;
        
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
        
        #endregion constructor/destructor
        
        #region IGraphViewer
        public void Initialize(INode root)
        {
            //via FRoot GraphViewer has access to the whole active graph for searching globally
            FRoot = root;
        }
        #endregion IGraphViewer
        
        #region IWindowListener
        public void WindowAddedCB(IWindow window)
        {
            //nothing todo
        }
        
        public void WindowRemovedCB(IWindow window)
        {
            if (window == FActiveWindow)
            {
                FWindowLabel.Text = "root";
                FAttachButton.Text = "Attach";
                FAttached = false;
                
                UpdateRoot(FRoot.GetChildren()[0]);
            }
        }
        #endregion IWindowListener
        
        #region INodeSelectionListener
        public void NodeSelectionChangedCB(INode[] nodes)
        {
            FActivePatchNode.SelectNodes(nodes);
        }
        #endregion INodeSelectionListener
        
        #region IWindowSelectionListener
        public void WindowSelectionChangeCB(IWindow window)
        {
            //if graphviewer itself is being activated, focus the edit field
            if (window.GetCaption() == "GraphViewer")
            {
                FSearchTextBox.Focus();
                FSearchTextBox.SelectAll();
            }
            else if (!FAttached)
                if ((window.GetWindowType() == TWindowType.Module) || (window.GetWindowType() == TWindowType.Patch))
            {
                if (window != FActiveWindow)
                {
                    if (FActiveWindow != null)
                    {
                        FActivePatchNode.UnSubscribe();
                    }
                    
                    FActiveWindow = window;
                    UpdateRoot(window.GetNode());
                    UpdateWindowLabel(window.GetNode());
                }
            }
        }
        
        private void UpdateWindowLabel(INode patch)
        {
            var ni = patch.GetNodeInfo();
            string file = System.IO.Path.GetFileName(ni.Filename);
            
            //unsaved patch
            if (string.IsNullOrEmpty(file))
                FWindowLabel.Text = ni.Filename;
            //patch with valid filename
            else
                FWindowLabel.Text = file + "  " + System.IO.Path.GetDirectoryName(patch.GetNodeInfo().Filename);
        }
        #endregion IWindowSelectionListener
        
        private INode FindParent(INode parent, INode target)
        {
            INode[] children = parent.GetChildren();
            
            if (children != null)
            {
                foreach(INode child in children)
                {
                    if (child == target)
                        return parent;
                    else
                    {
                        INode p = FindParent(child, target);
                        if (p != null)
                            return p;
                    }
                }
                return null;
            }
            else
                return null;
        }
        
        private void UpdateRoot(INode patch)
        {
            if (FActivePatchNode != null)
            {
                FActivePatchNode.UnSubscribe();
            }
            
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            mappingRegistry.RegisterDefaultMapping<IMenuEntry, DefaultContextMenuProvider>();
            mappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
            mappingRegistry.RegisterDefaultMapping<IDroppable, DefaultDragDropProvider>();
            
            FActivePatchNode = new PatchNode(patch);
            FTreeViewer.Registry = mappingRegistry;
            FTreeViewer.Input = FActivePatchNode;
        }
        
        #region TreeViewer Events
        void FTreeViewerClick(ModelMapper sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                FGraphViewerHost.SelectNodes(new INode[1]{(sender.Model as PatchNode).Node});
        }
        
        void FTreeViewerDoubleClick(ModelMapper sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if ((sender.Model as PatchNode).Name == "..")
                {
                    //FSuperRoot provides access to the whole graph
                    //go look for the current node recursively and take its parent
                    INode parent = FindParent(FRoot, FActivePatchNode.Node);
                    
                    //set the new found parent as root to the TreeViewer
                    UpdateRoot(parent);
                    FActiveWindow = null;
                    UpdateWindowLabel(parent);
                }
                else if ((sender.Model as PatchNode).Node.GetChildren() == null)
                {
                    //open the patch this node is in
                    FGraphViewerHost.ShowPatchOfNode((sender.Model as PatchNode).Node);
                    
                    //and select the node
                    FGraphViewerHost.SelectNodes(new INode[1]{(sender.Model as PatchNode).Node});
                }
                else
                {
                    UpdateRoot((sender.Model as PatchNode).Node);
                    FActiveWindow = null;
                    UpdateWindowLabel((sender.Model as PatchNode).Node);
                }
            }
        }
        #endregion TreeViewer Events
        
        #region Search
        void FFindTextBoxTextChanged(object sender, EventArgs e)
        {
            UpdateSearch();
        }
        
        private void AddNodesByTag(PatchNode searchResult, PatchNode sourceTree, string tag)
        {
            //go through child nodes of sourceTree recursively and see if any contains the tag
            foreach (PatchNode pn in sourceTree)
            {
                //now first go downstream recursively
                //to see if this pn is needed in the hierarchy to hold any matching downstream nodes
                //create a dummy to attach possible matching downstream nodes
                var parent = new PatchNode(null);
                parent.Node = pn.Node;
                AddNodesByTag(parent, pn, tag);
                
                if ((parent.Count > 0) || (pn.Name.ToLower().Contains(tag)))
                {
                    searchResult.Add(parent);
                    FPlainSearchList.Add(parent);
                }
            }
        }
        
        private void UpdateSearch()
        {
            string tag = FSearchTextBox.Text.Trim().ToLower();
            
            var searchResult = new PatchNode(null);
            FPlainSearchList.Clear();
            FSearchIndex = 0;
            
            switch (FSearchMode)
            {
                case SearchMode.Global:
                    {
                        //go through child nodes of FSuperRoot recursively and see if any contains the tag
                        AddNodesByTag(searchResult, new PatchNode(FRoot), tag);
                        break;
                    }
                case SearchMode.Local:
                    {
                        //go through child nodes of FActivePatch and see if any contains the tag
                        foreach (PatchNode pn in FActivePatchNode)
                            if (pn.Name.ToLower().Contains(tag))
                        {
                            searchResult.Add(pn);
                            FPlainSearchList.Add(pn);
                        }
                        break;
                    }
                case SearchMode.Downstream:
                    {
                        //go through child nodes of FActivePatch recursively and see if any contains the tag
                        AddNodesByTag(searchResult, FActivePatchNode, tag);
                        break;
                    }
            }
            
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            mappingRegistry.RegisterDefaultMapping<IMenuEntry, DefaultContextMenuProvider>();
            mappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
            mappingRegistry.RegisterDefaultMapping<IDroppable, DefaultDragDropProvider>();
            
            FTreeViewer.Registry = mappingRegistry;
            FTreeViewer.Input = searchResult;
            
            if ((FSearchMode == SearchMode.Downstream) || (FSearchMode == SearchMode.Global))
                FTreeViewer.Expand(searchResult, true);
            else
                FTreeViewer.Collapse(FActivePatchNode, false);
            
            FNodeCountLabel.Text = "Matching Nodes: " + FPlainSearchList.Count.ToString();
        }
        #endregion Search
        
        void FAttachButtonClick(object sender, EventArgs e)
        {
            FAttached = !FAttached;
            if (FAttached)
            {
                FAttachButton.Text = "Attached to:";
                FAttachButton.BackColor = Color.DarkGray;
            }
            else
            {
                FAttachButton.Text = "Attach";
                FAttachButton.BackColor = Color.LightGray;
            }
        }
        
        void FFindTextBoxMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                FSearchTextBox.Text = "";
            }
        }
        
        void FResetButtonClick(object sender, EventArgs e)
        {
            FSearchTextBox.Text = "";
        }
        
        void ActivateButton(Button button)
        {
            FSearchTextBox.Focus();
            
            FGlobalButton.BackColor = Color.LightGray;
            FLocalButton.BackColor = Color.LightGray;
            FDownstreamButton.BackColor = Color.LightGray;
            
            button.BackColor = Color.DarkGray;
            
            UpdateSearch();
        }
        
        void GlobalButtonClick(object sender, EventArgs e)
        {
            FSearchMode = SearchMode.Global;
            ActivateButton(sender as Button);
            FActiveWindow = null;
            UpdateWindowLabel(FRoot);
        }
        
        void LocalButtonClick(object sender, EventArgs e)
        {
            FSearchMode = SearchMode.Local;
            ActivateButton(sender as Button);
        }
        
        void DownstreamButtonClick(object sender, EventArgs e)
        {
            FSearchMode = SearchMode.Downstream;
            ActivateButton(sender as Button);
        }
        
        void FSearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(FSearchTextBox.Text)) && (e.KeyCode == Keys.F3))
            {
                if (!e.Shift)
                    FSearchIndex = (FSearchIndex + 1) % FPlainSearchList.Count;
                else
                {
                    FSearchIndex -= 1;
                    if (FSearchIndex == -1)
                        FSearchIndex = FPlainSearchList.Count - 1;
                }
                
                //open the patch this node is in
                FGraphViewerHost.ShowPatchOfNode(FPlainSearchList[FSearchIndex].Node);
                
                //and select the node
                FGraphViewerHost.SelectNodes(new INode[1]{FPlainSearchList[FSearchIndex].Node});
                
                //refocus in order to allow further next search in case a patch was opened 
                FSearchTextBox.Focus();
            }
            else if ((e.KeyCode == Keys.A) && (e.Control))
            {
                var nodes = new List<INode>();
                foreach (PatchNode pn in FPlainSearchList)
                    nodes.Add(pn.Node);
                
                FGraphViewerHost.SelectNodes(nodes.ToArray());
            }
        }
    }
}
