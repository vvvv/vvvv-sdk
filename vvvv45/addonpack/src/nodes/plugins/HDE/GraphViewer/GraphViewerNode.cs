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

using VVVV.PluginInterfaces.V1;
using VVVV.HDE.Viewer;
using VVVV.HDE.Viewer.Model;
using VVVV.Graph;
using VVVV.Graph.Provider;

//the vvvv node namespace
namespace VVVV.Nodes
{
    //class definition, inheriting from UserControl for the GUI stuff
    public class GraphViewerPluginNode: UserControl, IHDEPlugin, INodeSelectionListener
    {
        #region field declaration
        
        //the host (mandatory)
        private IPluginHost FPluginHost;
        private IHDEHost FHDEHost;
        
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
        	this.textBox1 = new System.Windows.Forms.TextBox();
        	this.treeViewer = new VVVV.HDE.Viewer.PanelTreeViewer();
        	this.SuspendLayout();
        	// 
        	// textBox1
        	// 
        	this.textBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
        	this.textBox1.Location = new System.Drawing.Point(0, 366);
        	this.textBox1.Name = "textBox1";
        	this.textBox1.Size = new System.Drawing.Size(310, 20);
        	this.textBox1.TabIndex = 1;
        	// 
        	// treeViewer
        	// 
        	this.treeViewer.AutoScroll = true;
        	this.treeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
        	this.treeViewer.Location = new System.Drawing.Point(0, 0);
        	this.treeViewer.Name = "treeViewer";
        	this.treeViewer.ShowRoot = true;
        	this.treeViewer.Size = new System.Drawing.Size(310, 366);
        	this.treeViewer.TabIndex = 2;
        	// 
        	// GraphViewerPluginNode
        	// 
        	this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        	this.Controls.Add(this.treeViewer);
        	this.Controls.Add(this.textBox1);
        	this.DoubleBuffered = true;
        	this.Name = "GraphViewerPluginNode";
        	this.Size = new System.Drawing.Size(310, 386);
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.TextBox textBox1;
        private VVVV.HDE.Viewer.PanelTreeViewer treeViewer;
        
        #region pin creation
        
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
            
            FHDEHost.AddListener(this);
            
            //create AdapterFactory
            GraphAdapterFactory af = new GraphAdapterFactory();
            var cp = new AdapterFactoryContentProvider(af);
            var lp = new AdapterFactoryLabelProvider(af);
            
            //create IContentProvider and hand it to the treeView
            treeViewer.SetContentProvider(cp);
            
            //create ILabelProvider and hand it to the treeView
            treeViewer.SetLabelProvider(lp);
        }
        
        public void NodeSelectionChangedCB(INode[] nodes)
        {
            //convert the INode structure to a proper data model
            PatchNode root = new PatchNode(null, null);
            
            List<INode> children = new List<INode>();
            if (nodes != null)
                foreach (INode n in nodes)
            {
                children.Clear();
                for (int i = 0; i < n.GetChildCount(); i++)
                    children.Add(n.GetChild(i));
                root.Add(new PatchNode(n, children.ToArray()));
            }

            //set the root of the data model to the treeViewer
            treeViewer.SetRoot(root);
        }
        
        #endregion pin creation
        
        void TreeViewerOnLeftClick(object sender, EventArgs e)
        {
            textBox1.Text = (sender as PatchNode).GetNodeInfo().Username;
        }
    }
}
