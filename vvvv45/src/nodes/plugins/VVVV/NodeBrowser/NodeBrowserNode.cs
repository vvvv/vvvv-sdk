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
	public class NodeBrowserPluginNode: UserControl, INodeBrowser, INodeInfoListener, IWindowSelectionListener
	{
		#region field declaration
		
		//the hosts
		private IHDEHost FHDEHost;
		[Import]
		public INodeBrowserHost NodeBrowserHost {get; set;}
		// Track whether Dispose has been called.
		private bool FDisposed = false;
		
		//further fields
		private bool FNeedsRedraw = false;
		private string FPath;

		#endregion field declaration
		
		#region constructor/destructor
		//alternative constructor for standalone use
		public NodeBrowserPluginNode()
		{
			DefaultConstructor();
		}
		
		[ImportingConstructor]
		public NodeBrowserPluginNode(IHDEHost host)
		{
			DefaultConstructor();
			
			//register as nodeinfolistener at hdehost
			FHDEHost = host;
			FHDEHost.AddListener(this);
			
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
		
		private void InitializeComponent()
		{
			this.FBackgroundWorker = new System.ComponentModel.BackgroundWorker();
			this.FClonePanel = new VVVV.Nodes.NodeBrowser.ClonePanel();
			this.FTagPanel = new VVVV.Nodes.NodeBrowser.TagPanel();
			this.FCategoryPanel = new VVVV.Nodes.NodeBrowser.CategoryPanel();
			this.SuspendLayout();
			// 
			// FBackgroundWorker
			// 
			this.FBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.FBackgroundWorkerDoWork);
			// 
			// FClonePanel
			// 
			this.FClonePanel.BackColor = System.Drawing.Color.Silver;
			this.FClonePanel.Location = new System.Drawing.Point(187, 148);
			this.FClonePanel.Name = "FClonePanel";
			this.FClonePanel.Size = new System.Drawing.Size(100, 108);
			this.FClonePanel.TabIndex = 0;
			this.FClonePanel.Visible = false;
			this.FClonePanel.OnPanelChange += new VVVV.Nodes.NodeBrowser.PanelChangeHandler(this.FNodeBrowser_OnPanelChange);
			// 
			// FTagPanel
			// 
			this.FTagPanel.AllowDragDrop = true;
			this.FTagPanel.AndTags = true;
			this.FTagPanel.Location = new System.Drawing.Point(17, 24);
			this.FTagPanel.Name = "FTagPanel";
			this.FTagPanel.Path = null;
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
			this.Size = new System.Drawing.Size(373, 379);
			this.ResumeLayout(false);
		}
		private VVVV.Nodes.NodeBrowser.CategoryPanel FCategoryPanel;
		private VVVV.Nodes.NodeBrowser.TagPanel FTagPanel;
		private VVVV.Nodes.NodeBrowser.ClonePanel FClonePanel;
		private System.ComponentModel.BackgroundWorker FBackgroundWorker;
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
						FClonePanel.Initialize(nodeInfo);
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
			if ((text.EndsWith(".v4p")) || (text.EndsWith(".fx")) || (text.EndsWith(".dll")))
				NodeBrowserHost.CreateNodeFromFile(Path.Combine(Path.GetDirectoryName(FPath), text));
			else
				NodeBrowserHost.CreateComment(text);
		}
		
		void FNodeBrowser_ShowNodeReference(INodeInfo nodeInfo)
		{
			NodeBrowserHost.ShowNodeReference(nodeInfo);
		}
		
		void FNodeBrowser_ShowHelpPatch(INodeInfo nodeInfo)
		{
			NodeBrowserHost.ShowHelpPatch(nodeInfo);
		}
		
		void FClonePanel_Closed(INodeInfo nodeInfo, string Name, string Category, string Version)
		{
			if (nodeInfo != null)
				NodeBrowserHost.CloneNode(nodeInfo, Name, Category, Version);
			
			FNodeBrowser_OnPanelChange(NodeBrowserPage.ByTags, null);
		}
		
		#region INodeBrowser
		public void Initialize(string path, string text)
		{
			FTagPanel.Initialize(text);
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
		
		#region INodeInfoListener
		public void NodeInfoAddedCB(INodeInfo nodeInfo)
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
		
		public void NodeInfoUpdatedCB(INodeInfo nodeInfo)
		{
			FTagPanel.Update(nodeInfo);
        	FClonePanel.Update(nodeInfo);
        	FCategoryPanel.Update(nodeInfo);

        	FNeedsRedraw = true;
		}
		
		public void NodeInfoRemovedCB(INodeInfo nodeInfo)
		{
			FTagPanel.Remove(nodeInfo);
			FClonePanel.Remove(nodeInfo);
			FCategoryPanel.Remove(nodeInfo);
			
			FNeedsRedraw = true;
		}
		#endregion INodeInfoListener
		
		#region IWindowSelectionListener
		public void WindowSelectionChangeCB(IWindow window)
		{
			var windowtype = window.GetWindowType();
			
			if ((windowtype == WindowType.Patch) || (windowtype == WindowType.Module))
			{
				FPath = window.GetNode().GetNodeInfo().Filename;
				if (Path.IsPathRooted(FPath))
					FTagPanel.Path = FPath;
				else //seems to be on an unsaved patch
					FTagPanel.Path = "";
				
				if ((FTagPanel.Path != FPath) || (FNeedsRedraw))
				{
					//init view
					FBackgroundWorker.RunWorkerAsync();
				}				 
			}
		}
		#endregion IWindowSelectionListener
		
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Tab)
			{
				if (FClonePanel.Visible)
					FClonePanel.SelectNextControl(FClonePanel.ActiveControl, true, true, false, true);
				else
				{
					FTagPanel.AndTags = !FTagPanel.AndTags; //!FAndTags = !FAndTags;
					FTagPanel.Redraw();
				}
				return true;
			}
			else if ((keyData == (Keys.Tab | Keys.Shift)) && (FClonePanel.Visible))
			{
				FClonePanel.SelectNextControl(FClonePanel.ActiveControl, false, true, false, true);
				return true;
			}
			else
				return base.ProcessDialogKey(keyData);
		}
		
		void FBackgroundWorkerDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			FTagPanel.Redraw();
			FCategoryPanel.Redraw();
		}
	}

	public enum NodeBrowserPage {ByCategory, ByTags, Clone};
	public delegate void PanelChangeHandler(NodeBrowserPage page, INodeInfo nodeInfo);
	public delegate void CreateNodeHandler(INodeInfo nodeInfo);
	public delegate void CreateNodeFromStringHandler(string text);
}
