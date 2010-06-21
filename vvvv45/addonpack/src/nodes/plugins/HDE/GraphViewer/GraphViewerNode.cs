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

using VVVV.Utils.Event;
using VVVV.Utils.Unity;
using VVVV.PluginInterfaces.V1;
using VVVV.HDE.Viewer;
using VVVV.HDE.Viewer.Model;

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
        	this.FTreeViewer = new VVVV.HDE.Viewer.TreeViewer();
        	this.FFindTextBox = new System.Windows.Forms.TextBox();
        	this.panel2 = new System.Windows.Forms.Panel();
        	this.FDownStreamRadioButton = new System.Windows.Forms.RadioButton();
        	this.FLocalRadioButton = new System.Windows.Forms.RadioButton();
        	this.FGlobalRadioButton = new System.Windows.Forms.RadioButton();
        	this.panel3 = new System.Windows.Forms.Panel();
        	this.FWindowLabel = new System.Windows.Forms.Label();
        	this.FAttachButton = new System.Windows.Forms.Button();
        	this.panel2.SuspendLayout();
        	this.panel3.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// FTreeViewer
        	// 
        	this.FTreeViewer.AutoScroll = true;
        	this.FTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.FTreeViewer.FlatStyle = true;
        	this.FTreeViewer.Location = new System.Drawing.Point(0, 62);
        	this.FTreeViewer.Name = "FTreeViewer";
        	this.FTreeViewer.ShowLines = true;
        	this.FTreeViewer.ShowPlusMinus = true;
        	this.FTreeViewer.ShowRoot = false;
        	this.FTreeViewer.ShowRootLines = true;
        	this.FTreeViewer.ShowTooltip = false;
        	this.FTreeViewer.Size = new System.Drawing.Size(310, 324);
        	this.FTreeViewer.TabIndex = 4;
        	this.FTreeViewer.LeftClick += new System.EventHandler(this.FTreeViewerLeftClick);
        	this.FTreeViewer.LeftDoubleClick += new System.EventHandler(this.FTreeViewerLeftDoubleClick);
        	// 
        	// FFindTextBox
        	// 
        	this.FFindTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.FFindTextBox.Dock = System.Windows.Forms.DockStyle.Top;
        	this.FFindTextBox.Location = new System.Drawing.Point(0, 42);
        	this.FFindTextBox.Name = "FFindTextBox";
        	this.FFindTextBox.Size = new System.Drawing.Size(310, 20);
        	this.FFindTextBox.TabIndex = 0;
        	this.FFindTextBox.TextChanged += new System.EventHandler(this.FFindTextBoxTextChanged);
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
        	// GraphViewerPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Controls.Add(this.FTreeViewer);
        	this.Controls.Add(this.FFindTextBox);
        	this.Controls.Add(this.panel2);
        	this.Controls.Add(this.panel3);
        	this.DoubleBuffered = true;
        	this.Name = "GraphViewerPluginNode";
        	this.Size = new System.Drawing.Size(310, 386);
        	this.panel2.ResumeLayout(false);
        	this.panel2.PerformLayout();
        	this.panel3.ResumeLayout(false);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.Label FWindowLabel;
        private System.Windows.Forms.Button FAttachButton;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox FFindTextBox;
        private System.Windows.Forms.RadioButton FGlobalRadioButton;
        private System.Windows.Forms.RadioButton FLocalRadioButton;
        private System.Windows.Forms.RadioButton FDownStreamRadioButton;
        private System.Windows.Forms.Panel panel2;
        private VVVV.HDE.Viewer.TreeViewer FTreeViewer;
        
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
            
            //now create a child container, which knows how to map the HDE model.
            FChildContainer = FHDEHost.UnityContainer.CreateChildContainer();
            FChildContainer.AddNewExtension<GraphViewerModelContainerExtension>();
            
            //create an event hub which is used by the event extension to route events.
            //FHDEHost.Container.RegisterType<IEventHub, EventHub>(new ContainerControlledLifetimeManager());
            //FHDEHost.Container.AddNewExtension<EventExtension<PropertyChangedEventHandler, PropertyChangedEventArgs>>();
            
            //create a IContentProvider and hand it to the treeViewer
            var cp = new UnityContentProvider(FChildContainer);
            FTreeViewer.SetContentProvider(cp);
            
            //create ILabelProvider and hand it to the treeViewer
            var lp = new UnityLabelProvider(FChildContainer);
            FTreeViewer.SetLabelProvider(lp);
            
            //create ISelectionProvider and hand it to the treeViewer
            var sp = new UnitySelectionProvider(FChildContainer);
            FTreeViewer.SetSelectionProvider(sp);
            
            //this will trigger the initial WindowSelectionChangeCB
            FHDEHost.AddListener(this);
        }
        
        public void SetGraphViewerHost(IGraphViewerHost host)
        {
            FGraphViewerHost = host;
        }
        
        public void Initialize(INode root)
        {
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
            
            FActivePatchNode = FChildContainer.BuildUp(new PatchNode(patch));
            
            FTreeViewer.SetRoot(FActivePatchNode);
        }
        
        #region TreeViewer Events
        void FTreeViewerLeftDoubleClick(object sender, EventArgs e)
        {
            if ((sender as PatchNode).Text == "..")
            {
                //FSuperRoot provides access to the whole graph
                //go look for the current node recursively and take its parent
                INode parent = FindParent(FSuperRoot, FActivePatchNode.Node);
                
                //set the new found parent as root to the TreeViewer
                UpdateRoot(parent);
                FActiveWindow = null;
            }
            else if ((sender as PatchNode).Node.GetChildren() == null)
            {
                //open the patch this node is in
                FGraphViewerHost.ShowPatchOfNode((sender as PatchNode).Node);
                
                //and select the node
                FGraphViewerHost.SelectNode((sender as PatchNode).Node);
            }
            else
            {
                UpdateRoot((sender as PatchNode).Node);
                FActiveWindow = null;
            }            
        }
        
        void FTreeViewerLeftClick(object sender, EventArgs e)
        {
            FGraphViewerHost.SelectNode((sender as PatchNode).Node);
        }
        #endregion TreeViewer Events
        
        private void AddNodesByTag(PatchNode searchResult, PatchNode sourceTree, string tag)
        {
            //go through child nodes of sourceTree recursively and see if any contains the tag
            foreach (PatchNode pn in sourceTree.GetChildren())
            {
                //now first go downstream recursively
                //to see if this pn is needed in the hierarchy to hold any matching downstream nodes
                //create a dummy to attach possible matching downstream nodes
                var parent = new PatchNode(null);
                parent.Node = pn.Node;
                AddNodesByTag(parent, pn, tag);
                
                if ((parent.GetChildren().Length > 0) || (pn.Text.ToLower().Contains(tag)))
                    searchResult.Add(parent);
            }
        }
        
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
                foreach (PatchNode pn in FActivePatchNode.GetChildren())
                    if (pn.Text.ToLower().Contains(tag))
                        searchResult.Add(pn);
            }
            else if (FDownStreamRadioButton.Checked)
            {
                //go through child nodes of FActivePatch recursively and see if any contains the tag
                AddNodesByTag(searchResult, FActivePatchNode, tag);
            }
            
            FTreeViewer.SetRoot(searchResult);
            FTreeViewer.Expand(searchResult, true);
        }
        
        void FAttachButtonClick(object sender, EventArgs e)
        {
            FAttached = !FAttached;
            if (FAttached)
                FAttachButton.Text = "Attached to:";
            else
                FAttachButton.Text = "Attach";
        }
    }
}
