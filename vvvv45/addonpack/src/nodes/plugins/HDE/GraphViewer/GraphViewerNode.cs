#region licence/info

//////project name
//vvvv plugin template with gui

//////description
//basic vvvv plugin template with gui.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Practices.Unity;

using VVVV.PluginInterfaces.V1;
using VVVV.HDE.Viewer;
using VVVV.Core;

//the vvvv node namespace
namespace VVVV.Nodes.GraphViewer
{
    //class definition, inheriting from UserControl for the GUI stuff
    public class GraphViewerPluginNode: UserControl, IHDEPlugin, IGraphViewer, INodeSelectionListener, IWindowListener, IWindowSelectionListener
    {
        #region field declaration
        
        //the host (mandatory)
        private IPluginHost FPluginHost;
        private IHDEHost FHDEHost;
        private IGraphViewerHost FGraphViewerHost;
        private IUnityContainer FChildContainer;
        private IWindow FActiveWindow;
        private PatchNode FActivePatchNode;
        private INode FSuperRoot;
        private bool FAttached = false;
        
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //further fields
        System.Collections.Generic.List<INode> FNodes = new List<INode>();
        
        #endregion field declaration
        
        #region constructor/destructor
        public GraphViewerPluginNode()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            InitializeComponent();
            
            FFindTextBox.ContextMenu = new ContextMenu();
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
        
        #endregion constructor/destructor
        
        #region node name and infos
        
        //provide node infos
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();
                    
                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "GraphViewer";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HDE";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";
                    
                    FPluginInfo.ShortCut = "Ctrl+F";
                    
                    //the nodes author: your sign
                    FPluginInfo.Author = "anonymous";
                    //describe the nodes function
                    FPluginInfo.Help = "Offers a basic code layout to start from when writing a vvvv plugin with GUI";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";
                    
                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";
                    
                    //define the nodes initial size in box-mode
                    FPluginInfo.InitialBoxSize = new Size(200, 100);
                    //define the nodes initial size in window-mode
                    FPluginInfo.InitialWindowSize = new Size(300, 500);
                    //define the nodes initial component mode
                    FPluginInfo.InitialComponentMode = TComponentMode.InAWindow;
                    
                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }
        
        #endregion node name and infos
        
