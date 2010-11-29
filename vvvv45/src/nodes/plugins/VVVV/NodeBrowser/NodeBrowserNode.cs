#region usings
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel.Composition;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Core.Collections;

using VVVV.Core.View;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
#endregion usings

//the vvvv node namespace
namespace VVVV.Nodes.NodeBrowser
{
	[PluginInfo(Name = "NodeBrowser",
	            Category = "VVVV",
	            Shortcut = "Ctrl+N",
	            Author = "vvvv group",
	            Help = "The NodeInfo Browser",
	            InitialBoxWidth = 100,
	            InitialBoxHeight = 200,
	            InitialWindowWidth = 300,
	            InitialWindowHeight = 500,
	            InitialComponentMode = TComponentMode.InAWindow)]
	public class NodeBrowserPluginNode: UserControl, INodeBrowser, IWindowSelectionListener
	{
		#region field declaration
		
		//the hosts
		public IHDEHost HDEHost
		{
			get;
			private set;
		}
		
		[Import]
		public INodeBrowserHost NodeBrowserHost {get; set;}
		[Import]
		public ILogger FLogger {get; set;}
		
		private INodeInfoFactory FNodeInfoFactory;
		public INodeInfoFactory NodeInfoFactory
		{
			get { return FNodeInfoFactory; }
			set
			{
				FNodeInfoFactory = value;

				foreach (var nodeInfo in FNodeInfoFactory.NodeInfos)
					NodeInfoAddedCB(FNodeInfoFactory, nodeInfo);

				FNodeInfoFactory.NodeInfoAdded += NodeInfoAddedCB;
				FNodeInfoFactory.NodeInfoUpdated += NodeInfoUpdatedCB;
				FNodeInfoFactory.NodeInfoRemoved += NodeInfoRemovedCB;
			}
		}
		
		public IWindow CurrentPatchWindow
		{
			get;
			private set;
		}

		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		//further fields
		private bool FNeedsRedraw = false;
		public string CurrentDir
		{
			get
			{
				if (CurrentPatchWindow != null)
				{
					var node = CurrentPatchWindow.GetNode();
					var nodeInfo = node.GetNodeInfo();
					var systemname = nodeInfo.Systemname;
					var filename = nodeInfo.Filename;
					
					if (Path.IsPathRooted(filename))
						return Path.GetDirectoryName(filename);
					else
						return string.Empty;
				}
				else
					return string.Empty;
			}
		}

		#endregion field declaration
		
		#region constructor/destructor
		//alternative constructor for standalone use
		public NodeBrowserPluginNode()
		{
			DefaultConstructor();
		}
		
		[ImportingConstructor]
		public NodeBrowserPluginNode(IHDEHost host, INodeInfoFactory nodeInfoFactory)
		{
			DefaultConstructor();
			
			//register as IWindowSelectionListener at hdehost
			HDEHost = host;
			HDEHost.AddListener(this);
			
			NodeInfoFactory = nodeInfoFactory;
			
			//init category view
			FCategoryPanel.Redraw();
		}
		
