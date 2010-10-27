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

namespace VVVV.Nodes.Finder
{
    public enum SearchMode {Global, Local, Downstream};
    
    [PluginInfo(Name = "Finder",
                Category = "HDE",
                Shortcut = "Ctrl+F",
                Author = "vvvv group",
                Help = "Finds Nodes, Comments and Send/Receive channels.",
                InitialBoxWidth = 200,
                InitialBoxHeight = 100,
                InitialWindowWidth = 340,
                InitialWindowHeight = 460,
                InitialComponentMode = TComponentMode.InAWindow)]
    public class FinderPluginNode: UserControl, IPluginHDE, IWindowSelectionListener
    {
        #region field declaration
        
        //the host (mandatory)
        private IHDEHost FHDEHost;
        private List<PatchNode> FPlainSearchList = new List<PatchNode>();
        private INode FRoot;
        private SearchMode FSearchMode = SearchMode.Local;
        
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //further fields
        System.Collections.Generic.List<INode> FNodes = new List<INode>();
        
        #endregion field declaration
        
        #region constructor/destructor
        [ImportingConstructor]
        public FinderPluginNode(IHDEHost host)
        {
            FHDEHost = host;
            //this will trigger the initial WindowSelectionChangeCB
            FHDEHost.AddListener(this);
            
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FSearchTextBox.ContextMenu = new ContextMenu();
            
            FHDEHost.GetRoot(out FRoot);
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            mappingRegistry.RegisterDefaultMapping<IMenuEntry, DefaultContextMenuProvider>();
            mappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
            mappingRegistry.RegisterDefaultMapping<IDroppable, DefaultDragDropProvider>();
            
            FHierarchyViewer.Registry = mappingRegistry;
            FHierarchyViewer.Input = new PatchNode(FRoot, this);
        }
        
        private void InitializeComponent()
        {
        	this.FSearchTextBox = new System.Windows.Forms.TextBox();
        	this.panel1 = new System.Windows.Forms.Panel();
        	this.FResetButton = new System.Windows.Forms.Button();
        	this.FNodeCountLabel = new System.Windows.Forms.Label();
        	this.FHierarchyViewer = new VVVV.HDE.Viewer.WinFormsViewer.HierarchyViewer();
        	this.panel1.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// FSearchTextBox
        	// 
        	this.FSearchTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.FSearchTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.FSearchTextBox.Location = new System.Drawing.Point(0, 0);
        	this.FSearchTextBox.Name = "FSearchTextBox";
        	this.FSearchTextBox.Size = new System.Drawing.Size(704, 20);
        	this.FSearchTextBox.TabIndex = 0;
        	this.FSearchTextBox.TextChanged += new System.EventHandler(this.FFindTextBoxTextChanged);
        	this.FSearchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FSearchTextBoxKeyDown);
        	this.FSearchTextBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FFindTextBoxMouseClick);
        	// 
        	// panel1
        	// 
        	this.panel1.Controls.Add(this.FSearchTextBox);
        	this.panel1.Controls.Add(this.FResetButton);
        	this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
        	this.panel1.Location = new System.Drawing.Point(0, 0);
        	this.panel1.Name = "panel1";
        	this.panel1.Size = new System.Drawing.Size(724, 20);
        	this.panel1.TabIndex = 7;
        	// 
        	// FResetButton
        	// 
        	this.FResetButton.Dock = System.Windows.Forms.DockStyle.Right;
        	this.FResetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        	this.FResetButton.Location = new System.Drawing.Point(704, 0);
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
        	this.FNodeCountLabel.Size = new System.Drawing.Size(724, 15);
        	this.FNodeCountLabel.TabIndex = 8;
        	this.FNodeCountLabel.Text = "Selected Nodes: ";
        	// 
        	// FHierarchyViewer
        	// 
        	this.FHierarchyViewer.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.FHierarchyViewer.Location = new System.Drawing.Point(0, 20);
        	this.FHierarchyViewer.Name = "FHierarchyViewer";
        	this.FHierarchyViewer.Size = new System.Drawing.Size(724, 351);
        	this.FHierarchyViewer.TabIndex = 9;
        	// 
        	// FinderPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Controls.Add(this.FHierarchyViewer);
        	this.Controls.Add(this.FNodeCountLabel);
        	this.Controls.Add(this.panel1);
        	this.DoubleBuffered = true;
        	this.Name = "FinderPluginNode";
        	this.Size = new System.Drawing.Size(724, 386);
        	this.panel1.ResumeLayout(false);
        	this.panel1.PerformLayout();
        	this.ResumeLayout(false);
        }
        private VVVV.HDE.Viewer.WinFormsViewer.HierarchyViewer FHierarchyViewer;
        private System.Windows.Forms.Label FNodeCountLabel;
        private System.Windows.Forms.TextBox FSearchTextBox;
        private System.Windows.Forms.Button FResetButton;
        private System.Windows.Forms.Panel panel1;
        
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
         
        #region IWindowSelectionListener
        public void WindowSelectionChangeCB(IWindow window)
        {
            //todo: mark current patch like "you are here"
        }
        #endregion IWindowSelectionListener
        
        public void UpdateView()
        {
            FHierarchyViewer.Reload();
        }
        
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
                var parent = new PatchNode(null, this);
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
            
            var searchResult = new PatchNode(null, this);
            FPlainSearchList.Clear();
            
          /*  switch (FSearchMode)
            {
                case SearchMode.Global:
                    {
                        //go through child nodes of FSuperRoot recursively and see if any contains the tag
                        AddNodesByTag(searchResult, new PatchNode(FRoot, this), tag);
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
            }*/
            
            var mappingRegistry = new MappingRegistry();
            mappingRegistry.RegisterDefaultMapping<INamed, DefaultNameProvider>();
            mappingRegistry.RegisterDefaultMapping<IMenuEntry, DefaultContextMenuProvider>();
            mappingRegistry.RegisterDefaultMapping<IDraggable, DefaultDragDropProvider>();
            mappingRegistry.RegisterDefaultMapping<IDroppable, DefaultDragDropProvider>();
            
            FHierarchyViewer.Registry = mappingRegistry;
            FHierarchyViewer.Input = searchResult;
            
          /*  if ((FSearchMode == SearchMode.Downstream) || (FSearchMode == SearchMode.Global))
                FHierarchyViewer.Expand(searchResult, true);
            else
                FHierarchyViewer.Collapse(FActivePatchNode, false);
            */
            FNodeCountLabel.Text = "Matching Nodes: " + FPlainSearchList.Count.ToString();
        }
        #endregion Search
        
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
        
        void FSearchTextBoxKeyDown(object sender, KeyEventArgs e)
        {
         /*   if ((!string.IsNullOrEmpty(FSearchTextBox.Text)) && (e.KeyCode == Keys.F3))
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
            }*/
        }
    }
}
