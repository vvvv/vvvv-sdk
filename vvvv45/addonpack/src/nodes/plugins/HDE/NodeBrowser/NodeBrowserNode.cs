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
using VVVV.HDE.Viewer.Model;

//the vvvv node namespace
namespace VVVV.Nodes.NodeBrowser
{
	
	//class definition, inheriting from UserControl for the GUI stuff
	public class NodeBrowserPluginNode: UserControl, IHDEPlugin, INodeInfoListener
	{
		#region field declaration
		
		//the hosts
		private IPluginHost FPluginHost;
		private IHDEHost FHDEHost;
		// Track whether Dispose has been called.
		private bool FDisposed = false;
				
		//further fields
		CategoryModel FCategoryModel = new CategoryModel();
		AlphabetModel FAlphabetModel = new AlphabetModel();
		
		#endregion field declaration
		
		#region constructor/destructor
		public NodeBrowserPluginNode()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			tabControlMain.SelectedIndex = 1;
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
                
				//nothing to declare
				
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
					FPluginInfo.Name = "NodeBrowser";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "HDE";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "The NodeInfo Browser";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "tag";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//define the nodes initial size in box-mode
					FPluginInfo.InitialBoxSize = new Size(100, 200);
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
			this.tabControlMain = new System.Windows.Forms.TabControl();
			this.tabAlphabetical = new System.Windows.Forms.TabPage();
			this.alphabetTreeViewer = new VVVV.HDE.Viewer.TreeViewer();
			this.tabCategory = new System.Windows.Forms.TabPage();
			this.categoryTreeViewer = new VVVV.HDE.Viewer.PanelTreeViewer();
			this.tabControlMain.SuspendLayout();
			this.tabAlphabetical.SuspendLayout();
			this.tabCategory.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlMain
			// 
			this.tabControlMain.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControlMain.Controls.Add(this.tabAlphabetical);
			this.tabControlMain.Controls.Add(this.tabCategory);
			this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlMain.Location = new System.Drawing.Point(0, 0);
			this.tabControlMain.Name = "tabControlMain";
			this.tabControlMain.SelectedIndex = 0;
			this.tabControlMain.Size = new System.Drawing.Size(325, 520);
			this.tabControlMain.TabIndex = 0;
			// 
			// tabAlphabetical
			// 
			this.tabAlphabetical.AutoScroll = true;
			this.tabAlphabetical.Controls.Add(this.alphabetTreeViewer);
			this.tabAlphabetical.Location = new System.Drawing.Point(4, 25);
			this.tabAlphabetical.Name = "tabAlphabetical";
			this.tabAlphabetical.Padding = new System.Windows.Forms.Padding(3);
			this.tabAlphabetical.Size = new System.Drawing.Size(317, 491);
			this.tabAlphabetical.TabIndex = 0;
			this.tabAlphabetical.Text = "Alphabetical";
			this.tabAlphabetical.UseVisualStyleBackColor = true;
			// 
			// alphabetTreeViewer
			// 
			this.alphabetTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.alphabetTreeViewer.Location = new System.Drawing.Point(3, 3);
			this.alphabetTreeViewer.Name = "alphabetTreeViewer";
			this.alphabetTreeViewer.ShowRoot = false;
			this.alphabetTreeViewer.Size = new System.Drawing.Size(311, 485);
			this.alphabetTreeViewer.TabIndex = 0;
			// 
			// tabCategory
			// 
			this.tabCategory.AutoScroll = true;
			this.tabCategory.Controls.Add(this.categoryTreeViewer);
			this.tabCategory.Location = new System.Drawing.Point(4, 25);
			this.tabCategory.Name = "tabCategory";
			this.tabCategory.Padding = new System.Windows.Forms.Padding(3);
			this.tabCategory.Size = new System.Drawing.Size(317, 491);
			this.tabCategory.TabIndex = 1;
			this.tabCategory.Text = "By Category";
			this.tabCategory.UseVisualStyleBackColor = true;
			// 
			// categoryTreeViewer
			// 
			this.categoryTreeViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.categoryTreeViewer.Location = new System.Drawing.Point(3, 3);
			this.categoryTreeViewer.Name = "categoryTreeViewer";
			this.categoryTreeViewer.ShowRoot = true;
			this.categoryTreeViewer.Size = new System.Drawing.Size(311, 485);
			this.categoryTreeViewer.TabIndex = 0;
			// 
			// NodeBrowserPluginNode
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Controls.Add(this.tabControlMain);
			this.Name = "NodeBrowserPluginNode";
			this.Size = new System.Drawing.Size(325, 520);
			this.tabControlMain.ResumeLayout(false);
			this.tabAlphabetical.ResumeLayout(false);
			this.tabCategory.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private VVVV.HDE.Viewer.TreeViewer alphabetTreeViewer;
		private VVVV.HDE.Viewer.PanelTreeViewer categoryTreeViewer;
		private System.Windows.Forms.TabPage tabCategory;
		private System.Windows.Forms.TabPage tabAlphabetical;
		private System.Windows.Forms.TabControl tabControlMain;
		
		#region initialization
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FPluginHost = Host;
		}
		
		public void SetHDEHost(IHDEHost Host)
		{
			//assign host
			FHDEHost = Host;
			
			//register nodeinfolisteners at hdehost
			FHDEHost.AddListener(this);
			
			//create AdapterFactory and provider
            NodeListAdapterFactory af = new NodeListAdapterFactory();
            var cp = new AdapterFactoryContentProvider(af);
            var lp = new AdapterFactoryLabelProvider(af);
            //var ddp = new AdapterFactoryDragDropProvider(af);
            
            //hand providers over to viewers
            categoryTreeViewer.SetContentProvider(cp);
            categoryTreeViewer.SetLabelProvider(lp);
            //categoryTreeViewer.SetDragDropProvider(ddp);
            
            alphabetTreeViewer.SetContentProvider(cp);
            alphabetTreeViewer.SetLabelProvider(lp);
            //alphabetTreeViewer.SetDragDropProvider(ddp);

            //hand model root over to viewers
            categoryTreeViewer.SetRoot(FCategoryModel);
            //alphabetTreeViewer.ShowRoot = true;
            alphabetTreeViewer.SetRoot(FAlphabetModel);
		}

		#endregion initialization
		
		public void NodeInfoAddedCB(INodeInfo nodeInfo)
		{
		    //insert the nodeInfo into the data model
		    FCategoryModel.Add(nodeInfo);
		    FAlphabetModel.Add(nodeInfo);
		    //the contentprovider will call its changed event to update the view
		}
		
		public void NodeInfoRemovedCB(INodeInfo nodeInfo)
		{
		    FCategoryModel.Remove(nodeInfo);
		    FAlphabetModel.Remove(nodeInfo);
		}
		
		public void SetFilterTags(string tags)
		{
		    
		}
	}
}