		private void DefaultConstructor()
		{
			// The InitializeComponent() call is required for Windows Forms designer support.
			InitializeComponent();
			
			FClonePanel.Closed += new ClonePanelEventHandler(FClonePanel_Closed);

			FClonePanel.Dock = DockStyle.Fill;
			FCategoryPanel.Dock = DockStyle.Fill;
			FTagPanel.Dock = DockStyle.Fill;
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
					HDEHost.RemoveListener(this);
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
		
		private void InitializeComponent()
		{
			this.FClonePanel = new VVVV.Nodes.NodeBrowser.ClonePanel();
			this.FTagPanel = new VVVV.Nodes.NodeBrowser.TagPanel();
			this.FCategoryPanel = new VVVV.Nodes.NodeBrowser.CategoryPanel();
			this.SuspendLayout();
			// 
			// FClonePanel
			// 
			this.FClonePanel.BackColor = System.Drawing.Color.Silver;
			this.FClonePanel.Location = new System.Drawing.Point(187, 148);
			this.FClonePanel.Name = "FClonePanel";
			this.FClonePanel.Padding = new System.Windows.Forms.Padding(8);
			this.FClonePanel.Size = new System.Drawing.Size(250, 300);
			this.FClonePanel.TabIndex = 0;
			this.FClonePanel.Visible = false;
			// 
			// FTagPanel
			// 
			this.FTagPanel.AllowDragDrop = true;
			this.FTagPanel.AndTags = true;
			this.FTagPanel.Location = new System.Drawing.Point(17, 24);
			this.FTagPanel.Name = "FTagPanel";
			this.FTagPanel.Size = new System.Drawing.Size(120, 115);
			this.FTagPanel.TabIndex = 1;
			this.FTagPanel.OnCreateNode += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.FNodeBrowser_CreateNode);
			this.FTagPanel.OnPanelChange += new VVVV.Nodes.NodeBrowser.PanelChangeHandler(this.FNodeBrowser_OnPanelChange);
			this.FTagPanel.OnShowHelpPatch += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.FNodeBrowser_ShowHelpPatch);
			this.FTagPanel.OnCreateNodeFromString += new VVVV.Nodes.NodeBrowser.CreateNodeFromStringHandler(this.FNodeBrowser_CreateNodeFromString);
			this.FTagPanel.OnShowNodeReference += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.FNodeBrowser_ShowNodeReference);
			// 
			// FCategoryPanel
			// 
			this.FCategoryPanel.Location = new System.Drawing.Point(17, 161);
			this.FCategoryPanel.Name = "FCategoryPanel";
			this.FCategoryPanel.Size = new System.Drawing.Size(119, 85);
			this.FCategoryPanel.TabIndex = 2;
			this.FCategoryPanel.Visible = false;
			this.FCategoryPanel.OnCreateNode += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.FNodeBrowser_CreateNode);
			this.FCategoryPanel.OnPanelChange += new VVVV.Nodes.NodeBrowser.PanelChangeHandler(this.FNodeBrowser_OnPanelChange);
			this.FCategoryPanel.OnShowHelpPatch += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.FNodeBrowser_ShowHelpPatch);
			this.FCategoryPanel.OnShowNodeReference += new VVVV.Nodes.NodeBrowser.CreateNodeHandler(this.FNodeBrowser_ShowNodeReference);
			// 
			// NodeBrowserPluginNode
			// 
			this.BackColor = System.Drawing.Color.Silver;
			this.Controls.Add(this.FCategoryPanel);
			this.Controls.Add(this.FTagPanel);
			this.Controls.Add(this.FClonePanel);
			this.DoubleBuffered = true;
			this.Name = "NodeBrowserPluginNode";
			this.Size = new System.Drawing.Size(599, 479);
			this.ResumeLayout(false);
		}
		private VVVV.Nodes.NodeBrowser.CategoryPanel FCategoryPanel;
		private VVVV.Nodes.NodeBrowser.TagPanel FTagPanel;
		private VVVV.Nodes.NodeBrowser.ClonePanel FClonePanel;
		#endregion constructor/destructor
		
		void FNodeBrowser_OnPanelChange(NodeBrowserPage page, INodeInfo nodeInfo)
		{
			switch (page)
			{
				case NodeBrowserPage.ByCategory:
					{
						FClonePanel.Visible = false;
						FTagPanel.Visible = false;
						
						FCategoryPanel.Visible = true;
						break;
					}
				case NodeBrowserPage.ByTags:
					{
						FClonePanel.Visible = false;
						FCategoryPanel.Visible = false;
						
						FTagPanel.Visible = true;
						break;
					}
				case NodeBrowserPage.Clone:
					{
						FTagPanel.Visible = false;
						FCategoryPanel.Visible = false;
						
						FClonePanel.Visible = true;
						
						var path = HDEHost.ExePath;
						if (!string.IsNullOrEmpty(CurrentDir))
							path = CurrentDir;
						
						if (nodeInfo.Factory != null)
							path = path.ConcatPath(nodeInfo.Factory.JobStdSubPath);
						FClonePanel.Initialize(nodeInfo, path);
						break;
					}
			}
		}

		void FNodeBrowser_CreateNode(INodeInfo nodeInfo)
		{
			NodeBrowserHost.CreateNode(nodeInfo);
		}
		
		void FNodeBrowser_CreateNodeFromString(string text)
		{
			// TODO: Ask factories about file extensions.
			if ((text.EndsWith(".v4p")) || (text.EndsWith(".fx")) || (text.EndsWith(".dll")))
				NodeBrowserHost.CreateNodeFromFile(Path.Combine(CurrentDir, text));
			else
				NodeBrowserHost.CreateComment(text);
		}
		
		void FNodeBrowser_ShowNodeReference(INodeInfo nodeInfo)
		{
			HDEHost.ShowNodeReference(nodeInfo);
		}
		
		void FNodeBrowser_ShowHelpPatch(INodeInfo nodeInfo)
		{
			HDEHost.ShowHelpPatch(nodeInfo);
		}
		
		void FClonePanel_Closed(INodeInfo nodeInfo, string Name, string Category, string Version, string path)
		{
			if (nodeInfo != null)
				NodeBrowserHost.CloneNode(nodeInfo, path, Name, Category, Version);
			
			FNodeBrowser_OnPanelChange(NodeBrowserPage.ByTags, null);
		}
		
		#region INodeBrowser
		public void Initialize(string text)
		{
			FTagPanel.Initialize(this, text);
			FNodeBrowser_OnPanelChange(NodeBrowserPage.ByTags, null);
		}
		
		public new void DragDrop(bool allow)
		{
			FTagPanel.AllowDragDrop = allow;
		}
		
		public void AfterShow()
		{
			FTagPanel.AfterShow();
		}
		
		public void BeforeHide()
		{
			FTagPanel.BeforeHide();
			FCategoryPanel.BeforeHide();
		}
		#endregion INodeBrowser
		
		#region INodeInfoFactory events
		public void NodeInfoAddedCB(object sender, INodeInfo nodeInfo)
		{
			string nodeVersion = nodeInfo.Version;

			//don't include legacy or ignored nodes in the list
			if (((!string.IsNullOrEmpty(nodeVersion)) && (nodeVersion.ToLower().Contains("legacy"))) || (nodeInfo.Ignore))
				return;
			
			FTagPanel.Add(nodeInfo);
			FClonePanel.Add(nodeInfo);
			FCategoryPanel.Add(nodeInfo);
			
			FNeedsRedraw = true;
		}
		
		public void NodeInfoUpdatedCB(object sender, INodeInfo nodeInfo)
		{
		    string nodeVersion = nodeInfo.Version;

			//don't include legacy or ignored nodes in the list
			if (((!string.IsNullOrEmpty(nodeVersion)) && (nodeVersion.ToLower().Contains("legacy"))) || (nodeInfo.Ignore))
				return;
			
			FTagPanel.Update(nodeInfo);
			FClonePanel.Update(nodeInfo);
			FCategoryPanel.Update(nodeInfo);

			FNeedsRedraw = true;
		}
		
		public void NodeInfoRemovedCB(object sender, INodeInfo nodeInfo)
		{
			FTagPanel.Remove(nodeInfo);
			FClonePanel.Remove(nodeInfo);
			FCategoryPanel.Remove(nodeInfo);
			
			FNeedsRedraw = true;
		}
		#endregion INodeInfoFactory events
		
		#region IWindowSelectionListener
		public void WindowSelectionChangeCB(IWindow window)
		{
			var windowtype = window.GetWindowType();
			
			if ((windowtype == WindowType.Patch) || (windowtype == WindowType.Module))
			{
				CurrentPatchWindow = window;
				
				//cant do in thread. would not update outside IDE
				if (FNeedsRedraw) //as doesn't show localfiles needs no redraw on pathchange
					FCategoryPanel.Redraw();
				
				FNeedsRedraw = false;
			}
		}
		#endregion IWindowSelectionListener
		
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Tab)
			{
				if (FClonePanel.Visible)
					FClonePanel.SelectNextControl(FClonePanel.ActiveControl, true, true, true, true);
				else
				{
					FTagPanel.AndTags = !FTagPanel.AndTags;
					FTagPanel.Redraw();
				}
				return true;
			}
			else if ((keyData == (Keys.Tab | Keys.Shift)) && (FClonePanel.Visible))
			{
				FClonePanel.SelectNextControl(FClonePanel.ActiveControl, false, true, true, true);
				return true;
			}
			else if (keyData == Keys.Escape && FClonePanel.Visible)
			{
				FNodeBrowser_OnPanelChange(NodeBrowserPage.ByTags, null);
				return true;
			}
			else
				return base.ProcessDialogKey(keyData);
		}
	}

	public enum NodeBrowserPage {ByCategory, ByTags, Clone};
	public delegate void PanelChangeHandler(NodeBrowserPage page, INodeInfo nodeInfo);
	public delegate void CreateNodeHandler(INodeInfo nodeInfo);
	public delegate void CreateNodeFromStringHandler(string text);
}