        private void InitializeComponent()
        {
        	this.FTreeViewer = new VVVV.HDE.Viewer.WinFormsTreeViewer.TreeViewer();
        	this.FFindTextBox = new System.Windows.Forms.TextBox();
        	this.panel2 = new System.Windows.Forms.Panel();
        	this.FDownStreamRadioButton = new System.Windows.Forms.RadioButton();
        	this.FLocalRadioButton = new System.Windows.Forms.RadioButton();
        	this.FGlobalRadioButton = new System.Windows.Forms.RadioButton();
        	this.panel3 = new System.Windows.Forms.Panel();
        	this.FWindowLabel = new System.Windows.Forms.Label();
        	this.FAttachButton = new System.Windows.Forms.Button();
        	this.panel1 = new System.Windows.Forms.Panel();
        	this.FResetButton = new System.Windows.Forms.Button();
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
        	this.FTreeViewer.Location = new System.Drawing.Point(0, 62);
        	this.FTreeViewer.Name = "FTreeViewer";
        	this.FTreeViewer.Root = null;
        	this.FTreeViewer.ShowLines = true;
        	this.FTreeViewer.ShowPlusMinus = true;
        	this.FTreeViewer.ShowRoot = false;
        	this.FTreeViewer.ShowRootLines = true;
        	this.FTreeViewer.ShowTooltip = true;
        	this.FTreeViewer.Size = new System.Drawing.Size(310, 324);
        	this.FTreeViewer.TabIndex = 4;
        	this.FTreeViewer.Click += new VVVV.HDE.Viewer.WinFormsTreeViewer.ClickHandler(this.FTreeViewerClick);
        	// 
        	// FFindTextBox
        	// 
        	this.FFindTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.FFindTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.FFindTextBox.Location = new System.Drawing.Point(0, 0);
        	this.FFindTextBox.Name = "FFindTextBox";
        	this.FFindTextBox.Size = new System.Drawing.Size(290, 20);
        	this.FFindTextBox.TabIndex = 0;
        	this.FFindTextBox.TextChanged += new System.EventHandler(this.FFindTextBoxTextChanged);
        	this.FFindTextBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.FFindTextBoxMouseClick);
        	// 
        	// panel2
        	// 
        	this.panel2.Controls.Add(this.FDownStreamRadioButton);
        	this.panel2.Controls.Add(this.FLocalRadioButton);
        	this.panel2.Controls.Add(this.FGlobalRadioButton);
        	this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
        	this.panel2.Location = new System.Drawing.Point(0, 22);
        	this.panel2.Name = "panel2";
        	this.panel2.Size = new System.Drawing.Size(310, 20);
        	this.panel2.TabIndex = 4;
        	// 
        	// FDownStreamRadioButton
        	// 
        	this.FDownStreamRadioButton.AutoSize = true;
        	this.FDownStreamRadioButton.Dock = System.Windows.Forms.DockStyle.Left;
        	this.FDownStreamRadioButton.Location = new System.Drawing.Point(122, 0);
        	this.FDownStreamRadioButton.Name = "FDownStreamRadioButton";
        	this.FDownStreamRadioButton.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        	this.FDownStreamRadioButton.Size = new System.Drawing.Size(92, 20);
        	this.FDownStreamRadioButton.TabIndex = 3;
        	this.FDownStreamRadioButton.Text = "Downstream";
        	this.FDownStreamRadioButton.UseVisualStyleBackColor = true;
        	this.FDownStreamRadioButton.CheckedChanged += new System.EventHandler(this.FGlobalRadioButtonCheckedChanged);
        	// 
        	// FLocalRadioButton
        	// 
        	this.FLocalRadioButton.AutoSize = true;
        	this.FLocalRadioButton.Checked = true;
        	this.FLocalRadioButton.Dock = System.Windows.Forms.DockStyle.Left;
        	this.FLocalRadioButton.Location = new System.Drawing.Point(63, 0);
        	this.FLocalRadioButton.Name = "FLocalRadioButton";
        	this.FLocalRadioButton.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        	this.FLocalRadioButton.Size = new System.Drawing.Size(59, 20);
        	this.FLocalRadioButton.TabIndex = 2;
        	this.FLocalRadioButton.TabStop = true;
        	this.FLocalRadioButton.Text = "Local";
        	this.FLocalRadioButton.UseVisualStyleBackColor = true;
        	this.FLocalRadioButton.CheckedChanged += new System.EventHandler(this.FGlobalRadioButtonCheckedChanged);
        	// 
        	// FGlobalRadioButton
        	// 
        	this.FGlobalRadioButton.AutoSize = true;
        	this.FGlobalRadioButton.Dock = System.Windows.Forms.DockStyle.Left;
        	this.FGlobalRadioButton.Location = new System.Drawing.Point(0, 0);
        	this.FGlobalRadioButton.Name = "FGlobalRadioButton";
        	this.FGlobalRadioButton.Padding = new System.Windows.Forms.Padding(0, 0, 8, 0);
        	this.FGlobalRadioButton.Size = new System.Drawing.Size(63, 20);
        	this.FGlobalRadioButton.TabIndex = 1;
        	this.FGlobalRadioButton.Text = "Global";
        	this.FGlobalRadioButton.UseVisualStyleBackColor = true;
        	this.FGlobalRadioButton.CheckedChanged += new System.EventHandler(this.FGlobalRadioButtonCheckedChanged);
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
        	this.FWindowLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.FWindowLabel.Location = new System.Drawing.Point(75, 0);
        	this.FWindowLabel.Name = "FWindowLabel";
        	this.FWindowLabel.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
        	this.FWindowLabel.Size = new System.Drawing.Size(235, 22);
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
        	this.FAttachButton.Size = new System.Drawing.Size(75, 22);
        	this.FAttachButton.TabIndex = 6;
        	this.FAttachButton.Text = "Attach";
        	this.FAttachButton.UseVisualStyleBackColor = true;
        	this.FAttachButton.Click += new System.EventHandler(this.FAttachButtonClick);
        	// 
        	// panel1
        	// 
        	this.panel1.Controls.Add(this.FFindTextBox);
        	this.panel1.Controls.Add(this.FResetButton);
        	this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
        	this.panel1.Location = new System.Drawing.Point(0, 42);
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
        	// GraphViewerPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Controls.Add(this.FTreeViewer);
        	this.Controls.Add(this.panel1);
        	this.Controls.Add(this.panel2);
        	this.Controls.Add(this.panel3);
        	this.DoubleBuffered = true;
        	this.Name = "GraphViewerPluginNode";
        	this.Size = new System.Drawing.Size(310, 386);
        	this.panel2.ResumeLayout(false);
        	this.panel2.PerformLayout();
        	this.panel3.ResumeLayout(false);
        	this.panel1.ResumeLayout(false);
        	this.panel1.PerformLayout();
        	this.ResumeLayout(false);
        }
        private System.Windows.Forms.Button FResetButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label FWindowLabel;
        private System.Windows.Forms.Button FAttachButton;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox FFindTextBox;
        private System.Windows.Forms.RadioButton FGlobalRadioButton;
        private System.Windows.Forms.RadioButton FLocalRadioButton;
        private System.Windows.Forms.RadioButton FDownStreamRadioButton;
        private System.Windows.Forms.Panel panel2;
        private VVVV.HDE.Viewer.WinFormsTreeViewer.TreeViewer FTreeViewer;
        
        #region initialization
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost host)
        {
            //assign host
            FPluginHost = host;
        }
        
        public void SetHDEHost(IHDEHost host)
        {
            //assign host
            FHDEHost = host;
            
            /* var shell = new Shell();
            var nodeMapper = new ModelMapper(FNodes, shell.MappingRegistry);
            FTreeViewer.Root = nodeMapper;
             */
            //this will trigger the initial WindowSelectionChangeCB
            FHDEHost.AddListener(this);
        }
        
        public void SetGraphViewerHost(IGraphViewerHost host)
        {
            FGraphViewerHost = host;
        }
        
        public void Initialize(INode root)
        {
            //via FSuperRoot GraphViewer has access to the whole active graph for searching globally
            FSuperRoot = root;
        }
        #endregion initialization
        
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
                
                UpdateRoot(FSuperRoot.GetChildren()[0]);
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
            if (!FAttached)
                if ((window.GetWindowType() == TWindowType.Module) || (window.GetWindowType() == TWindowType.Patch))
            {
                if (window != FActiveWindow)
                {
                    if (FActiveWindow != null)
                    {
                        FActivePatchNode.UnSubscribe();
                        FHDEHost.UnityContainer.Teardown(FActiveWindow);
                    }
                    
                    UpdateRoot(window.GetNode());
                    FActiveWindow = window;
                    FWindowLabel.Text = FActiveWindow.GetCaption();
                }
            }
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
            if (FActiveWindow != null)
            {
                FActivePatchNode.UnSubscribe();
                FHDEHost.UnityContainer.Teardown(FActiveWindow);
            }
            
            var shell = new Shell();
            FActivePatchNode = new PatchNode(patch);
            var nodeMapper = new ModelMapper(FActivePatchNode, shell.MappingRegistry);
            FTreeViewer.Root = nodeMapper;
        }
        
        #region TreeViewer Events
        void FTreeViewerClick(ModelMapper sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                FGraphViewerHost.SelectNode((sender.Model as PatchNode).Node);
        }
        
        void FTreeViewerDoubleClick(ModelMapper sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if ((sender.Model as PatchNode).Name == "..")
                {
                    //FSuperRoot provides access to the whole graph
                    //go look for the current node recursively and take its parent
                    INode parent = FindParent(FSuperRoot, FActivePatchNode.Node);
                    
                    //set the new found parent as root to the TreeViewer
                    UpdateRoot(parent);
                    FActiveWindow = null;
                }
                else if ((sender.Model as PatchNode).Node.GetChildren() == null)
                {
                    //open the patch this node is in
                    FGraphViewerHost.ShowPatchOfNode((sender.Model as PatchNode).Node);
                    
                    //and select the node
                    FGraphViewerHost.SelectNode((sender.Model as PatchNode).Node);
                }
                else
                {
                    UpdateRoot((sender.Model as PatchNode).Node);
                    FActiveWindow = null;
                }
            }
        }
        #endregion TreeViewer Events
        
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
                    searchResult.Add(parent);
            }
        }
        
        #region Search
        void FFindTextBoxTextChanged(object sender, EventArgs e)
        {
            UpdateSearch();
        }
        
        void FGlobalRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            UpdateSearch();
        }
        
        private void UpdateSearch()
        {
            string tag = FFindTextBox.Text.Trim().ToLower();
            
            var searchResult = new PatchNode(null);
            
            if (FGlobalRadioButton.Checked)
            {
                //go through child nodes of FSuperRoot recursively and see if any contains the tag
                AddNodesByTag(searchResult, new PatchNode(FSuperRoot), tag);
            }
            else if (FLocalRadioButton.Checked)
            {
                //go through child nodes of FActivePatch and see if any contains the tag
                foreach (PatchNode pn in FActivePatchNode)
                    if (pn.Name.ToLower().Contains(tag))
                        searchResult.Add(pn);
            }
            else if (FDownStreamRadioButton.Checked)
            {
                //go through child nodes of FActivePatch recursively and see if any contains the tag
                AddNodesByTag(searchResult, FActivePatchNode, tag);
            }
            
            //FTreeViewer.Root = searchResult;
            var shell = new Shell();
            var nodeMapper = new ModelMapper(searchResult, shell.MappingRegistry);
            FTreeViewer.Root = nodeMapper;
            
            FTreeViewer.Expand(searchResult, true);
        }
        #endregion Search
        
        void FAttachButtonClick(object sender, EventArgs e)
        {
            FAttached = !FAttached;
            if (FAttached)
                FAttachButton.Text = "Attached to:";
            else
                FAttachButton.Text = "Attach";
        }
        
        void FFindTextBoxMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                FFindTextBox.Text = "";
            }
        }
        
        void FResetButtonClick(object sender, EventArgs e)
        {
        	FFindTextBox.Text = "";
        }
    }
}
